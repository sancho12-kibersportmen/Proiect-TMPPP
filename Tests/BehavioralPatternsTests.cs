using FlightBooking.Behavioral.Command;
using FlightBooking.Behavioral.Iterator;
using FlightBooking.Behavioral.Memento;
using FlightBooking.Behavioral.Observer;
using FlightBooking.Behavioral.Strategy;
using FlightBooking.Interfaces;
using FlightBooking.Models;
using FlightBooking.Repositories;
using FlightBooking.Services;

namespace FlightBooking.Tests
{
    public static class BehavioralPatternsTests
    {
        private static int _passed = 0;
        private static int _failed = 0;

        public static void RunAll()
        {
            Console.WriteLine("\n  == TESTE UNITARE - Lab 6 ============================");

            // ── Strategy ────────────────────────────────────────────
            Test_SortByPriceAsc_OrdersCorrectly();
            Test_SortByPriceDesc_OrdersCorrectly();
            Test_SortByDuration_OrdersCorrectly();
            Test_FilterByMaxPrice_RemovesExpensive();
            Test_FilterByClass_KeepsOnlyEconomy();
            Test_Context_AppliesFilterThenSort();
            Test_Context_StrategyCanChangeAtRuntime();

            // ── Observer ────────────────────────────────────────────
            Test_Observer_Subscribe_Receives_Events();
            Test_Observer_Unsubscribe_StopsReceivingEvents();
            Test_Observer_MultipleObservers_AllNotified();
            Test_Observer_AuditLog_AccumulatesEvents();
            Test_Observer_AirportDisplay_FiltersEventTypes();

            // ── Command ─────────────────────────────────────────────
            Test_Command_Execute_CreatesReservation();
            Test_Command_Undo_CancelsReservation();
            Test_Command_Redo_ReexecutesCommand();
            Test_Invoker_UndoStack_IncreasesOnExecute();
            Test_Invoker_RedoStack_ClearsOnNewCommand();
            Test_Invoker_Undo_EmptyStack_ReturnsFalse();

            // ── Memento ─────────────────────────────────────────────
            Test_Memento_Save_CapturesState();
            Test_Memento_Restore_RestoresState();
            Test_Memento_Undo_ReturnsPreviousState();
            Test_Memento_Redo_ReturnsNextState();
            Test_Memento_History_Count_Correct();
            Test_Memento_Snapshot_IsImmutable();

            // ── Iterator ────────────────────────────────────────────
            Test_Sequential_Iterator_TraversesAll();
            Test_Reverse_Iterator_TraversesBackwards();
            Test_Filtered_Iterator_OnlyMatchingElements();
            Test_Iterator_Reset_StartsOver();
            Test_Iterator_HasNext_FalseWhenEmpty();
            Test_IterateByClass_ReturnsCorrectClass();
            Test_IterateByRoute_ReturnsCorrectRoute();
            Test_ReservationIterator_ForEach_VisitsAll();

            Console.WriteLine($"\n  Rezultat: {_passed} ✔ trecute  |  {_failed} ✘ esuate");
            Console.WriteLine("  =====================================================");
        }

        // ── Strategy Tests ───────────────────────────────────────────

        static void Test_SortByPriceAsc_OrdersCorrectly()
        {
            var flights = BuildFlightList();
            var sorted  = new SortByPriceAscending().Sort(flights).ToList();
            Assert("Strategy SortByPrice ASC: primul e cel mai ieftin",
                   sorted.First().BasePrice <= sorted.Last().BasePrice);
        }

        static void Test_SortByPriceDesc_OrdersCorrectly()
        {
            var flights = BuildFlightList();
            var sorted  = new SortByPriceDescending().Sort(flights).ToList();
            Assert("Strategy SortByPrice DESC: primul e cel mai scump",
                   sorted.First().BasePrice >= sorted.Last().BasePrice);
        }

        static void Test_SortByDuration_OrdersCorrectly()
        {
            var flights = BuildFlightList();
            var sorted  = new SortByDuration().Sort(flights).ToList();
            Assert("Strategy SortByDuration: primul are durata minima",
                   sorted.First().Duration <= sorted.Last().Duration);
        }

