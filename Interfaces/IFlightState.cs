namespace FlightBooking.Interfaces
{
    // ── State abstract ───────────────────────────────────────────────
    // FlightContext este definit in acelasi namespace pentru a evita
    // dependente circulare intre Interfaces si Behavioral.State
    public interface IFlightState
    {
        string StateName { get; }
        void CheckIn(IFlightContext ctx);
        void Board(IFlightContext ctx);
        void Depart(IFlightContext ctx);
        void Land(IFlightContext ctx);
        void Cancel(IFlightContext ctx);
        void Delay(IFlightContext ctx, int minutes);
    }

    // Interfata pentru Context (rupe dependenta circulara)
    public interface IFlightContext
    {
        string FlightNumber { get; }
        int    DelayMinutes { get; }
        void   TransitionTo(IFlightState newState);
        void   AddDelay(int minutes);
        string CurrentState { get; }
    }
}
