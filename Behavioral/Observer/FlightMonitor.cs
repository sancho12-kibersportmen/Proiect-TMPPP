using FlightBooking.Interfaces;
using FlightBooking.Models;
using FlightBooking.Services;

namespace FlightBooking.Behavioral.Observer
{
    // ══════════════════════════════════════════════════════════════════
    //  OBSERVER PATTERN
    //
    //  Problema rezolvata: mai multi clienti si sisteme trebuie
    //  notificati automat cand un zbor se schimba (pret, intarziere,
    //  poarta de imbarcare). Fara Observer, FlightMonitor ar trebui sa
    //  cunoasca toti consumatorii — cuplare stransa.
    //
    //  Solutia: FlightMonitor (Subject) mentine o lista de observatori.
    //  La orice schimbare, notifica toti observatorii inregistrati.
    //  Observatorii pot fi adaugati/eliminati fara a modifica Subject-ul.
    // ══════════════════════════════════════════════════════════════════

    // ── Subject concret ──────────────────────────────────────────────
    // SRP: gestioneaza starea zborurilor si notifica observatorii
    public class FlightMonitor : IFlightSubject
    {
        private readonly List<IFlightObserver> _observers = new();
        private readonly object _lock = new();

        public void Subscribe(IFlightObserver observer)
        {
            lock (_lock) _observers.Add(observer);
            AppLogger.Instance.Info("FlightMonitor",
                $"Observer abonat: {observer.ObserverName}");
        }

        public void Unsubscribe(IFlightObserver observer)
        {
            lock (_lock) _observers.Remove(observer);
            AppLogger.Instance.Info("FlightMonitor",
                $"Observer dezabonat: {observer.ObserverName}");
        }

        public void NotifyObservers(FlightEvent flightEvent)
        {
            List<IFlightObserver> snapshot;
            lock (_lock) snapshot = _observers.ToList();

            foreach (var observer in snapshot)
                observer.OnFlightUpdated(flightEvent);
        }

        // ── Actiuni care declanseaza notificari ──────────────────────

        public void ReportDelay(Flight flight, int delayMinutes)
        {
            var ev = new FlightEvent(
                FlightEventType.Delayed, flight,
                $"Zborul {flight.FlightNumber} este intarziat cu {delayMinutes} minute.",
                oldValue: 0, newValue: delayMinutes);
            NotifyObservers(ev);
        }

        public void ReportGateChange(Flight flight, string oldGate, string newGate)
        {
            var ev = new FlightEvent(
                FlightEventType.GateChanged, flight,
                $"Poarta zborului {flight.FlightNumber} s-a schimbat: {oldGate} -> {newGate}.",
                oldValue: oldGate, newValue: newGate);
            NotifyObservers(ev);
        }

        public void ReportCancellation(Flight flight)
        {
            var ev = new FlightEvent(
                FlightEventType.Cancelled, flight,
                $"Zborul {flight.FlightNumber} a fost anulat!");
            NotifyObservers(ev);
        }

        public void ReportPriceChange(Flight flight, decimal oldPrice, decimal newPrice)
        {
            var ev = new FlightEvent(
                FlightEventType.PriceChanged, flight,
                $"Pretul zborului {flight.FlightNumber}: {oldPrice:C} -> {newPrice:C}.",
                oldValue: oldPrice, newValue: newPrice);
            NotifyObservers(ev);
        }
    }

    // ── Observatori concreti ─────────────────────────────────────────

    // Observer 1 – Pasager (primeste notificari pe email)
    public class PassengerObserver : IFlightObserver
    {
        private readonly string _passengerName;
        private readonly string _email;
        private readonly List<FlightEvent> _receivedEvents = new();

        public PassengerObserver(string passengerName, string email)
        {
            _passengerName = passengerName;
            _email         = email;
        }

        public string ObserverName => $"Pasager:{_passengerName}";

        public void OnFlightUpdated(FlightEvent flightEvent)
        {
            _receivedEvents.Add(flightEvent);
            var icon = flightEvent.EventType switch
            {
                FlightEventType.Delayed     => "⏰",
                FlightEventType.Cancelled   => "❌",
                FlightEventType.GateChanged => "🚪",
                FlightEventType.PriceChanged=> "💰",
                _                           => "ℹ️"
            };
            Console.WriteLine($"  [{ObserverName}] {icon} EMAIL catre {_email}:");
            Console.WriteLine($"    {flightEvent.Message}");
        }

        public IReadOnlyList<FlightEvent> ReceivedEvents => _receivedEvents.AsReadOnly();
    }

    // Observer 2 – Sistem de afisaj aeroport
    public class AirportDisplayObserver : IFlightObserver
    {
        private readonly string _displayBoard;

        public AirportDisplayObserver(string displayBoard)
            => _displayBoard = displayBoard;

        public string ObserverName => $"Display:{_displayBoard}";

        public void OnFlightUpdated(FlightEvent flightEvent)
        {
            // Afisajul prezinta doar anumite tipuri de evenimente
            if (flightEvent.EventType is FlightEventType.Delayed
                                      or FlightEventType.Cancelled
                                      or FlightEventType.GateChanged)
            {
                Console.WriteLine($"  [{ObserverName}] 📺 DISPLAY ACTUALIZAT:");
                Console.WriteLine($"    [{flightEvent.Flight.FlightNumber}] " +
                                  $"{flightEvent.EventType}: {flightEvent.Message}");
            }
        }
    }

    // Observer 3 – Sistem de audit / logging
    public class AuditObserver : IFlightObserver
    {
        private readonly List<FlightEvent> _auditLog = new();

        public string ObserverName => "AuditSystem";

        public void OnFlightUpdated(FlightEvent flightEvent)
        {
            _auditLog.Add(flightEvent);
            AppLogger.Instance.Info("AuditObserver",
                $"[AUDIT] {flightEvent.EventType} | {flightEvent.Flight.FlightNumber} | " +
                $"{flightEvent.OccurredAt:HH:mm:ss}");
        }

        public IReadOnlyList<FlightEvent> AuditLog => _auditLog.AsReadOnly();
    }
}
