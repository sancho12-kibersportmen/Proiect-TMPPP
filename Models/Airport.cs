namespace FlightBooking.Models
{
    // SRP: clasa se ocupa exclusiv de datele unui aeroport
    public class Airport
    {
        public string Code { get; private set; }       // ex: KIV, OTP, CDG
        public string Name { get; private set; }
        public string City { get; private set; }
        public string Country { get; private set; }

        public Airport(string code, string name, string city, string country)
        {
            Code    = code;
            Name    = name;
            City    = city;
            Country = country;
        }

        public override string ToString() => $"{Code} – {Name} ({City}, {Country})";
    }
}
