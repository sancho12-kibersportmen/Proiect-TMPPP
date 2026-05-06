using FlightBooking.Factories;
using FlightBooking.Interfaces;
using FlightBooking.Models;
using FlightBooking.Repositories;
using FlightBooking.Services;
using FlightBooking.Structural.Adapter;
using FlightBooking.Structural.Composite;
using FlightBooking.Structural.Facade;

namespace FlightBooking.Tests
{
    // ══════════════════════════════════════════════════════════════════
    //  TESTE UNITARE – Laborator 4
    //  Adapter | Composite | Façade
    // ══════════════════════════════════════════════════════════════════
    public static class StructuralPatternsTests
    {
        private static int _passed = 0;
        private static int _failed = 0;

        public static void RunAll()
        {
            Console.WriteLine("\n  == TESTE UNITARE – Lab 4 ============================");

            // ── Adapter ─────────────────────────────────────────────
            Test_PayPalAdapter_Implements_IPaymentProcessor();
            Test_StripeAdapter_Implements_IPaymentProcessor();
            Test_MaibAdapter_Implements_IPaymentProcessor();
            Test_PayPalAdapter_ProcessPayment_Returns_NonEmpty_TxId();
            Test_StripeAdapter_ProcessPayment_Returns_NonEmpty_TxId();
            Test_MaibAdapter_ProcessPayment_Returns_NonEmpty_TxId();
            Test_PayPalAdapter_Refund_Returns_True();
            Test_Adapters_Have_Correct_ProcessorNames();

            // ── Composite ───────────────────────────────────────────
            Test_FlightSegment_Is_IItineraryComponent();
            Test_Itinerary_TotalPrice_Is_Sum_Of_Children();
            Test_Itinerary_TotalDuration_Is_Sum();
            Test_Itinerary_StopCount_Correct();
            Test_TravelPackage_Applies_Discount();
            Test_Composite_Nested_TravelPackage();
            Test_Itinerary_Display_Does_Not_Throw();

            // ── Facade ──────────────────────────────────────────────
            Test_Facade_SearchFlights_Returns_Results();
            Test_Facade_BookAndPay_Returns_Success();
            Test_Facade_CancelBooking_Returns_True();
            Test_Facade_BuildRoundTrip_Has_Two_Segments();

            Console.WriteLine($"\n  Rezultat: {_passed} ✔ trecute  |  {_failed} ✘ esuate");
            Console.WriteLine("  =====================================================");
        }

        // ── Adapter Tests ────────────────────────────────────────────

        static void Test_PayPalAdapter_Implements_IPaymentProcessor()
        {
            IPaymentProcessor p = new PayPalAdapter(new PayPalGateway());
            Assert("PayPalAdapter implementeaza IPaymentProcessor", p != null);
        }

        static void Test_StripeAdapter_Implements_IPaymentProcessor()
        {
            IPaymentProcessor p = new StripeAdapter(new StripeGateway());
            Assert("StripeAdapter implementeaza IPaymentProcessor", p != null);
        }

        static void Test_MaibAdapter_Implements_IPaymentProcessor()
        {
            IPaymentProcessor p = new MaibAdapter(new MaibBankGateway());
            Assert("MaibAdapter implementeaza IPaymentProcessor", p != null);
        }

        static void Test_PayPalAdapter_ProcessPayment_Returns_NonEmpty_TxId()
        {
            var adapter = new PayPalAdapter(new PayPalGateway());
            var txId    = adapter.ProcessPayment("test@test.md", 150m, "Test");
            Assert("PayPalAdapter.ProcessPayment returneaza TxId nevid", !string.IsNullOrEmpty(txId));
        }

        static void Test_StripeAdapter_ProcessPayment_Returns_NonEmpty_TxId()
        {
            var adapter = new StripeAdapter(new StripeGateway());
            var txId    = adapter.ProcessPayment("cus_test123", 89m, "Test");
            Assert("StripeAdapter.ProcessPayment returneaza TxId nevid (ch_...)", txId.StartsWith("ch_"));
        }

