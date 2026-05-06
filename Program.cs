using FlightBooking.Factories;
using FlightBooking.Factories.Builder;
using FlightBooking.Interfaces;
using FlightBooking.Models;
using FlightBooking.Repositories;
using FlightBooking.Services;
using FlightBooking.Tests;
using FlightBooking.Utils;

// ===================================================================
//  FLIGHT BOOKING SYSTEM  –  Lab 1 + 2 + 3
//  OOP + SOLID | Factory Method + Abstract Factory | Builder + Prototype + Singleton
// ===================================================================

Console.OutputEncoding = System.Text.Encoding.UTF8;

// == Singleton: primul acces initializeaza logger-ul ================
AppLogger.Instance.Info("Program", "Aplicatia pornita.");

// == DIP: Composition Root ==========================================
IFlightRepository      flightRepo      = new InMemoryFlightRepository();
IReservationRepository reservationRepo = new InMemoryReservationRepository();
IPricingStrategy       pricing         = new EarlyBirdPricingStrategy(new StandardPricingStrategy());

var searchService  = new FlightSearchService(flightRepo);
var bookingService = new BookingService(reservationRepo, pricing);

DataSeeder.Seed(flightRepo);

// ===================================================================
//  LAB 3a – BUILDER: construire fluenta a zborurilor
// ===================================================================
PrintHeader("LAB 3a – BUILDER PATTERN");

// 1. Fluent API direct
var kiv = new Airport("KIV", "Chisinau Int'l",    "Chisinau",  "Moldova");
var cdg = new Airport("CDG", "Charles de Gaulle",  "Paris",     "Franta");
var lhr = new Airport("LHR", "Heathrow",           "Londra",    "Anglia");
var jfk = new Airport("JFK", "John F. Kennedy",    "New York",  "SUA");

var manualFlight = new FlightBuilder()
    .WithFlightNumber("MV999")
    .WithOrigin(kiv)
    .WithDestination(cdg)
    .WithDeparture(DateTime.UtcNow.AddDays(15).Date.AddHours(9))
    .WithArrival(DateTime.UtcNow.AddDays(15).Date.AddHours(12))
    .WithClass(SeatClass.Business)
    .WithBasePrice(380m)
    .WithSeats(40)
    .Build();

Console.WriteLine($"  [Fluent]   {manualFlight}");
flightRepo.Add(manualFlight);

// 2. Director — retete predefinite
var director = new FlightDirector(new FlightBuilder());

var econFlight  = director.BuildEconomyShortHaul("DIR-EC1", kiv, lhr,
                      DateTime.UtcNow.AddDays(8).Date.AddHours(7));
var bizFlight   = director.BuildBusinessLongHaul("DIR-BZ1", kiv, jfk,
                      DateTime.UtcNow.AddDays(20).Date.AddHours(10));
var firstFlight = director.BuildFirstClassIntercontinental("DIR-FC1", kiv, jfk,
                      DateTime.UtcNow.AddDays(25).Date.AddHours(11));

Console.WriteLine($"  [Director Economy]   {econFlight}");
Console.WriteLine($"  [Director Business]  {bizFlight}");
Console.WriteLine($"  [Director FirstCls]  {firstFlight}");

flightRepo.Add(econFlight);
flightRepo.Add(bizFlight);
flightRepo.Add(firstFlight);

// ===================================================================
//  LAB 3b – PROTOTYPE: generare program de zboruri regulate
// ===================================================================
PrintHeader("LAB 3b – PROTOTYPE PATTERN");

var otp = new Airport("OTP", "Henri Coanda", "Bucuresti", "Romania");

// Definim template-uri (prototipuri)
var templateKivOtp = new FlightTemplate(
    "MV-KIV-OTP", kiv, otp,
    new TimeOnly(8, 0), new TimeSpan(1, 30, 0),
    SeatClass.Economy, 79m, 150);

var templateKivCdg = new FlightTemplate(
    "MV-KIV-CDG", kiv, cdg,
    new TimeOnly(14, 0), new TimeSpan(3, 0, 0),
    SeatClass.Economy, 189m, 180);

