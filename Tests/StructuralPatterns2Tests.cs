using FlightBooking.Interfaces;
using FlightBooking.Models;
using FlightBooking.Repositories;
using FlightBooking.Services;
using FlightBooking.Structural.Bridge;
using FlightBooking.Structural.Decorator;
using FlightBooking.Structural.Flyweight;
using FlightBooking.Structural.Proxy;

namespace FlightBooking.Tests
{
    public static class StructuralPatterns2Tests
    {
        private static int _passed = 0;
        private static int _failed = 0;

        public static void RunAll()
        {
            Console.WriteLine("\n  == TESTE UNITARE - Lab 5 ============================");

            // ── Flyweight ────────────────────────────────────────────
            Test_Flyweight_SameCode_ReturnsSameInstance();
            Test_Flyweight_DifferentCodes_ReturnsDifferentInstances();
            Test_Flyweight_PoolSize_Correct();
            Test_Flyweight_ReusedCount_Correct();
            Test_RouteKey_SameRoute_SameInstance();
            Test_RouteKey_DifferentRoutes_DifferentInstances();

            // ── Decorator ────────────────────────────────────────────
            Test_BasicService_Price_Equals_TicketPrice();
            Test_BaggageDecorator_Adds_35();
            Test_InsuranceDecorator_Adds_18();
            Test_PriorityDecorator_Adds_12();
            Test_MealDecorator_Adds_15();
            Test_LoungeDecorator_Adds_45();
            Test_Decorator_Stacking_PriceCorrect();
            Test_Decorator_Description_Contains_AllServices();

            // ── Bridge ───────────────────────────────────────────────
            Test_PlainTextRenderer_RenderTitle_Format();
            Test_HtmlRenderer_RenderTitle_ContainsH2();
            Test_MarkdownRenderer_RenderTitle_ContainsHash();
            Test_ConfirmationSender_With_PlainText_NoException();
            Test_ConfirmationSender_With_Html_NoException();
            Test_ReminderSender_With_Markdown_NoException();
            Test_Bridge_3x3_Combinations();

            // ── Proxy ────────────────────────────────────────────────
            Test_CachedProxy_FirstCall_IsMiss();
            Test_CachedProxy_SecondCall_IsHit();
            Test_CachedProxy_HitCount_Increments();
            Test_AuthProxy_Guest_ThrowsUnauthorized();
            Test_AuthProxy_RegisteredUser_CanBook_Economy();
            Test_AuthProxy_RegisteredUser_CannotBook_FirstClass();
            Test_AuthProxy_PremiumUser_CanBook_FirstClass();
            Test_AuthProxy_InactiveUser_ThrowsUnauthorized();
            Test_LoggingProxy_LogsSearches();

            Console.WriteLine($"\n  Rezultat: {_passed} ✔ trecute  |  {_failed} ✘ esuate");
            Console.WriteLine("  =====================================================");
        }

        // ── Flyweight Tests ──────────────────────────────────────────

        static void Test_Flyweight_SameCode_ReturnsSameInstance()
        {
            var factory = new AirportFlyweightFactory();
            var a1 = factory.GetAirport("KIV", "Chisinau", "Chisinau", "Moldova");
            var a2 = factory.GetAirport("KIV", "Chisinau", "Chisinau", "Moldova");
            Assert("Flyweight: acelasi cod -> aceeasi instanta (ReferenceEqual)",
                   ReferenceEquals(a1, a2));
        }

        static void Test_Flyweight_DifferentCodes_ReturnsDifferentInstances()
        {
            var factory = new AirportFlyweightFactory();
            var kiv = factory.GetAirport("KIV", "Chisinau", "Chisinau", "Moldova");
            var otp = factory.GetAirport("OTP", "Otopeni",  "Bucuresti","Romania");
            Assert("Flyweight: coduri diferite -> instante diferite",
                   !ReferenceEquals(kiv, otp));
        }

        static void Test_Flyweight_PoolSize_Correct()
        {
            var factory = new AirportFlyweightFactory();
            factory.GetAirport("KIV","Chisinau","Chisinau","Moldova");
            factory.GetAirport("OTP","Otopeni","Bucuresti","Romania");
            factory.GetAirport("KIV","Chisinau","Chisinau","Moldova"); // duplicat
            Assert("Flyweight: PoolSize = 2 (nu 3)", factory.PoolSize == 2);
        }

        static void Test_Flyweight_ReusedCount_Correct()
        {
            var factory = new AirportFlyweightFactory();
            factory.GetAirport("LHR","Heathrow","Londra","Anglia");
            factory.GetAirport("LHR","Heathrow","Londra","Anglia");
            factory.GetAirport("LHR","Heathrow","Londra","Anglia");
            Assert("Flyweight: ReusedCount = 2", factory.ReusedCount == 2);
        }

