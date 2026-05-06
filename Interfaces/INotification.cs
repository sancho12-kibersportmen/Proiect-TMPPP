namespace FlightBooking.Interfaces
{
    // ── Produs abstract (Factory Method) ────────────────────────────────
    // ISP: interfata specifica exclusiv pentru notificari
    public interface INotification
    {
        string Channel { get; }
        void Send(string recipient, string subject, string body);
    }
}
