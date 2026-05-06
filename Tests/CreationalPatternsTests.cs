using FlightBooking.Factories.Builder;
using FlightBooking.Models;
using FlightBooking.Services;

namespace FlightBooking.Tests
{
    // ══════════════════════════════════════════════════════════════════
    //  TESTE UNITARE – Laborator 3
    //  Builder | Prototype | Singleton
    // ══════════════════════════════════════════════════════════════════
    public static class CreationalPatternsTests
    {
        private static int _passed = 0;
        private static int _failed = 0;

        public static void RunAll()
        {
            Console.WriteLine("\n  ══ TESTE UNITARE – Lab 3 ════════════════════════");

            // ── Builder ─────────────────────────────────────────────
            Test_Builder_Creates_Valid_Flight();
            Test_Builder_MethodChaining_Returns_Same_Builder();
            Test_Builder_Throws_When_Departure_After_Arrival();
            Test_Builder_Throws_When_Price_Zero();
            Test_Director_EconomyShortHaul_Duration_2h();
            Test_Director_Business_Class_Correct();
            Test_Director_FirstClass_Sets_10_Seats();
            Test_Builder_Reset_Restores_Defaults();

            // ── Prototype ───────────────────────────────────────────
            Test_ShallowCopy_Creates_New_Instance();
            Test_ShallowCopy_Shares_Airport_References();
            Test_DeepCopy_Creates_New_Airport_Objects();
            Test_DeepCopy_Independence_From_Original();
            Test_CreateFlightForDate_Correct_Departure();
            Test_Prototype_Price_Preserved_In_Clone();

            // ── Singleton ───────────────────────────────────────────
            Test_Singleton_Same_Instance();
            Test_Singleton_Logs_Are_Accumulated();
            Test_Singleton_Thread_Safety();

            Console.WriteLine($"\n  Rezultat: {_passed} ✔ trecute  |  {_failed} ✘ esuate");
            Console.WriteLine("  ══════════════════════════════════════════════════");
        }

        // ── Builder Tests ────────────────────────────────────────────

        static void Test_Builder_Creates_Valid_Flight()
        {
            var flight = BuildSampleFlight();
            Assert("Builder creeaza un Flight valid (nu null)", flight != null);
            Assert("Builder: FlightNumber setat corect", flight!.FlightNumber == "TEST01");
            Assert("Builder: BasePrice setat corect", flight.BasePrice == 120m);
        }

        static void Test_Builder_MethodChaining_Returns_Same_Builder()
        {
            var builder = new FlightBuilder();
            var b1 = builder.WithFlightNumber("X1");
            var b2 = b1.WithBasePrice(50m);
            Assert("Method chaining: returneaza acelasi builder", ReferenceEquals(b1, b2));
        }

        static void Test_Builder_Throws_When_Departure_After_Arrival()
        {
            var now = DateTime.UtcNow;
            try
            {
                new FlightBuilder()
                    .WithDeparture(now.AddHours(5))
                    .WithArrival(now.AddHours(1))
                    .Build();
                Assert("Builder arunca exceptie cand departure > arrival", false);
            }
            catch (InvalidOperationException)
            {
                Assert("Builder arunca exceptie cand departure > arrival", true);
            }
        }

        static void Test_Builder_Throws_When_Price_Zero()
        {
            try
            {
                new FlightBuilder().WithBasePrice(0m).Build();
                Assert("Builder arunca exceptie la pret 0", false);
            }
            catch (InvalidOperationException)
            {
                Assert("Builder arunca exceptie la pret 0", true);
            }
        }

        static void Test_Director_EconomyShortHaul_Duration_2h()
        {
            var flight = BuildViaDirector_Economy();
            Assert("Director Economy: durata 2h",
                   flight.Duration == TimeSpan.FromHours(2));
        }

        static void Test_Director_Business_Class_Correct()
        {
            var flight = BuildViaDirector_Business();
            Assert("Director Business: clasa Business",
                   flight.Class == SeatClass.Business);
        }

        static void Test_Director_FirstClass_Sets_10_Seats()
        {
            var departure = DateTime.UtcNow.AddDays(30);
            var kiv = new Airport("KIV","Chisinau","Chisinau","Moldova");
            var jfk = new Airport("JFK","JFK Airport","New York","SUA");
            var director = new FlightDirector(new FlightBuilder());
            var flight = director.BuildFirstClassIntercontinental("FC001", kiv, jfk, departure);
            Assert("Director FirstClass: 10 locuri", flight.TotalSeats == 10);
        }

        static void Test_Builder_Reset_Restores_Defaults()
        {
            var builder = new FlightBuilder();
            builder.WithFlightNumber("CUSTOM").WithBasePrice(999m);
            builder.Reset();
            var flight = builder.Build();
            Assert("Builder.Reset: revine la valorile implicite (price=99)",
                   flight.BasePrice == 99m);
        }

        // ── Prototype Tests ──────────────────────────────────────────

        static void Test_ShallowCopy_Creates_New_Instance()
        {
            var template = BuildTemplate("MV101");
            var clone    = template.ShallowCopy();
            Assert("ShallowCopy: instanta noua (nu acelasi obiect)",
                   !ReferenceEquals(template, clone));
        }

