using FlightBooking.Behavioral.Command;
using FlightBooking.Behavioral.Iterator;
using FlightBooking.Behavioral.Memento;
using FlightBooking.Behavioral.Observer;
using FlightBooking.Behavioral.Strategy;
using FlightBooking.Factories;
using FlightBooking.Factories.Builder;
using FlightBooking.Interfaces;
using FlightBooking.Models;
using FlightBooking.Repositories;
using FlightBooking.Services;
using FlightBooking.Structural.Adapter;
using FlightBooking.Structural.Bridge;
using FlightBooking.Structural.Composite;
using FlightBooking.Structural.Decorator;
using FlightBooking.Structural.Facade;
using FlightBooking.Structural.Flyweight;
using FlightBooking.Structural.Proxy;
using FlightBooking.Tests;
using FlightBooking.Utils;

// ===================================================================
//  FLIGHT BOOKING SYSTEM  -  Lab 1-6 COMPLET
// ===================================================================
Console.OutputEncoding = System.Text.Encoding.UTF8;
AppLogger.Instance.Info("Program", "Aplicatia pornita.");

// == Composition Root ===============================================
IFlightRepository      flightRepo      = new InMemoryFlightRepository();
IReservationRepository reservationRepo = new InMemoryReservationRepository();
IPricingStrategy       pricing         = new EarlyBirdPricingStrategy(new StandardPricingStrategy());
var searchService  = new FlightSearchService(flightRepo);
var bookingService = new BookingService(reservationRepo, pricing);
DataSeeder.Seed(flightRepo);

var kiv = new Airport("KIV","Chisinau Int'l","Chisinau","Moldova");
var otp = new Airport("OTP","Henri Coanda","Bucuresti","Romania");
var cdg = new Airport("CDG","Charles de Gaulle","Paris","Franta");
var lhr = new Airport("LHR","Heathrow","Londra","Anglia");
var jfk = new Airport("JFK","JFK Airport","New York","SUA");

var director = new FlightDirector(new FlightBuilder());
var econFlight  = director.BuildEconomyShortHaul("DIR-EC1", kiv, lhr,
    DateTime.UtcNow.AddDays(8).Date.AddHours(7));
var bizFlight   = director.BuildBusinessLongHaul("DIR-BZ1", kiv, jfk,
    DateTime.UtcNow.AddDays(20).Date.AddHours(10));
var firstFlight = director.BuildFirstClassIntercontinental("DIR-FC1", kiv, jfk,
    DateTime.UtcNow.AddDays(25).Date.AddHours(11));
flightRepo.Add(econFlight);
flightRepo.Add(bizFlight);
flightRepo.Add(firstFlight);

var demoPass = new Passenger("Ion","Popescu","ion@demo.md","MD123");
var demoFlt  = flightRepo.GetAll().First(f => f.HasAvailableSeats());
var demoRes  = bookingService.CreateReservation(demoPass, demoFlt,
    SeatGenerator.Generate(demoFlt.TotalSeats));
bookingService.ConfirmReservation(demoRes.ReservationId);
demoRes = bookingService.GetReservation(demoRes.ReservationId)!;

// ==================================================================
//  LAB 6a - STRATEGY: sortare si filtrare zboruri
// ==================================================================
PrintHeader("LAB 6a - STRATEGY PATTERN");

var allFlights = flightRepo.GetAll().ToList();
Console.WriteLine($"  Total zboruri: {allFlights.Count}");

var ctx = new FlightSearchContext();

Console.WriteLine("\n  -- Sortare dupa pret crescator --");
ctx.SetSortStrategy(new SortByPriceAscending());
var sorted = ctx.Execute(allFlights).Take(3).ToList();
sorted.ForEach(f => Console.WriteLine($"    {f.FlightNumber}: {f.BasePrice:C} | {f.Class}"));

Console.WriteLine("\n  -- Filtrare Economy + sortare dupa durata --");
ctx.SetFilterStrategy(new FilterByClass(SeatClass.Economy));
ctx.SetSortStrategy(new SortByDuration());
var filtered = ctx.Execute(allFlights).Take(3).ToList();
filtered.ForEach(f => Console.WriteLine($"    {f.FlightNumber}: {f.Duration:hh\\:mm}h | {f.BasePrice:C}"));

Console.WriteLine("\n  -- Schimbare strategie la runtime: pret max 150 --");
ctx.SetFilterStrategy(new FilterByMaxPrice(150m));
ctx.SetSortStrategy(new SortByPriceDescending());
var cheap = ctx.Execute(allFlights).ToList();
Console.WriteLine($"    Zboruri sub 150 EUR: {cheap.Count}");

// ==================================================================
//  LAB 6b - OBSERVER: notificari la schimbari zbor
// ==================================================================
PrintHeader("LAB 6b - OBSERVER PATTERN");

