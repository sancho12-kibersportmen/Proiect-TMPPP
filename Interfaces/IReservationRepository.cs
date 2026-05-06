using FlightBooking.Models;

namespace FlightBooking.Interfaces
{
    // ISP: interfata separata pentru rezervari (nu amestecam cu zborurile)
    public interface IReservationRepository
    {
        void          Save(Reservation reservation);
        Reservation?  GetById(string reservationId);
        IEnumerable<Reservation> GetByPassengerEmail(string email);
        IEnumerable<Reservation> GetAll();
    }
}
