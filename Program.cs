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
//  FLIGHT BOOKING SYSTEM  –  Lab 1-5
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

// Aeroporturi frecvent folosite
var kiv = new Airport("KIV","Chisinau Int'l","Chisinau","Moldova");
var otp = new Airport("OTP","Henri Coanda","Bucuresti","Romania");
var cdg = new Airport("CDG","Charles de Gaulle","Paris","Franta");
var lhr = new Airport("LHR","Heathrow","Londra","Anglia");
var jfk = new Airport("JFK","JFK Airport","New York","SUA");

// Director pentru zboruri demo
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

// Rezervare demo pentru demonstratii
var demoPassenger   = new Passenger("Ion","Popescu","ion@demo.md","MD123");
var demoFlight      = flightRepo.GetAll().First(f => f.HasAvailableSeats());
var demoSeat        = SeatGenerator.Generate(demoFlight.TotalSeats);
var demoReservation = bookingService.CreateReservation(demoPassenger, demoFlight, demoSeat);
bookingService.ConfirmReservation(demoReservation.ReservationId);
demoReservation     = bookingService.GetReservation(demoReservation.ReservationId)!;

// ==================================================================
//  LAB 5a – FLYWEIGHT: pool de aeroporturi partajate
// ==================================================================
PrintHeader("LAB 5a – FLYWEIGHT PATTERN");

var fw = new AirportFlyweightFactory();

// Simulam 500 de zboruri care folosesc aceleasi aeroporturi
Console.WriteLine("  Simulare 500 zboruri cu aeroporturi partajate...\n");
var rng = new Random(42);
string[] codes = { "KIV","OTP","CDG","LHR","FRA","IST","JFK","MAD","BCN","VIE" };

for (int i = 0; i < 500; i++)
{
    var orig = codes[rng.Next(codes.Length)];
    var dest = codes[rng.Next(codes.Length)];
    fw.GetAirport(orig, orig + " Airport", orig + " City", "Country");
    fw.GetAirport(dest, dest + " Airport", dest + " City", "Country");
}

fw.PrintPool();
Console.WriteLine();
fw.PrintStats();

// Demo RouteKey
Console.WriteLine("\n  RouteKey Flyweight:");
var r1 = RouteKey.Get("KIV","OTP");
var r2 = RouteKey.Get("KIV","OTP");  // refolosit
var r3 = RouteKey.Get("KIV","CDG");
Console.WriteLine($"  r1 == r2 (ReferenceEqual): {ReferenceEquals(r1, r2)}  <- partajate!");
Console.WriteLine($"  RouteKey pool size: {RouteKey.PoolSize}");

// ==================================================================
//  LAB 5b – DECORATOR: servicii extra pe bilet
// ==================================================================
PrintHeader("LAB 5b – DECORATOR PATTERN");

var demoTicket = demoReservation.Tickets.First();

// Configuratie 1: bilet simplu
Console.WriteLine("  -- Configuratie 1: Bilet de baza --");
IFlightService service1 = new BasicFlightService(demoTicket);
service1.ShowDetails();
Console.WriteLine($"  TOTAL: {service1.Price:C}\n");

// Configuratie 2: bagaj + asigurare
Console.WriteLine("  -- Configuratie 2: Bilet + Bagaj + Asigurare --");
IFlightService service2 = new BasicFlightService(demoTicket);
service2 = new BaggageDecorator(service2);
service2 = new TravelInsuranceDecorator(service2);
service2.ShowDetails();
Console.WriteLine($"  TOTAL: {service2.Price:C}\n");

// Configuratie 3: pachet complet (toate decoratorii)
Console.WriteLine("  -- Configuratie 3: Pachet Complet (toate serviciile) --");
IFlightService service3 = new BasicFlightService(demoTicket);
service3 = new BaggageDecorator(service3, 32);
service3 = new TravelInsuranceDecorator(service3);
service3 = new PriorityBoardingDecorator(service3);
service3 = new MealDecorator(service3, "Vegetarian");
service3 = new LoungeAccessDecorator(service3);
service3.ShowDetails();
Console.WriteLine($"  TOTAL: {service3.Price:C}");

// ==================================================================
//  LAB 5c – BRIDGE: tipuri de notificare x formate de mesaj
// ==================================================================
PrintHeader("LAB 5c – BRIDGE PATTERN");

Console.WriteLine("  -- Abstractizare: Confirmare | Implementor: PlainText --");
new BookingConfirmationSender(new PlainTextRenderer()).Send(demoReservation);

Console.WriteLine("  -- Abstractizare: Confirmare | Implementor: HTML --");
new BookingConfirmationSender(new HtmlRenderer()).Send(demoReservation);

Console.WriteLine("  -- Abstractizare: Reminder 24h | Implementor: Markdown --");
new FlightReminderSender(new MarkdownRenderer(), 24).Send(demoReservation);

Console.WriteLine("  -- Abstractizare: Anulare | Implementor: PlainText --");
var cancelRes = new Reservation(demoPassenger);
cancelRes.AddTicket(demoTicket);
cancelRes.Cancel();
new CancellationSender(new PlainTextRenderer()).Send(cancelRes);

// ==================================================================
//  LAB 5d – PROXY: Virtual (Cache) + Protection + Logging
// ==================================================================
PrintHeader("LAB 5d – PROXY PATTERN");

