using FlightBooking.Interfaces;
using FlightBooking.Models;

namespace FlightBooking.Factories.Builder
{
    // ══════════════════════════════════════════════════════════════════
    //  BUILDER PATTERN
    //
    //  Problema rezolvata: clasa Flight are 8 parametri in constructor —
    //  usor de confundat, greu de citit, imposibil de folosit optional.
    //
    //  Solutia: FlightBuilder construieste un Flight pas cu pas, prin
    //  metode fluente (method chaining). Director-ul (FlightDirector)
    //  encapsuleaza retete predefinite (ex. "zbor economic standard").
    // ══════════════════════════════════════════════════════════════════

    // ── Builder concret ──────────────────────────────────────────────
    // SRP: raspunde exclusiv de asamblarea unui obiect Flight
    public class FlightBuilder : IFlightRouteBuilder
    {
        // Valorile implicite — vor fi suprascrise prin metode fluente
        private string    _flightNumber  = "XX000";
        private Airport   _origin        = new("KIV","Chisinau Int'l","Chisinau","Moldova");
        private Airport   _destination   = new("OTP","Henri Coanda","Bucuresti","Romania");
        private DateTime  _departureTime = DateTime.UtcNow.AddDays(7);
        private DateTime  _arrivalTime   = DateTime.UtcNow.AddDays(7).AddHours(2);
        private SeatClass _class         = SeatClass.Economy;
        private decimal   _basePrice     = 99m;
        private int       _totalSeats    = 150;

        // ── Metode fluente (method chaining) ────────────────────────
        public IFlightRouteBuilder WithFlightNumber(string flightNumber)
            { _flightNumber  = flightNumber;  return this; }

        public IFlightRouteBuilder WithOrigin(Airport origin)
            { _origin        = origin;        return this; }

        public IFlightRouteBuilder WithDestination(Airport destination)
            { _destination   = destination;   return this; }

        public IFlightRouteBuilder WithDeparture(DateTime departureTime)
            { _departureTime = departureTime; return this; }

        public IFlightRouteBuilder WithArrival(DateTime arrivalTime)
            { _arrivalTime   = arrivalTime;   return this; }

        public IFlightRouteBuilder WithClass(SeatClass seatClass)
            { _class         = seatClass;     return this; }

        public IFlightRouteBuilder WithBasePrice(decimal price)
            { _basePrice     = price;         return this; }

        public IFlightRouteBuilder WithSeats(int totalSeats)
            { _totalSeats    = totalSeats;    return this; }

        // ── Pasul final: construieste obiectul ───────────────────────
        public Flight Build()
        {
            if (_departureTime >= _arrivalTime)
                throw new InvalidOperationException("Ora decolarii trebuie sa fie inaintea aterizarii.");
            if (_basePrice <= 0)
                throw new InvalidOperationException("Pretul de baza trebuie sa fie pozitiv.");
            if (_totalSeats <= 0)
                throw new InvalidOperationException("Numarul de locuri trebuie sa fie pozitiv.");

            return new Flight(
                _flightNumber,
                _origin,
                _destination,
                _departureTime,
                _arrivalTime,
                _class,
                _basePrice,
                _totalSeats
            );
        }

        // ── Reset: reutilizeaza builder-ul pentru un zbor nou ────────
        public FlightBuilder Reset()
        {
            _flightNumber  = "XX000";
            _origin        = new("KIV","Chisinau Int'l","Chisinau","Moldova");
            _destination   = new("OTP","Henri Coanda","Bucuresti","Romania");
            _departureTime = DateTime.UtcNow.AddDays(7);
            _arrivalTime   = DateTime.UtcNow.AddDays(7).AddHours(2);
            _class         = SeatClass.Economy;
            _basePrice     = 99m;
            _totalSeats    = 150;
            return this;
        }
    }
}