        static void Test_RouteKey_SameRoute_SameInstance()
        {
            var r1 = RouteKey.Get("KIV", "OTP");
            var r2 = RouteKey.Get("KIV", "OTP");
            Assert("RouteKey: aceeasi ruta -> aceeasi instanta", ReferenceEquals(r1, r2));
        }

        static void Test_RouteKey_DifferentRoutes_DifferentInstances()
        {
            var r1 = RouteKey.Get("KIV", "CDG");
            var r2 = RouteKey.Get("KIV", "LHR");
            Assert("RouteKey: rute diferite -> instante diferite", !ReferenceEquals(r1, r2));
        }

        // ── Decorator Tests ──────────────────────────────────────────

        static void Test_BasicService_Price_Equals_TicketPrice()
        {
            var svc = new BasicFlightService(BuildTicket(100m));
            Assert("BasicFlightService.Price == 100", svc.Price == 100m);
        }

        static void Test_BaggageDecorator_Adds_35()
        {
            IFlightService svc = new BasicFlightService(BuildTicket(100m));
            svc = new BaggageDecorator(svc);
            Assert("BaggageDecorator adauga 35", svc.Price == 135m);
        }

        static void Test_InsuranceDecorator_Adds_18()
        {
            IFlightService svc = new BasicFlightService(BuildTicket(100m));
            svc = new TravelInsuranceDecorator(svc);
            Assert("InsuranceDecorator adauga 18", svc.Price == 118m);
        }

        static void Test_PriorityDecorator_Adds_12()
        {
            IFlightService svc = new BasicFlightService(BuildTicket(100m));
            svc = new PriorityBoardingDecorator(svc);
            Assert("PriorityDecorator adauga 12", svc.Price == 112m);
        }

        static void Test_MealDecorator_Adds_15()
        {
            IFlightService svc = new BasicFlightService(BuildTicket(100m));
            svc = new MealDecorator(svc);
            Assert("MealDecorator adauga 15", svc.Price == 115m);
        }

        static void Test_LoungeDecorator_Adds_45()
        {
            IFlightService svc = new BasicFlightService(BuildTicket(100m));
            svc = new LoungeAccessDecorator(svc);
            Assert("LoungeDecorator adauga 45", svc.Price == 145m);
        }

        static void Test_Decorator_Stacking_PriceCorrect()
        {
            IFlightService svc = new BasicFlightService(BuildTicket(100m));
            svc = new BaggageDecorator(svc);           // +35 = 135
            svc = new TravelInsuranceDecorator(svc);   // +18 = 153
            svc = new PriorityBoardingDecorator(svc);  // +12 = 165
            svc = new MealDecorator(svc);              // +15 = 180
            Assert("Decorator stacking: 100+35+18+12+15 = 180", svc.Price == 180m);
        }

        static void Test_Decorator_Description_Contains_AllServices()
        {
            IFlightService svc = new BasicFlightService(BuildTicket(100m));
            svc = new BaggageDecorator(svc);
            svc = new TravelInsuranceDecorator(svc);
            var desc = svc.GetDescription();
            Assert("Decorator: description contine 'Bagaj'",      desc.Contains("Bagaj"));
            Assert("Decorator: description contine 'Asigurare'",  desc.Contains("Asigurare"));
        }

        // ── Bridge Tests ─────────────────────────────────────────────

        static void Test_PlainTextRenderer_RenderTitle_Format()
        {
            var r = new PlainTextRenderer();
            Assert("PlainText RenderTitle contine '==='",
                   r.RenderTitle("Test").Contains("==="));
        }

        static void Test_HtmlRenderer_RenderTitle_ContainsH2()
        {
            var r = new HtmlRenderer();
            Assert("Html RenderTitle contine '<h2'",
                   r.RenderTitle("Test").Contains("<h2"));
        }

        static void Test_MarkdownRenderer_RenderTitle_ContainsHash()
        {
            var r = new MarkdownRenderer();
            Assert("Markdown RenderTitle contine '##'",
                   r.RenderTitle("Test").Contains("##"));
        }

        static void Test_ConfirmationSender_With_PlainText_NoException()
        {
            var res = BuildReservation();
            try
            {
                RedirectConsole(() =>
                    new BookingConfirmationSender(new PlainTextRenderer()).Send(res));
                Assert("Bridge: ConfirmationSender + PlainText fara exceptie", true);
            }
            catch { Assert("Bridge: ConfirmationSender + PlainText fara exceptie", false); }
        }

