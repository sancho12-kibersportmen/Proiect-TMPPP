using FlightBooking.Behavioral.ChainOfResponsibility;
using FlightBooking.Behavioral.Command;
using FlightBooking.Behavioral.Iterator;
using FlightBooking.Behavioral.Mediator;
using FlightBooking.Behavioral.Memento;
using FlightBooking.Behavioral.Observer;
using FlightBooking.Behavioral.State;
using FlightBooking.Behavioral.Strategy;
using FlightBooking.Behavioral.TemplateMethod;
using FlightBooking.Behavioral.Visitor;
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
//  FLIGHT BOOKING SYSTEM  -  Lab 1-7 COMPLET
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

var director    = new FlightDirector(new FlightBuilder());
var econFlight  = director.BuildEconomyShortHaul("DIR-EC1",kiv,lhr,DateTime.UtcNow.AddDays(8).Date.AddHours(7));
var bizFlight   = director.BuildBusinessLongHaul("DIR-BZ1",kiv,jfk,DateTime.UtcNow.AddDays(20).Date.AddHours(10));
var firstFlight = director.BuildFirstClassIntercontinental("DIR-FC1",kiv,jfk,DateTime.UtcNow.AddDays(25).Date.AddHours(11));
flightRepo.Add(econFlight); flightRepo.Add(bizFlight); flightRepo.Add(firstFlight);

var demoPass = new Passenger("Ion","Popescu","ion@demo.md","MD123");
var demoFlt  = flightRepo.GetAll().First(f => f.HasAvailableSeats());
var demoRes  = bookingService.CreateReservation(demoPass, demoFlt, SeatGenerator.Generate(demoFlt.TotalSeats));
bookingService.ConfirmReservation(demoRes.ReservationId);
demoRes = bookingService.GetReservation(demoRes.ReservationId)!;

// ==================================================================
//  LAB 7a - CHAIN OF RESPONSIBILITY: validare cerere rezervare
// ==================================================================
PrintHeader("LAB 7a - CHAIN OF RESPONSIBILITY");

var chain = BookingValidationChainBuilder.BuildStandardChain();

// Cerere valida — trece toate handler-ele
Console.WriteLine("\n  -- Cerere VALIDA --");
var validRequest = new BookingRequest
{
    Passenger    = new Passenger("Maria","Ionescu","maria@test.md","RO654321"),
    Flight       = flightRepo.GetAll().First(f => f.HasAvailableSeats() && f.Class == SeatClass.Economy),
    SeatNumber   = "5A",
    CustomerId   = "cus_maria123",
    BudgetLimit  = 500m,
    PassengerAge = 28
};
var validResult = chain.Handle(validRequest);
Console.WriteLine($"\n  Rezultat: {(validResult.IsValid ? "VALID ✔" : "INVALID ✘")}");

// Cerere invalida — buget insuficient
Console.WriteLine("\n  -- Cerere INVALIDA (buget insuficient) --");
var budgetRequest = new BookingRequest
{
    Passenger    = new Passenger("Vasile","Marin","vasile@test.md","MD999"),
    Flight       = bizFlight,
    SeatNumber   = "3B",
    CustomerId   = "cus_vas",
    BudgetLimit  = 50m,    // prea mic
    PassengerAge = 30
};
var budgetResult = chain.Handle(budgetRequest);
Console.WriteLine($"\n  Blocat de: {budgetResult.BlockedBy}");
Console.WriteLine($"  Eroare: {budgetResult.Errors.FirstOrDefault()}");

// Cerere invalida — minor
Console.WriteLine("\n  -- Cerere INVALIDA (pasager minor) --");
var minorRequest = new BookingRequest
{
    Passenger    = new Passenger("Alex","Pop","alex@test.md","MD888"),
    Flight       = econFlight,
    SeatNumber   = "9C",
    CustomerId   = "cus_alex",
    BudgetLimit  = 500m,
    PassengerAge = 15
};
var minorResult = chain.Handle(minorRequest);
Console.WriteLine($"\n  Blocat de: {minorResult.BlockedBy}");

// ==================================================================
//  LAB 7b - STATE: ciclul de viata al unui zbor
// ==================================================================
PrintHeader("LAB 7b - STATE PATTERN");

