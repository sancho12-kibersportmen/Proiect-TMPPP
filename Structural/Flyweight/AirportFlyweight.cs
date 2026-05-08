using System.Runtime.CompilerServices;
using FlightBooking.Models;

namespace FlightBooking.Structural.Flyweight
{
    // FLYWEIGHT PATTERN
    // Stare intrinseca (partajata): Code, Name, City, Country
    // Stare extrinseca (in client): FlightNumber, DepartureTime, Price

    // Flyweight Factory
    public class AirportFlyweightFactory
    {
        private readonly Dictionary<string, Airport> _pool =
            new(StringComparer.OrdinalIgnoreCase);
        private int _requestCount = 0;
        private int _newCount     = 0;

        public Airport GetAirport(string code, string name, string city, string country)
        {
            _requestCount++;
            if (_pool.TryGetValue(code, out var existing))
                return existing;
            var airport = new Airport(code, name, city, country);
            _pool[code] = airport;
            _newCount++;
            return airport;
        }

        public Airport GetAirport(string code)
        {
            if (_pool.TryGetValue(code, out var airport)) return airport;
            throw new KeyNotFoundException($"Aeroportul '{code}' nu e in pool.");
        }

        public int PoolSize     => _pool.Count;
        public int RequestCount => _requestCount;
        public int NewCount     => _newCount;
        public int ReusedCount  => _requestCount - _newCount;

        public void PrintStats()
        {
            Console.WriteLine("  [Flyweight Stats]");
            Console.WriteLine($"    Instante unice in pool : {PoolSize}");
            Console.WriteLine($"    Total cereri           : {RequestCount}");
            Console.WriteLine($"    Instante noi create    : {NewCount}");
            Console.WriteLine($"    Instante reutilizate   : {ReusedCount}");
            Console.WriteLine($"    Memorie economisita    : ~{ReusedCount * 120} bytes");
        }

        public void PrintPool()
        {
            Console.WriteLine($"  Pool aeroporturi ({PoolSize} instante unice):");
            foreach (var (code, ap) in _pool)
                Console.WriteLine($"    {code} -> {ap}  [HashCode: {RuntimeHelpers.GetHashCode(ap)}]");
        }
    }

    // RouteKey Flyweight — ruta partajata
    public class RouteKey
    {
        public string OriginCode      { get; }
        public string DestinationCode { get; }
        private RouteKey(string o, string d) { OriginCode = o; DestinationCode = d; }
        public override string ToString() => $"{OriginCode}->{DestinationCode}";

        private static readonly Dictionary<string, RouteKey> _pool = new();

        public static RouteKey Get(string origin, string destination)
        {
            var key = $"{origin}|{destination}";
            if (!_pool.TryGetValue(key, out var route))
            {
                route = new RouteKey(origin, destination);
                _pool[key] = route;
            }
            return route;
        }

        public static int PoolSize => _pool.Count;
    }
}
