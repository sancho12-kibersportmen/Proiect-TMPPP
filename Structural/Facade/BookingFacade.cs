using FlightBooking.Factories;
using FlightBooking.Interfaces;
using FlightBooking.Models;
using FlightBooking.Services;
using FlightBooking.Structural.Adapter;
using FlightBooking.Structural.Composite;
using FlightBooking.Utils;

namespace FlightBooking.Structural.Facade
{
    // ══════════════════════════════════════════════════════════════════
    //  FAÇADE PATTERN
    //
    //  Problema rezolvata: pentru a rezerva un zbor, clientul trebuie
    //  sa cunoasca si sa orchestreze 6+ servicii:
    //    FlightSearchService, BookingService, PaymentService,
    //    NotificationFactory, TicketOutputService, AppLogger...
    //
    //  Solutia: BookingFacade ofera 3 metode simple care ascund toata
    //  complexitatea subsistemului. Clientul apeleaza:
    //    facade.SearchFlights(...)
    //    facade.BookAndPay(...)
    //    facade.CancelBooking(...)
    //  ... si atat.
    // ══════════════════════════════════════════════════════════════════

    public class BookingFacade
    {
        // ── Subsistemele interne (ascunse de la client) ──────────────
        private readonly FlightSearchService  _searchService;
        private readonly BookingService       _bookingService;
        private readonly PaymentService       _paymentService;
        private readonly TicketOutputService  _outputService;
        private readonly NotificationFactory  _notifier;

        // DIP: toate dependentele sunt injectate
        public BookingFacade(
            FlightSearchService  searchService,
            BookingService       bookingService,
            PaymentService       paymentService,
            TicketOutputService  outputService,
            NotificationFactory  notifier)
        {
            _searchService  = searchService  ?? throw new ArgumentNullException(nameof(searchService));
            _bookingService = bookingService ?? throw new ArgumentNullException(nameof(bookingService));
            _paymentService = paymentService ?? throw new ArgumentNullException(nameof(paymentService));
            _outputService  = outputService  ?? throw new ArgumentNullException(nameof(outputService));
            _notifier       = notifier       ?? throw new ArgumentNullException(nameof(notifier));
        }

        // ═══════════════════════════════════════════════════════════
        //  Metoda 1 – Cautare simplificata
        // ═══════════════════════════════════════════════════════════
        public IEnumerable<Flight> SearchFlights(
            string originCode, string destinationCode,
            DateTime date, SeatClass? seatClass = null)
        {
            AppLogger.Instance.Info("BookingFacade",
                $"Cautare: {originCode}→{destinationCode} | {date:dd MMM yyyy}");

            return _searchService.Search(originCode, destinationCode, date, seatClass);
        }

        // ═══════════════════════════════════════════════════════════
        //  Metoda 2 – Rezervare + Plata + Notificare + Afisare bilet
        //  (orchestreaza 5 subsisteme intr-un singur apel)
        // ═══════════════════════════════════════════════════════════
        public BookingResult BookAndPay(
            Passenger passenger,
            Flight    flight,
            string    customerId)
        {
            AppLogger.Instance.Info("BookingFacade",
                $"Initiere BookAndPay pentru {passenger.FullName} | {flight.FlightNumber}");

            try
            {
                // Pas 1: Rezervare
                var seatNumber  = SeatGenerator.Generate(flight.TotalSeats);
                var reservation = _bookingService.CreateReservation(passenger, flight, seatNumber);

                // Pas 2: Plata
                var transactionId = _paymentService.PayForReservation(reservation, customerId);

                // Pas 3: Confirmare rezervare
                _bookingService.ConfirmReservation(reservation.ReservationId);
                reservation = _bookingService.GetReservation(reservation.ReservationId)!;

                // Pas 4: Notificare pasager
                _notifier.NotifyReservationConfirmed(reservation);

                // Pas 5: Afisare bilet
                Console.WriteLine();
                _outputService.DisplayTicket(reservation.Tickets.First());
                _outputService.PrintBoardingPass(reservation.Tickets.First());

                return new BookingResult(true, reservation.ReservationId, transactionId, null);
            }
            catch (Exception ex)
            {
                AppLogger.Instance.Error("BookingFacade", $"Eroare BookAndPay: {ex.Message}");
                return new BookingResult(false, null, null, ex.Message);
            }
        }

        // ═══════════════════════════════════════════════════════════
        //  Metoda 3 – Anulare + Rambursare + Notificare
        // ═══════════════════════════════════════════════════════════
        public bool CancelBooking(string reservationId, string transactionId)
        {
            AppLogger.Instance.Info("BookingFacade",
                $"Initiere anulare rezervare #{reservationId}");

            try
            {
                var reservation = _bookingService.GetReservation(reservationId);
                if (reservation == null)
                {
                    AppLogger.Instance.Warning("BookingFacade",
                        $"Rezervarea #{reservationId} nu a fost gasita.");
                    return false;
                }

                // Pas 1: Rambursare
                _paymentService.RefundReservation(transactionId, reservation.TotalPrice);

                // Pas 2: Anulare rezervare
                _bookingService.CancelReservation(reservationId);

                // Pas 3: Notificare
                reservation = _bookingService.GetReservation(reservationId)!;
                _notifier.NotifyReservationCancelled(reservation);

                return true;
            }
            catch (Exception ex)
            {
                AppLogger.Instance.Error("BookingFacade", $"Eroare CancelBooking: {ex.Message}");
                return false;
            }
        }

        // ═══════════════════════════════════════════════════════════
        //  Metoda 4 – Construire itinerar compus (Composite + Facade)
        // ═══════════════════════════════════════════════════════════
        public Itinerary BuildRoundTripItinerary(
            string name,
            Flight outbound, decimal outboundPrice,
            Flight inbound,  decimal inboundPrice)
        {
            var itinerary = new Itinerary(name);
            itinerary.Add(new FlightSegment(outbound, outboundPrice));
            itinerary.Add(new FlightSegment(inbound,  inboundPrice));
            return itinerary;
        }
    }

    // ── Rezultatul operatiei BookAndPay ──────────────────────────────
    public record BookingResult(
        bool    Success,
        string? ReservationId,
        string? TransactionId,
        string? ErrorMessage);
}
