using FlightBooking.Factories.Output;
using FlightBooking.Interfaces;
using FlightBooking.Models;

namespace FlightBooking.Factories
{
    // ══════════════════════════════════════════════════════════════════
    //  ABSTRACT FACTORY PATTERN
    //
    //  Fiecare fabrica concreta creeaza o "familie" coerenta de obiecte:
    //    - ConsoleOutputFactory  → formatter consola + boarding pass consola
    //    - EmailOutputFactory    → formatter HTML    + boarding pass HTML
    //    - PdfOutputFactory      → formatter PDF     + boarding pass PDF
    //
    //  Clientul (TicketOutputService) lucreaza EXCLUSIV cu IOutputFactory
    //  si nu stie niciodata de clasele concrete.
    // ══════════════════════════════════════════════════════════════════

    // Fabrica concreta 1 – familia Console
    public class ConsoleOutputFactory : IOutputFactory
    {
        public ITicketFormatter CreateTicketFormatter() => new ConsoleTicketFormatter();
        public IBoardingPass    CreateBoardingPass(Ticket ticket)
        {
            var bp = new ConsoleBoardingPass();
            bp.Generate(ticket);
            return bp;
        }
    }

    // Fabrica concreta 2 – familia Email (HTML)
    public class EmailOutputFactory : IOutputFactory
    {
        public ITicketFormatter CreateTicketFormatter() => new EmailTicketFormatter();
        public IBoardingPass    CreateBoardingPass(Ticket ticket)
        {
            var bp = new EmailBoardingPass();
            bp.Generate(ticket);
            return bp;
        }
    }

    // Fabrica concreta 3 – familia PDF
    public class PdfOutputFactory : IOutputFactory
    {
        public ITicketFormatter CreateTicketFormatter() => new PdfTicketFormatter();
        public IBoardingPass    CreateBoardingPass(Ticket ticket)
        {
            var bp = new PdfBoardingPass();
            bp.Generate(ticket);
            return bp;
        }
    }
}