        static void Test_ConfirmationSender_With_Html_NoException()
        {
            var res = BuildReservation();
            try
            {
                RedirectConsole(() =>
                    new BookingConfirmationSender(new HtmlRenderer()).Send(res));
                Assert("Bridge: ConfirmationSender + Html fara exceptie", true);
            }
            catch { Assert("Bridge: ConfirmationSender + Html fara exceptie", false); }
        }

        static void Test_ReminderSender_With_Markdown_NoException()
        {
            var res = BuildReservation();
            try
            {
                RedirectConsole(() =>
                    new FlightReminderSender(new MarkdownRenderer(), 24).Send(res));
                Assert("Bridge: ReminderSender + Markdown fara exceptie", true);
            }
            catch { Assert("Bridge: ReminderSender + Markdown fara exceptie", false); }
        }

        static void Test_Bridge_3x3_Combinations()
        {
            // 3 tipuri notificare × 3 formate = 9 combinatii, toate fara exceptie
            var res = BuildReservation();
            IMessageRenderer[] renderers = [new PlainTextRenderer(), new HtmlRenderer(), new MarkdownRenderer()];
            NotificationSender[] senders(IMessageRenderer r) =>
            [
                new BookingConfirmationSender(r),
                new CancellationSender(r),
                new FlightReminderSender(r)
            ];

            int errors = 0;
            foreach (var renderer in renderers)
                foreach (var sender in senders(renderer))
                    try { RedirectConsole(() => sender.Send(res)); }
                    catch { errors++; }

            Assert("Bridge: toate 9 combinatii (3x3) fara exceptie", errors == 0);
        }

        // ── Proxy Tests ──────────────────────────────────────────────

        static void Test_CachedProxy_FirstCall_IsMiss()
        {
            var (proxy, _) = BuildCachedProxy();
            proxy.Search("KIV", "OTP", DateTime.UtcNow.AddDays(7).Date);
            Assert("CachedProxy: primul apel -> miss (0 hits)", proxy.CacheHits() == 0);
        }

        static void Test_CachedProxy_SecondCall_IsHit()
        {
            var (proxy, _) = BuildCachedProxy();
            var date = DateTime.UtcNow.AddDays(7).Date;
            proxy.Search("KIV", "OTP", date);
            proxy.Search("KIV", "OTP", date);  // al doilea apel -> hit
            Assert("CachedProxy: al doilea apel -> hit (1 hit)", proxy.CacheHits() == 1);
        }

        static void Test_CachedProxy_HitCount_Increments()
        {
            var (proxy, _) = BuildCachedProxy();
            var date = DateTime.UtcNow.AddDays(7).Date;
            proxy.Search("KIV", "OTP", date);
            proxy.Search("KIV", "OTP", date);
            proxy.Search("KIV", "OTP", date);
            Assert("CachedProxy: 3 apeluri -> 2 hits", proxy.CacheHits() == 2);
        }

        static void Test_AuthProxy_Guest_ThrowsUnauthorized()
        {
            var proxy = BuildAuthProxy(UserRole.Guest);
            try
            {
                proxy.CreateReservation(BuildPassenger(), BuildFlight(SeatClass.Economy), "1A");
                Assert("AuthProxy: Guest arunca UnauthorizedAccessException", false);
            }
            catch (UnauthorizedAccessException)
            {
                Assert("AuthProxy: Guest arunca UnauthorizedAccessException", true);
            }
        }

        static void Test_AuthProxy_RegisteredUser_CanBook_Economy()
        {
            var proxy = BuildAuthProxy(UserRole.RegisteredUser);
            try
            {
                var res = proxy.CreateReservation(BuildPassenger(), BuildFlight(SeatClass.Economy), "5B");
                Assert("AuthProxy: RegisteredUser poate rezerva Economy", res != null);
            }
            catch { Assert("AuthProxy: RegisteredUser poate rezerva Economy", false); }
        }

        static void Test_AuthProxy_RegisteredUser_CannotBook_FirstClass()
        {
            var proxy = BuildAuthProxy(UserRole.RegisteredUser);
            try
            {
                proxy.CreateReservation(BuildPassenger(), BuildFlight(SeatClass.FirstClass), "1A");
                Assert("AuthProxy: RegisteredUser NU poate rezerva FirstClass", false);
            }
            catch (UnauthorizedAccessException)
            {
                Assert("AuthProxy: RegisteredUser NU poate rezerva FirstClass", true);
            }
        }

        static void Test_AuthProxy_PremiumUser_CanBook_FirstClass()
        {
            var proxy = BuildAuthProxy(UserRole.PremiumUser);
            try
            {
                var res = proxy.CreateReservation(BuildPassenger(), BuildFlight(SeatClass.FirstClass), "1A");
                Assert("AuthProxy: PremiumUser poate rezerva FirstClass", res != null);
            }
            catch { Assert("AuthProxy: PremiumUser poate rezerva FirstClass", false); }
        }

