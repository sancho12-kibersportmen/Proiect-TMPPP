using FlightBooking.Interfaces;
using FlightBooking.Models;

namespace FlightBooking.Services
{
    // SRP: se ocupa exclusiv de logica de rezervare
    // DIP: depinde de interfete (IBookingService, IReservationRepository, IPricingStrategy)
    //      nu de implementari concrete
    public class BookingService : IBookingService
    {
        private readonly IReservationRepository _reservationRepo;
        private readonly IPricingStrategy       _pricingStrategy;

        // DIP: dependentele sunt injectate, nu instantiate intern
        public BookingService(IReservationRepository reservationRepo, IPricingStrategy pricingStrategy)
        {
            _reservationRepo = reservationRepo ?? throw new ArgumentNullException(nameof(reservationRepo));
            _pricingStrategy = pricingStrategy ?? throw new ArgumentNullException(nameof(pricingStrategy));
        }

        public Reservation CreateReservation(Passenger passenger, Flight flight, string seatNumber)
        {
            if (!flight.HasAvailableSeats())
            {
                AppLogger.Instance.Warning("BookingService",
                    $"Zborul {flight.FlightNumber} nu are locuri disponibile.");
                throw new InvalidOperationException($"Zborul {flight.FlightNumber} nu are locuri disponibile.");
            }

            var price       = _pricingStrategy.Calculate(flight);
            var ticket      = new Ticket(flight, passenger, seatNumber, price);
            var reservation = new Reservation(passenger);

            reservation.AddTicket(ticket);
            flight.ReserveSeat();
            _reservationRepo.Save(reservation);

            AppLogger.Instance.Info("BookingService",
                $"Rezervare creata #{reservation.ReservationId} | {passenger.FullName} | " +
                $"{flight.FlightNumber} | Loc:{seatNumber} | {price:C}");

            return reservation;
        }

        public void ConfirmReservation(string reservationId)
        {
            var reservation = GetReservationOrThrow(reservationId);
            reservation.MarkAsPaid();
            _reservationRepo.Save(reservation);
            AppLogger.Instance.Info("BookingService",
                $"Rezervare #{reservationId} confirmata. Total: {reservation.TotalPrice:C}");
        }

        public void CancelReservation(string reservationId)
        {
            var reservation = GetReservationOrThrow(reservationId);
            reservation.Cancel();
            _reservationRepo.Save(reservation);
            AppLogger.Instance.Warning("BookingService",
                $"Rezervare #{reservationId} anulata de {reservation.Passenger.FullName}");
        }

        public Reservation? GetReservation(string reservationId) =>
            _reservationRepo.GetById(reservationId);

        // ─── helper privat ───────────────────────────────────────────────────
        private Reservation GetReservationOrThrow(string id) =>
            _reservationRepo.GetById(id)
            ?? throw new KeyNotFoundException($"Rezervarea cu ID-ul '{id}' nu a fost gasita.");
    }
}
