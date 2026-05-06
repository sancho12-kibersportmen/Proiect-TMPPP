using FlightBooking.Interfaces;
using FlightBooking.Models;

namespace FlightBooking.Factories.Output
{
    // ══════════════════════════════════════════════════════════════════
    //  Produse concrete pentru familia "Console Output"
    // ══════════════════════════════════════════════════════════════════

    public class ConsoleTicketFormatter : ITicketFormatter
    {
        public string Format(Ticket ticket) =>
            $"  ┌─ Bilet #{ticket.TicketId} ─────────────────────────────┐\n" +
            $"  │ Pasager : {ticket.Passenger.FullName,-38}│\n" +
            $"  │ Zbor    : {ticket.Flight.FlightNumber,-38}│\n" +
            $"  │ Ruta    : {ticket.Flight.Origin.Code} → {ticket.Flight.Destination.Code,-34}│\n" +
            $"  │ Data    : {ticket.Flight.DepartureTime:dd MMM yyyy HH:mm,-38}│\n" +
            $"  │ Clasa   : {ticket.Flight.Class,-38}│\n" +
            $"  │ Loc     : {ticket.SeatNumber,-38}│\n" +
            $"  │ Pret    : {ticket.FinalPrice:C,-38}│\n" +
            $"  │ Status  : {ticket.Status,-38}│\n" +
            $"  └────────────────────────────────────────────────────┘";
    }

    public class ConsoleBoardingPass : IBoardingPass
    {
        private string _content = string.Empty;

        public string Generate(Ticket ticket)
        {
            _content =
                $"\n  ╔══════════════════════════════════════════════╗\n" +
                $"  ║         ✈  BOARDING PASS  ✈                  ║\n" +
                $"  ╠══════════════════════════════════════════════╣\n" +
                $"  ║ {ticket.Passenger.FullName,-44} ║\n" +
                $"  ║ {ticket.Flight.Origin.City,-20} → {ticket.Flight.Destination.City,-21} ║\n" +
                $"  ║ {ticket.Flight.FlightNumber,-10} {ticket.Flight.DepartureTime:dd MMM HH:mm,-15} LOC: {ticket.SeatNumber,-10} ║\n" +
                $"  ║ {ticket.Flight.Class,-20} {ticket.FinalPrice:C,-23} ║\n" +
                $"  ╚══════════════════════════════════════════════╝";
            return _content;
        }

        public void Print() => Console.WriteLine(_content);
    }

    // ══════════════════════════════════════════════════════════════════
    //  Produse concrete pentru familia "Email Output"
    // ══════════════════════════════════════════════════════════════════

    public class EmailTicketFormatter : ITicketFormatter
    {
        public string Format(Ticket ticket) =>
            $"<html><body>" +
            $"<h2>Biletul tau #{ticket.TicketId}</h2>" +
            $"<p><b>Pasager:</b> {ticket.Passenger.FullName}</p>" +
            $"<p><b>Zbor:</b> {ticket.Flight.FlightNumber} " +
            $"({ticket.Flight.Origin.Code} → {ticket.Flight.Destination.Code})</p>" +
            $"<p><b>Data:</b> {ticket.Flight.DepartureTime:dd MMM yyyy HH:mm}</p>" +
            $"<p><b>Loc:</b> {ticket.SeatNumber} | " +
            $"<b>Clasa:</b> {ticket.Flight.Class}</p>" +
            $"<p><b>Pret:</b> {ticket.FinalPrice:C}</p>" +
            $"<p>Status: <b>{ticket.Status}</b></p>" +
            $"</body></html>";
    }

    public class EmailBoardingPass : IBoardingPass
    {
        private string _content = string.Empty;

        public string Generate(Ticket ticket)
        {
            _content =
                $"<html><body style='font-family:monospace;background:#f0f4ff;padding:20px'>" +
                $"<div style='border:2px solid #003580;border-radius:8px;padding:16px;max-width:400px'>" +
                $"<h1 style='color:#003580'>✈ BOARDING PASS</h1>" +
                $"<p><b>{ticket.Passenger.FullName}</b></p>" +
                $"<p>{ticket.Flight.Origin.City} → {ticket.Flight.Destination.City}</p>" +
                $"<p>Zbor: {ticket.Flight.FlightNumber} | " +
                $"Data: {ticket.Flight.DepartureTime:dd MMM HH:mm}</p>" +
                $"<p>Loc: <b>{ticket.SeatNumber}</b> | Clasa: {ticket.Flight.Class}</p>" +
                $"</div></body></html>";
            return _content;
        }

        public void Print() =>
            Console.WriteLine($"  [HTML EMAIL BOARDING PASS]\n  {_content[..Math.Min(120, _content.Length)]}...");
    }

    // ══════════════════════════════════════════════════════════════════
    //  Produse concrete pentru familia "PDF Output"
    // ══════════════════════════════════════════════════════════════════

    public class PdfTicketFormatter : ITicketFormatter
    {
        public string Format(Ticket ticket) =>
            $"[PDF] Ticket:{ticket.TicketId} | " +
            $"Pasager:{ticket.Passenger.FullName} | " +
            $"Zbor:{ticket.Flight.FlightNumber} | " +
            $"Ruta:{ticket.Flight.Origin.Code}-{ticket.Flight.Destination.Code} | " +
            $"Data:{ticket.Flight.DepartureTime:yyyyMMdd-HHmm} | " +
            $"Loc:{ticket.SeatNumber} | Pret:{ticket.FinalPrice:F2}EUR";
    }

    public class PdfBoardingPass : IBoardingPass
    {
        private string _content = string.Empty;

        public string Generate(Ticket ticket)
        {
            _content = $"[PDF BOARDING PASS] {ticket.Passenger.FullName} | " +
                       $"{ticket.Flight.Origin.Code}→{ticket.Flight.Destination.Code} | " +
                       $"{ticket.Flight.FlightNumber} | {ticket.Flight.DepartureTime:dd MMM HH:mm} | " +
                       $"Loc:{ticket.SeatNumber}";
            return _content;
        }

        public void Print() => Console.WriteLine($"  🖨️  {_content}");
    }
}
