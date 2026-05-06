using FlightBooking.Interfaces;

namespace FlightBooking.Structural.Composite
{
    // ── Composite: un itinerar care contine alte componente ──────────
    // Poate contine FlightSegment-uri (Leaf) SAU alte Itinerary-uri (Composite)
    public class Itinerary : IItineraryComponent
    {
        private readonly List<IItineraryComponent> _children = new();

        public string Name { get; }

        public Itinerary(string name) =>
            Name = name ?? throw new ArgumentNullException(nameof(name));

        // ── Gestionare copii ─────────────────────────────────────────
        public void Add(IItineraryComponent component)
        {
            if (component == null) throw new ArgumentNullException(nameof(component));
            _children.Add(component);
        }

        public void Remove(IItineraryComponent component) => _children.Remove(component);

        public IReadOnlyList<IItineraryComponent> Children => _children.AsReadOnly();

        // ── Proprietati agregate (calcul recursiv pe arbore) ─────────
        public decimal  TotalPrice    => _children.Sum(c => c.TotalPrice);
        public TimeSpan TotalDuration => TimeSpan.FromTicks(_children.Sum(c => c.TotalDuration.Ticks));
        public int      StopCount     => _children.Count - 1 + _children.Sum(c => c.StopCount);

        // ── Afisare ierarhica ────────────────────────────────────────
        public void Display(int indent = 0)
        {
            var pad = new string(' ', indent * 2);
            Console.WriteLine($"{pad}📋 {Name}");
            Console.WriteLine($"{pad}   Pret total  : {TotalPrice:C}");
            Console.WriteLine($"{pad}   Durata total: {TotalDuration:hh\\:mm}h");
            Console.WriteLine($"{pad}   Segmente    : {_children.Count}");
            Console.WriteLine();

            foreach (var child in _children)
            {
                child.Display(indent + 1);
                Console.WriteLine();
            }
        }
    }
}