Console.WriteLine($"  Template 1: {templateKivOtp}");
Console.WriteLine($"  Template 2: {templateKivCdg}");

// Clonam si generam zboruri pentru urmatoarele 3 zile
var scheduler = new FlightScheduler(flightRepo);
scheduler.RegisterTemplate(templateKivOtp);
scheduler.RegisterTemplate(templateKivCdg);

var from      = DateTime.UtcNow.AddDays(1).Date;
var to        = DateTime.UtcNow.AddDays(3).Date;
var generated = scheduler.GenerateSchedule(from, to).ToList();

Console.WriteLine($"\n  Zboruri generate (Prototype x {generated.Count}):");
foreach (var f in generated.Take(4))
    Console.WriteLine($"    {f}");
if (generated.Count > 4)
    Console.WriteLine($"    ... si alte {generated.Count - 4} zboruri.");

// Demo shallow vs deep copy
Console.WriteLine("\n  -- Shallow vs Deep Copy --");
var shallow = templateKivOtp.ShallowCopy();
var deep    = templateKivOtp.DeepCopy();
Console.WriteLine($"  Original  Airport ref: {templateKivOtp.Origin.GetHashCode()}");
Console.WriteLine($"  Shallow   Airport ref: {shallow.Origin.GetHashCode()}  " +
                  $"(same={ReferenceEquals(templateKivOtp.Origin, shallow.Origin)})");
Console.WriteLine($"  Deep      Airport ref: {deep.Origin.GetHashCode()}  " +
                  $"(same={ReferenceEquals(templateKivOtp.Origin, deep.Origin)})");

// ===================================================================
//  LAB 3c – SINGLETON: demonstratie logger
// ===================================================================
PrintHeader("LAB 3c – SINGLETON PATTERN (AppLogger)");

// Simulam mai multi componenti care acceseaza acelasi logger
var pass1      = new Passenger("Ion",  "Popescu", "ion@test.md",   "MD111");
var pass2      = new Passenger("Maria","Ionescu", "maria@test.md", "MD222");

var res1 = bookingService.CreateReservation(pass1, econFlight,  "12A");
var res2 = bookingService.CreateReservation(pass2, firstFlight, "1A");

bookingService.ConfirmReservation(res1.ReservationId);
bookingService.CancelReservation(res2.ReservationId);

// Dovada ca e un singur logger (aceeasi instanta)
var logger1 = AppLogger.Instance;
var logger2 = AppLogger.Instance;
Console.WriteLine($"\n  logger1 == logger2? {ReferenceEquals(logger1, logger2)}   <- SINGLETON confirmat");
Console.WriteLine($"  Total intrari in log: {AppLogger.Instance.EntryCount}");

var warnings = AppLogger.Instance.GetByLevel(LogLevel.Warning).ToList();
Console.WriteLine($"  Warning-uri: {warnings.Count}");

// ===================================================================
//  LAB 2 – Factory Method + Abstract Factory (recap)
// ===================================================================
PrintHeader("LAB 2 – FACTORY METHOD + ABSTRACT FACTORY (recap)");

NotificationFactory[] notifFactories = [
    new EmailNotificationFactory(),
    new SmsNotificationFactory(),
    new PushNotificationFactory()
];
foreach (var f in notifFactories)
{
    f.NotifyReservationConfirmed(res1);
}

var ticket = res1.Tickets.First();
var outputSvc = new TicketOutputService(new ConsoleOutputFactory());
outputSvc.DisplayTicket(ticket);
outputSvc.PrintBoardingPass(ticket);

// ===================================================================
//  TESTE UNITARE – Lab 2 + Lab 3
// ===================================================================
FactoryTests.RunAll();
CreationalPatternsTests.RunAll();

AppLogger.Instance.Info("Program", "Aplicatia incheiata cu succes.");
Console.WriteLine();

static void PrintHeader(string title)
{
    Console.WriteLine();
    var pad = new string('=', Math.Max(0, 46 - title.Length));
    Console.WriteLine($"  == {title} {pad}");
}
