namespace FlightBooking.Models
{
    public enum ReservationStatus { Created, Paid, Cancelled }

    // SRP: gestioneaza datele unei rezervari (poate contine mai multe bilete – dus/intors)
    public class Reservation
    {
        public string            ReservationId { get; private set; }
        public Passenger         Passenger     { get; private set; }
        public List<Ticket>      Tickets       { get; private set; }
        public ReservationStatus Status        { get; private set; }
        public DateTime          CreatedAt     { get; private set; }

        public decimal TotalPrice => Tickets.Sum(t => t.FinalPrice);

        public Reservation(Passenger passenger)
        {
            ReservationId = Guid.NewGuid().ToString("N")[..8].ToUpper();
            Passenger     = passenger;
            Tickets       = new List<Ticket>();
            Status        = ReservationStatus.Created;
            CreatedAt     = DateTime.UtcNow;
        }

        public void AddTicket(Ticket ticket) => Tickets.Add(ticket);

        public void MarkAsPaid()
        {
            Status = ReservationStatus.Paid;
            foreach (var t in Tickets) t.Confirm();
        }

        public void Cancel()
        {
            Status = ReservationStatus.Cancelled;
            foreach (var t in Tickets) t.Cancel();
        }

        public override string ToString() =>
            $"Rezervare #{ReservationId} | {Passenger.FullName} | " +
            $"Total: {TotalPrice:C} | Status: {Status} | Bilete: {Tickets.Count}";
    }
}
