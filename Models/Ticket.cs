namespace FlightBooking.Models
{
    public enum TicketStatus { Pending, Confirmed, Cancelled }

    // SRP: reprezinta un bilet de avion emis
    // LSP: poate fi substituit cu EconomyTicket / BusinessTicket etc.
    public class Ticket
    {
        public string       TicketId   { get; private set; }
        public Flight       Flight     { get; private set; }
        public Passenger    Passenger  { get; private set; }
        public string       SeatNumber { get; private set; }
        public decimal      FinalPrice { get; private set; }
        public TicketStatus Status     { get; private set; }
        public DateTime     IssuedAt   { get; private set; }

        public Ticket(Flight flight, Passenger passenger, string seatNumber, decimal finalPrice)
        {
            TicketId   = Guid.NewGuid().ToString("N")[..8].ToUpper();
            Flight     = flight;
            Passenger  = passenger;
            SeatNumber = seatNumber;
            FinalPrice = finalPrice;
            Status     = TicketStatus.Pending;
            IssuedAt   = DateTime.UtcNow;
        }

        public void Confirm()  => Status = TicketStatus.Confirmed;
        public void Cancel()   => Status = TicketStatus.Cancelled;

        public override string ToString() =>
            $"Bilet #{TicketId} | {Flight.FlightNumber} | " +
            $"{Passenger.FullName} | Loc: {SeatNumber} | " +
            $"{FinalPrice:C} | Status: {Status}";
    }
}
