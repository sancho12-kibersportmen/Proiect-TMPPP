using FlightBooking.Models;

namespace FlightBooking.Interfaces
{
    // ── Produs abstract (Abstract Factory – familia "formatare bilet") ──
    public interface ITicketFormatter
    {
        string Format(Ticket ticket);
    }
}