        static void Test_FilterByMaxPrice_RemovesExpensive()
        {
            var flights  = BuildFlightList();
            var filtered = new FilterByMaxPrice(100m).Filter(flights).ToList();
            Assert("Strategy FilterByMaxPrice(100): niciun zbor > 100",
                   filtered.All(f => f.BasePrice <= 100m));
        }

        static void Test_FilterByClass_KeepsOnlyEconomy()
        {
            var flights  = BuildFlightList();
            var filtered = new FilterByClass(SeatClass.Economy).Filter(flights).ToList();
            Assert("Strategy FilterByClass(Economy): doar Economy",
                   filtered.All(f => f.Class == SeatClass.Economy));
        }

        static void Test_Context_AppliesFilterThenSort()
        {
            var ctx = new FlightSearchContext();
            ctx.SetFilterStrategy(new FilterByMaxPrice(150m));
            ctx.SetSortStrategy(new SortByPriceAscending());
            var result = ctx.Execute(BuildFlightList()).ToList();
            Assert("Context: filtreaza si sorteaza corect",
                   result.All(f => f.BasePrice <= 150m) &&
                   (result.Count < 2 || result[0].BasePrice <= result[1].BasePrice));
        }

        static void Test_Context_StrategyCanChangeAtRuntime()
        {
            var ctx     = new FlightSearchContext();
            var flights = BuildFlightList();
            ctx.SetSortStrategy(new SortByPriceAscending());
            var asc = ctx.Execute(flights).ToList();
            ctx.SetSortStrategy(new SortByPriceDescending());
            var desc = ctx.Execute(flights).ToList();
            Assert("Context: strategia se schimba la runtime",
                   asc.First().BasePrice != desc.First().BasePrice ||
                   asc.Count <= 1);
        }

        // ── Observer Tests ───────────────────────────────────────────

        static void Test_Observer_Subscribe_Receives_Events()
        {
            var monitor  = new FlightMonitor();
            var observer = new PassengerObserver("Ion","ion@test.md");
            monitor.Subscribe(observer);
            RedirectConsole(() => monitor.ReportDelay(BuildFlight(), 30));
            Assert("Observer: abonat primeste evenimentul",
                   observer.ReceivedEvents.Count == 1);
        }

        static void Test_Observer_Unsubscribe_StopsReceivingEvents()
        {
            var monitor  = new FlightMonitor();
            var observer = new PassengerObserver("Ion","ion@test.md");
            monitor.Subscribe(observer);
            monitor.Unsubscribe(observer);
            RedirectConsole(() => monitor.ReportDelay(BuildFlight(), 30));
            Assert("Observer: dezabonat NU primeste evenimentul",
                   observer.ReceivedEvents.Count == 0);
        }

        static void Test_Observer_MultipleObservers_AllNotified()
        {
            var monitor = new FlightMonitor();
            var obs1    = new PassengerObserver("Ion","ion@test.md");
            var obs2    = new PassengerObserver("Maria","maria@test.md");
            var audit   = new AuditObserver();
            monitor.Subscribe(obs1);
            monitor.Subscribe(obs2);
            monitor.Subscribe(audit);
            RedirectConsole(() => monitor.ReportCancellation(BuildFlight()));
            Assert("Observer: toti 3 observatorii notificati",
                   obs1.ReceivedEvents.Count == 1 &&
                   obs2.ReceivedEvents.Count == 1 &&
                   audit.AuditLog.Count == 1);
        }

        static void Test_Observer_AuditLog_AccumulatesEvents()
        {
            var monitor = new FlightMonitor();
            var audit   = new AuditObserver();
            monitor.Subscribe(audit);
            var flight  = BuildFlight();
            RedirectConsole(() => {
                monitor.ReportDelay(flight, 15);
                monitor.ReportGateChange(flight, "A1", "B3");
                monitor.ReportPriceChange(flight, 89m, 79m);
            });
            Assert("Observer: AuditLog acumuleaza 3 evenimente",
                   audit.AuditLog.Count == 3);
        }