var monitor   = new FlightMonitor();
var passenger1 = new PassengerObserver("Ion Popescu",  "ion@demo.md");
var passenger2 = new PassengerObserver("Maria Ionescu","maria@demo.md");
var display    = new AirportDisplayObserver("Terminal A");
var audit      = new AuditObserver();

monitor.Subscribe(passenger1);
monitor.Subscribe(passenger2);
monitor.Subscribe(display);
monitor.Subscribe(audit);

Console.WriteLine("\n  Simulare evenimente zbor:\n");
monitor.ReportDelay(econFlight, 45);
Console.WriteLine();
monitor.ReportGateChange(econFlight, "A12", "B07");
Console.WriteLine();
monitor.ReportPriceChange(bizFlight, 650m, 520m);
Console.WriteLine();

// Dezabonare pasager2 si anulare zbor
monitor.Unsubscribe(passenger2);
Console.WriteLine();
monitor.ReportCancellation(firstFlight);
Console.WriteLine();
Console.WriteLine($"  Pasager1 a primit: {passenger1.ReceivedEvents.Count} evenimente");
Console.WriteLine($"  Pasager2 a primit: {passenger2.ReceivedEvents.Count} (dezabonat inainte de anulare)");
Console.WriteLine($"  Audit log: {audit.AuditLog.Count} intrari");

// ==================================================================
//  LAB 6c - COMMAND: Undo/Redo rezervari
// ==================================================================
PrintHeader("LAB 6c - COMMAND PATTERN (Undo/Redo)");

var invoker   = new BookingCommandInvoker();
var testFlt   = flightRepo.GetAll().First(f => f.HasAvailableSeats() && f.Class == SeatClass.Economy);

var cmd1 = new CreateReservationCommand(bookingService,
    new Passenger("Andrei","Stan","andrei@test.md","MD001"), testFlt, "10A");
var cmd2 = new CreateReservationCommand(bookingService,
    new Passenger("Elena","Marin","elena@test.md","MD002"), testFlt, "10B");

Console.WriteLine("\n  -- Execute x2 --");
invoker.Execute(cmd1);
invoker.Execute(cmd2);
invoker.PrintHistory();

Console.WriteLine("\n  -- Undo (anuleaza ultima rezervare) --");
invoker.Undo();
invoker.PrintHistory();

Console.WriteLine("\n  -- Redo (reface rezervarea anulata) --");
invoker.Redo();
invoker.PrintHistory();

Console.WriteLine("\n  -- Undo din nou --");
invoker.Undo();

// Confirmare rezervare cu Command
if (cmd1.CreatedReservation != null)
{
    var confirmCmd = new ConfirmReservationCommand(
        bookingService, cmd1.CreatedReservation.ReservationId);
    Console.WriteLine("\n  -- Confirmare rezervare prin Command --");
    invoker.Execute(confirmCmd);
    Console.WriteLine("\n  -- Undo la Confirmare --");
    invoker.Undo();
}

// ==================================================================
//  LAB 6d - MEMENTO: Save/Load profil pasager
// ==================================================================
PrintHeader("LAB 6d - MEMENTO PATTERN (Save/Load)");

var profile = new PassengerProfile
{
    FirstName      = "Ion",
    LastName       = "Popescu",
    Email          = "ion@demo.md",
    PassportNo     = "MD123456",
    SeatPreference = "Window",
    MealPreference = "Standard",
    FrequentFlyer  = "MV12345"
};

var history = new ProfileHistory();

profile.Print("Starea initiala: ");
history.Push(profile.Save("Initial"));

// Modificare 1
profile.SeatPreference = "Aisle";
profile.MealPreference = "Vegetarian";
profile.Print("Dupa modificare 1: ");
history.Push(profile.Save("Vegetarian+Aisle"));

// Modificare 2
profile.FrequentFlyer  = "MV99999";
profile.NewsletterSub  = true;
profile.Print("Dupa modificare 2: ");
history.Push(profile.Save("Premium upgrade"));

Console.WriteLine();
history.PrintHistory();

// Undo - revenim la starea anterioara
Console.WriteLine("\n  -- Undo (revenim la Vegetarian+Aisle) --");
var prev = history.Undo();
if (prev != null) profile.Restore(prev);
profile.Print("Dupa Undo: ");

// Undo inca o data
Console.WriteLine("\n  -- Undo (revenim la Initial) --");
prev = history.Undo();
if (prev != null) profile.Restore(prev);
profile.Print("Dupa al 2-lea Undo: ");

// Redo
Console.WriteLine("\n  -- Redo --");
var next = history.Redo();
if (next != null) profile.Restore(next);
profile.Print("Dupa Redo: ");

// ==================================================================
//  LAB 6e - ITERATOR: parcurgere colectii
// ==================================================================
PrintHeader("LAB 6e - ITERATOR PATTERN");