var flightCtx = new FlightContext("MV404");
Console.WriteLine();

// Flux normal: Scheduled -> CheckIn -> Boarding -> InFlight -> Landed
flightCtx.CheckIn();
flightCtx.Board();
flightCtx.Delay(20);    // intarziere la imbarcare
flightCtx.Depart();
flightCtx.Land();

Console.WriteLine($"\n  Stare finala: {flightCtx.CurrentState} | Intarziere totala: {flightCtx.DelayMinutes}min");

// Actiune invalida dupa aterizare
Console.WriteLine("\n  -- Actiune invalida dupa aterizare --");
flightCtx.Cancel();    // nu trebuie sa functioneze

// Zbor anulat
Console.WriteLine("\n  -- Zbor anulat din starea CheckIn --");
var cancelledFlight = new FlightContext("MV505");
cancelledFlight.CheckIn();
cancelledFlight.Cancel();
Console.WriteLine($"  Stare finala: {cancelledFlight.CurrentState}");

// ==================================================================
//  LAB 7c - MEDIATOR: comunicare prin hub central
// ==================================================================
PrintHeader("LAB 7c - MEDIATOR PATTERN");

var resComp   = new ReservationComponent();
var payComp   = new PaymentComponent();
var notifComp = new NotificationComponent();
var invComp   = new InventoryComponent();
var mediator  = new ConcreteBookingMediator(resComp, payComp, notifComp, invComp);

// Rezervare prin mediator (orchestreaza automat: rezervare -> plata -> confirmare -> notificare -> inventar)
var medPass = new Passenger("Elena","Marin","elena@test.md","RO111222");
var medFlt  = flightRepo.GetAll().First(f => f.HasAvailableSeats() && f.Class == SeatClass.Economy);
var medRes  = bookingService.CreateReservation(medPass, medFlt, SeatGenerator.Generate(medFlt.TotalSeats));
mediator.BookFlight(medRes);

Console.WriteLine($"\n  Notificari trimise: {notifComp.SentMessages.Count}");
Console.WriteLine($"  Locuri blocate in inventar: {invComp.ReservedSeats}");

// Anulare prin mediator
Console.WriteLine();
mediator.CancelFlight();
Console.WriteLine($"\n  Notificari dupa anulare: {notifComp.SentMessages.Count}");
Console.WriteLine($"  Locuri in inventar dupa anulare: {invComp.ReservedSeats}");

// ==================================================================
//  LAB 7d - TEMPLATE METHOD: generare rapoarte
// ==================================================================
PrintHeader("LAB 7d - TEMPLATE METHOD PATTERN");

var allFlights      = flightRepo.GetAll().Take(3).ToList();
var allReservations = reservationRepo.GetAll().Take(3).ToList();

Console.WriteLine("  -- Raport TEXT --");
var textReport = new TextFlightReport().GenerateReport(allFlights, allReservations);
Console.WriteLine(textReport);

Console.WriteLine("  -- Raport CSV (primele 3 linii) --");
var csvReport = new CsvFlightReport().GenerateReport(allFlights, allReservations);
foreach (var line in csvReport.Split('\n').Take(4))
    Console.WriteLine($"  {line}");

Console.WriteLine("\n  -- Raport HTML (fragmentul de structura) --");
var htmlReport = new HtmlFlightReport().GenerateReport(allFlights, allReservations);
Console.WriteLine($"  HTML generat: {htmlReport.Length} caractere");
Console.WriteLine($"  Contine tabele: {htmlReport.Contains("<table>")}");

// ==================================================================
//  LAB 7e - VISITOR: operatii pe structura de date
// ==================================================================
PrintHeader("LAB 7e - VISITOR PATTERN");

// Construim colectia vizitabila
var visitableCol = new VisitableBookingCollection();
foreach (var f in allFlights)       visitableCol.Add(f);
foreach (var r in allReservations)  visitableCol.Add(r);
visitableCol.Add(demoRes.Tickets.First());

// Visitor 1: Calculator TVA
Console.WriteLine("  -- Visitor 1: Calculator Taxe --");
var taxVisitor = new TaxCalculatorVisitor();
visitableCol.AcceptAll(taxVisitor);
taxVisitor.PrintSummary();

