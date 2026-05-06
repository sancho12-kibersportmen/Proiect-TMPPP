using FlightBooking.Models;

namespace FlightBooking.Interfaces
{
    // ── Interfata Builder ────────────────────────────────────────────
    // ISP: defineste exclusiv pasii de constructie ai unei rute de zbor
    public interface IFlightRouteBuilder
    {
        IFlightRouteBuilder WithOrigin(Airport origin);
        IFlightRouteBuilder WithDestination(Airport destination);
        IFlightRouteBuilder WithDeparture(DateTime departureTime);
        IFlightRouteBuilder WithArrival(DateTime arrivalTime);
        IFlightRouteBuilder WithClass(SeatClass seatClass);
        IFlightRouteBuilder WithBasePrice(decimal price);
        IFlightRouteBuilder WithSeats(int totalSeats);
        IFlightRouteBuilder WithFlightNumber(string flightNumber);
        Flight Build();
    }
}
