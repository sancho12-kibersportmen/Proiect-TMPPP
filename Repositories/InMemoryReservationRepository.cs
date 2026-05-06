using FlightBooking.Interfaces;
using FlightBooking.Models;

namespace FlightBooking.Repositories
{
    // SRP: stocarea rezervarilor in memorie
    public class InMemoryReservationRepository : IReservationRepository
    {
        private readonly Dictionary<string, Reservation> _reservations = new();

        public void Save(Reservation reservation)
        {
            if (reservation == null) throw new ArgumentNullException(nameof(reservation));
            _reservations[reservation.ReservationId] = reservation;
        }

        public Reservation? GetById(string reservationId) =>
            _reservations.TryGetValue(reservationId, out var r) ? r : null;

        public IEnumerable<Reservation> GetByPassengerEmail(string email) =>
            _reservations.Values
                .Where(r => r.Passenger.Email.Equals(email, StringComparison.OrdinalIgnoreCase));

        public IEnumerable<Reservation> GetAll() =>
            _reservations.Values.ToList().AsReadOnly();
    }
}
