namespace FlightBooking.Interfaces
{
    // ── Command abstract ─────────────────────────────────────────────
    public interface ICommand
    {
        string CommandName { get; }
        void Execute();
        void Undo();
        bool CanUndo { get; }
    }
}
