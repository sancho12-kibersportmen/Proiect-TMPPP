using FlightBooking.Interfaces;
using FlightBooking.Models;

namespace FlightBooking.Services
{
    // ── Client al Abstract Factory ───────────────────────────────────
    // SRP: se ocupa exclusiv de afisarea/exportul biletelor
    // DIP: depinde de IOutputFactory, nu de fabricile concrete
    public class TicketOutputService
    {
        private readonly IOutputFactory _factory;

        public TicketOutputService(IOutputFactory factory)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        }

        public void DisplayTicket(Ticket ticket)
        {
            var formatter = _factory.CreateTicketFormatter();
            Console.WriteLine(formatter.Format(ticket));
        }

        public void PrintBoardingPass(Ticket ticket)
        {
            var boardingPass = _factory.CreateBoardingPass(ticket);
            boardingPass.Print();
        }

        public void DisplayAll(IEnumerable<Ticket> tickets)
        {
            foreach (var ticket in tickets)
            {
                DisplayTicket(ticket);
                PrintBoardingPass(ticket);
                Console.WriteLine();
            }
        }
    }
}
