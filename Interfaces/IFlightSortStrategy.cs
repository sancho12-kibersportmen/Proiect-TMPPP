using FlightBooking.Models;

namespace FlightBooking.Interfaces
{
    // ── Strategy: sortare zboruri ────────────────────────────────────
    public interface IFlightSortStrategy
    {
        string StrategyName { get; }
        IEnumerable<Flight> Sort(IEnumerable<Flight> flights);
    }

    // ── Strategy: filtrare zboruri ───────────────────────────────────
    public interface IFlightFilterStrategy
    {
        string StrategyName { get; }
        IEnumerable<Flight> Filter(IEnumerable<Flight> flights);
    }
}
