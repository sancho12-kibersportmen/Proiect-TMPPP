using FlightBooking.Interfaces;
using FlightBooking.Models;

namespace FlightBooking.Behavioral.Visitor
{
    // ══════════════════════════════════════════════════════════════════
    //  VISITOR PATTERN
    //
    //  Problema rezolvata: vrem sa calculam taxa, sa exportam in JSON,
    //  sa generam statistici si sa facem audit asupra biletelor,
    //  rezervarilor si zborurilor — FARA a adauga metode in clasele lor.
    //
    //  Solutia: fiecare operatie noua devine un Visitor separat.
    //  Clasele (Ticket, Reservation, Flight) implementeaza Accept()
    //  si delega operatia catre visitor. OCP: adaugam comportament fara
    //  sa modificam clasele de date.
    // ══════════════════════════════════════════════════════════════════

    // ── Visitor 1: Calculator de taxe ────────────────────────────────
    public class TaxCalculatorVisitor : IBookingVisitor
    {
        public string  VisitorName    => "TaxCalculator";
        public decimal TotalTax       { get; private set; } = 0m;
        public decimal TotalRevenue   { get; private set; } = 0m;

        private const decimal TaxRate = 0.19m;   // TVA 19%

        public void Visit(Ticket ticket)
        {
            var tax = ticket.FinalPrice * TaxRate;
            TotalTax     += tax;
            TotalRevenue += ticket.FinalPrice;
            Console.WriteLine($"  [TaxCalc] Bilet #{ticket.TicketId}: " +
                              $"Pret={ticket.FinalPrice:C} | TVA={tax:C}");
        }

        public void Visit(Reservation reservation)
        {
            Console.WriteLine($"  [TaxCalc] Rezervare #{reservation.ReservationId}: " +
                              $"Total={reservation.TotalPrice:C} | " +
                              $"TVA total={reservation.TotalPrice * TaxRate:C}");
        }

        public void Visit(Flight flight)
        {
            Console.WriteLine($"  [TaxCalc] Zbor {flight.FlightNumber}: " +
                              $"Pret baza={flight.BasePrice:C} | " +
                              $"TVA={flight.BasePrice * TaxRate:C}");
        }

        public void PrintSummary()
        {
            Console.WriteLine($"\n  [TaxCalc] SUMAR TAXE:");
            Console.WriteLine($"    Venituri totale : {TotalRevenue:C}");
            Console.WriteLine($"    TVA total (19%) : {TotalTax:C}");
            Console.WriteLine($"    Net (fara TVA)  : {TotalRevenue - TotalTax:C}");
        }
    }

    // ── Visitor 2: Export JSON ────────────────────────────────────────
    public class JsonExportVisitor : IBookingVisitor
    {
        public string VisitorName => "JsonExporter";
        private readonly List<string> _jsonParts = new();

        public void Visit(Ticket ticket)
        {
            _jsonParts.Add(
                $"{{\"type\":\"ticket\",\"id\":\"{ticket.TicketId}\"," +
                $"\"flight\":\"{ticket.Flight.FlightNumber}\"," +
                $"\"passenger\":\"{ticket.Passenger.FullName}\"," +
                $"\"seat\":\"{ticket.SeatNumber}\"," +
                $"\"price\":{ticket.FinalPrice:F2}," +
                $"\"status\":\"{ticket.Status}\"}}");
        }

        public void Visit(Reservation reservation)
        {
            _jsonParts.Add(
                $"{{\"type\":\"reservation\",\"id\":\"{reservation.ReservationId}\"," +
                $"\"passenger\":\"{reservation.Passenger.FullName}\"," +
                $"\"total\":{reservation.TotalPrice:F2}," +
                $"\"status\":\"{reservation.Status}\"," +
                $"\"tickets\":{reservation.Tickets.Count}}}");
        }

        public void Visit(Flight flight)
        {
            _jsonParts.Add(
                $"{{\"type\":\"flight\",\"number\":\"{flight.FlightNumber}\"," +
                $"\"origin\":\"{flight.Origin.Code}\"," +
                $"\"destination\":\"{flight.Destination.Code}\"," +
                $"\"class\":\"{flight.Class}\"," +
                $"\"price\":{flight.BasePrice:F2}," +
                $"\"seats\":{flight.AvailableSeats}}}");
        }

        public string GetJson()
            => "[\n  " + string.Join(",\n  ", _jsonParts) + "\n]";

        public int ExportedCount => _jsonParts.Count;
    }

    // ── Visitor 3: Statistici ─────────────────────────────────────────
    public class StatisticsVisitor : IBookingVisitor
    {
        public string VisitorName => "StatisticsCollector";

