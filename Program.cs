using FlightBooking.Factories;
using FlightBooking.Factories.Builder;
using FlightBooking.Interfaces;
using FlightBooking.Models;
using FlightBooking.Repositories;
using FlightBooking.Services;
using FlightBooking.Structural.Adapter;
using FlightBooking.Structural.Composite;
using FlightBooking.Structural.Facade;
using FlightBooking.Tests;
using FlightBooking.Utils;

// ===================================================================
//  FLIGHT BOOKING SYSTEM  –  Lab 1 + 2 + 3 + 4
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

// ===================================================================
//  LAB 3 – Builder + Prototype + Singleton (recap)
// ===================================================================
PrintHeader("LAB 3 – BUILDER / PROTOTYPE / SINGLETON");

var kiv = new Airport("KIV", "Chisinau Int'l",   "Chisinau", "Moldova");
var otp = new Airport("OTP", "Henri Coanda",     "Bucuresti","Romania");
var cdg = new Airport("CDG", "Charles de Gaulle","Paris",    "Franta");
var lhr = new Airport("LHR", "Heathrow",         "Londra",   "Anglia");
var jfk = new Airport("JFK", "JFK Airport",      "New York", "SUA");

var director    = new FlightDirector(new FlightBuilder());
var econFlight  = director.BuildEconomyShortHaul("DIR-EC1", kiv, lhr,
                      DateTime.UtcNow.AddDays(8).Date.AddHours(7));
var bizFlight   = director.BuildBusinessLongHaul("DIR-BZ1", kiv, jfk,
                      DateTime.UtcNow.AddDays(20).Date.AddHours(10));

flightRepo.Add(econFlight);
flightRepo.Add(bizFlight);

var templateKivOtp = new FlightTemplate(
    "MV-KIV-OTP", kiv, otp,
    new TimeOnly(8, 0), new TimeSpan(1, 30, 0),
    SeatClass.Economy, 79m, 150);

var templateKivCdg = new FlightTemplate(
    "MV-KIV-CDG", kiv, cdg,
    new TimeOnly(14, 0), new TimeSpan(3, 0, 0),
    SeatClass.Economy, 189m, 180);

var scheduler = new FlightScheduler(flightRepo);
scheduler.RegisterTemplate(templateKivOtp);
scheduler.RegisterTemplate(templateKivCdg);
var generated = scheduler.GenerateSchedule(
    DateTime.UtcNow.AddDays(1).Date,
    DateTime.UtcNow.AddDays(2).Date).ToList();

Console.WriteLine($"  Builder:    {econFlight}");
Console.WriteLine($"  Prototype:  {generated.Count} zboruri generate din {scheduler.Templates.Count} template-uri");
Console.WriteLine($"  Singleton:  Logger activ, {AppLogger.Instance.EntryCount} intrari");

// ===================================================================
//  LAB 4a – ADAPTER: plata prin 3 procesatoare diferite
// ===================================================================
PrintHeader("LAB 4a – ADAPTER PATTERN");

IPaymentProcessor[] processors =
[
    new PayPalAdapter(new PayPalGateway()),
    new StripeAdapter(new StripeGateway()),
    new MaibAdapter(new MaibBankGateway())
];

var testPassenger   = new Passenger("Ion","Popescu","ion.popescu@email.md","MD123456");
var testFlight      = flightRepo.GetAll().First(f => f.HasAvailableSeats());
var testSeat        = SeatGenerator.Generate(testFlight.TotalSeats);
var testReservation = bookingService.CreateReservation(testPassenger, testFlight, testSeat);
bookingService.ConfirmReservation(testReservation.ReservationId);
testReservation     = bookingService.GetReservation(testReservation.ReservationId)!;

Console.WriteLine($"\n  Rezervare #{testReservation.ReservationId} | Total: {testReservation.TotalPrice:C}");
Console.WriteLine("  Platim prin 3 procesatoare (acelasi cod, adaptoare diferite):\n");

