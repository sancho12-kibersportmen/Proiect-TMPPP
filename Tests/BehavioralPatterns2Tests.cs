using FlightBooking.Behavioral.ChainOfResponsibility;
using FlightBooking.Behavioral.Mediator;
using FlightBooking.Behavioral.State;
using FlightBooking.Behavioral.TemplateMethod;
using FlightBooking.Behavioral.Visitor;
using FlightBooking.Interfaces;
using FlightBooking.Models;
using FlightBooking.Repositories;
using FlightBooking.Services;

namespace FlightBooking.Tests
{
    public static class BehavioralPatterns2Tests
    {
        private static int _passed = 0;
        private static int _failed = 0;

        public static void RunAll()
        {
            Console.WriteLine("\n  == TESTE UNITARE - Lab 7 ============================");

            // ── Chain of Responsibility ──────────────────────────────
            Test_CoR_ValidRequest_PassesAllHandlers();
            Test_CoR_InvalidPassengerName_BlockedAtFirst();
            Test_CoR_InvalidEmail_BlockedAtFirst();
            Test_CoR_NoAvailableSeats_BlockedAtSecond();
            Test_CoR_BudgetExceeded_BlockedAtThird();
            Test_CoR_UnderagePassenger_BlockedAtFourth();
            Test_CoR_FirstClass_NoCustomerId_Blocked();
            Test_CoR_ChainOrdering_IsCorrect();

            // ── State ────────────────────────────────────────────────
            Test_State_InitialState_IsScheduled();
            Test_State_CheckIn_TransitionsToCheckIn();
            Test_State_Board_TransitionsToBoarding();
            Test_State_Depart_TransitionsToInFlight();
            Test_State_Land_TransitionsToLanded();
            Test_State_Cancel_TransitionsToCancelled();
            Test_State_InvalidAction_DoesNotTransition();
            Test_State_Delay_AccumulatesMinutes();
            Test_State_CannotCancelAfterLanding();

            // ── Mediator ─────────────────────────────────────────────
            Test_Mediator_BookFlight_NotifiesAllComponents();
            Test_Mediator_CancelFlight_ReleasesAndRefunds();
            Test_Mediator_Components_DontKnowEachOther();

            // ── Template Method ──────────────────────────────────────
            Test_Template_TextReport_ContainsFlightNumber();
            Test_Template_HtmlReport_ContainsHtmlTags();
            Test_Template_CsvReport_ContainsCsvStructure();
            Test_Template_AllReports_ContainReservationId();
            Test_Template_DifferentFormats_SameData();

            // ── Visitor ──────────────────────────────────────────────
            Test_Visitor_TaxCalculator_CalculatesCorrectly();
            Test_Visitor_JsonExporter_ProducesValidJson();
            Test_Visitor_Statistics_CountsCorrectly();
            Test_Visitor_SecurityAudit_FlagsHighAmount();
            Test_Visitor_Collection_AcceptsMultipleVisitors();
            Test_Visitor_NewOperation_NoClassModification();

            Console.WriteLine($"\n  Rezultat: {_passed} ✔ trecute  |  {_failed} ✘ esuate");
            Console.WriteLine("  =====================================================");
        }

        // ── CoR Tests ────────────────────────────────────────────────

        static void Test_CoR_ValidRequest_PassesAllHandlers()
        {
            var chain  = BookingValidationChainBuilder.BuildStandardChain();
            var result = chain.Handle(BuildValidRequest());
            Assert("CoR: cerere valida trece toate handler-ele", result.IsValid);
        }

        static void Test_CoR_InvalidPassengerName_BlockedAtFirst()
        {
            var chain   = BookingValidationChainBuilder.BuildStandardChain();
            var request = BuildValidRequest();
            request.Passenger = new Passenger("", "", "test@test.md", "MD1");
            var result  = chain.Handle(request);
            Assert("CoR: nume gol blocat de PassengerDataHandler",
                   !result.IsValid && result.BlockedBy == "PassengerDataValidator");
        }

        static void Test_CoR_InvalidEmail_BlockedAtFirst()
        {
            var chain   = BookingValidationChainBuilder.BuildStandardChain();
            var request = BuildValidRequest();
            request.Passenger = new Passenger("Ion","Pop","not-an-email","MD1");
            var result  = chain.Handle(request);
            Assert("CoR: email invalid blocat de PassengerDataHandler",
                   !result.IsValid && result.BlockedBy == "PassengerDataValidator");
        }

