using FlightBooking.Interfaces;
using FlightBooking.Models;
using FlightBooking.Services;

namespace FlightBooking.Structural.Proxy
{
    // ══════════════════════════════════════════════════════════════════
    //  PROXY PATTERN  (3 tipuri)
    //
    //  1. VIRTUAL PROXY (CachedFlightSearchProxy)
    //     Problema: cautarea zborurilor e costisitoare (baza de date,
    //     filtrari complexe). Aceleasi query-uri sunt repetate des.
    //     Solutia: proxy-ul cachueaza rezultatele si le returneaza
    //     instant la cereri repetate, fara a accesa serviciul real.
    //
    //  2. PROTECTION PROXY (AuthenticatedBookingProxy)
    //     Problema: nu toti utilizatorii pot face rezervari (ex. cont
    //     suspendat, varsta minima, zona restrictionata).
    //     Solutia: proxy-ul verifica drepturile INAINTE de a delega
    //     operatia catre serviciul real.
    //
    //  3. LOGGING PROXY (LoggingFlightSearchProxy)
    //     Problema: vrem sa monitorizam toate cautarile fara a modifica
    //     FlightSearchService (OCP).
    //     Solutia: proxy-ul logheaza fiecare cerere si raspuns, apoi
    //     delega catre serviciul real.
    // ══════════════════════════════════════════════════════════════════

    // ── 1. Virtual Proxy – Cache pentru cautare zboruri ──────────────
    public class CachedFlightSearchProxy
    {
        private readonly FlightSearchService _realService;
        private readonly Dictionary<string, CacheEntry> _cache = new();
        private readonly TimeSpan _cacheDuration;

        private int _cacheHits   = 0;
        private int _cacheMisses = 0;
        public  int ReusedCount  => _cacheHits;

        public CachedFlightSearchProxy(FlightSearchService realService,
                                       TimeSpan? cacheDuration = null)
        {
            _realService   = realService ?? throw new ArgumentNullException(nameof(realService));
            _cacheDuration = cacheDuration ?? TimeSpan.FromMinutes(5);
        }

        public IEnumerable<Flight> Search(string originCode, string destinationCode,
                                          DateTime date, SeatClass? seatClass = null)
        {
            var cacheKey = $"{originCode}|{destinationCode}|{date:yyyyMMdd}|{seatClass}";

            // Verificam cache-ul
            if (_cache.TryGetValue(cacheKey, out var entry) && !entry.IsExpired)
            {
                _cacheHits++;
                AppLogger.Instance.Info("CachedProxy",
                    $"CACHE HIT: {cacheKey} ({entry.Results.Count} rezultate)");
                return entry.Results;
            }

            // Cache miss — apelam serviciul real
            _cacheMisses++;
            AppLogger.Instance.Info("CachedProxy",
                $"CACHE MISS: {cacheKey} — apel catre FlightSearchService");

            var results = _realService.Search(originCode, destinationCode, date, seatClass)
                                      .ToList();

            _cache[cacheKey] = new CacheEntry(results, _cacheDuration);
            return results;
        }

        public void InvalidateCache() => _cache.Clear();

        public void PrintStats()
        {
            Console.WriteLine("  [CachedProxy Stats]");
            Console.WriteLine($"    Cache hits  : {_cacheHits}");
            Console.WriteLine($"    Cache misses: {_cacheMisses}");
            Console.WriteLine($"    Intrari cache: {_cache.Count}");
            var ratio = _cacheHits + _cacheMisses > 0
                ? (double)_cacheHits / (_cacheHits + _cacheMisses) * 100 : 0;
            Console.WriteLine($"    Hit ratio   : {ratio:F1}%");
        }

        private record CacheEntry(List<Flight> Results, TimeSpan Duration)
        {
            private readonly DateTime _createdAt = DateTime.UtcNow;
            public bool IsExpired => DateTime.UtcNow - _createdAt > Duration;
        }
    }

    // ── 2. Protection Proxy – Autorizare pentru rezervari ────────────
    public enum UserRole { Guest, RegisteredUser, PremiumUser, Admin }

