using FlightBooking.Models;
using FlightBooking.Services;

namespace FlightBooking.Behavioral.TemplateMethod
{
    // ══════════════════════════════════════════════════════════════════
    //  TEMPLATE METHOD PATTERN
    //
    //  Problema rezolvata: generarea de rapoarte (text, HTML, CSV)
    //  urmeaza acelasi flux: initializare -> header -> continut ->
    //  footer -> finalizare. Doar formatul difera.
    //
    //  Solutia: clasa de baza defineste SCHELETUL algoritmului in
    //  metoda sablon (GenerateReport). Pasii variabili sunt abstract/
    //  virtual — subclasele le suprascriu fara a atinge scheletul.
    // ══════════════════════════════════════════════════════════════════

    // ── Clasa de baza: scheletul algoritmului ────────────────────────
    public abstract class FlightReportTemplate
    {
        // ── Template Method (FINAL – nu se suprascrie) ───────────────
        public string GenerateReport(IEnumerable<Flight> flights,
                                     IEnumerable<Reservation> reservations)
        {
            AppLogger.Instance.Info("TemplateMethod",
                $"Generare raport {ReportName}...");

            var sb = new System.Text.StringBuilder();

            // Pasul 1: Initializare (hook optional)
            sb.Append(Initialize());

            // Pasul 2: Header (abstract – obligatoriu in subclasa)
            sb.Append(RenderHeader());

            // Pasul 3: Continut zboruri (abstract)
            sb.Append(RenderFlightsSection(flights.ToList()));

            // Pasul 4: Continut rezervari (abstract)
            sb.Append(RenderReservationsSection(reservations.ToList()));

            // Pasul 5: Sumar (hook optional cu implementare default)
            sb.Append(RenderSummary(flights.ToList(), reservations.ToList()));

            // Pasul 6: Footer (abstract)
            sb.Append(RenderFooter());

            // Pasul 7: Finalizare (hook optional)
            sb.Append(Finalize());

            return sb.ToString();
        }

        public abstract string ReportName { get; }

        // ── Pasi abstracti (TREBUIE suprascrise) ────────────────────
        protected abstract string RenderHeader();
        protected abstract string RenderFlightsSection(List<Flight> flights);
        protected abstract string RenderReservationsSection(List<Reservation> reservations);
        protected abstract string RenderFooter();

        // ── Hook-uri (optionale, au implementare default) ────────────
        protected virtual string Initialize()  => string.Empty;
        protected virtual string Finalize()    => string.Empty;
        protected virtual string RenderSummary(List<Flight> flights,
                                               List<Reservation> reservations)
        {
            int totalFlights = flights.Count;
            int totalRes     = reservations.Count;
            decimal revenue  = reservations.Sum(r => r.TotalPrice);
            return $"\nSUMAR: {totalFlights} zboruri | {totalRes} rezervari | Venituri: {revenue:C}\n";
        }
    }

    // ── Raport 1: Text simplu ────────────────────────────────────────
    public class TextFlightReport : FlightReportTemplate
    {
        public override string ReportName => "TextReport";

        protected override string RenderHeader() =>
            "=== RAPORT ZBORURI SI REZERVARI ===\n" +
            $"Generat: {DateTime.UtcNow:dd MMM yyyy HH:mm}\n" +
            new string('-', 50) + "\n";

        protected override string RenderFlightsSection(List<Flight> flights)
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("\n[ZBORURI]");
            foreach (var f in flights)
                sb.AppendLine($"  {f.FlightNumber,-10} {f.Origin.Code}->{f.Destination.Code,-8} " +
                              $"{f.DepartureTime:dd MMM HH:mm,-20} {f.Class,-12} {f.BasePrice:C}");
            return sb.ToString();
        }