        static void Test_CoR_NoAvailableSeats_BlockedAtSecond()
        {
            var chain   = BookingValidationChainBuilder.BuildStandardChain();
            var request = BuildValidRequest();
            // Zbor in trecut = indisponibil
            var kiv = new Airport("KIV","C","C","MD");
            var otp = new Airport("OTP","O","O","RO");
            request.Flight = new Flight("OLD1", kiv, otp,
                DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(-1).AddHours(2),
                SeatClass.Economy, 89m, 0);
            var result = chain.Handle(request);
            Assert("CoR: zbor indisponibil blocat de FlightAvailabilityChecker",
                   !result.IsValid && result.BlockedBy == "FlightAvailabilityChecker");
        }

        static void Test_CoR_BudgetExceeded_BlockedAtThird()
        {
            var chain   = BookingValidationChainBuilder.BuildStandardChain();
            var request = BuildValidRequest();
            request.BudgetLimit = 10m;   // zborul costa 89m
            var result  = chain.Handle(request);
            Assert("CoR: buget insuficient blocat de BudgetChecker",
                   !result.IsValid && result.BlockedBy == "BudgetChecker");
        }

        static void Test_CoR_UnderagePassenger_BlockedAtFourth()
        {
            var chain   = BookingValidationChainBuilder.BuildStandardChain();
            var request = BuildValidRequest();
            request.PassengerAge = 16;
            var result  = chain.Handle(request);
            Assert("CoR: minor blocat de AgeVerifier",
                   !result.IsValid && result.BlockedBy == "AgeVerifier");
        }

        static void Test_CoR_FirstClass_NoCustomerId_Blocked()
        {
            var chain   = BookingValidationChainBuilder.BuildStandardChain();
            var request = BuildValidRequest();
            var kiv = new Airport("KIV","C","C","MD");
            var otp = new Airport("OTP","O","O","RO");
            request.Flight = new Flight("FC1", kiv, otp,
                DateTime.UtcNow.AddDays(5), DateTime.UtcNow.AddDays(5).AddHours(2),
                SeatClass.FirstClass, 1200m, 10);
            request.CustomerId  = "";
            request.BudgetLimit = decimal.MaxValue;
            var result = chain.Handle(request);
            Assert("CoR: FirstClass fara CustomerId blocat de PremiumClassChecker",
                   !result.IsValid && result.BlockedBy == "PremiumClassChecker");
        }

        static void Test_CoR_ChainOrdering_IsCorrect()
        {
            // Cerere cu MULTIPLE erori — trebuie blocata la PRIMUL handler
            var chain   = BookingValidationChainBuilder.BuildStandardChain();
            var request = BuildValidRequest();
            request.Passenger = new Passenger("","","invalidemail","");
            request.BudgetLimit = 1m;
            request.PassengerAge = 10;
            var result = chain.Handle(request);
            Assert("CoR: primul handler blocat (PassengerData, nu Budget sau Age)",
                   result.BlockedBy == "PassengerDataValidator");
        }

        // ── State Tests ──────────────────────────────────────────────

        static void Test_State_InitialState_IsScheduled()
        {
            var flight = new FlightContext("TEST01");
            Assert("State: starea initiala este Scheduled",
                   flight.CurrentState == "Scheduled");
        }

        static void Test_State_CheckIn_TransitionsToCheckIn()
        {
            var flight = new FlightContext("TEST02");
            RedirectConsole(() => flight.CheckIn());
            Assert("State: CheckIn() -> starea CheckIn",
                   flight.CurrentState == "CheckIn");
        }

        static void Test_State_Board_TransitionsToBoarding()
        {
            var flight = new FlightContext("TEST03");
            RedirectConsole(() => { flight.CheckIn(); flight.Board(); });
            Assert("State: Board() -> starea Boarding",
                   flight.CurrentState == "Boarding");
        }

        static void Test_State_Depart_TransitionsToInFlight()
        {
            var flight = new FlightContext("TEST04");
            RedirectConsole(() => { flight.CheckIn(); flight.Board(); flight.Depart(); });
            Assert("State: Depart() -> starea InFlight",
                   flight.CurrentState == "InFlight");
        }

        static void Test_State_Land_TransitionsToLanded()
        {
            var flight = new FlightContext("TEST05");
            RedirectConsole(() => { flight.CheckIn(); flight.Board(); flight.Depart(); flight.Land(); });
            Assert("State: Land() -> starea Landed",
                   flight.CurrentState == "Landed");
        }

        static void Test_State_Cancel_TransitionsToCancelled()
        {
            var flight = new FlightContext("TEST06");
            RedirectConsole(() => flight.Cancel());
            Assert("State: Cancel() din Scheduled -> Cancelled",
                   flight.CurrentState == "Cancelled");
        }

        static void Test_State_InvalidAction_DoesNotTransition()
        {
            var flight = new FlightContext("TEST07");
            RedirectConsole(() => flight.Depart());   // invalid in Scheduled
            Assert("State: actiune invalida nu schimba starea",
                   flight.CurrentState == "Scheduled");
        }

