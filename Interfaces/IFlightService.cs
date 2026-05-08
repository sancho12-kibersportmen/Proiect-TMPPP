namespace FlightBooking.Interfaces
{
    // ── Componenta abstracta (Component) pentru Decorator ───────────
    // Defineste interfata comuna pentru componenta de baza si decoratori.
    // ISP: interfata specifica domeniului serviciilor de zbor.
    public interface IFlightService
    {
        string  ServiceName  { get; }
        decimal Price        { get; }
        string  GetDescription();
        void    ShowDetails();
    }
}
