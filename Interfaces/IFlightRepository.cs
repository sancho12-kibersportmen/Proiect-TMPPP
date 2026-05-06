using FlightBooking.Models;

namespace FlightBooking.Interfaces
{
    // ISP: interfata specifica doar pentru operatii cu zboruri
    // DIP: serviciile depind de aceasta abstractizare, nu de implementare concreta
    public interface IFlightRepository
    {
        void       Add(Flight flight);
        Flight?    GetByFlightNumber(string flightNumber);
        IEnumerable<Flight> GetAll();
        IEnumerable<Flight> Search(string originCode, string destinationCode, DateTime date, SeatClass? seatClass = null);
    }
}
