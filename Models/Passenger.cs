namespace FlightBooking.Models
{
    // SRP: clasa se ocupa exclusiv de datele unui pasager
    public class Passenger
    {
        public string FirstName  { get; private set; }
        public string LastName   { get; private set; }
        public string Email      { get; private set; }
        public string PassportNo { get; private set; }

        public Passenger(string firstName, string lastName, string email, string passportNo)
        {
            FirstName  = firstName;
            LastName   = lastName;
            Email      = email;
            PassportNo = passportNo;
        }

        public string FullName => $"{FirstName} {LastName}";

        public override string ToString() => $"{FullName} | {Email} | Passport: {PassportNo}";
    }
}
