using FlightBooking.Factories;
using FlightBooking.Factories.Notifications;
using FlightBooking.Factories.Output;
using FlightBooking.Interfaces;
using FlightBooking.Models;
using FlightBooking.Repositories;
using FlightBooking.Services;

namespace FlightBooking.Tests
{
    // ══════════════════════════════════════════════════════════════════
    //  TESTE UNITARE  –  Laborator 2
    //  Ruleaza cu: TestRunner.Run() din Program.cs
    //  (Nu necesita framework extern – teste manuale cu assert propriu)
    // ══════════════════════════════════════════════════════════════════
    public static class FactoryTests
    {
        private static int _passed = 0;
        private static int _failed = 0;

        public static void RunAll()
        {
            Console.WriteLine("\n  ══ TESTE UNITARE ════════════════════════════════");

            // ── Factory Method ──────────────────────────────────────
            Test_EmailFactory_Creates_EmailNotification();
            Test_SmsFactory_Creates_SmsNotification();
            Test_PushFactory_Creates_PushNotification();
            Test_Notification_Channel_Names();
            Test_NotificationFactory_SendsWithoutException();

            // ── Abstract Factory ────────────────────────────────────
            Test_ConsoleFactory_Creates_ConsoleFormatter();
            Test_EmailFactory_Creates_HtmlFormatter();
            Test_PdfFactory_Creates_PdfFormatter();
            Test_ConsoleFormatter_Contains_TicketId();
            Test_EmailFormatter_Contains_Html();
            Test_PdfFormatter_Contains_Pdf_Tag();
            Test_AbstractFactory_FamilyCoherence_Console();
            Test_AbstractFactory_FamilyCoherence_Email();

            Console.WriteLine($"\n  Rezultat: {_passed} ✔ trecute  |  {_failed} ✘ esuate");
            Console.WriteLine("  ══════════════════════════════════════════════════");
        }

        // ── Factory Method Tests ─────────────────────────────────────

        static void Test_EmailFactory_Creates_EmailNotification()
        {
            var factory = new EmailNotificationFactory();
            var notif   = factory.CreateNotification();
            Assert("EmailFactory creeaza EmailNotification",
                   notif is EmailNotification);
        }

        static void Test_SmsFactory_Creates_SmsNotification()
        {
            var factory = new SmsNotificationFactory();
            var notif   = factory.CreateNotification();
            Assert("SmsFactory creeaza SmsNotification",
                   notif is SmsNotification);
        }

        static void Test_PushFactory_Creates_PushNotification()
        {
            var factory = new PushNotificationFactory();
            var notif   = factory.CreateNotification();
            Assert("PushFactory creeaza PushNotification",
                   notif is PushNotification);
        }

        static void Test_Notification_Channel_Names()
        {
            Assert("EmailNotification.Channel == 'Email'",
                   new EmailNotification().Channel == "Email");
            Assert("SmsNotification.Channel == 'SMS'",
                   new SmsNotification().Channel == "SMS");
            Assert("PushNotification.Channel == 'Push'",
                   new PushNotification().Channel == "Push");
        }

        static void Test_NotificationFactory_SendsWithoutException()
        {
            var reservation = BuildDemoReservation();
            try
            {
                new EmailNotificationFactory().NotifyReservationConfirmed(reservation);
                new SmsNotificationFactory().NotifyReservationConfirmed(reservation);
                new PushNotificationFactory().NotifyReservationConfirmed(reservation);
                Assert("Toate fabricile trimit notificarea fara exceptie", true);
            }
            catch
            {
                Assert("Toate fabricile trimit notificarea fara exceptie", false);
            }
        }

        // ── Abstract Factory Tests ───────────────────────────────────

        static void Test_ConsoleFactory_Creates_ConsoleFormatter()
        {
            IOutputFactory factory = new ConsoleOutputFactory();
            var formatter = factory.CreateTicketFormatter();
            Assert("ConsoleOutputFactory creeaza ConsoleTicketFormatter",
                   formatter is ConsoleTicketFormatter);
        }

