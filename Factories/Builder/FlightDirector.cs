using FlightBooking.Interfaces;
using FlightBooking.Models;

namespace FlightBooking.Factories.Builder
{
    // ── Director ─────────────────────────────────────────────────────
    // SRP: encapsuleaza "retete" de construire predefinite
    // DIP: lucreaza cu IFlightRouteBuilder, nu cu FlightBuilder direct
    //
    // Avantaj: clientul nu trebuie sa stie pasii de construire;
    //          directorul ii orchestreaza in ordinea corecta.
    public class FlightDirector
    {
        private readonly IFlightRouteBuilder _builder;

        public FlightDirector(IFlightRouteBuilder builder)
        {
            _builder = builder ?? throw new ArgumentNullException(nameof(builder));
        }

        // ── Reteta 1: zbor economic scurt (2h, pret mic) ─────────────
        public Flight BuildEconomyShortHaul(
            string flightNumber, Airport origin, Airport destination,
            DateTime departure, decimal price = 89m, int seats = 150)
        {
            return _builder
                .WithFlightNumber(flightNumber)
                .WithOrigin(origin)
                .WithDestination(destination)
                .WithDeparture(departure)
                .WithArrival(departure.AddHours(2))
                .WithClass(SeatClass.Economy)
                .WithBasePrice(price)
                .WithSeats(seats)
                .Build();
        }

        // ── Reteta 2: zbor Business lung (5h, pret premium) ──────────
        public Flight BuildBusinessLongHaul(
            string flightNumber, Airport origin, Airport destination,
            DateTime departure, decimal price = 650m, int seats = 30)
        {
            return _builder
                .WithFlightNumber(flightNumber)
                .WithOrigin(origin)
                .WithDestination(destination)
                .WithDeparture(departure)
                .WithArrival(departure.AddHours(5))
                .WithClass(SeatClass.Business)
                .WithBasePrice(price)
                .WithSeats(seats)
                .Build();
        }

        // ── Reteta 3: zbor First Class intercontinental ───────────────
        public Flight BuildFirstClassIntercontinental(
            string flightNumber, Airport origin, Airport destination,
            DateTime departure, decimal price = 1200m, int seats = 10)
        {
            return _builder
                .WithFlightNumber(flightNumber)
                .WithOrigin(origin)
                .WithDestination(destination)
                .WithDeparture(departure)
                .WithArrival(departure.AddHours(10))
                .WithClass(SeatClass.FirstClass)
                .WithBasePrice(price)
                .WithSeats(seats)
                .Build();
        }
    }
}
