using FlightBooking.Factories.Notifications;
using FlightBooking.Interfaces;
using FlightBooking.Models;

namespace FlightBooking.Factories
{
    // ══════════════════════════════════════════════════════════════════
    //  FACTORY METHOD PATTERN
    //
    //  Creator abstract: defineste metoda de fabrica CreateNotification().
    //  Subclasele decid CE obiect concret sa instantieze.
    //  Codul care foloseste notificarile lucreaza doar cu INotification,
    //  fara sa stie de Email / SMS / Push.
    // ══════════════════════════════════════════════════════════════════

    // Creator abstract
    public abstract class NotificationFactory
    {
        // ── Factory Method ──────────────────────────────────────────
        public abstract INotification CreateNotification();

        // ── Logica comuna (template) ────────────────────────────────
        // Toti creatorii folosesc aceeasi logica de trimitere,
        // dar fiecare instanciaza alt tip de notificare.
        public void NotifyReservationConfirmed(Reservation reservation)
        {
            var notification = CreateNotification();
            var subject      = $"Rezervarea #{reservation.ReservationId} confirmata!";
            var body         = BuildConfirmationBody(reservation);
            notification.Send(reservation.Passenger.Email, subject, body);
        }

        public void NotifyReservationCancelled(Reservation reservation)
        {
            var notification = CreateNotification();
            var subject      = $"Rezervarea #{reservation.ReservationId} anulata";
            var body         = $"Rezervarea dumneavoastra a fost anulata. Total: {reservation.TotalPrice:C}";
            notification.Send(reservation.Passenger.Email, subject, body);
        }

        private static string BuildConfirmationBody(Reservation res)
        {
            var ticket = res.Tickets.First();
            return $"Buna ziua, {res.Passenger.FullName}! " +
                   $"Zborul {ticket.Flight.FlightNumber} " +
                   $"{ticket.Flight.Origin.Code}→{ticket.Flight.Destination.Code} " +
                   $"din {ticket.Flight.DepartureTime:dd MMM yyyy HH:mm} " +
                   $"a fost confirmat. Loc: {ticket.SeatNumber}. " +
                   $"Total achitat: {res.TotalPrice:C}.";
        }
    }

    // ── Creatori concreti ────────────────────────────────────────────

    // Creator concret 1
    public class EmailNotificationFactory : NotificationFactory
    {
        public override INotification CreateNotification() => new EmailNotification();
    }

    // Creator concret 2
    public class SmsNotificationFactory : NotificationFactory
    {
        public override INotification CreateNotification() => new SmsNotification();
    }

    // Creator concret 3
    public class PushNotificationFactory : NotificationFactory
    {
        public override INotification CreateNotification() => new PushNotification();
    }
}
