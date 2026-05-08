using FlightBooking.Interfaces;
using FlightBooking.Services;

namespace FlightBooking.Behavioral.State
{
    // ══════════════════════════════════════════════════════════════════
    //  STATE PATTERN
    // ══════════════════════════════════════════════════════════════════

    // ── Context ──────────────────────────────────────────────────────
    public class FlightContext : IFlightContext
    {
        private IFlightState _state;
        public  string FlightNumber { get; }
        public  int    DelayMinutes { get; private set; } = 0;
        public  string CurrentState => _state.StateName;

        public FlightContext(string flightNumber)
        {
            FlightNumber = flightNumber;
            _state       = new ScheduledState();
            Console.WriteLine($"  [Flight {FlightNumber}] Creat in starea: {_state.StateName}");
        }

        public void TransitionTo(IFlightState newState)
        {
            Console.WriteLine($"  [Flight {FlightNumber}] {_state.StateName} -> {newState.StateName}");
            _state = newState;
            AppLogger.Instance.Info("FlightState", $"{FlightNumber}: tranzitie la {newState.StateName}");
        }

        public void AddDelay(int minutes) => DelayMinutes += minutes;

        // Delegare
        public void CheckIn()              => _state.CheckIn(this);
        public void Board()                => _state.Board(this);
        public void Depart()               => _state.Depart(this);
        public void Land()                 => _state.Land(this);
        public void Cancel()               => _state.Cancel(this);
        public void Delay(int minutes)     => _state.Delay(this, minutes);
    }

    // ── Stare 1: Scheduled ───────────────────────────────────────────
    public class ScheduledState : IFlightState
    {
        public string StateName => "Scheduled";

        public void CheckIn(IFlightContext ctx)
        {
            Console.WriteLine($"  [{ctx.FlightNumber}] Check-in deschis!");
            ctx.TransitionTo(new CheckInState());
        }

        public void Board(IFlightContext ctx)
            => Invalid(ctx, "Trebuie sa faceti check-in intai.");

        public void Depart(IFlightContext ctx)
            => Invalid(ctx, "Zborul nu a inceput inca.");

        public void Land(IFlightContext ctx)
            => Invalid(ctx, "Zborul nu a decolat.");

        public void Cancel(IFlightContext ctx)
        {
            Console.WriteLine($"  [{ctx.FlightNumber}] Zbor anulat din starea Scheduled.");
            ctx.TransitionTo(new CancelledState());
        }

        public void Delay(IFlightContext ctx, int minutes)
        {
            ctx.AddDelay(minutes);
            Console.WriteLine($"  [{ctx.FlightNumber}] Intarziere {minutes}min. Total: {ctx.DelayMinutes}min.");
        }

        private void Invalid(IFlightContext ctx, string msg)
            => Console.WriteLine($"  [{ctx.FlightNumber}] X Operatie invalida in {StateName}: {msg}");
    }

    // ── Stare 2: CheckIn ─────────────────────────────────────────────
    public class CheckInState : IFlightState
    {
        public string StateName => "CheckIn";

        public void CheckIn(IFlightContext ctx)
            => Console.WriteLine($"  [{ctx.FlightNumber}] Check-in deja deschis.");

        public void Board(IFlightContext ctx)
        {
            Console.WriteLine($"  [{ctx.FlightNumber}] Imbarcare inceput!");
            ctx.TransitionTo(new BoardingState());
        }

        public void Depart(IFlightContext ctx)
            => Invalid(ctx, "Pasagerii nu au imbarcat inca.");

        public void Land(IFlightContext ctx)
            => Invalid(ctx, "Zborul nu a decolat.");

        public void Cancel(IFlightContext ctx)
        {
            Console.WriteLine($"  [{ctx.FlightNumber}] Zbor anulat in timpul Check-In.");
            ctx.TransitionTo(new CancelledState());
        }

        public void Delay(IFlightContext ctx, int minutes)
        {
            ctx.AddDelay(minutes);
            Console.WriteLine($"  [{ctx.FlightNumber}] Intarziere {minutes}min in CheckIn.");
        }

        private void Invalid(IFlightContext ctx, string msg)
            => Console.WriteLine($"  [{ctx.FlightNumber}] X Operatie invalida in {StateName}: {msg}");
    }