        static void Test_EmailFactory_Creates_HtmlFormatter()
        {
            IOutputFactory factory = new EmailOutputFactory();
            var formatter = factory.CreateTicketFormatter();
            Assert("EmailOutputFactory creeaza EmailTicketFormatter",
                   formatter is EmailTicketFormatter);
        }

        static void Test_PdfFactory_Creates_PdfFormatter()
        {
            IOutputFactory factory = new PdfOutputFactory();
            var formatter = factory.CreateTicketFormatter();
            Assert("PdfOutputFactory creeaza PdfTicketFormatter",
                   formatter is PdfTicketFormatter);
        }

        static void Test_ConsoleFormatter_Contains_TicketId()
        {
            var ticket    = BuildDemoTicket();
            var formatter = new ConsoleTicketFormatter();
            var output    = formatter.Format(ticket);
            Assert("ConsoleFormatter contine TicketId",
                   output.Contains(ticket.TicketId));
        }

        static void Test_EmailFormatter_Contains_Html()
        {
            var ticket    = BuildDemoTicket();
            var formatter = new EmailTicketFormatter();
            var output    = formatter.Format(ticket);
            Assert("EmailFormatter contine tag-uri HTML",
                   output.Contains("<html>") && output.Contains("</html>"));
        }

        static void Test_PdfFormatter_Contains_Pdf_Tag()
        {
            var ticket    = BuildDemoTicket();
            var formatter = new PdfTicketFormatter();
            var output    = formatter.Format(ticket);
            Assert("PdfFormatter contine prefixul [PDF]",
                   output.StartsWith("[PDF]"));
        }

        static void Test_AbstractFactory_FamilyCoherence_Console()
        {
            // O fabrica console trebuie sa produca AMBELE produse de tip console
            IOutputFactory factory   = new ConsoleOutputFactory();
            var ticket               = BuildDemoTicket();
            var formatter            = factory.CreateTicketFormatter();
            var bp                   = factory.CreateBoardingPass(ticket);
            Assert("ConsoleOutputFactory: familia coerenta (Console+Console)",
                   formatter is ConsoleTicketFormatter && bp is ConsoleBoardingPass);
        }

        static void Test_AbstractFactory_FamilyCoherence_Email()
        {
            IOutputFactory factory = new EmailOutputFactory();
            var ticket             = BuildDemoTicket();
            var formatter          = factory.CreateTicketFormatter();
            var bp                 = factory.CreateBoardingPass(ticket);
            Assert("EmailOutputFactory: familia coerenta (Email+Email)",
                   formatter is EmailTicketFormatter && bp is EmailBoardingPass);
        }

        // ── Helpers ──────────────────────────────────────────────────

        static Ticket BuildDemoTicket()
        {
            var origin  = new Airport("KIV", "Chisinau Int'l", "Chisinau", "Moldova");
            var dest    = new Airport("OTP", "Henri Coanda", "Bucuresti", "Romania");
            var flight  = new Flight("MV101", origin, dest,
                                     DateTime.UtcNow.AddDays(5),
                                     DateTime.UtcNow.AddDays(5).AddHours(2),
                                     SeatClass.Economy, 89m, 120);
            var pass    = new Passenger("Ion", "Popescu", "ion@test.md", "MD123");
            return new Ticket(flight, pass, "12A", 89m);
        }

        static Reservation BuildDemoReservation()
        {
            var ticket      = BuildDemoTicket();
            var reservation = new Reservation(ticket.Passenger);
            reservation.AddTicket(ticket);
            reservation.MarkAsPaid();
            return reservation;
        }

        static void Assert(string testName, bool condition)
        {
            if (condition)
            {
                Console.WriteLine($"  ✔  {testName}");
                _passed++;
            }
            else
            {
                Console.WriteLine($"  ✘  {testName}  ← ESUAT");
                _failed++;
            }
        }
    }
}
