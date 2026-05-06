using FlightBooking.Interfaces;

namespace FlightBooking.Structural.Composite
{
    // ── Composite de nivel superior: pachet de calatorie ────────────
    // Contine mai multe Itinerary-uri (dus + intors, sau mai multe destinatii)
    // Demonstreaza ca Composite-ul poate contine si alte Composite-uri.
    public class TravelPackage : IItineraryComponent
    {
        private readonly List<IItineraryComponent> _components = new();
        private readonly decimal _discountPercent;

        public string Name { get; }

        public TravelPackage(string name, decimal discountPercent = 0m)
        {
            Name             = name;
            _discountPercent = discountPercent;
        }

        public void Add(IItineraryComponent component)    => _components.Add(component);
        public void Remove(IItineraryComponent component) => _components.Remove(component);

        // Pretul total include reducerea de pachet
        public decimal  TotalPrice    => _components.Sum(c => c.TotalPrice) * (1 - _discountPercent / 100);
        public TimeSpan TotalDuration => TimeSpan.FromTicks(_components.Sum(c => c.TotalDuration.Ticks));
        public int      StopCount     => _components.Sum(c => c.StopCount);

        public void Display(int indent = 0)
        {
            var pad = new string(' ', indent * 2);
            Console.WriteLine($"{pad}🧳 PACHET: {Name}");
            if (_discountPercent > 0)
                Console.WriteLine($"{pad}   Reducere pachet: {_discountPercent}%");
            Console.WriteLine($"{pad}   Pret total (dupa reducere): {TotalPrice:C}");
            Console.WriteLine($"{pad}   Componente: {_components.Count}");
            Console.WriteLine();

            foreach (var comp in _components)
            {
                comp.Display(indent + 1);
            }
        }
    }
}