foreach (var processor in processors)
{
    var paymentSvc = new PaymentService(processor);
    var txId       = paymentSvc.PayForReservation(testReservation, testPassenger.Email);
    Console.WriteLine($"  -> TxID: {txId}\n");
}

// ===================================================================
//  LAB 4b – COMPOSITE: itinerar ierarhic de zbor
// ===================================================================
PrintHeader("LAB 4b – COMPOSITE PATTERN");

var flights = flightRepo.GetAll().ToList();

// Leaf-uri individuale
var seg1 = new FlightSegment(flights[0], 89m);
var seg2 = new FlightSegment(flights[1], 79m);
var seg3 = new FlightSegment(flights[2], 189m);
var seg4 = new FlightSegment(flights[3], 175m);

// Composite nivel 1 – itinerar simplu dus-intors
var roundTrip = new Itinerary("Chisinau <-> Paris (Dus-Intors)");
roundTrip.Add(seg1);
roundTrip.Add(seg2);

// Composite nivel 1 – zbor cu escala
var withLayover = new Itinerary("Chisinau -> Londra (cu escala Bucuresti)");
withLayover.Add(seg3);
withLayover.Add(seg4);

// Composite nivel 2 – pachet vacanta (contine doua itinerare)
var vacationPackage = new TravelPackage("Pachet Vacanta Europa", discountPercent: 10m);
vacationPackage.Add(roundTrip);
vacationPackage.Add(withLayover);

Console.WriteLine("  Structura ierarhica:\n");
vacationPackage.Display();

Console.WriteLine($"  Pret fara reducere : {roundTrip.TotalPrice + withLayover.TotalPrice:C}");
Console.WriteLine($"  Pret cu 10% reducere: {vacationPackage.TotalPrice:C}");
Console.WriteLine($"  Escale totale pachet: {vacationPackage.StopCount}");

// ===================================================================
//  LAB 4c – FACADE: un singur punct de intrare
// ===================================================================
PrintHeader("LAB 4c – FACADE PATTERN");

// Construim Facade cu toate dependentele
var facadePayment = new PaymentService(new StripeAdapter(new StripeGateway()));
var facade = new BookingFacade(
    searchService,
    bookingService,
    facadePayment,
    new TicketOutputService(new ConsoleOutputFactory()),
    new EmailNotificationFactory()
);

// Clientul apeleaza 3 metode simple, nu stie de subsisteme
Console.WriteLine("  -- Cautare prin Facade --");
var facadeFlights = facade.SearchFlights("KIV", "IST",
    DateTime.UtcNow.AddDays(3).Date).ToList();
Console.WriteLine($"  Zboruri gasite: {facadeFlights.Count}");

Console.WriteLine("\n  -- BookAndPay prin Facade (5 pasi intr-un singur apel) --");
var passenger2 = new Passenger("Maria","Ionescu","maria@test.md","RO654321");
var result     = facade.BookAndPay(passenger2, facadeFlights.First(), "maria@test.md");
Console.WriteLine($"\n  Rezultat: Success={result.Success} | " +
                  $"ReservationId={result.ReservationId} | TxId={result.TransactionId}");

Console.WriteLine("\n  -- CancelBooking prin Facade (3 pasi intr-un singur apel) --");
var cancelled = facade.CancelBooking(result.ReservationId!, result.TransactionId!);
Console.WriteLine($"  Anulat cu succes: {cancelled}");

// ===================================================================
//  TESTE UNITARE – Lab 2 + 3 + 4
// ===================================================================
FactoryTests.RunAll();
CreationalPatternsTests.RunAll();
StructuralPatternsTests.RunAll();

AppLogger.Instance.Info("Program", $"Finalizat. Total log entries: {AppLogger.Instance.EntryCount}");
Console.WriteLine();

static void PrintHeader(string title)
{
    Console.WriteLine();
    var pad = new string('=', Math.Max(0, 48 - title.Length));
    Console.WriteLine($"  == {title} {pad}");
}