        protected override string RenderReservationsSection(List<Reservation> reservations)
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("\n[REZERVARI]");
            foreach (var r in reservations)
                sb.AppendLine($"  #{r.ReservationId,-10} {r.Passenger.FullName,-25} " +
                              $"{r.TotalPrice:C,-10} {r.Status}");
            return sb.ToString();
        }

        protected override string RenderFooter() =>
            new string('=', 50) + "\n[SFARSIT RAPORT]\n";
    }

    // ── Raport 2: HTML ───────────────────────────────────────────────
    public class HtmlFlightReport : FlightReportTemplate
    {
        public override string ReportName => "HtmlReport";

        protected override string Initialize() =>
            "<!DOCTYPE html><html><head>" +
            "<style>body{font-family:Arial}table{border-collapse:collapse;width:100%}" +
            "th,td{border:1px solid #ddd;padding:8px}th{background:#003580;color:white}</style>" +
            "</head><body>";

        protected override string RenderHeader() =>
            "<h1>Raport Zboruri si Rezervari</h1>" +
            $"<p>Generat: {DateTime.UtcNow:dd MMM yyyy HH:mm}</p>";

        protected override string RenderFlightsSection(List<Flight> flights)
        {
            var sb = new System.Text.StringBuilder();
            sb.Append("<h2>Zboruri</h2><table><tr>" +
                      "<th>Nr. Zbor</th><th>Ruta</th><th>Data</th><th>Clasa</th><th>Pret</th>" +
                      "<th>Locuri</th></tr>");
            foreach (var f in flights)
                sb.Append($"<tr><td>{f.FlightNumber}</td>" +
                          $"<td>{f.Origin.Code}->{f.Destination.Code}</td>" +
                          $"<td>{f.DepartureTime:dd MMM HH:mm}</td>" +
                          $"<td>{f.Class}</td><td>{f.BasePrice:C}</td>" +
                          $"<td>{f.AvailableSeats}/{f.TotalSeats}</td></tr>");
            sb.Append("</table>");
            return sb.ToString();
        }

        protected override string RenderReservationsSection(List<Reservation> reservations)
        {
            var sb = new System.Text.StringBuilder();
            sb.Append("<h2>Rezervari</h2><table><tr>" +
                      "<th>ID</th><th>Pasager</th><th>Total</th><th>Status</th></tr>");
            foreach (var r in reservations)
                sb.Append($"<tr><td>{r.ReservationId}</td>" +
                          $"<td>{r.Passenger.FullName}</td>" +
                          $"<td>{r.TotalPrice:C}</td><td>{r.Status}</td></tr>");
            sb.Append("</table>");
            return sb.ToString();
        }

        protected override string RenderSummary(List<Flight> flights, List<Reservation> res)
        {
            decimal revenue = res.Sum(r => r.TotalPrice);
            return $"<div style='background:#e8f0fe;padding:12px;margin:12px 0'>" +
                   $"<b>Sumar:</b> {flights.Count} zboruri | " +
                   $"{res.Count} rezervari | Venituri: <b>{revenue:C}</b></div>";
        }

        protected override string RenderFooter() =>
            "<footer><p><i>FlightBooking System</i></p></footer>";

        protected override string Finalize() => "</body></html>";
    }

    // ── Raport 3: CSV ────────────────────────────────────────────────
    public class CsvFlightReport : FlightReportTemplate
    {
        public override string ReportName => "CsvReport";

        protected override string RenderHeader() =>
            "TIP,DETALII,DATA,PRET,STATUS\n";

        protected override string RenderFlightsSection(List<Flight> flights)
        {
            var sb = new System.Text.StringBuilder();
            foreach (var f in flights)
                sb.AppendLine($"ZBOR,{f.FlightNumber} {f.Origin.Code}->{f.Destination.Code}," +
                              $"{f.DepartureTime:dd-MM-yyyy HH:mm},{f.BasePrice:F2},ACTIV");
            return sb.ToString();
        }

        protected override string RenderReservationsSection(List<Reservation> reservations)
        {
            var sb = new System.Text.StringBuilder();
            foreach (var r in reservations)
                sb.AppendLine($"REZERVARE,#{r.ReservationId} {r.Passenger.FullName}," +
                              $"{r.CreatedAt:dd-MM-yyyy HH:mm},{r.TotalPrice:F2},{r.Status}");
            return sb.ToString();
        }

        protected override string RenderFooter() =>
            $"SUMAR,Total,{DateTime.UtcNow:dd-MM-yyyy},, \n";

        // CSV nu are sumar vizual
        protected override string RenderSummary(List<Flight> f, List<Reservation> r)
            => string.Empty;
    }
}