var flightCollection = new FlightCollection();
flightCollection.AddRange(flightRepo.GetAll());
Console.WriteLine($"  Colectie: {flightCollection.Count} zboruri\n");

// Iterator secvential
Console.WriteLine("  -- Iterator Secvential (primele 3) --");
var seqIter = flightCollection.CreateIterator();
int shown   = 0;
while (seqIter.HasNext() && shown < 3)
{
    Console.WriteLine($"    [{seqIter.Position + 1}] {seqIter.Next()}");
    shown++;
}

// Iterator invers
Console.WriteLine("\n  -- Iterator Invers (ultimele 3) --");
var revIter = flightCollection.CreateReverseIterator();
shown = 0;
while (revIter.HasNext() && shown < 3)
{
    Console.WriteLine($"    {revIter.Next()}");
    shown++;
}

// Iterator filtrat - doar Economy cu locuri disponibile
Console.WriteLine("\n  -- Iterator Filtrat (Economy, locuri disponibile) --");
var filtIter = flightCollection.CreateFilteredIterator(
    f => f.Class == SeatClass.Economy && f.HasAvailableSeats());
int econCount = 0;
while (filtIter.HasNext()) { filtIter.Next(); econCount++; }
Console.WriteLine($"    Zboruri Economy disponibile: {econCount}");

// Iterator pe ruta
Console.WriteLine("\n  -- Iterator pe Ruta KIV->OTP --");
var routeIter = flightCollection.IterateByRoute("KIV","OTP");
int routeCount = 0;
while (routeIter.HasNext()) { routeIter.Next(); routeCount++; }
Console.WriteLine($"    Zboruri KIV->OTP: {routeCount}");

// Iterator rezervari
Console.WriteLine("\n  -- Iterator Rezervari --");
var allReservations = reservationRepo.GetAll().ToList();
var resIter = new ReservationIterator(allReservations);
Console.WriteLine($"    Total rezervari: {resIter.Count}");
var confirmed = resIter.Where(r => r.Status == ReservationStatus.Paid).ToList();
Console.WriteLine($"    Rezervari confirmate: {confirmed.Count}");

// ==================================================================
//  RECAP Lab 4+5 (scurt)
// ==================================================================
PrintHeader("LAB 4+5 - RECAP STRUCTURAL PATTERNS");

// Adapter
Console.WriteLine("  [Adapter] Plata prin MAIB:");
new PaymentService(new MaibAdapter(new MaibBankGateway()))
    .PayForReservation(demoRes, demoPass.Email);

// Composite
Console.WriteLine("\n  [Composite] Pachet dus-intors:");
var itin = new Itinerary("KIV <-> Paris");
itin.Add(new FlightSegment(econFlight, 145m));
itin.Add(new FlightSegment(bizFlight,  130m));
Console.WriteLine($"    Pret total: {itin.TotalPrice:C} | Durata: {itin.TotalDuration:hh\\:mm}h");

// Decorator
Console.WriteLine("\n  [Decorator] Bilet cu servicii extra:");
IFlightService svc = new BasicFlightService(demoRes.Tickets.First());
svc = new BaggageDecorator(svc);
svc = new PriorityBoardingDecorator(svc);
Console.WriteLine($"    {svc.ServiceName} -> Total: {svc.Price:C}");

// Flyweight
Console.WriteLine("\n  [Flyweight] Pool aeroporturi:");
var fw = new AirportFlyweightFactory();
for (int i = 0; i < 100; i++)
    fw.GetAirport("KIV","Chisinau","Chisinau","Moldova");
Console.WriteLine($"    100 cereri -> 1 instanta unica, {fw.ReusedCount} reutilizari");

// Bridge
Console.WriteLine("\n  [Bridge] Notificare Markdown:");
new FlightReminderSender(new MarkdownRenderer(), 24).Send(demoRes);

// Proxy
Console.WriteLine("  [Proxy] Acces restrictionat (Guest):");
var guestProxy = new AuthenticatedBookingProxy(
    bookingService, new UserContext("guest1", UserRole.Guest));
try { guestProxy.CreateReservation(demoPass, econFlight, "1A"); }
catch (UnauthorizedAccessException ex)
{ Console.WriteLine($"    BLOCAT: {ex.Message}"); }

// ==================================================================
//  TOATE TESTELE UNITARE
// ==================================================================
FactoryTests.RunAll();
CreationalPatternsTests.RunAll();
StructuralPatternsTests.RunAll();
StructuralPatterns2Tests.RunAll();
BehavioralPatternsTests.RunAll();

Console.WriteLine($"\n  Total log entries: {AppLogger.Instance.EntryCount}");
AppLogger.Instance.Info("Program","Toate laboratoarele finalizate cu succes!");
Console.WriteLine();

static void PrintHeader(string title)
{
    Console.WriteLine();
    var pad = new string('=', Math.Max(0, 52 - title.Length));
    Console.WriteLine($"  == {title} {pad}");
}
