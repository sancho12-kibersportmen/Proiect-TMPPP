using FlightBooking.Interfaces;
using FlightBooking.Models;
using FlightBooking.Services;

namespace FlightBooking.Behavioral.ChainOfResponsibility
{
    // ══════════════════════════════════════════════════════════════════
    //  CHAIN OF RESPONSIBILITY PATTERN
    //
    //  Problema rezolvata: o cerere de rezervare trebuie validata prin
    //  mai multi pasi independenti: date pasager, disponibilitate zbor,
    //  limita de buget, varsta minima, documente valide.
    //  Fara CoR, am avea un if-else gigantic sau o clasa cu 10 metode.
    //
    //  Solutia: fiecare validator este un Handler independent.
    //  Fiecare Handler decide: procesa cererea SAU o transmite mai
    //  departe in lant. Se pot adauga/elimina/reordona fara modificari.
    // ══════════════════════════════════════════════════════════════════

    // ── Handler abstract de baza ─────────────────────────────────────
    public abstract class BaseBookingHandler : IBookingHandler
    {
        private IBookingHandler? _next;

        public IBookingHandler SetNext(IBookingHandler next)
        {
            _next = next;
            return next;   // permite lantul: h1.SetNext(h2).SetNext(h3)
        }

        public virtual BookingValidationResult Handle(BookingRequest request)
        {
            // Daca nu am urmatorul handler, returnam rezultat gol (valid)
            return _next?.Handle(request) ?? new BookingValidationResult();
        }

        protected BookingValidationResult PassToNext(BookingRequest request)
            => _next?.Handle(request) ?? new BookingValidationResult();

        public abstract string HandlerName { get; }
    }

    // ── Handler 1: Validare date pasager ────────────────────────────
    public class PassengerDataHandler : BaseBookingHandler
    {
        public override string HandlerName => "PassengerDataValidator";

        public override BookingValidationResult Handle(BookingRequest request)
        {
            AppLogger.Instance.Info("CoR", $"{HandlerName}: validare date pasager...");

            if (string.IsNullOrWhiteSpace(request.Passenger.FirstName) ||
                string.IsNullOrWhiteSpace(request.Passenger.LastName))
            {
                var result = new BookingValidationResult();
                result.AddError(HandlerName, "Numele pasagerului este obligatoriu.");
                return result;
            }

            if (string.IsNullOrWhiteSpace(request.Passenger.Email) ||
                !request.Passenger.Email.Contains('@'))
            {
                var result = new BookingValidationResult();
                result.AddError(HandlerName, "Adresa de email este invalida.");
                return result;
            }

            if (string.IsNullOrWhiteSpace(request.Passenger.PassportNo))
            {
                var result = new BookingValidationResult();
                result.AddError(HandlerName, "Numarul pasaportului este obligatoriu.");
                return result;
            }

            Console.WriteLine($"  [{HandlerName}] ✔ Date pasager valide — transmit mai departe.");
            return PassToNext(request);
        }
    }

    // ── Handler 2: Disponibilitate zbor ─────────────────────────────
    public class FlightAvailabilityHandler : BaseBookingHandler
    {
        public override string HandlerName => "FlightAvailabilityChecker";

        public override BookingValidationResult Handle(BookingRequest request)
        {
            AppLogger.Instance.Info("CoR", $"{HandlerName}: verificare disponibilitate...");

            if (!request.Flight.HasAvailableSeats())
            {
                var result = new BookingValidationResult();
                result.AddError(HandlerName,
                    $"Zborul {request.Flight.FlightNumber} nu are locuri disponibile.");
                return result;
            }

            if (request.Flight.DepartureTime <= DateTime.UtcNow)
            {
                var result = new BookingValidationResult();
                result.AddError(HandlerName,
                    "Nu se pot face rezervari pentru zboruri din trecut.");
                return result;
            }

            Console.WriteLine($"  [{HandlerName}] ✔ Zbor disponibil — transmit mai departe.");
            return PassToNext(request);
        }
    }

    // ── Handler 3: Verificare buget ──────────────────────────────────
    public class BudgetCheckHandler : BaseBookingHandler
    {
        public override string HandlerName => "BudgetChecker";

        public override BookingValidationResult Handle(BookingRequest request)
        {
            AppLogger.Instance.Info("CoR", $"{HandlerName}: verificare buget...");

            if (request.Flight.BasePrice > request.BudgetLimit)
            {
                var result = new BookingValidationResult();
                result.AddError(HandlerName,
                    $"Pretul zborului ({request.Flight.BasePrice:C}) depaseste " +
                    $"bugetul disponibil ({request.BudgetLimit:C}).");
                return result;
            }

            if (request.Flight.BasePrice > request.BudgetLimit * 0.8m)
                new BookingValidationResult().AddWarning(HandlerName,
                    "Pretul este aproape de limita de buget.");

            Console.WriteLine($"  [{HandlerName}] ✔ Buget suficient — transmit mai departe.");
            return PassToNext(request);
        }
    }

    // ── Handler 4: Verificare varsta minima ──────────────────────────
    public class AgeVerificationHandler : BaseBookingHandler
    {
        private const int MinAge = 18;
        public override string HandlerName => "AgeVerifier";

        public override BookingValidationResult Handle(BookingRequest request)
        {
            AppLogger.Instance.Info("CoR", $"{HandlerName}: verificare varsta...");

            if (request.PassengerAge < MinAge)
            {
                var result = new BookingValidationResult();
                result.AddError(HandlerName,
                    $"Pasagerul trebuie sa aiba minim {MinAge} ani. " +
                    $"Varsta declarata: {request.PassengerAge}.");
                return result;
            }

            Console.WriteLine($"  [{HandlerName}] ✔ Varsta corecta — transmit mai departe.");
            return PassToNext(request);
        }
    }

    // ── Handler 5: Verificare clasa First Class (documente) ──────────
    public class PremiumClassHandler : BaseBookingHandler
    {
        public override string HandlerName => "PremiumClassChecker";

        public override BookingValidationResult Handle(BookingRequest request)
        {
            AppLogger.Instance.Info("CoR", $"{HandlerName}: verificare acces premium...");

            if (request.Flight.Class == SeatClass.FirstClass &&
                string.IsNullOrWhiteSpace(request.CustomerId))
            {
                var result = new BookingValidationResult();
                result.AddError(HandlerName,
                    "First Class necesita cont de client verificat (CustomerId).");
                return result;
            }

            Console.WriteLine($"  [{HandlerName}] ✔ Acces autorizat — lant finalizat.");
            return PassToNext(request);
        }
    }

    // ── Builder pentru lant ──────────────────────────────────────────
    // Construieste lantul standard de validare
    public static class BookingValidationChainBuilder
    {
        public static IBookingHandler BuildStandardChain()
        {
            var passenger    = new PassengerDataHandler();
            var availability = new FlightAvailabilityHandler();
            var budget       = new BudgetCheckHandler();
            var age          = new AgeVerificationHandler();
            var premium      = new PremiumClassHandler();

            passenger.SetNext(availability)
                     .SetNext(budget)
                     .SetNext(age)
                     .SetNext(premium);

            return passenger;   // primul handler = intrarea in lant
        }
    }
}
