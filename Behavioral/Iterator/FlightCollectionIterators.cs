using FlightBooking.Models;

namespace FlightBooking.Behavioral.Iterator
{
    // ══════════════════════════════════════════════════════════════════
    //  ITERATOR PATTERN
    //
    //  Problema rezolvata: codul client trebuie sa parcurga colectii de
    //  zboruri in moduri diferite (toate, doar Economy, dupa ruta, etc.)
    //  fara sa cunoasca structura interna a colectiei.
    //
    //  Solutia: IFlightIterator defineste interfata standard de parcurgere.
    //  FlightCollection (Aggregate) creeaza iteratori specifici.
    //  Clientul parcurge colectia prin iterator, fara sa stie ca e o
    //  lista, un arbore sau o baza de date.
    // ══════════════════════════════════════════════════════════════════

    // ── Iterator abstract ────────────────────────────────────────────
    public interface IFlightIterator
    {
        bool    HasNext();
        Flight  Next();
        void    Reset();
        Flight? Current { get; }
        int     Position { get; }
    }

    // ── Aggregate abstract ───────────────────────────────────────────
    public interface IFlightAggregate
    {
        IFlightIterator CreateIterator();
        IFlightIterator CreateFilteredIterator(Func<Flight, bool> predicate);
        IFlightIterator CreateReverseIterator();
        int Count { get; }
    }

    // ── Iterator concret 1: parcurgere secventiala ───────────────────
    public class SequentialFlightIterator : IFlightIterator
    {
        private readonly IList<Flight> _flights;
        private int _position = -1;

        public SequentialFlightIterator(IList<Flight> flights)
            => _flights = flights ?? throw new ArgumentNullException(nameof(flights));

        public bool   HasNext()  => _position + 1 < _flights.Count;
        public Flight Next()
        {
            if (!HasNext()) throw new InvalidOperationException("Nu mai exista elemente.");
            return _flights[++_position];
        }
        public void   Reset()    => _position = -1;
        public Flight? Current   => _position >= 0 && _position < _flights.Count
                                    ? _flights[_position] : null;
        public int    Position   => _position;
    }

    // ── Iterator concret 2: parcurgere inversa ───────────────────────
    public class ReverseFlightIterator : IFlightIterator
    {
        private readonly IList<Flight> _flights;
        private int _position;

        public ReverseFlightIterator(IList<Flight> flights)
        {
            _flights  = flights ?? throw new ArgumentNullException(nameof(flights));
            _position = _flights.Count;
        }

        public bool   HasNext()  => _position - 1 >= 0;
        public Flight Next()
        {
            if (!HasNext()) throw new InvalidOperationException("Nu mai exista elemente.");
            return _flights[--_position];
        }
        public void   Reset()    => _position = _flights.Count;
        public Flight? Current   => _position >= 0 && _position < _flights.Count
                                    ? _flights[_position] : null;
        public int    Position   => _position;
    }

    // ── Iterator concret 3: parcurgere filtrata ──────────────────────
    public class FilteredFlightIterator : IFlightIterator
    {
        private readonly IList<Flight> _source;
        private readonly Func<Flight, bool> _predicate;
        private List<Flight>? _filtered;
        private int _position = -1;

        public FilteredFlightIterator(IList<Flight> source, Func<Flight, bool> predicate)
        {
            _source    = source    ?? throw new ArgumentNullException(nameof(source));
            _predicate = predicate ?? throw new ArgumentNullException(nameof(predicate));
        }

        private List<Flight> Filtered => _filtered ??= _source.Where(_predicate).ToList();

        public bool   HasNext()  => _position + 1 < Filtered.Count;
        public Flight Next()
        {
            if (!HasNext()) throw new InvalidOperationException("Nu mai exista elemente.");
            return Filtered[++_position];
        }
        public void   Reset()    => _position = -1;
        public Flight? Current   => _position >= 0 && _position < Filtered.Count
                                    ? Filtered[_position] : null;
        public int    Position   => _position;
        public int    FilteredCount => Filtered.Count;
    }

    // ── Aggregate concret: colectie de zboruri ───────────────────────
    // SRP: gestioneaza colectia si fabrica iteratori
    public class FlightCollection : IFlightAggregate
    {
        private readonly List<Flight> _flights = new();

        public void Add(Flight flight)    => _flights.Add(flight);
        public void AddRange(IEnumerable<Flight> flights) => _flights.AddRange(flights);
        public int  Count                 => _flights.Count;

        // Fabrica de iteratori
        public IFlightIterator CreateIterator()
            => new SequentialFlightIterator(_flights);

        public IFlightIterator CreateFilteredIterator(Func<Flight, bool> predicate)
            => new FilteredFlightIterator(_flights, predicate);

        public IFlightIterator CreateReverseIterator()
            => new ReverseFlightIterator(_flights);

        // Iteratori specializati (conveniente)
        public IFlightIterator IterateByClass(SeatClass seatClass)
            => CreateFilteredIterator(f => f.Class == seatClass);

        public IFlightIterator IterateByRoute(string origin, string destination)
            => CreateFilteredIterator(f =>
                f.Origin.Code.Equals(origin, StringComparison.OrdinalIgnoreCase) &&
                f.Destination.Code.Equals(destination, StringComparison.OrdinalIgnoreCase));

        public IFlightIterator IterateAvailable()
            => CreateFilteredIterator(f => f.HasAvailableSeats());

        // Suport pentru foreach C# (IEnumerable)
        public IEnumerator<Flight> GetEnumerator() => _flights.GetEnumerator();
    }

    // ── Iterator pentru rezervari (alt tip de colectie) ──────────────
    public class ReservationIterator
    {
        private readonly List<Reservation> _reservations;
        private int _position = -1;

        public ReservationIterator(IEnumerable<Reservation> reservations)
            => _reservations = reservations.ToList();

        public bool        HasNext()  => _position + 1 < _reservations.Count;
        public Reservation Next()     => _reservations[++_position];
        public void        Reset()    => _position = -1;
        public int         Count      => _reservations.Count;

        // Parcurgere cu actiune (visitor-like)
        public void ForEach(Action<Reservation> action)
        {
            Reset();
            while (HasNext()) action(Next());
        }

        public IEnumerable<Reservation> Where(Func<Reservation, bool> predicate)
        {
            Reset();
            while (HasNext())
            {
                var r = Next();
                if (predicate(r)) yield return r;
            }
        }
    }
}
