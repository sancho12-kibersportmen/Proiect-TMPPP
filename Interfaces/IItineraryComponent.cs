namespace FlightBooking.Interfaces
{
    // ── Componenta abstracta (Component) ────────────────────────────
    // Atat FlightSegment (Leaf) cat si Itinerary (Composite)
    // implementeaza aceasta interfata — tratate UNIFORM de client.
    public interface IItineraryComponent
    {
        string   Name        { get; }
        decimal  TotalPrice  { get; }
        TimeSpan TotalDuration { get; }
        int      StopCount   { get; }
        void     Display(int indent = 0);
    }
}