        private int     _flightCount       = 0;
        private int     _reservationCount  = 0;
        private int     _ticketCount       = 0;
        private decimal _totalRevenue      = 0m;
        private int     _totalSeatsBooked  = 0;
        private readonly Dictionary<SeatClass, int> _classCounts = new();
        private readonly Dictionary<string, int>    _routeCounts = new();

        public void Visit(Flight flight)
        {
            _flightCount++;
            if (!_classCounts.ContainsKey(flight.Class))
                _classCounts[flight.Class] = 0;
            _classCounts[flight.Class]++;

            var route = $"{flight.Origin.Code}->{flight.Destination.Code}";
            if (!_routeCounts.ContainsKey(route))
                _routeCounts[route] = 0;
            _routeCounts[route]++;
        }

        public void Visit(Reservation reservation)
        {
            _reservationCount++;
            _totalRevenue += reservation.TotalPrice;
        }

        public void Visit(Ticket ticket)
        {
            _ticketCount++;
            _totalSeatsBooked++;
        }

        public void PrintStatistics()
        {
            Console.WriteLine("  [Statistics] STATISTICI:");
            Console.WriteLine($"    Zboruri analizate    : {_flightCount}");
            Console.WriteLine($"    Rezervari            : {_reservationCount}");
            Console.WriteLine($"    Bilete emise         : {_ticketCount}");
            Console.WriteLine($"    Venituri totale      : {_totalRevenue:C}");
            Console.WriteLine($"    Mediu/rezervare      : " +
                $"{(_reservationCount > 0 ? _totalRevenue / _reservationCount : 0):C}");

            if (_classCounts.Any())
            {
                Console.WriteLine("    Zboruri pe clase:");
                foreach (var (cls, cnt) in _classCounts)
                    Console.WriteLine($"      {cls}: {cnt}");
            }

            if (_routeCounts.Any())
            {
                var topRoute = _routeCounts.OrderByDescending(r => r.Value).First();
                Console.WriteLine($"    Ruta cea mai frecventa: {topRoute.Key} ({topRoute.Value}x)");
            }
        }
    }

    // ── Visitor 4: Audit de securitate ────────────────────────────────
    public class SecurityAuditVisitor : IBookingVisitor
    {
        public string VisitorName => "SecurityAuditor";
        private readonly List<string> _auditLog = new();

        public void Visit(Ticket ticket)
        {
            if (string.IsNullOrWhiteSpace(ticket.Passenger.PassportNo))
                _auditLog.Add($"[WARN] Bilet #{ticket.TicketId}: Pasaport lipsa!");
            else
                _auditLog.Add($"[OK]   Bilet #{ticket.TicketId}: Pasaport verificat " +
                              $"({ticket.Passenger.PassportNo})");
        }

        public void Visit(Reservation reservation)
        {
            if (reservation.Status == ReservationStatus.Cancelled)
                _auditLog.Add($"[INFO] Rezervare #{reservation.ReservationId}: Anulata.");
            else if (reservation.TotalPrice > 1000m)
                _auditLog.Add($"[FLAG] Rezervare #{reservation.ReservationId}: " +
                              $"Suma mare ({reservation.TotalPrice:C}) — necesita verificare.");
            else
                _auditLog.Add($"[OK]   Rezervare #{reservation.ReservationId}: OK.");
        }

        public void Visit(Flight flight)
        {
            var fillRate = flight.TotalSeats > 0
                ? (double)(flight.TotalSeats - flight.AvailableSeats) / flight.TotalSeats * 100
                : 0;
            _auditLog.Add($"[OK]   Zbor {flight.FlightNumber}: " +
                          $"Ocupare {fillRate:F1}% ({flight.TotalSeats - flight.AvailableSeats}/{flight.TotalSeats})");
        }

        public void PrintReport()
        {
            Console.WriteLine("  [SecurityAudit] RAPORT:");
            foreach (var entry in _auditLog)
                Console.WriteLine($"    {entry}");
        }

        public IReadOnlyList<string> AuditLog => _auditLog.AsReadOnly();
    }

    // ── Colectie vizitabila ───────────────────────────────────────────
    // Aplica un visitor la toate elementele dintr-o colectie mixta
    public class VisitableBookingCollection
    {
        private readonly List<Ticket>      _tickets      = new();
        private readonly List<Reservation> _reservations = new();
        private readonly List<Flight>      _flights      = new();

        public void Add(Ticket ticket)           => _tickets.Add(ticket);
        public void Add(Reservation reservation) => _reservations.Add(reservation);
        public void Add(Flight flight)           => _flights.Add(flight);

        public void AcceptAll(IBookingVisitor visitor)
        {
            foreach (var f in _flights)      visitor.Visit(f);
            foreach (var r in _reservations) visitor.Visit(r);
            foreach (var t in _tickets)      visitor.Visit(t);
        }
    }
}