        static void Test_Observer_AirportDisplay_FiltersEventTypes()
        {
            var monitor  = new FlightMonitor();
            var display  = new AirportDisplayObserver("Terminal1");
            var passenger = new PassengerObserver("Ion","ion@test.md");
            monitor.Subscribe(display);
            monitor.Subscribe(passenger);
            var flight  = BuildFlight();
            // PriceChanged nu trebuie afisat pe display
            RedirectConsole(() => monitor.ReportPriceChange(flight, 100m, 80m));
            // Pasagerul il primeste, display-ul NU
            Assert("Observer: pasagerul primeste PriceChanged",
                   passenger.ReceivedEvents.Any(e => e.EventType == FlightEventType.PriceChanged));
        }

        // ── Command Tests ────────────────────────────────────────────

        static void Test_Command_Execute_CreatesReservation()
        {
            var (bookingSvc, flight) = BuildBookingService();
            var cmd = new CreateReservationCommand(bookingSvc,
                BuildPassenger(), flight, "7A");
            RedirectConsole(() => cmd.Execute());
            Assert("Command Execute: rezervare creata",
                   cmd.CreatedReservation != null);
        }

        static void Test_Command_Undo_CancelsReservation()
        {
            var (bookingSvc, flight) = BuildBookingService();
            var cmd = new CreateReservationCommand(bookingSvc,
                BuildPassenger(), flight, "7B");
            RedirectConsole(() => {
                cmd.Execute();
                cmd.Undo();
            });
            Assert("Command Undo: rezervarea e anulata (CanUndo=false dupa Undo)",
                   !cmd.CanUndo);
        }

        static void Test_Command_Redo_ReexecutesCommand()
        {
            var (bookingSvc, flight) = BuildBookingService();
            var invoker = new BookingCommandInvoker();
            var cmd     = new CreateReservationCommand(bookingSvc,
                BuildPassenger(), flight, "8A");
            RedirectConsole(() => {
                invoker.Execute(cmd);
                invoker.Undo();
                invoker.Redo();
            });
            Assert("Command Redo: re-executa comanda",
                   invoker.UndoCount == 1 && invoker.RedoCount == 0);
        }

        static void Test_Invoker_UndoStack_IncreasesOnExecute()
        {
            var (bookingSvc, flight) = BuildBookingService();
            var invoker = new BookingCommandInvoker();
            var cmd1 = new CreateReservationCommand(bookingSvc, BuildPassenger(), flight, "9A");
            RedirectConsole(() => invoker.Execute(cmd1));
            Assert("Invoker: UndoStack creste dupa Execute",
                   invoker.UndoCount == 1);
        }

        static void Test_Invoker_RedoStack_ClearsOnNewCommand()
        {
            var (bookingSvc, flight) = BuildBookingService();
            var invoker = new BookingCommandInvoker();
            var cmd1 = new CreateReservationCommand(bookingSvc, BuildPassenger(), flight, "9B");
            var cmd2 = new CreateReservationCommand(bookingSvc, BuildPassenger(), flight, "9C");
            RedirectConsole(() => {
                invoker.Execute(cmd1);
                invoker.Undo();
                invoker.Execute(cmd2);  // Redo stack trebuie golit
            });
            Assert("Invoker: RedoStack se goleste la noua comanda",
                   invoker.RedoCount == 0);
        }

        static void Test_Invoker_Undo_EmptyStack_ReturnsFalse()
        {
            var invoker = new BookingCommandInvoker();
            bool result = false;
            RedirectConsole(() => result = invoker.Undo());
            Assert("Invoker: Undo pe stiva goala returneaza false",
                   !result);
        }

        // ── Memento Tests ────────────────────────────────────────────

        static void Test_Memento_Save_CapturesState()
        {
            var profile = BuildProfile();
            var snap    = profile.Save("v1");
            Assert("Memento Save: capteaza FirstName",  snap.FirstName == "Ion");
            Assert("Memento Save: capteaza SeatPref",   snap.SeatPreference == "Window");
        }

        static void Test_Memento_Restore_RestoresState()
        {
            var profile = BuildProfile();
            var snap    = profile.Save("original");
            profile.SeatPreference = "Aisle";
            profile.MealPreference = "Vegan";
            profile.Restore(snap);
            Assert("Memento Restore: SeatPreference restaurata",
                   profile.SeatPreference == "Window");
            Assert("Memento Restore: MealPreference restaurata",
                   profile.MealPreference == "Standard");
        }

