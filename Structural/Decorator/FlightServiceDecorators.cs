using FlightBooking.Interfaces;
using FlightBooking.Models;

namespace FlightBooking.Structural.Decorator
{
    // ══════════════════════════════════════════════════════════════════
    //  DECORATOR PATTERN
    //
    //  Problema rezolvata: pasagerii pot adauga optional la biletul de
    //  baza servicii extra: bagaj, asigurare, prioriby boarding, meal.
    //  Fara Decorator am avea o explozie combinatoriala de subclase:
    //  FlightWithBaggage, FlightWithInsurance, FlightWithBoth, etc.
    //
    //  Solutia: fiecare serviciu extra este un Decorator care "infasoara"
    //  componenta de baza (sau alt decorator) si adauga comportament nou.
    //  Se pot combina dinamic in orice ordine, fara modificari de cod.
    // ══════════════════════════════════════════════════════════════════

    // ── Componenta concreta (ConcreteComponent) ──────────────────────
    // Biletul de baza — fara niciun serviciu extra
    public class BasicFlightService : IFlightService
    {
        private readonly Ticket _ticket;

        public BasicFlightService(Ticket ticket)
            => _ticket = ticket ?? throw new ArgumentNullException(nameof(ticket));

        public string  ServiceName  => "Bilet de baza";
        public decimal Price        => _ticket.FinalPrice;

        public string GetDescription() =>
            $"Zbor {_ticket.Flight.FlightNumber} " +
            $"{_ticket.Flight.Origin.Code}->{_ticket.Flight.Destination.Code} | " +
            $"Loc: {_ticket.SeatNumber} | Clasa: {_ticket.Flight.Class}";

        public void ShowDetails()
        {
            Console.WriteLine($"  [Bilet de baza]");
            Console.WriteLine($"    {GetDescription()}");
            Console.WriteLine($"    Pret: {Price:C}");
        }
    }

    // ── Decorator abstract (Decorator) ───────────────────────────────
    // Pastreaza o referinta la componenta infasurata si delega apelurile.
    public abstract class FlightServiceDecorator : IFlightService
    {
        protected readonly IFlightService _wrapped;

        protected FlightServiceDecorator(IFlightService wrapped)
            => _wrapped = wrapped ?? throw new ArgumentNullException(nameof(wrapped));

        public virtual string  ServiceName   => _wrapped.ServiceName;
        public virtual decimal Price         => _wrapped.Price;
        public virtual string  GetDescription() => _wrapped.GetDescription();
        public virtual void    ShowDetails()    => _wrapped.ShowDetails();
    }

    // ── Decorator concret 1 – Bagaj cala (23 kg) ────────────────────
    public class BaggageDecorator : FlightServiceDecorator
    {
        private const decimal BaggagePrice = 35m;
        private readonly int  _weightKg;

        public BaggageDecorator(IFlightService wrapped, int weightKg = 23)
            : base(wrapped) => _weightKg = weightKg;

        public override string  ServiceName => $"{_wrapped.ServiceName} + Bagaj {_weightKg}kg";
        public override decimal Price       => _wrapped.Price + BaggagePrice;

        public override string GetDescription() =>
            _wrapped.GetDescription() + $" | Bagaj cala {_weightKg}kg";

        public override void ShowDetails()
        {
            _wrapped.ShowDetails();
            Console.WriteLine($"  [+ Bagaj cala {_weightKg}kg]  +{BaggagePrice:C}");
        }
    }

    // ── Decorator concret 2 – Asigurare de calatorie ─────────────────
    public class TravelInsuranceDecorator : FlightServiceDecorator
    {
        private const decimal InsurancePrice = 18m;

        public TravelInsuranceDecorator(IFlightService wrapped) : base(wrapped) { }

        public override string  ServiceName => $"{_wrapped.ServiceName} + Asigurare";
        public override decimal Price       => _wrapped.Price + InsurancePrice;

        public override string GetDescription() =>
            _wrapped.GetDescription() + " | Asigurare medicala + anulare";

        public override void ShowDetails()
        {
            _wrapped.ShowDetails();
            Console.WriteLine($"  [+ Asigurare calatorie]  +{InsurancePrice:C}");
        }
    }

    // ── Decorator concret 3 – Priority Boarding ───────────────────────
    public class PriorityBoardingDecorator : FlightServiceDecorator
    {
        private const decimal PriorityPrice = 12m;

        public PriorityBoardingDecorator(IFlightService wrapped) : base(wrapped) { }

        public override string  ServiceName => $"{_wrapped.ServiceName} + Priority";
        public override decimal Price       => _wrapped.Price + PriorityPrice;

        public override string GetDescription() =>
            _wrapped.GetDescription() + " | Priority boarding + Fast Track";

        public override void ShowDetails()
        {
            _wrapped.ShowDetails();
            Console.WriteLine($"  [+ Priority Boarding]    +{PriorityPrice:C}");
        }
    }

    // ── Decorator concret 4 – Masa la bord ───────────────────────────
    public class MealDecorator : FlightServiceDecorator
    {
        private const decimal MealPrice = 15m;
        private readonly string _mealType;

        public MealDecorator(IFlightService wrapped, string mealType = "Standard")
            : base(wrapped) => _mealType = mealType;

        public override string  ServiceName => $"{_wrapped.ServiceName} + Masa({_mealType})";
        public override decimal Price       => _wrapped.Price + MealPrice;

        public override string GetDescription() =>
            _wrapped.GetDescription() + $" | Masa la bord ({_mealType})";

        public override void ShowDetails()
        {
            _wrapped.ShowDetails();
            Console.WriteLine($"  [+ Masa la bord ({_mealType})]  +{MealPrice:C}");
        }
    }

    // ── Decorator concret 5 – Lounge Access ──────────────────────────
    public class LoungeAccessDecorator : FlightServiceDecorator
    {
        private const decimal LoungePrice = 45m;

        public LoungeAccessDecorator(IFlightService wrapped) : base(wrapped) { }

        public override string  ServiceName => $"{_wrapped.ServiceName} + Lounge";
        public override decimal Price       => _wrapped.Price + LoungePrice;

        public override string GetDescription() =>
            _wrapped.GetDescription() + " | Acces lounge aeroport";

        public override void ShowDetails()
        {
            _wrapped.ShowDetails();
            Console.WriteLine($"  [+ Lounge Access]        +{LoungePrice:C}");
        }
    }
}
