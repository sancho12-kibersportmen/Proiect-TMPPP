using FlightBooking.Models;
using FlightBooking.Services;

namespace FlightBooking.Behavioral.Mediator
{
    // ══════════════════════════════════════════════════════════════════
    //  MEDIATOR PATTERN
    //
    //  Problema rezolvata: componentele BookingService, PaymentService,
    //  NotificationService si InventoryService trebuie sa colaboreze
    //  la o rezervare, dar nu trebuie sa se cunoasca direct intre ele
    //  (altfel creem o retea de dependente haotice N*N).
    //
    //  Solutia: toate componentele comunica EXCLUSIV prin
    //  BookingMediator. Fiecare stie doar de mediator, nu de celelalte.
    //  Mediatorul orchestreaza fluxul si reduce cuplarea la N*1.
    // ══════════════════════════════════════════════════════════════════

    // ── Interfata Mediator ───────────────────────────────────────────
    public interface IBookingMediator
    {
        void Notify(MediatorComponent sender, string eventName, object? data = null);
    }

    // ── Componenta de baza (cunoaste doar mediatorul) ────────────────
    public abstract class MediatorComponent
    {
        protected IBookingMediator _mediator = null!;
        public abstract string ComponentName { get; }

        public void SetMediator(IBookingMediator mediator)
            => _mediator = mediator;

        protected void Send(string eventName, object? data = null)
            => _mediator.Notify(this, eventName, data);
    }

    // ── Componente concrete ──────────────────────────────────────────

    public class ReservationComponent : MediatorComponent
    {
        private Reservation? _lastReservation;
        public override string ComponentName => "ReservationService";

        public void CreateReservation(Reservation reservation)
        {
            _lastReservation = reservation;
            Console.WriteLine($"  [{ComponentName}] Rezervare creata #{reservation.ReservationId}");
            Send("ReservationCreated", reservation);
        }

        public void ConfirmReservation()
        {
            if (_lastReservation == null) return;
            _lastReservation.MarkAsPaid();
            Console.WriteLine($"  [{ComponentName}] Rezervare confirmata #{_lastReservation.ReservationId}");
            Send("ReservationConfirmed", _lastReservation);
        }

        public void CancelReservation()
        {
            if (_lastReservation == null) return;
            _lastReservation.Cancel();
            Console.WriteLine($"  [{ComponentName}] Rezervare anulata #{_lastReservation.ReservationId}");
            Send("ReservationCancelled", _lastReservation);
        }
    }

    public class PaymentComponent : MediatorComponent
    {
        public override string ComponentName => "PaymentService";
        public string? LastTransactionId { get; private set; }

        public void ProcessPayment(Reservation reservation)
        {
            LastTransactionId = $"TX-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
            Console.WriteLine($"  [{ComponentName}] Plata procesata: {reservation.TotalPrice:C} | TxID: {LastTransactionId}");
            Send("PaymentProcessed", LastTransactionId);
        }

        public void Refund(decimal amount)
        {
            Console.WriteLine($"  [{ComponentName}] Rambursare initiata: {amount:C}");
            Send("RefundInitiated", amount);
        }
    }

    public class NotificationComponent : MediatorComponent
    {
        public override string ComponentName => "NotificationService";
        public List<string> SentMessages { get; } = new();

        public void SendConfirmation(Reservation reservation)
        {
            var msg = $"Rezervare #{reservation.ReservationId} confirmata pentru {reservation.Passenger.FullName}";
            SentMessages.Add(msg);
            Console.WriteLine($"  [{ComponentName}] EMAIL: {msg}");
        }

        public void SendCancellation(Reservation reservation)
        {
            var msg = $"Rezervare #{reservation.ReservationId} anulata.";
            SentMessages.Add(msg);
            Console.WriteLine($"  [{ComponentName}] EMAIL: {msg}");
        }

