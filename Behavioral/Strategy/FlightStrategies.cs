using FlightBooking.Services;
using FlightBooking.Interfaces;
using FlightBooking.Models;

namespace FlightBooking.Behavioral.Strategy
{
    // ══════════════════════════════════════════════════════════════════
    //  STRATEGY PATTERN
    //
    //  Problema rezolvata: utilizatorul poate sorta/filtra zborurile
    //  dupa mai multe criterii (pret, durata, ora, clasa). Fara Strategy
    //  am avea un switch gigantic in FlightSearchService.
    //
    //  Solutia: fiecare algoritm este o clasa separata care implementeaza
    //  interfata comuna. FlightSearchContext schimba strategia la runtime
    //  fara sa modifice codul existent (OCP).
    // ══════════════════════════════════════════════════════════════════

    // ── Strategii de sortare ─────────────────────────────────────────

    public class SortByPriceAscending : IFlightSortStrategy
    {
        public string StrategyName => "Pret crescator";
        public IEnumerable<Flight> Sort(IEnumerable<Flight> flights)
            => flights.OrderBy(f => f.BasePrice);
    }

    public class SortByPriceDescending : IFlightSortStrategy
    {
        public string StrategyName => "Pret descrescator";
        public IEnumerable<Flight> Sort(IEnumerable<Flight> flights)
            => flights.OrderByDescending(f => f.BasePrice);
    }

    public class SortByDuration : IFlightSortStrategy
    {
        public string StrategyName => "Durata zborului";
        public IEnumerable<Flight> Sort(IEnumerable<Flight> flights)
            => flights.OrderBy(f => f.Duration);
    }

    public class SortByDepartureTime : IFlightSortStrategy
    {
        public string StrategyName => "Ora de plecare";
        public IEnumerable<Flight> Sort(IEnumerable<Flight> flights)
            => flights.OrderBy(f => f.DepartureTime);
    }

    public class SortByAvailableSeats : IFlightSortStrategy
    {
        public string StrategyName => "Locuri disponibile";
        public IEnumerable<Flight> Sort(IEnumerable<Flight> flights)
            => flights.OrderByDescending(f => f.AvailableSeats);
    }

    // ── Strategii de filtrare ────────────────────────────────────────

    public class FilterByMaxPrice : IFlightFilterStrategy
    {
        private readonly decimal _maxPrice;
        public FilterByMaxPrice(decimal maxPrice) => _maxPrice = maxPrice;
        public string StrategyName => $"Pret max {_maxPrice:C}";
        public IEnumerable<Flight> Filter(IEnumerable<Flight> flights)
            => flights.Where(f => f.BasePrice <= _maxPrice);
    }

    public class FilterByClass : IFlightFilterStrategy
    {
        private readonly SeatClass _class;
        public FilterByClass(SeatClass seatClass) => _class = seatClass;
        public string StrategyName => $"Clasa {_class}";
        public IEnumerable<Flight> Filter(IEnumerable<Flight> flights)
            => flights.Where(f => f.Class == _class);
    }

    public class FilterByAvailableSeats : IFlightFilterStrategy
    {
        private readonly int _minSeats;
        public FilterByAvailableSeats(int minSeats = 1) => _minSeats = minSeats;
        public string StrategyName => $"Min {_minSeats} locuri libere";
        public IEnumerable<Flight> Filter(IEnumerable<Flight> flights)
            => flights.Where(f => f.AvailableSeats >= _minSeats);
    }

    public class FilterByMaxDuration : IFlightFilterStrategy
    {
        private readonly TimeSpan _maxDuration;
        public FilterByMaxDuration(TimeSpan maxDuration) => _maxDuration = maxDuration;
        public string StrategyName => $"Durata max {_maxDuration:hh\\:mm}h";
        public IEnumerable<Flight> Filter(IEnumerable<Flight> flights)
            => flights.Where(f => f.Duration <= _maxDuration);
    }

    // ── Context – foloseste strategiile ──────────────────────────────
    // SRP: orchestreaza cautarea cu strategii interschimbabile
    // DIP: depinde de interfete, nu de implementari concrete
    public class FlightSearchContext
    {
        private IFlightSortStrategy?   _sortStrategy;
        private IFlightFilterStrategy? _filterStrategy;

        // Schimbarea strategiei la runtime (fara modificari de cod)
        public void SetSortStrategy(IFlightSortStrategy strategy)
        {
            _sortStrategy = strategy;
            AppLogger.Instance.Info("SearchContext",
                $"Strategie sortare setata: {strategy.StrategyName}");
        }

        public void SetFilterStrategy(IFlightFilterStrategy strategy)
        {
            _filterStrategy = strategy;
            AppLogger.Instance.Info("SearchContext",
                $"Strategie filtrare setata: {strategy.StrategyName}");
        }

        public IEnumerable<Flight> Execute(IEnumerable<Flight> flights)
        {
            var result = flights;

            if (_filterStrategy != null)
            {
                result = _filterStrategy.Filter(result);
                AppLogger.Instance.Info("SearchContext",
                    $"Filtrare aplicata: {_filterStrategy.StrategyName}");
            }

            if (_sortStrategy != null)
            {
                result = _sortStrategy.Sort(result);
                AppLogger.Instance.Info("SearchContext",
                    $"Sortare aplicata: {_sortStrategy.StrategyName}");
            }

            return result.ToList();
        }
    }
}