    public class UserContext
    {
        public string   UserId   { get; }
        public UserRole Role     { get; }
        public bool     IsActive { get; }

        public UserContext(string userId, UserRole role, bool isActive = true)
        {
            UserId   = userId;
            Role     = role;
            IsActive = isActive;
        }
    }

    public class AuthenticatedBookingProxy
    {
        private readonly IBookingService _realService;
        private readonly UserContext     _user;

        public AuthenticatedBookingProxy(IBookingService realService, UserContext user)
        {
            _realService = realService ?? throw new ArgumentNullException(nameof(realService));
            _user        = user        ?? throw new ArgumentNullException(nameof(user));
        }

        public Reservation CreateReservation(Passenger passenger, Flight flight, string seatNumber)
        {
            // Verificari de securitate INAINTE de a delega
            if (!_user.IsActive)
                throw new UnauthorizedAccessException(
                    $"Contul utilizatorului '{_user.UserId}' este suspendat.");

            if (_user.Role == UserRole.Guest)
                throw new UnauthorizedAccessException(
                    "Utilizatorii Guest nu pot face rezervari. Va rugam sa va inregistrati.");

            if (flight.Class == SeatClass.FirstClass && _user.Role < UserRole.PremiumUser)
                throw new UnauthorizedAccessException(
                    "Biletele First Class sunt disponibile doar pentru utilizatorii Premium.");

            AppLogger.Instance.Info("AuthProxy",
                $"Acces autorizat: {_user.UserId} ({_user.Role}) -> rezervare {flight.FlightNumber}");

            // Delegam catre serviciul real
            return _realService.CreateReservation(passenger, flight, seatNumber);
        }

        public void CancelReservation(string reservationId)
        {
            if (!_user.IsActive)
                throw new UnauthorizedAccessException("Cont suspendat.");

            AppLogger.Instance.Info("AuthProxy",
                $"Anulare autorizata: {_user.UserId} -> rezervare #{reservationId}");

            _realService.CancelReservation(reservationId);
        }

        public Reservation? GetReservation(string reservationId)
            => _realService.GetReservation(reservationId);

        public void ConfirmReservation(string reservationId)
            => _realService.ConfirmReservation(reservationId);
    }

    // ── 3. Logging Proxy – Monitorizare cautari ───────────────────────
    public class LoggingFlightSearchProxy
    {
        private readonly FlightSearchService _realService;
        private readonly List<SearchLog>     _searchLogs = new();

        public LoggingFlightSearchProxy(FlightSearchService realService)
            => _realService = realService ?? throw new ArgumentNullException(nameof(realService));

        public IEnumerable<Flight> Search(string originCode, string destinationCode,
                                          DateTime date, SeatClass? seatClass = null)
        {
            var startTime = DateTime.UtcNow;

            AppLogger.Instance.Info("LoggingProxy",
                $"Cautare: {originCode}->{destinationCode} {date:dd MMM} {seatClass}");

            var results = _realService.Search(originCode, destinationCode, date, seatClass)
                                      .ToList();

            var duration = DateTime.UtcNow - startTime;
            _searchLogs.Add(new SearchLog(originCode, destinationCode, date,
                                          results.Count, duration));

            AppLogger.Instance.Info("LoggingProxy",
                $"Rezultate: {results.Count} zboruri | Durata: {duration.TotalMilliseconds:F1}ms");

            return results;
        }

        public void PrintSearchReport()
        {
            Console.WriteLine($"  [LoggingProxy] Raport cautari ({_searchLogs.Count} total):");
            foreach (var log in _searchLogs)
                Console.WriteLine($"    {log.Origin}->{log.Dest} | " +
                                  $"{log.Date:dd MMM} | {log.ResultCount} rezultate | " +
                                  $"{log.Duration.TotalMilliseconds:F1}ms");
        }

        private record SearchLog(string Origin, string Dest, DateTime Date,
                                  int ResultCount, TimeSpan Duration);
    }
}
