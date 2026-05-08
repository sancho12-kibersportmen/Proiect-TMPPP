using FlightBooking.Models;

namespace FlightBooking.Interfaces
{
    // ── Observer ─────────────────────────────────────────────────────
    public interface IFlightObserver
    {
        string ObserverName { get; }
        void OnFlightUpdated(FlightEvent flightEvent);
    }

    // ── Subject ──────────────────────────────────────────────────────
    public interface IFlightSubject
    {
        void Subscribe(IFlightObserver observer);
        void Unsubscribe(IFlightObserver observer);
        void NotifyObservers(FlightEvent flightEvent);
    }

    // ── Eveniment zbor ───────────────────────────────────────────────
    public enum FlightEventType
    {
        PriceChanged, SeatsUpdated, Delayed, Cancelled, GateChanged
    }

    public class FlightEvent
    {
        public FlightEventType EventType  { get; }
        public Flight          Flight     { get; }
        public string          Message    { get; }
        public DateTime        OccurredAt { get; }
        public object?         OldValue   { get; }
        public object?         NewValue   { get; }

        public FlightEvent(FlightEventType eventType, Flight flight,
                           string message, object? oldValue = null, object? newValue = null)
        {
            EventType  = eventType;
            Flight     = flight;
            Message    = message;
            OldValue   = oldValue;
            NewValue   = newValue;
            OccurredAt = DateTime.UtcNow;
        }
    }
}
