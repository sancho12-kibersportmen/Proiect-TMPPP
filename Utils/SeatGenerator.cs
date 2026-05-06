namespace FlightBooking.Utils
{
    // SRP: responsabilitate unica – generarea de numere de locuri
    public static class SeatGenerator
    {
        private static readonly Random _rng = new();

        public static string Generate(int totalSeats)
        {
            int row    = _rng.Next(1, totalSeats / 6 + 1);
            char col   = (char)('A' + _rng.Next(0, 6));
            return $"{row}{col}";
        }
    }
}
