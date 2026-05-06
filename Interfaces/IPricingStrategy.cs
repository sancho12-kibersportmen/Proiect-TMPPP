using FlightBooking.Models;

namespace FlightBooking.Interfaces
{
    // ISP + OCP: adaugam noi strategii de pret fara a modifica codul existent
    public interface IPricingStrategy
    {
        decimal Calculate(Flight flight);
    }
}
