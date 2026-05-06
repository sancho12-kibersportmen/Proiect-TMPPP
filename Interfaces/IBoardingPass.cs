using FlightBooking.Models;

namespace FlightBooking.Interfaces
{
    // ── Al doilea produs abstract (Abstract Factory – familia "boarding pass") ──
    public interface IBoardingPass
    {
        string Generate(Ticket ticket);
        void   Print();
    }
}
