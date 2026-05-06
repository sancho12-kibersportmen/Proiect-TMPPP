using FlightBooking.Interfaces;
using FlightBooking.Models;

namespace FlightBooking.Utils
{
    // SRP: se ocupa exclusiv de popularea datelor initiale
    public static class DataSeeder
    {
        public static void Seed(IFlightRepository repo)
        {
            var kiv = new Airport("KIV", "Aeroportul International Chisinau", "Chisinau", "Moldova");
            var otp = new Airport("OTP", "Aeroportul International Henri Coanda", "Bucuresti", "Romania");
            var cdg = new Airport("CDG", "Charles de Gaulle", "Paris", "Franta");
            var lhr = new Airport("LHR", "Heathrow", "Londra", "Anglia");
            var fra = new Airport("FRA", "Frankfurt am Main", "Frankfurt", "Germania");
            var ist = new Airport("IST", "Istanbul Airport", "Istanbul", "Turcia");

            var now = DateTime.UtcNow;

            repo.Add(new Flight("MV101", kiv, otp, now.AddDays(7).Date.AddHours(8),
                                now.AddDays(7).Date.AddHours(9.5), SeatClass.Economy, 89m, 120));

            repo.Add(new Flight("MV102", kiv, otp, now.AddDays(7).Date.AddHours(18),
                                now.AddDays(7).Date.AddHours(19.5), SeatClass.Economy, 79m, 120));

            repo.Add(new Flight("MV201", kiv, cdg, now.AddDays(10).Date.AddHours(6),
                                now.AddDays(10).Date.AddHours(9), SeatClass.Economy, 189m, 180));

            repo.Add(new Flight("MV202", kiv, cdg, now.AddDays(10).Date.AddHours(14),
                                now.AddDays(10).Date.AddHours(17), SeatClass.Business, 420m, 30));

            repo.Add(new Flight("MV301", otp, lhr, now.AddDays(5).Date.AddHours(7),
                                now.AddDays(5).Date.AddHours(10), SeatClass.Economy, 145m, 150));

            repo.Add(new Flight("MV302", kiv, fra, now.AddDays(14).Date.AddHours(9),
                                now.AddDays(14).Date.AddHours(11.5), SeatClass.Economy, 210m, 140));

            repo.Add(new Flight("MV303", kiv, ist, now.AddDays(3).Date.AddHours(16),
                                now.AddDays(3).Date.AddHours(18.5), SeatClass.Economy, 99m, 160));

            repo.Add(new Flight("MV303B", kiv, ist, now.AddDays(3).Date.AddHours(16),
                                now.AddDays(3).Date.AddHours(18.5), SeatClass.Business, 99m, 20));

            repo.Add(new Flight("MV304", kiv, lhr, now.AddDays(20).Date.AddHours(11),
                                now.AddDays(20).Date.AddHours(14), SeatClass.FirstClass, 350m, 10));
        }
    }
}