        static void Test_MaibAdapter_ProcessPayment_Returns_NonEmpty_TxId()
        {
            var adapter = new MaibAdapter(new MaibBankGateway());
            var txId    = adapter.ProcessPayment("1234", 200m, "Test");
            Assert("MaibAdapter.ProcessPayment returneaza TxId nevid", !string.IsNullOrEmpty(txId));
        }

        static void Test_PayPalAdapter_Refund_Returns_True()
        {
            var adapter = new PayPalAdapter(new PayPalGateway());
            var result  = adapter.Refund("PAYPAL-TXID123", 50m);
            Assert("PayPalAdapter.Refund returneaza true", result);
        }

        static void Test_Adapters_Have_Correct_ProcessorNames()
        {
            Assert("PayPalAdapter.ProcessorName == 'PayPal'",
                   new PayPalAdapter(new PayPalGateway()).ProcessorName == "PayPal");
            Assert("StripeAdapter.ProcessorName == 'Stripe'",
                   new StripeAdapter(new StripeGateway()).ProcessorName == "Stripe");
            Assert("MaibAdapter.ProcessorName == 'MAIB'",
                   new MaibAdapter(new MaibBankGateway()).ProcessorName == "MAIB");
        }

        // ── Composite Tests ──────────────────────────────────────────

        static void Test_FlightSegment_Is_IItineraryComponent()
        {
            var seg = BuildSegment(100m, 1.5);
            Assert("FlightSegment implementeaza IItineraryComponent",
                   seg is IItineraryComponent);
        }

        static void Test_Itinerary_TotalPrice_Is_Sum_Of_Children()
        {
            var itin = new Itinerary("Test");
            itin.Add(BuildSegment(100m, 2));
            itin.Add(BuildSegment(200m, 3));
            Assert("Itinerary.TotalPrice = suma copiilor",
                   itin.TotalPrice == 300m);
        }

        static void Test_Itinerary_TotalDuration_Is_Sum()
        {
            var itin = new Itinerary("Test");
            itin.Add(BuildSegment(100m, 2));
            itin.Add(BuildSegment(100m, 3));
            Assert("Itinerary.TotalDuration = suma duratelor",
                   itin.TotalDuration == TimeSpan.FromHours(5));
        }

        static void Test_Itinerary_StopCount_Correct()
        {
            var itin = new Itinerary("Test");
            itin.Add(BuildSegment(100m, 1));
            itin.Add(BuildSegment(100m, 1));
            itin.Add(BuildSegment(100m, 1));
            // 3 segmente = 2 escale
            Assert("Itinerary.StopCount = numarul de escale corect",
                   itin.StopCount == 2);
        }

        static void Test_TravelPackage_Applies_Discount()
        {
            var pkg = new TravelPackage("Vacanta", discountPercent: 10m);
            pkg.Add(BuildSegment(200m, 2));
            pkg.Add(BuildSegment(200m, 2));
            // 400 - 10% = 360
            Assert("TravelPackage aplica reducerea corect (400 - 10% = 360)",
                   pkg.TotalPrice == 360m);
        }

        static void Test_Composite_Nested_TravelPackage()
        {
            // Composite de Composite: TravelPackage contine Itinerary-uri
            var outbound = new Itinerary("Dus");
            outbound.Add(BuildSegment(150m, 2));

            var inbound = new Itinerary("Intors");
            inbound.Add(BuildSegment(130m, 2));

            var pkg = new TravelPackage("Dus-Intors", discountPercent: 5m);
            pkg.Add(outbound);
            pkg.Add(inbound);

            // (150 + 130) * 0.95 = 266
            Assert("Composite nested: pret corect (280 * 0.95 = 266)",
                   pkg.TotalPrice == 266m);
        }

