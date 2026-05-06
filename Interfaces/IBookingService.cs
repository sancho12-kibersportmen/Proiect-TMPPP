using FlightBooking.Models;

namespace FlightBooking.Interfaces
{
    // ISP: interfata specifica pentru logica de rezervare
    public interface IBookingService
    {
        Reservation CreateReservation(Passenger passenger, Flight flight, string seatNumber);
        void        ConfirmReservation(string reservationId);
        void        CancelReservation(string reservationId);
        Reservation? GetReservation(string reservationId);
    }
}