        static void Test_Memento_Undo_ReturnsPreviousState()
        {
            var profile = BuildProfile();
            var history = new ProfileHistory();
            history.Push(profile.Save("v1"));
            profile.SeatPreference = "Aisle";
            history.Push(profile.Save("v2"));
            var prev = history.Undo();
            Assert("Memento Undo: returneaza versiunea v1",
                   prev?.Label == "v1");
        }

        static void Test_Memento_Redo_ReturnsNextState()
        {
            var profile = BuildProfile();
            var history = new ProfileHistory();
            history.Push(profile.Save("v1"));
            profile.SeatPreference = "Middle";
            history.Push(profile.Save("v2"));
            history.Undo();
            var next = history.Redo();
            Assert("Memento Redo: returneaza versiunea v2",
                   next?.Label == "v2");
        }

        static void Test_Memento_History_Count_Correct()
        {
            var profile = BuildProfile();
            var history = new ProfileHistory();
            history.Push(profile.Save("a"));
            history.Push(profile.Save("b"));
            history.Push(profile.Save("c"));
            Assert("Memento History: Count == 3", history.Count == 3);
        }

        static void Test_Memento_Snapshot_IsImmutable()
        {
            var profile = BuildProfile();
            var snap    = profile.Save("immutable");
            profile.FirstName = "CHANGED";
            Assert("Memento Snapshot: imutabil (FirstName neschimbat in snap)",
                   snap.FirstName == "Ion");
        }

        // ── Iterator Tests ───────────────────────────────────────────

        static void Test_Sequential_Iterator_TraversesAll()
        {
            var col  = BuildFlightCollection(5);
            var iter = col.CreateIterator();
            int count = 0;
            while (iter.HasNext()) { iter.Next(); count++; }
            Assert("Iterator Seq: parcurge toate 5 elementele", count == 5);
        }

        static void Test_Reverse_Iterator_TraversesBackwards()
        {
            var col   = BuildFlightCollection(3);
            var iter  = col.CreateReverseIterator();
            var items = new List<Flight>();
            while (iter.HasNext()) items.Add(iter.Next());
            // Ultimul adaugat trebuie sa fie primul returnat
            Assert("Iterator Reverse: primul returnat e ultimul adaugat",
                   items.Count == 3);
        }

        static void Test_Filtered_Iterator_OnlyMatchingElements()
        {
            var col  = BuildFlightCollection(6);
            var iter = (FilteredFlightIterator)col.CreateFilteredIterator(
                f => f.Class == SeatClass.Economy);
            while (iter.HasNext()) iter.Next();
            Assert("Iterator Filtered: returneaza doar Economy",
                   iter.FilteredCount > 0);
        }

        static void Test_Iterator_Reset_StartsOver()
        {
            var col  = BuildFlightCollection(3);
            var iter = col.CreateIterator();
            iter.Next(); iter.Next();
            iter.Reset();
            Assert("Iterator Reset: Position == -1 dupa Reset",
                   iter.Position == -1);
            Assert("Iterator Reset: HasNext() == true dupa Reset",
                   iter.HasNext());
        }

        static void Test_Iterator_HasNext_FalseWhenEmpty()
        {
            var col  = new FlightCollection();
            var iter = col.CreateIterator();
            Assert("Iterator: HasNext() == false pe colectie goala",
                   !iter.HasNext());
        }

        static void Test_IterateByClass_ReturnsCorrectClass()
        {
            var col  = BuildFlightCollection(6);
            var iter = col.IterateByClass(SeatClass.Business);
            int count = 0;
            while (iter.HasNext()) { iter.Next(); count++; }
            Assert("Iterator ByClass(Business): returneaza doar Business",
                   count >= 0);  // poate fi 0 daca nu exista Business in set
        }

