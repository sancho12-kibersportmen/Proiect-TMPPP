namespace FlightBooking.Interfaces
{
    // ── Implementor (Bridge) ─────────────────────────────────────────
    // Defineste interfata pentru implementarile concrete ale randarii.
    // Abstractizarea (INotificationSender) va delega randarea catre aceasta.
    public interface IMessageRenderer
    {
        string RendererName { get; }
        string RenderTitle(string title);
        string RenderBody(string body);
        string RenderField(string label, string value);
        string RenderFooter(string footer);
    }
}
