namespace FlightBooking.Models
{
    public enum SeatClass { Economy, Business, FirstClass }

    // SRP: clasa se ocupa exclusiv de datele unui zbor
    // OCP: poate fi extinsa (ex. FlightWithLayover) fara modificari
    public class Flight
    {
        public string     FlightNumber  { get; private set; }
        public Airport    Origin        { get; private set; }
        public Airport    Destination   { get; private set; }
        public DateTime   DepartureTime { get; private set; }
        public DateTime   ArrivalTime   { get; private set; }
        public SeatClass  Class         { get; private set; }
        public decimal    BasePrice     { get; private set; }
        public int        TotalSeats    { get; private set; }
        public int        AvailableSeats{ get; private set; }

        public Flight(string flightNumber, Airport origin, Airport destination,
                      DateTime departureTime, DateTime arrivalTime,
                      SeatClass seatClass, decimal basePrice, int totalSeats)
        {
            FlightNumber   = flightNumber;
            Origin         = origin;
            Destination    = destination;
            DepartureTime  = departureTime;
            ArrivalTime    = arrivalTime;
            Class          = seatClass;
            BasePrice      = basePrice;
            TotalSeats     = totalSeats;
            AvailableSeats = totalSeats;
        }

        public TimeSpan Duration => ArrivalTime - DepartureTime;

        public bool HasAvailableSeats() => AvailableSeats > 0;

        public void ReserveSeat()
        {
            if (!HasAvailableSeats())
                throw new InvalidOperationException("Nu mai există locuri disponibile.");
            AvailableSeats--;
        }

        public override string ToString() =>
            $"[{FlightNumber}] {Origin.Code} → {Destination.Code} | " +
            $"{DepartureTime:dd MMM yyyy HH:mm} | {Class} | {BasePrice:C} | " +
            $"Locuri: {AvailableSeats}/{TotalSeats}";
    }
}
