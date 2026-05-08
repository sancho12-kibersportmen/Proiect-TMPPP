using FlightBooking.Models;

namespace FlightBooking.Interfaces
{
    // ── Handler abstract (Chain of Responsibility) ───────────────────
    public interface IBookingHandler
    {
        IBookingHandler SetNext(IBookingHandler next);
        BookingValidationResult Handle(BookingRequest request);
    }

    // ── Cerere de rezervare (data ce circula prin lant) ──────────────
    public class BookingRequest
    {
        public Passenger Passenger   { get; set; } = null!;
        public Flight    Flight      { get; set; } = null!;
        public string    SeatNumber  { get; set; } = string.Empty;
        public string    CustomerId  { get; set; } = string.Empty;
        public decimal   BudgetLimit { get; set; } = decimal.MaxValue;
        public int       PassengerAge{ get; set; } = 18;
    }

    // ── Rezultatul validarii ─────────────────────────────────────────
    public class BookingValidationResult
    {
        public bool          IsValid  { get; set; } = true;
        public List<string>  Errors   { get; set; } = new();
        public List<string>  Warnings { get; set; } = new();
        public string?       BlockedBy{ get; set; }

        public void AddError(string handlerName, string message)
        {
            IsValid   = false;
            BlockedBy = handlerName;
            Errors.Add($"[{handlerName}] {message}");
        }

        public void AddWarning(string handlerName, string message)
            => Warnings.Add($"[{handlerName}] {message}");
    }
}