// Visitor 2: Export JSON
Console.WriteLine("\n  -- Visitor 2: Export JSON --");
var jsonVisitor = new JsonExportVisitor();
visitableCol.AcceptAll(jsonVisitor);
var json = jsonVisitor.GetJson();
Console.WriteLine($"  Exportate: {jsonVisitor.ExportedCount} elemente");
Console.WriteLine($"  JSON (primele 200 chars): {json[..Math.Min(200, json.Length)]}...");

// Visitor 3: Statistici
Console.WriteLine("\n  -- Visitor 3: Statistici --");
var statsVisitor = new StatisticsVisitor();
visitableCol.AcceptAll(statsVisitor);
statsVisitor.PrintStatistics();

// Visitor 4: Audit securitate
Console.WriteLine("\n  -- Visitor 4: Audit Securitate --");
var auditVisitor = new SecurityAuditVisitor();
visitableCol.AcceptAll(auditVisitor);
auditVisitor.PrintReport();

// ==================================================================
//  RECAP Lab 6 (Strategy + Observer + Command + Memento + Iterator)
// ==================================================================
PrintHeader("LAB 6 - RECAP BEHAVIORAL PATTERNS (Lab 6)");

// Strategy
var ctx = new FlightSearchContext();
ctx.SetFilterStrategy(new FilterByClass(SeatClass.Economy));
ctx.SetSortStrategy(new SortByPriceAscending());
var stratResult = ctx.Execute(flightRepo.GetAll()).Take(2).ToList();
Console.WriteLine($"  [Strategy] {stratResult.Count} zboruri Economy sortate dupa pret");

// Observer
var monitor = new FlightMonitor();
var obs     = new PassengerObserver("Ion","ion@demo.md");
monitor.Subscribe(obs);
monitor.ReportDelay(econFlight, 30);
Console.WriteLine($"  [Observer] Pasagerul a primit {obs.ReceivedEvents.Count} eveniment(e)");

// Command
var invoker = new BookingCommandInvoker();
var cmdFlt  = flightRepo.GetAll().First(f => f.HasAvailableSeats());
var cmd = new CreateReservationCommand(bookingService,
    new Passenger("Cmd","User","cmd@test.md","MD555"), cmdFlt, "8A");
invoker.Execute(cmd);
invoker.Undo();
Console.WriteLine($"  [Command] Undo stack: {invoker.UndoCount} | Redo stack: {invoker.RedoCount}");

// Memento
var profile = new PassengerProfile { FirstName="Ion", SeatPreference="Window", MealPreference="Standard" };
var history = new ProfileHistory();
history.Push(profile.Save("v1"));
profile.SeatPreference = "Aisle";
history.Push(profile.Save("v2"));
var prev = history.Undo();
if (prev != null) profile.Restore(prev);
Console.WriteLine($"  [Memento] Dupa Undo: SeatPreference={profile.SeatPreference}");

// Iterator
var col = new FlightCollection();
col.AddRange(flightRepo.GetAll());
var iter = col.CreateFilteredIterator(f => f.Class == SeatClass.Economy && f.HasAvailableSeats());
int cnt = 0; while (iter.HasNext()) { iter.Next(); cnt++; }
Console.WriteLine($"  [Iterator] {cnt} zboruri Economy disponibile iterate");

// ==================================================================
//  TOATE TESTELE UNITARE
// ==================================================================
FactoryTests.RunAll();
CreationalPatternsTests.RunAll();
StructuralPatternsTests.RunAll();
StructuralPatterns2Tests.RunAll();
BehavioralPatternsTests.RunAll();
BehavioralPatterns2Tests.RunAll();

Console.WriteLine($"\n  Total log entries: {AppLogger.Instance.EntryCount}");
AppLogger.Instance.Info("Program","PROIECT COMPLET - toate laboratoarele finalizate!");
Console.WriteLine();

static void PrintHeader(string title)
{
    Console.WriteLine();
    var pad = new string('=', Math.Max(0, 52 - title.Length));
    Console.WriteLine($"  == {title} {pad}");
}