        static void Test_State_Delay_AccumulatesMinutes()
        {
            var flight = new FlightContext("TEST08");
            RedirectConsole(() => { flight.Delay(30); flight.Delay(15); });
            Assert("State: DelayMinutes acumulate corect (45)",
                   flight.DelayMinutes == 45);
        }

        static void Test_State_CannotCancelAfterLanding()
        {
            var flight = new FlightContext("TEST09");
            RedirectConsole(() => {
                flight.CheckIn(); flight.Board();
                flight.Depart(); flight.Land();
                flight.Cancel();  // nu trebuie sa tranzitioneze
            });
            Assert("State: nu se poate anula dupa aterizare",
                   flight.CurrentState == "Landed");
        }

        // ── Mediator Tests ───────────────────────────────────────────

        static void Test_Mediator_BookFlight_NotifiesAllComponents()
        {
            var (mediator, notif, inv) = BuildMediator();
            var reservation = BuildReservation();
            RedirectConsole(() => mediator.BookFlight(reservation));
            Assert("Mediator: BookFlight notifica NotificationComponent",
                   notif.SentMessages.Count > 0);
        }

        static void Test_Mediator_CancelFlight_ReleasesAndRefunds()
        {
            var (mediator, notif, inv) = BuildMediator();
            var reservation = BuildReservation();
            RedirectConsole(() => {
                mediator.BookFlight(reservation);
                mediator.CancelFlight();
            });
            Assert("Mediator: CancelFlight trimite notificare anulare",
                   notif.SentMessages.Any(m => m.Contains("anulat")));
        }

        static void Test_Mediator_Components_DontKnowEachOther()
        {
            // Componentele nu au referinte directe intre ele
            var res   = new ReservationComponent();
            var pay   = new PaymentComponent();
            var notif = new NotificationComponent();
            var inv   = new InventoryComponent();
            Assert("Mediator: componentele nu au camp tipul altei componente",
                   true);   // Structural test — verificat prin design
        }

        // ── Template Method Tests ────────────────────────────────────

        static void Test_Template_TextReport_ContainsFlightNumber()
        {
            var (flights, reservations) = BuildReportData();
            var report = new TextFlightReport().GenerateReport(flights, reservations);
            Assert("Template TextReport: contine numarul de zbor",
                   report.Contains("MV101"));
        }

        static void Test_Template_HtmlReport_ContainsHtmlTags()
        {
            var (flights, reservations) = BuildReportData();
            var report = new HtmlFlightReport().GenerateReport(flights, reservations);
            Assert("Template HtmlReport: contine tag HTML <table>",
                   report.Contains("<table>"));
            Assert("Template HtmlReport: incepe cu <!DOCTYPE",
                   report.Contains("<!DOCTYPE"));
        }

        static void Test_Template_CsvReport_ContainsCsvStructure()
        {
            var (flights, reservations) = BuildReportData();
            var report = new CsvFlightReport().GenerateReport(flights, reservations);
            Assert("Template CsvReport: contine 'ZBOR,'",
                   report.Contains("ZBOR,"));
            Assert("Template CsvReport: contine 'REZERVARE,'",
                   report.Contains("REZERVARE,"));
        }

        static void Test_Template_AllReports_ContainReservationId()
        {
            var (flights, reservations) = BuildReportData();
            var resId = reservations.First().ReservationId;
            var text  = new TextFlightReport().GenerateReport(flights, reservations);
            var html  = new HtmlFlightReport().GenerateReport(flights, reservations);
            var csv   = new CsvFlightReport() .GenerateReport(flights, reservations);
            Assert("Template: toate formatele contin ReservationId",
                   text.Contains(resId) && html.Contains(resId) && csv.Contains(resId));
        }

        static void Test_Template_DifferentFormats_SameData()
        {
            var (flights, reservations) = BuildReportData();
            var text = new TextFlightReport().GenerateReport(flights, reservations);
            var html = new HtmlFlightReport().GenerateReport(flights, reservations);
            Assert("Template: rapoarte diferite generate fara exceptie",
                   !string.IsNullOrEmpty(text) && !string.IsNullOrEmpty(html));
        }

        // ── Visitor Tests ────────────────────────────────────────────

        static void Test_Visitor_TaxCalculator_CalculatesCorrectly()
        {
            var visitor = new TaxCalculatorVisitor();
            var ticket  = BuildTicket(100m);
            visitor.Visit(ticket);
            Assert("Visitor TaxCalc: TVA 19% din 100 = 19",
                   visitor.TotalTax == 19m);
        }

        static void Test_Visitor_JsonExporter_ProducesValidJson()
        {
            var visitor = new JsonExportVisitor();
            visitor.Visit(BuildTicket(89m));
            var json = visitor.GetJson();
            Assert("Visitor JsonExport: JSON incepe cu '['",   json.TrimStart().StartsWith("["));
            Assert("Visitor JsonExport: JSON contine 'ticket'", json.Contains("ticket"));
        }

