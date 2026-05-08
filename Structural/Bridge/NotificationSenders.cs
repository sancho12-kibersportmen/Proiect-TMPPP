using FlightBooking.Interfaces;
using FlightBooking.Models;

namespace FlightBooking.Structural.Bridge
{
    // ══════════════════════════════════════════════════════════════════
    //  BRIDGE PATTERN
    //
    //  Problema rezolvata: avem N tipuri de notificari (confirmare,
    //  anulare, reminder) si M formate de mesaj (PlainText, HTML,
    //  Markdown). Fara Bridge am avea N×M clase. Cu Bridge avem N+M.
    //
    //  Abstractizare: TIPUL de notificare (ce trimitem)
    //    → BookingConfirmationSender, CancellationSender, ReminderSender
    //  Implementor:   FORMATUL mesajului (cum il randam)
    //    → PlainTextRenderer, HtmlRenderer, MarkdownRenderer
    //
    //  Bridge-ul: abstractizarea contine o referinta la implementor
    //  si delega randarea catre el. Cele doua axe evolueaza independent.
    // ══════════════════════════════════════════════════════════════════

    // ── Abstractizare de baza ────────────────────────────────────────
    public abstract class NotificationSender
    {
        // Bridge: referinta la implementor
        protected readonly IMessageRenderer _renderer;

        protected NotificationSender(IMessageRenderer renderer)
            => _renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));

        public abstract void Send(Reservation reservation);

        protected void Output(string message)
            => Console.WriteLine($"  [{GetType().Name} via {_renderer.RendererName}]\n{message}\n");
    }

    // ── Abstractizare rafinata 1 – Confirmare rezervare ─────────────
    public class BookingConfirmationSender : NotificationSender
    {
        public BookingConfirmationSender(IMessageRenderer renderer) : base(renderer) { }

        public override void Send(Reservation reservation)
        {
            var ticket  = reservation.Tickets.First();
            var message = string.Join("\n", new[]
            {
                _renderer.RenderTitle("Rezervare Confirmata!"),
                _renderer.RenderBody($"Buna ziua, {reservation.Passenger.FullName}!"),
                _renderer.RenderField("Rezervare", reservation.ReservationId),
                _renderer.RenderField("Zbor",      ticket.Flight.FlightNumber),
                _renderer.RenderField("Ruta",      $"{ticket.Flight.Origin.Code} -> {ticket.Flight.Destination.Code}"),
                _renderer.RenderField("Data",      ticket.Flight.DepartureTime.ToString("dd MMM yyyy HH:mm")),
                _renderer.RenderField("Loc",       ticket.SeatNumber),
                _renderer.RenderField("Total",     reservation.TotalPrice.ToString("C")),
                _renderer.RenderFooter("Va multumim ca ati ales FlightBooking!")
            });
            Output(message);
        }
    }

    // ── Abstractizare rafinata 2 – Anulare rezervare ─────────────────
    public class CancellationSender : NotificationSender
    {
        public CancellationSender(IMessageRenderer renderer) : base(renderer) { }

        public override void Send(Reservation reservation)
        {
            var message = string.Join("\n", new[]
            {
                _renderer.RenderTitle("Rezervare Anulata"),
                _renderer.RenderBody($"Rezervarea #{reservation.ReservationId} a fost anulata."),
                _renderer.RenderField("Pasager",  reservation.Passenger.FullName),
                _renderer.RenderField("Rambursare", reservation.TotalPrice.ToString("C")),
                _renderer.RenderFooter("Rambursarea va fi procesata in 3-5 zile lucratoare.")
            });
            Output(message);
        }
    }

    // ── Abstractizare rafinata 3 – Reminder zbor ─────────────────────
    public class FlightReminderSender : NotificationSender
    {
        private readonly int _hoursBeforeFlight;

        public FlightReminderSender(IMessageRenderer renderer, int hoursBeforeFlight = 24)
            : base(renderer) => _hoursBeforeFlight = hoursBeforeFlight;

        public override void Send(Reservation reservation)
        {
            var ticket  = reservation.Tickets.First();
            var message = string.Join("\n", new[]
            {
                _renderer.RenderTitle($"Reminder: Zbor in {_hoursBeforeFlight}h"),
                _renderer.RenderBody("Nu uitati: zborul dumneavoastra este maine!"),
                _renderer.RenderField("Zbor",  ticket.Flight.FlightNumber),
                _renderer.RenderField("Poarta","Gate B12"),
                _renderer.RenderField("Check-in", "Online disponibil"),
                _renderer.RenderFooter("Sositi la aeroport cu cel putin 2 ore inainte.")
            });
            Output(message);
        }
    }
}