        static void Test_Itinerary_Display_Does_Not_Throw()
        {
            var itin = new Itinerary("Display Test");
            itin.Add(BuildSegment(99m, 1));
            try
            {
                var sw = new System.IO.StringWriter();
                Console.SetOut(sw);
                itin.Display();
                Console.SetOut(new System.IO.StreamWriter(Console.OpenStandardOutput())
                    { AutoFlush = true });
                Assert("Itinerary.Display() nu arunca exceptie", true);
            }
            catch
            {
                Assert("Itinerary.Display() nu arunca exceptie", false);
            }
        }

        // ── Facade Tests ─────────────────────────────────────────────

        static void Test_Facade_SearchFlights_Returns_Results()
        {
            var (facade, _) = BuildFacade();
            var date    = DateTime.UtcNow.AddDays(3).Date;
            var results = facade.SearchFlights("KIV", "IST", date).ToList();
            Assert("Facade.SearchFlights returneaza zboruri", results.Count > 0);
        }

        static void Test_Facade_BookAndPay_Returns_Success()
        {
            var (facade, flightRepo) = BuildFacade();
            var flight  = flightRepo.GetAll().First();
            var pass    = new Passenger("Test","User","test@test.md","MD999");
            var result  = facade.BookAndPay(pass, flight, "test@test.md");
            Assert("Facade.BookAndPay returneaza Success=true", result.Success);
            Assert("Facade.BookAndPay returneaza ReservationId nevid",
                   !string.IsNullOrEmpty(result.ReservationId));
        }

        static void Test_Facade_CancelBooking_Returns_True()
        {
            var (facade, flightRepo) = BuildFacade();
            var flight  = flightRepo.GetAll().Skip(1).First();
            var pass    = new Passenger("Cancel","User","cancel@test.md","MD888");
            var booking = facade.BookAndPay(pass, flight, "cancel@test.md");
            var result  = facade.CancelBooking(booking.ReservationId!, booking.TransactionId!);
            Assert("Facade.CancelBooking returneaza true", result);
        }

        static void Test_Facade_BuildRoundTrip_Has_Two_Segments()
        {
            var (facade, flightRepo) = BuildFacade();
            var flights = flightRepo.GetAll().ToList();
            var itin = facade.BuildRoundTripItinerary(
                "Chisinau ↔ Paris",
                flights[0], 189m,
                flights[1], 175m);
            Assert("Facade.BuildRoundTrip are 2 segmente",
                   itin.Children.Count == 2);
            Assert("Facade.BuildRoundTrip pret total = 364",
                   itin.TotalPrice == 364m);
        }

        // ── Helpers ──────────────────────────────────────────────────

        static FlightSegment BuildSegment(decimal price, double durationHours)
        {
            var kiv    = new Airport("KIV","Chisinau","Chisinau","Moldova");
            var otp    = new Airport("OTP","Otopeni","Bucuresti","Romania");
            var now    = DateTime.UtcNow.AddDays(5);
            var flight = new Flight("T01", kiv, otp,
                                    now, now.AddHours(durationHours),
                                    SeatClass.Economy, price, 100);
            return new FlightSegment(flight, price);
        }

        static (BookingFacade facade, IFlightRepository flightRepo) BuildFacade()
        {
            IFlightRepository      flightRepo      = new InMemoryFlightRepository();
            IReservationRepository reservationRepo = new InMemoryReservationRepository();
            IPricingStrategy       pricing         = new StandardPricingStrategy();

            Utils.DataSeeder.Seed(flightRepo);

            var searchSvc  = new FlightSearchService(flightRepo);
            var bookingSvc = new BookingService(reservationRepo, pricing);
            var paymentSvc = new PaymentService(new PayPalAdapter(new PayPalGateway()));
            var outputSvc  = new TicketOutputService(new ConsoleOutputFactory());
            var notifier   = new EmailNotificationFactory();

            return (new BookingFacade(searchSvc, bookingSvc, paymentSvc, outputSvc, notifier),
                    flightRepo);
        }

        static void Assert(string testName, bool condition)
        {
            if (condition) { Console.WriteLine($"  ✔  {testName}"); _passed++; }
            else           { Console.WriteLine($"  ✘  {testName}  <- ESUAT"); _failed++; }
        }
    }
}
