using FlightBooking.Models;

namespace FlightBooking.Interfaces
{
    // ── Visitor abstract ─────────────────────────────────────────────
    public interface IBookingVisitor
    {
        string VisitorName { get; }
        void Visit(Ticket ticket);
        void Visit(Reservation reservation);
        void Visit(Flight flight);
    }

    // ── Element abstract (accepta vizitatori) ────────────────────────
    public interface IVisitable
    {
        void Accept(IBookingVisitor visitor);
    }
}