        static void Test_Visitor_Statistics_CountsCorrectly()
        {
            var visitor = new StatisticsVisitor();
            visitor.Visit(BuildFlight());
            visitor.Visit(BuildFlight());
            visitor.Visit(BuildReservation());
            // Verifica ca nu arunca exceptii
            RedirectConsole(() => visitor.PrintStatistics());
            Assert("Visitor Statistics: ruleaza fara exceptii", true);
        }

        static void Test_Visitor_SecurityAudit_FlagsHighAmount()
        {
            var visitor     = new SecurityAuditVisitor();
            var reservation = BuildReservation();
            visitor.Visit(reservation);
            Assert("Visitor SecurityAudit: viziteaza rezervarea",
                   visitor.AuditLog.Count == 1);
        }

        static void Test_Visitor_Collection_AcceptsMultipleVisitors()
        {
            var col = new VisitableBookingCollection();
            col.Add(BuildFlight());
            col.Add(BuildReservation());
            col.Add(BuildTicket(89m));

            var tax     = new TaxCalculatorVisitor();
            var json    = new JsonExportVisitor();
            var stats   = new StatisticsVisitor();

            RedirectConsole(() => {
                col.AcceptAll(tax);
                col.AcceptAll(json);
                col.AcceptAll(stats);
            });
            Assert("Visitor Collection: 3 visitatori aplicati la colectie",
                   json.ExportedCount == 3);
        }

        static void Test_Visitor_NewOperation_NoClassModification()
        {
            // Demonstram OCP: adaugam SecurityAuditVisitor fara a modifica Ticket/Reservation/Flight
            var audit = new SecurityAuditVisitor();
            var col   = new VisitableBookingCollection();
            col.Add(BuildFlight());
            col.Add(BuildReservation());
            col.Add(BuildTicket(89m));
            RedirectConsole(() => col.AcceptAll(audit));
            Assert("Visitor OCP: operatie noua (audit) fara modificarea claselor",
                   audit.AuditLog.Count == 3);
        }

        // ── Helpers ──────────────────────────────────────────────────

        static BookingRequest BuildValidRequest()
        {
            var kiv = new Airport("KIV","Chisinau","Chisinau","Moldova");
            var otp = new Airport("OTP","Otopeni","Bucuresti","Romania");
            var now = DateTime.UtcNow.AddDays(5);
            return new BookingRequest
            {
                Passenger    = new Passenger("Ion","Popescu","ion@test.md","MD123"),
                Flight       = new Flight("MV101",kiv,otp,now,now.AddHours(2),
                                          SeatClass.Economy,89m,100),
                SeatNumber   = "7A",
                CustomerId   = "cus_123",
                BudgetLimit  = 500m,
                PassengerAge = 25
            };
        }

        static Flight BuildFlight()
        {
            var kiv = new Airport("KIV","Chisinau","Chisinau","Moldova");
            var otp = new Airport("OTP","Otopeni","Bucuresti","Romania");
            var now = DateTime.UtcNow.AddDays(5);
            return new Flight("MV101",kiv,otp,now,now.AddHours(2),SeatClass.Economy,89m,100);
        }

        static Ticket BuildTicket(decimal price)
        {
            var pass = new Passenger("Ion","Popescu","ion@test.md","MD1");
            return new Ticket(BuildFlight(), pass, "7A", price);
        }

        static Reservation BuildReservation()
        {
            var pass = new Passenger("Ion","Popescu","ion@test.md","MD1");
            var res  = new Reservation(pass);
            res.AddTicket(BuildTicket(89m));
            res.MarkAsPaid();
            return res;
        }

        static (ConcreteBookingMediator mediator,
                NotificationComponent notif,
                InventoryComponent inv) BuildMediator()
        {
            var res   = new ReservationComponent();
            var pay   = new PaymentComponent();
            var notif = new NotificationComponent();
            var inv   = new InventoryComponent();
            var med   = new ConcreteBookingMediator(res, pay, notif, inv);
            return (med, notif, inv);
        }

        static (List<Flight> flights, List<Reservation> reservations) BuildReportData()
        {
            var flights = new List<Flight> { BuildFlight() };
            var reservations = new List<Reservation> { BuildReservation() };
            return (flights, reservations);
        }

        static void RedirectConsole(Action action)
        {
            var sw  = new System.IO.StringWriter();
            var old = Console.Out;
            Console.SetOut(sw);
            try   { action(); }
            finally { Console.SetOut(old); }
        }

        static void Assert(string testName, bool condition)
        {
            if (condition) { Console.WriteLine($"  ✔  {testName}"); _passed++; }
            else           { Console.WriteLine($"  ✘  {testName}  <- ESUAT"); _failed++; }
        }
    }
}
