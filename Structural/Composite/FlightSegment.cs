using FlightBooking.Interfaces;
using FlightBooking.Models;

namespace FlightBooking.Structural.Composite
{
    // ══════════════════════════════════════════════════════════════════
    //  COMPOSITE PATTERN
    //
    //  Problema rezolvata: un pasager poate rezerva:
    //    - un zbor simplu (dus)       → Leaf
    //    - un zbor dus-intors          → Composite cu 2 Leaf-uri
    //    - un itinerar cu escale       → Composite cu N Leaf-uri
    //    - un pachet (zbor+hotel+auto) → Composite de Composite-uri
    //
    //  Clientul apeleaza Display() / TotalPrice pe ORICE nod din arbore
    //  fara sa stie daca e Leaf sau Composite.
    // ══════════════════════════════════════════════════════════════════

    // ── Leaf: un segment de zbor individual ─────────────────────────
    public class FlightSegment : IItineraryComponent
    {
        private readonly Flight  _flight;
        private readonly decimal _price;

        public FlightSegment(Flight flight, decimal price)
        {
            _flight = flight ?? throw new ArgumentNullException(nameof(flight));
            _price  = price;
        }

        public string   Name          => $"{_flight.FlightNumber}: {_flight.Origin.Code}→{_flight.Destination.Code}";
        public decimal  TotalPrice    => _price;
        public TimeSpan TotalDuration => _flight.Duration;
        public int      StopCount     => 0;   // Leaf nu are escale

        public void Display(int indent = 0)
        {
            var pad = new string(' ', indent * 2);
            Console.WriteLine($"{pad}✈  {Name}");
            Console.WriteLine($"{pad}   Data    : {_flight.DepartureTime:dd MMM yyyy HH:mm} → {_flight.ArrivalTime:HH:mm}");
            Console.WriteLine($"{pad}   Durata  : {TotalDuration:hh\\:mm}h | Clasa: {_flight.Class}");
            Console.WriteLine($"{pad}   Pret    : {TotalPrice:C}");
        }
    }
}
