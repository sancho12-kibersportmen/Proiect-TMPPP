using FlightBooking.Models;

namespace FlightBooking.Interfaces
{
    // ══════════════════════════════════════════════════════════════════
    //  ABSTRACT FACTORY
    //  Creeaza o "familie" de obiecte inrudite (formatter + boarding pass)
    //  fara a specifica clasele concrete.
    //  Fiecare canal de output (Console / Email / PDF) are propria fabrica.
    // ══════════════════════════════════════════════════════════════════
    public interface IOutputFactory
    {
        ITicketFormatter CreateTicketFormatter();
        IBoardingPass    CreateBoardingPass(Ticket ticket);
    }
}