        static void Test_ShallowCopy_Shares_Airport_References()
        {
            var template = BuildTemplate("MV102");
            var clone    = template.ShallowCopy();
            Assert("ShallowCopy: partajeaza referinta Airport (shallow)",
                   ReferenceEquals(template.Origin, clone.Origin));
        }

        static void Test_DeepCopy_Creates_New_Airport_Objects()
        {
            var template = BuildTemplate("MV103");
            var clone    = template.DeepCopy();
            Assert("DeepCopy: obiecte Airport independente (deep)",
                   !ReferenceEquals(template.Origin, clone.Origin));
        }

        static void Test_DeepCopy_Independence_From_Original()
        {
            var template = BuildTemplate("MV104");
            var clone    = template.DeepCopy();
            clone.BasePrice = 999m;
            Assert("DeepCopy: modificarea clonei nu afecteaza originalul",
                   template.BasePrice != 999m);
        }

        static void Test_CreateFlightForDate_Correct_Departure()
        {
            var template = BuildTemplate("MV105");
            var date     = new DateTime(2026, 6, 15);
            var flight   = template.CreateFlightForDate(date);
            Assert("Prototype: zbor generat pentru data corecta",
                   flight.DepartureTime.Date == date.Date);
            Assert("Prototype: ora de decolare corecta (08:00)",
                   flight.DepartureTime.Hour == 8);
        }

        static void Test_Prototype_Price_Preserved_In_Clone()
        {
            var template = BuildTemplate("MV106");
            var clone    = template.DeepCopy();
            Assert("Prototype: pretul este pastrat in clona",
                   clone.BasePrice == template.BasePrice);
        }

        // ── Singleton Tests ──────────────────────────────────────────

        static void Test_Singleton_Same_Instance()
        {
            var inst1 = AppLogger.Instance;
            var inst2 = AppLogger.Instance;
            Assert("Singleton: Instance returneaza mereu acelasi obiect",
                   ReferenceEquals(inst1, inst2));
        }

        static void Test_Singleton_Logs_Are_Accumulated()
        {
            var before = AppLogger.Instance.EntryCount;
            AppLogger.Instance.Info("Test", "Mesaj test acumulare");
            AppLogger.Instance.Info("Test", "Al doilea mesaj");
            var after  = AppLogger.Instance.EntryCount;
            Assert("Singleton: logurile se acumuleaza in aceeasi instanta",
                   after == before + 2);
        }

        static void Test_Singleton_Thread_Safety()
        {
            var instances = new AppLogger[10];
            var threads   = new Thread[10];

            for (int i = 0; i < 10; i++)
            {
                int idx = i;
                threads[idx] = new Thread(() => instances[idx] = AppLogger.Instance);
            }
            foreach (var t in threads) t.Start();
            foreach (var t in threads) t.Join();

            var allSame = instances.All(inst => ReferenceEquals(inst, AppLogger.Instance));
            Assert("Singleton: thread-safe (10 thread-uri, aceeasi instanta)", allSame);
        }

        // ── Helpers ──────────────────────────────────────────────────

        static Flight BuildSampleFlight()
        {
            var now = DateTime.UtcNow;
            var kiv = new Airport("KIV","Chisinau","Chisinau","Moldova");
            var otp = new Airport("OTP","Otopeni","Bucuresti","Romania");
            return new FlightBuilder()
                .WithFlightNumber("TEST01")
                .WithOrigin(kiv)
                .WithDestination(otp)
                .WithDeparture(now.AddDays(5))
                .WithArrival(now.AddDays(5).AddHours(2))
                .WithClass(SeatClass.Economy)
                .WithBasePrice(120m)
                .WithSeats(100)
                .Build();
        }

        static Flight BuildViaDirector_Economy()
        {
            var kiv = new Airport("KIV","Chisinau","Chisinau","Moldova");
            var otp = new Airport("OTP","Otopeni","Bucuresti","Romania");
            var dir = new FlightDirector(new FlightBuilder());
            return dir.BuildEconomyShortHaul("EC01", kiv, otp, DateTime.UtcNow.AddDays(5));
        }

        static Flight BuildViaDirector_Business()
        {
            var kiv = new Airport("KIV","Chisinau","Chisinau","Moldova");
            var cdg = new Airport("CDG","Charles de Gaulle","Paris","Franta");
            var dir = new FlightDirector(new FlightBuilder());
            return dir.BuildBusinessLongHaul("BZ01", kiv, cdg, DateTime.UtcNow.AddDays(10));
        }

        static FlightTemplate BuildTemplate(string flightNumber)
        {
            var kiv = new Airport("KIV","Chisinau","Chisinau","Moldova");
            var otp = new Airport("OTP","Otopeni","Bucuresti","Romania");
            return new FlightTemplate(
                flightNumber, kiv, otp,
                new TimeOnly(8, 0), new TimeSpan(1, 30, 0),
                SeatClass.Economy, 89m, 120
            );
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
                Console.WriteLine($"  ✘  {testName}  <- ESUAT");
                _failed++;
            }
        }
    }
}