// 1. Virtual Proxy (Cache)
Console.WriteLine("  -- Virtual Proxy (Cache) --");
var cachedProxy = new CachedFlightSearchProxy(searchService, TimeSpan.FromMinutes(5));
var date = DateTime.UtcNow.AddDays(3).Date;

cachedProxy.Search("KIV","IST", date);   // miss
cachedProxy.Search("KIV","IST", date);   // hit
cachedProxy.Search("KIV","IST", date);   // hit
cachedProxy.Search("KIV","OTP", DateTime.UtcNow.AddDays(7).Date); // miss
Console.WriteLine();
cachedProxy.PrintStats();

// 2. Protection Proxy
Console.WriteLine("\n  -- Protection Proxy (Autorizare) --");

var premiumUser  = new UserContext("premium1", UserRole.PremiumUser);
var regularUser  = new UserContext("user1",    UserRole.RegisteredUser);
var guestUser    = new UserContext("guest1",   UserRole.Guest);

var premiumProxy = new AuthenticatedBookingProxy(bookingService, premiumUser);
var regularProxy = new AuthenticatedBookingProxy(bookingService, regularUser);
var guestProxy   = new AuthenticatedBookingProxy(bookingService, guestUser);

// PremiumUser poate rezerva FirstClass
try {
    var res = premiumProxy.CreateReservation(
        new Passenger("VIP","Client","vip@test.md","MD777"),
        firstFlight, "1A");
    Console.WriteLine($"  [PremiumUser] Rezervare FirstClass: OK #{res.ReservationId}");
} catch (UnauthorizedAccessException ex) { Console.WriteLine($"  [PremiumUser] BLOCAT: {ex.Message}"); }

// RegisteredUser nu poate rezerva FirstClass
try {
    regularProxy.CreateReservation(
        new Passenger("Normal","User","user@test.md","MD888"),
        firstFlight, "1B");
    Console.WriteLine("  [RegisteredUser] Rezervare FirstClass: OK");
} catch (UnauthorizedAccessException ex) { Console.WriteLine($"  [RegisteredUser] BLOCAT: {ex.Message}"); }

// Guest nu poate rezerva deloc
try {
    guestProxy.CreateReservation(
        new Passenger("Guest","User","guest@test.md","MD999"),
        econFlight, "5C");
    Console.WriteLine("  [Guest] Rezervare: OK");
} catch (UnauthorizedAccessException ex) { Console.WriteLine($"  [Guest] BLOCAT: {ex.Message}"); }

// 3. Logging Proxy
Console.WriteLine("\n  -- Logging Proxy (Monitorizare) --");
var loggingProxy = new LoggingFlightSearchProxy(searchService);
loggingProxy.Search("KIV","IST", DateTime.UtcNow.AddDays(3).Date);
loggingProxy.Search("KIV","OTP", DateTime.UtcNow.AddDays(7).Date);
loggingProxy.Search("KIV","CDG", DateTime.UtcNow.AddDays(10).Date);
Console.WriteLine();
loggingProxy.PrintSearchReport();

// ==================================================================
//  LAB 4 recap – Adapter + Composite + Facade
// ==================================================================
PrintHeader("LAB 4 – ADAPTER / COMPOSITE / FACADE (recap)");

Console.WriteLine("  [Adapter] Plata prin 3 procesatoare:");
foreach (var proc in new IPaymentProcessor[]
{
    new PayPalAdapter(new PayPalGateway()),
    new StripeAdapter(new StripeGateway()),
    new MaibAdapter(new MaibBankGateway())
})
{
    new PaymentService(proc).PayForReservation(demoReservation, demoPassenger.Email);
}

Console.WriteLine("\n  [Composite] Pachet dus-intors:");
var itin = new Itinerary("KIV <-> OTP Dus-Intors");
itin.Add(new FlightSegment(econFlight, 89m));
itin.Add(new FlightSegment(bizFlight,  79m));
itin.Display();

Console.WriteLine("  [Facade] BookAndPay intr-un apel:");
var facade = new BookingFacade(searchService, bookingService,
    new PaymentService(new StripeAdapter(new StripeGateway())),
    new TicketOutputService(new ConsoleOutputFactory()),
    new EmailNotificationFactory());
var facadeResult = facade.BookAndPay(
    new Passenger("Maria","Ionescu","maria@test.md","RO654"),
    flightRepo.GetAll().First(f => f.HasAvailableSeats() && f.Class == SeatClass.Economy),
    "maria@test.md");
Console.WriteLine($"  Rezultat: Success={facadeResult.Success} | ID={facadeResult.ReservationId}");

// ==================================================================
//  TOATE TESTELE UNITARE
// ==================================================================
FactoryTests.RunAll();
CreationalPatternsTests.RunAll();
StructuralPatternsTests.RunAll();
StructuralPatterns2Tests.RunAll();

Console.WriteLine($"\n  Total log entries: {AppLogger.Instance.EntryCount}");
AppLogger.Instance.Info("Program","Aplicatia finalizata cu succes.");
Console.WriteLine();

static void PrintHeader(string title)
{
    Console.WriteLine();
    var pad = new string('=', Math.Max(0, 50 - title.Length));
    Console.WriteLine($"  == {title} {pad}");
}