    // ── Stare 3: Boarding ────────────────────────────────────────────
    public class BoardingState : IFlightState
    {
        public string StateName => "Boarding";

        public void CheckIn(IFlightContext ctx)
            => Console.WriteLine($"  [{ctx.FlightNumber}] Check-in inchis, imbarcarea e in curs.");

        public void Board(IFlightContext ctx)
            => Console.WriteLine($"  [{ctx.FlightNumber}] Imbarcarea este in curs...");

        public void Depart(IFlightContext ctx)
        {
            Console.WriteLine($"  [{ctx.FlightNumber}] Decolare!");
            ctx.TransitionTo(new DepartedState());
        }

        public void Land(IFlightContext ctx)
            => Invalid(ctx, "Zborul nu a decolat inca.");

        public void Cancel(IFlightContext ctx)
        {
            Console.WriteLine($"  [{ctx.FlightNumber}] Zbor anulat in timpul imbarcarii!");
            ctx.TransitionTo(new CancelledState());
        }

        public void Delay(IFlightContext ctx, int minutes)
        {
            ctx.AddDelay(minutes);
            Console.WriteLine($"  [{ctx.FlightNumber}] Intarziere {minutes}min la imbarcare.");
        }

        private void Invalid(IFlightContext ctx, string msg)
            => Console.WriteLine($"  [{ctx.FlightNumber}] X Operatie invalida in {StateName}: {msg}");
    }

    // ── Stare 4: InFlight ────────────────────────────────────────────
    public class DepartedState : IFlightState
    {
        public string StateName => "InFlight";

        public void CheckIn(IFlightContext ctx) => Invalid(ctx, "Zborul este in aer.");
        public void Board(IFlightContext ctx)   => Invalid(ctx, "Zborul este in aer.");

        public void Depart(IFlightContext ctx)
            => Console.WriteLine($"  [{ctx.FlightNumber}] Deja in zbor.");

        public void Land(IFlightContext ctx)
        {
            Console.WriteLine($"  [{ctx.FlightNumber}] Aterizare reusita!");
            ctx.TransitionTo(new LandedState());
        }

        public void Cancel(IFlightContext ctx)
            => Invalid(ctx, "Nu se poate anula un zbor in aer.");

        public void Delay(IFlightContext ctx, int minutes)
        {
            ctx.AddDelay(minutes);
            Console.WriteLine($"  [{ctx.FlightNumber}] Ruta modificata, intarziere {minutes}min.");
        }

        private void Invalid(IFlightContext ctx, string msg)
            => Console.WriteLine($"  [{ctx.FlightNumber}] X {StateName}: {msg}");
    }

    // ── Stare 5: Landed ──────────────────────────────────────────────
    public class LandedState : IFlightState
    {
        public string StateName => "Landed";
        public void CheckIn(IFlightContext ctx) => Invalid(ctx);
        public void Board(IFlightContext ctx)   => Invalid(ctx);
        public void Depart(IFlightContext ctx)  => Invalid(ctx);
        public void Land(IFlightContext ctx)    => Console.WriteLine($"  [{ctx.FlightNumber}] Deja aterizat.");
        public void Cancel(IFlightContext ctx)  => Invalid(ctx);
        public void Delay(IFlightContext ctx, int m) => Invalid(ctx);
        private void Invalid(IFlightContext ctx, string msg = "Zborul a aterizat, nicio actiune posibila.")
            => Console.WriteLine($"  [{ctx.FlightNumber}] X {msg}");
    }

    // ── Stare 6: Cancelled ───────────────────────────────────────────
    public class CancelledState : IFlightState
    {
        public string StateName => "Cancelled";
        public void CheckIn(IFlightContext ctx) => Invalid(ctx);
        public void Board(IFlightContext ctx)   => Invalid(ctx);
        public void Depart(IFlightContext ctx)  => Invalid(ctx);
        public void Land(IFlightContext ctx)    => Invalid(ctx);
        public void Cancel(IFlightContext ctx)  => Console.WriteLine($"  [{ctx.FlightNumber}] Deja anulat.");
        public void Delay(IFlightContext ctx, int m) => Invalid(ctx);
        private void Invalid(IFlightContext ctx, string msg = "Zborul este anulat.")
            => Console.WriteLine($"  [{ctx.FlightNumber}] X {msg}");
    }
}
