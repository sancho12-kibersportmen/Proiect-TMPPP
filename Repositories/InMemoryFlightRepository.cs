using FlightBooking.Interfaces;
using FlightBooking.Models;

namespace FlightBooking.Repositories
{
    // SRP: se ocupa exclusiv de stocarea si cautarea zborurilor in memorie
    // DIP: implementeaza IFlightRepository (serviciile nu stiu de aceasta clasa)
    public class InMemoryFlightRepository : IFlightRepository
    {
        private readonly List<Flight> _flights = new();

        public void Add(Flight flight)
        {
            if (flight == null) throw new ArgumentNullException(nameof(flight));
            _flights.Add(flight);
        }

        public Flight? GetByFlightNumber(string flightNumber) =>
            _flights.FirstOrDefault(f => f.FlightNumber == flightNumber);

        public IEnumerable<Flight> GetAll() => _flights.AsReadOnly();

        public IEnumerable<Flight> Search(string originCode, string destinationCode,
                                          DateTime date, SeatClass? seatClass = null)
        {
            return _flights.Where(f =>
                f.Origin.Code.Equals(originCode, StringComparison.OrdinalIgnoreCase) &&
                f.Destination.Code.Equals(destinationCode, StringComparison.OrdinalIgnoreCase) &&
                f.DepartureTime.Date == date.Date &&
                (seatClass == null || f.Class == seatClass) &&
                f.HasAvailableSeats());
        }
    }
}