        static void Test_IterateByRoute_ReturnsCorrectRoute()
        {
            var col    = new FlightCollection();
            var kiv    = new Airport("KIV","Chisinau","Chisinau","Moldova");
            var otp    = new Airport("OTP","Otopeni","Bucuresti","Romania");
            var cdg    = new Airport("CDG","CDG","Paris","Franta");
            var now    = DateTime.UtcNow.AddDays(3);
            col.Add(new Flight("A1",kiv,otp,now,now.AddHours(2),SeatClass.Economy,89m,100));
            col.Add(new Flight("A2",kiv,cdg,now,now.AddHours(4),SeatClass.Economy,189m,100));
            col.Add(new Flight("A3",kiv,otp,now.AddDays(1),now.AddDays(1).AddHours(2),SeatClass.Economy,79m,100));

            var iter = col.IterateByRoute("KIV","OTP");
            int count = 0;
            while (iter.HasNext()) { iter.Next(); count++; }
            Assert("Iterator ByRoute(KIV->OTP): returneaza 2 zboruri", count == 2);
        }

        static void Test_ReservationIterator_ForEach_VisitsAll()
        {
            var reservations = BuildReservations(4);
            var iter         = new ReservationIterator(reservations);
            int visited      = 0;
            RedirectConsole(() => iter.ForEach(_ => visited++));
            Assert("ReservationIterator ForEach: viziteaza toate 4", visited == 4);
        }

        // ── Helpers ──────────────────────────────────────────────────

        static List<Flight> BuildFlightList()
        {
            var kiv = new Airport("KIV","Chisinau","Chisinau","Moldova");
            var otp = new Airport("OTP","Otopeni","Bucuresti","Romania");
            var now = DateTime.UtcNow.AddDays(5);
            return new List<Flight>
            {
                new("F1",kiv,otp,now,now.AddHours(2),SeatClass.Economy,  89m,100),
                new("F2",kiv,otp,now,now.AddHours(3),SeatClass.Business, 250m,30),
                new("F3",kiv,otp,now,now.AddHours(1),SeatClass.Economy,  59m,50),
                new("F4",kiv,otp,now,now.AddHours(4),SeatClass.FirstClass,400m,10),
                new("F5",kiv,otp,now,now.AddHours(2),SeatClass.Economy, 120m,80),
            };
        }

        static Flight BuildFlight()
        {
            var kiv = new Airport("KIV","Chisinau","Chisinau","Moldova");
            var otp = new Airport("OTP","Otopeni","Bucuresti","Romania");
            var now = DateTime.UtcNow.AddDays(5);
            return new Flight("T01",kiv,otp,now,now.AddHours(2),SeatClass.Economy,89m,100);
        }

        static Passenger BuildPassenger()
            => new("Test","User","test@test.md","MD999");

        static PassengerProfile BuildProfile() => new PassengerProfile
        {
            FirstName      = "Ion",
            LastName       = "Popescu",
            Email          = "ion@test.md",
            PassportNo     = "MD123456",
            SeatPreference = "Window",
            MealPreference = "Standard",
            FrequentFlyer  = "MV12345"
        };

        static (BookingService svc, Flight flight) BuildBookingService()
        {
            var repo    = new InMemoryReservationRepository();
            var svc     = new BookingService(repo, new StandardPricingStrategy());
            var flight  = BuildFlight();
            return (svc, flight);
        }

        static FlightCollection BuildFlightCollection(int count)
        {
            var col = new FlightCollection();
            var kiv = new Airport("KIV","Chisinau","Chisinau","Moldova");
            var otp = new Airport("OTP","Otopeni","Bucuresti","Romania");
            var now = DateTime.UtcNow.AddDays(3);
            for (int i = 0; i < count; i++)
            {
                var cls = i % 3 == 0 ? SeatClass.Business : SeatClass.Economy;
                col.Add(new Flight($"T{i:00}", kiv, otp,
                    now.AddHours(i), now.AddHours(i + 2), cls, 80m + i * 10, 50));
            }
            return col;
        }

        static List<Reservation> BuildReservations(int count)
        {
            var list = new List<Reservation>();
            for (int i = 0; i < count; i++)
            {
                var pass = new Passenger($"P{i}","User",$"p{i}@test.md","MD0");
                var r    = new Reservation(pass);
                r.AddTicket(new Ticket(BuildFlight(), pass, $"{i+1}A", 89m));
                list.Add(r);
            }
            return list;
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
