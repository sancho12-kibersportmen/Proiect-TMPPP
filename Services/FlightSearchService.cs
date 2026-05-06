using FlightBooking.Interfaces;
using FlightBooking.Models;

namespace FlightBooking.Services
{
    // SRP: se ocupa exclusiv de cautarea zborurilor
    // DIP: depinde de IFlightRepository, nu de implementarea concreta
    public class FlightSearchService
    {
        private readonly IFlightRepository _flightRepo;

        public FlightSearchService(IFlightRepository flightRepo)
        {
            _flightRepo = flightRepo ?? throw new ArgumentNullException(nameof(flightRepo));
        }

        public IEnumerable<Flight> Search(string originCode, string destinationCode,
                                          DateTime date, SeatClass? seatClass = null)
        {
            if (string.IsNullOrWhiteSpace(originCode))      throw new ArgumentException("Codul aeroportului de origine este obligatoriu.");
            if (string.IsNullOrWhiteSpace(destinationCode)) throw new ArgumentException("Codul aeroportului de destinatie este obligatoriu.");
            if (originCode.Equals(destinationCode, StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("Originea si destinatia nu pot fi identice.");

            return _flightRepo.Search(originCode, destinationCode, date, seatClass);
        }

        public IEnumerable<Flight> GetAllFlights() => _flightRepo.GetAll();
    }
}