        public void SendPaymentConfirmation(string txId)
        {
            var msg = $"Plata confirmata: {txId}";
            SentMessages.Add(msg);
            Console.WriteLine($"  [{ComponentName}] EMAIL: {msg}");
        }
    }

    public class InventoryComponent : MediatorComponent
    {
        public override string ComponentName => "InventoryService";
        public int ReservedSeats { get; private set; } = 0;

        public void BlockSeat(string flightNumber, string seat)
        {
            ReservedSeats++;
            Console.WriteLine($"  [{ComponentName}] Loc {seat} blocat pe zborul {flightNumber}");
        }

        public void ReleaseSeat(string flightNumber, string seat)
        {
            if (ReservedSeats > 0) ReservedSeats--;
            Console.WriteLine($"  [{ComponentName}] Loc {seat} eliberat pe zborul {flightNumber}");
        }
    }

    // ── Mediator concret ─────────────────────────────────────────────
    // Cunoaste TOATE componentele si orchestreaza interactiunile
    public class ConcreteBookingMediator : IBookingMediator
    {
        private readonly ReservationComponent  _reservation;
        private readonly PaymentComponent      _payment;
        private readonly NotificationComponent _notification;
        private readonly InventoryComponent    _inventory;

        // Stocam datele contextuale intre evenimente
        private Reservation? _currentReservation;

        public ConcreteBookingMediator(
            ReservationComponent  reservation,
            PaymentComponent      payment,
            NotificationComponent notification,
            InventoryComponent    inventory)
        {
            _reservation  = reservation;
            _payment      = payment;
            _notification = notification;
            _inventory    = inventory;

            // Inregistram mediatorul in fiecare componenta
            _reservation .SetMediator(this);
            _payment     .SetMediator(this);
            _notification.SetMediator(this);
            _inventory   .SetMediator(this);
        }

        // ── Orchestrarea evenimentelor ───────────────────────────────
        public void Notify(MediatorComponent sender, string eventName, object? data)
        {
            AppLogger.Instance.Info("Mediator",
                $"Eveniment de la {sender.ComponentName}: {eventName}");

            switch (eventName)
            {
                case "ReservationCreated":
                    _currentReservation = data as Reservation;
                    if (_currentReservation != null)
                    {
                        var ticket = _currentReservation.Tickets.FirstOrDefault();
                        if (ticket != null)
                            _inventory.BlockSeat(
                                _currentReservation.Tickets.First().Flight.FlightNumber,
                                ticket.SeatNumber);
                        _payment.ProcessPayment(_currentReservation);
                    }
                    break;

                case "PaymentProcessed":
                    _reservation.ConfirmReservation();
                    break;

                case "ReservationConfirmed":
                    var confirmedRes = data as Reservation ?? _currentReservation;
                    if (confirmedRes != null)
                        _notification.SendConfirmation(confirmedRes);
                    if (data is string txId)
                        _notification.SendPaymentConfirmation(txId);
                    break;

                case "ReservationCancelled":
                    var cancelledRes = data as Reservation ?? _currentReservation;
                    if (cancelledRes != null)
                    {
                        var ticket = cancelledRes.Tickets.FirstOrDefault();
                        if (ticket != null)
                            _inventory.ReleaseSeat(
                                ticket.Flight.FlightNumber,
                                ticket.SeatNumber);
                        _payment.Refund(cancelledRes.TotalPrice);
                        _notification.SendCancellation(cancelledRes);
                    }
                    break;
            }
        }

        // ── Metoda de intrare pentru clientul extern ─────────────────
        public void BookFlight(Reservation reservation)
        {
            Console.WriteLine($"\n  [Mediator] Initiere flux rezervare pentru {reservation.Passenger.FullName}");
            _reservation.CreateReservation(reservation);
        }

        public void CancelFlight()
        {
            Console.WriteLine($"\n  [Mediator] Initiere flux anulare");
            _reservation.CancelReservation();
        }
    }
}