        static void Test_AuthProxy_InactiveUser_ThrowsUnauthorized()
        {
            var repo    = new InMemoryReservationRepository();
            IBookingService svc = new BookingService(repo, new StandardPricingStrategy());
            var user  = new UserContext("suspended_user", UserRole.RegisteredUser, isActive: false);
            var proxy = new AuthenticatedBookingProxy(svc, user);
            try
            {
                proxy.CreateReservation(BuildPassenger(), BuildFlight(SeatClass.Economy), "3C");
                Assert("AuthProxy: cont inactiv arunca UnauthorizedAccessException", false);
            }
            catch (UnauthorizedAccessException)
            {
                Assert("AuthProxy: cont inactiv arunca UnauthorizedAccessException", true);
            }
        }

        static void Test_LoggingProxy_LogsSearches()
        {
            var flightRepo = new InMemoryFlightRepository();
            Utils.DataSeeder.Seed(flightRepo);
            var searchSvc = new FlightSearchService(flightRepo);
            var proxy     = new LoggingFlightSearchProxy(searchSvc);

            proxy.Search("KIV", "IST", DateTime.UtcNow.AddDays(3).Date);
            proxy.Search("KIV", "OTP", DateTime.UtcNow.AddDays(7).Date);

            Assert("LoggingProxy: inregistreaza cautarile",
                   AppLogger.Instance.GetEntries()
                       .Any(e => e.Source == "LoggingProxy"));
        }

        // ── Helpers ──────────────────────────────────────────────────

        static Ticket BuildTicket(decimal price)
        {
            var kiv    = new Airport("KIV","Chisinau","Chisinau","Moldova");
            var otp    = new Airport("OTP","Otopeni","Bucuresti","Romania");
            var now    = DateTime.UtcNow.AddDays(5);
            var flight = new Flight("T01", kiv, otp, now, now.AddHours(2),
                                    SeatClass.Economy, price, 100);
            var pass   = new Passenger("Ion","Popescu","ion@test.md","MD1");
            return new Ticket(flight, pass, "7A", price);
        }

        static Flight BuildFlight(SeatClass cls)
        {
            var kiv = new Airport("KIV","Chisinau","Chisinau","Moldova");
            var otp = new Airport("OTP","Otopeni","Bucuresti","Romania");
            var now = DateTime.UtcNow.AddDays(5);
            return new Flight("T99", kiv, otp, now, now.AddHours(2), cls, 100m, 50);
        }

        static Passenger BuildPassenger() =>
            new("Test","User","test@test.md","MD999");

        static Reservation BuildReservation()
        {
            var ticket = BuildTicket(89m);
            var res    = new Reservation(ticket.Passenger);
            res.AddTicket(ticket);
            res.MarkAsPaid();
            return res;
        }

        static (CachedFlightSearchProxyTestable proxy, IFlightRepository repo)
            BuildCachedProxy()
        {
            var flightRepo = new InMemoryFlightRepository();
            Utils.DataSeeder.Seed(flightRepo);
            var searchSvc = new FlightSearchService(flightRepo);
            return (new CachedFlightSearchProxyTestable(searchSvc), flightRepo);
        }

        static AuthenticatedBookingProxy BuildAuthProxy(UserRole role)
        {
            var repo    = new InMemoryReservationRepository();
            IBookingService svc = new BookingService(repo, new StandardPricingStrategy());
            var user    = new UserContext("user1", role, isActive: true);
            return new AuthenticatedBookingProxy(svc, user);
        }

        static void RedirectConsole(Action action)
        {
            var sw  = new System.IO.StringWriter();
            var old = Console.Out;
            Console.SetOut(sw);
            try   { action(); }
            finally
            {
                Console.SetOut(old);
            }
        }

        static void Assert(string testName, bool condition)
        {
            if (condition) { Console.WriteLine($"  ✔  {testName}"); _passed++; }
            else           { Console.WriteLine($"  ✘  {testName}  <- ESUAT"); _failed++; }
        }
    }

    // Subclasa testabila care expune CacheHits
    public class CachedFlightSearchProxyTestable : CachedFlightSearchProxy
    {
        private int _hits = 0;
        public CachedFlightSearchProxyTestable(FlightSearchService svc) : base(svc) { }

        public new IEnumerable<Flight> Search(string o, string d, DateTime date,
                                               SeatClass? cls = null)
        {
            var before = base.ReusedCount;
            var result = base.Search(o, d, date, cls);
            if (base.ReusedCount > before) _hits++;
            return result;
        }

        public int CacheHits() => _hits;
    }
}
