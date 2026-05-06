using FlightBooking.Interfaces;
using FlightBooking.Models;
using FlightBooking.Repositories;
using FlightBooking.Services;
using FlightBooking.Utils;

// ═══════════════════════════════════════════════════════════════════
//  FLIGHT BOOKING SYSTEM  –  Laborator 1  (OOP + SOLID)
// ═══════════════════════════════════════════════════════════════════

Console.OutputEncoding = System.Text.Encoding.UTF8;

// ── DIP: compunem dependentele la nivelul cel mai inalt (Composition Root) ──
IFlightRepository       flightRepo       = new InMemoryFlightRepository();
IReservationRepository  reservationRepo  = new InMemoryReservationRepository();
IPricingStrategy        pricingStrategy  = new EarlyBirdPricingStrategy(new StandardPricingStrategy());

var searchService  = new FlightSearchService(flightRepo);
var bookingService = new BookingService(reservationRepo, pricingStrategy);

// ── Populam date initiale ────────────────────────────────────────────
DataSeeder.Seed(flightRepo);

// ════════════════════════════════════════════════════════════════════
//  DEMO 1 – Afisarea tuturor zborurilor disponibile
// ════════════════════════════════════════════════════════════════════
PrintHeader("TOATE ZBORURILE DISPONIBILE");
foreach (var f in searchService.GetAllFlights())
    Console.WriteLine($"  {f}");

// ════════════════════════════════════════════════════════════════════
//  DEMO 2 – Cautare zbor (alegere destinatie)
// ════════════════════════════════════════════════════════════════════
PrintHeader("CAUTARE ZBOR: KIV → IST");
var targetDate  = DateTime.UtcNow.AddDays(3).Date;
var foundFlights = searchService.Search("KIV", "IST", targetDate).ToList();

if (foundFlights.Count == 0)
{
    Console.WriteLine("  Nu s-au gasit zboruri.");
}
else
{
    foreach (var f in foundFlights)
        Console.WriteLine($"  {f}");
}

// ════════════════════════════════════════════════════════════════════
//  DEMO 3 – Formular de rezervare (simulat in consola)
// ════════════════════════════════════════════════════════════════════
PrintHeader("FORMULAR DE REZERVARE");

var passenger = new Passenger("Ion", "Popescu", "ion.popescu@email.md", "MD123456");
Console.WriteLine($"  Pasager : {passenger}");

var selectedFlight = foundFlights.First(f => f.Class == SeatClass.Economy);
Console.WriteLine($"  Zbor ales: {selectedFlight}");

var seatNumber  = SeatGenerator.Generate(selectedFlight.TotalSeats);
var reservation = bookingService.CreateReservation(passenger, selectedFlight, seatNumber);
Console.WriteLine($"\n  ✔ Rezervare creata: {reservation}");

// ════════════════════════════════════════════════════════════════════
//  DEMO 4 – Afisare bilet
// ════════════════════════════════════════════════════════════════════
PrintHeader("BILETUL TĂU");

bookingService.ConfirmReservation(reservation.ReservationId);
var confirmed = bookingService.GetReservation(reservation.ReservationId)!;

Console.WriteLine($"  {confirmed}");
foreach (var ticket in confirmed.Tickets)
{
    Console.WriteLine();
    Console.WriteLine($"  ╔══════════════════════════════════════════════╗");
    Console.WriteLine($"  ║          BOARDING PASS                       ║");
    Console.WriteLine($"  ║  Zbor   : {ticket.Flight.FlightNumber,-36}║");
    Console.WriteLine($"  ║  De la  : {ticket.Flight.Origin,-36}║");
    Console.WriteLine($"  ║  La     : {ticket.Flight.Destination,-36}║");
    Console.WriteLine($"  ║  Data   : {ticket.Flight.DepartureTime:dd MMM yyyy HH:mm,-28}║");
    Console.WriteLine($"  ║  Durata : {ticket.Flight.Duration:hh\\:mm}h{"",-33}║");
    Console.WriteLine($"  ║  Clasa  : {ticket.Flight.Class,-36}║");
    Console.WriteLine($"  ║  Loc    : {ticket.SeatNumber,-36}║");
    Console.WriteLine($"  ║  Pret   : {ticket.FinalPrice:C,-35}║");
    Console.WriteLine($"  ║  Status : {ticket.Status,-36}║");
    Console.WriteLine($"  ║  ID     : {ticket.TicketId,-36}║");
    Console.WriteLine($"  ╚══════════════════════════════════════════════╝");
}

// ════════════════════════════════════════════════════════════════════
//  DEMO 5 – Cautare rezervare dupa email (LSP demo)
// ════════════════════════════════════════════════════════════════════
PrintHeader("REZERVARILE PASAGERULUI (dupa email)");
var myReservations = reservationRepo.GetByPassengerEmail("ion.popescu@email.md");
foreach (var r in myReservations)
    Console.WriteLine($"  {r}");

Console.WriteLine();

// ─── helper ─────────────────────────────────────────────────────────
static void PrintHeader(string title)
{
    Console.WriteLine();
    Console.WriteLine($"  ══ {title} {'═'.ToString().PadRight(Math.Max(0, 46 - title.Length), '═')}");
}
