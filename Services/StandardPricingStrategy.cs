using FlightBooking.Interfaces;
using FlightBooking.Models;

namespace FlightBooking.Services
{
    // OCP: adaugam noi strategii (DiscountPricing, SeasonalPricing) fara a modifica aceasta clasa
    // SRP: calculeaza exclusiv pretul standard
    public class StandardPricingStrategy : IPricingStrategy
    {
        public decimal Calculate(Flight flight)
        {
            decimal multiplier = flight.Class switch
            {
                SeatClass.Economy   => 1.0m,
                SeatClass.Business  => 2.5m,
                SeatClass.FirstClass=> 4.0m,
                _                   => 1.0m
            };
            return flight.BasePrice * multiplier;
        }
    }

    // OCP: extindem comportamentul fara a modifica clasa de baza
    public class EarlyBirdPricingStrategy : IPricingStrategy
    {
        private readonly IPricingStrategy _base;
        private readonly int _daysAheadThreshold;

        public EarlyBirdPricingStrategy(IPricingStrategy baseStrategy, int daysAheadThreshold = 30)
        {
            _base = baseStrategy;
            _daysAheadThreshold = daysAheadThreshold;
        }

        public decimal Calculate(Flight flight)
        {
            var basePrice = _base.Calculate(flight);
            var daysUntilFlight = (flight.DepartureTime - DateTime.UtcNow).Days;
            return daysUntilFlight >= _daysAheadThreshold
                ? basePrice * 0.85m   // 15% reducere early bird
                : basePrice;
        }
    }
}
