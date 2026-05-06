namespace FlightBooking.Services
{
    // ══════════════════════════════════════════════════════════════════
    //  SINGLETON PATTERN  (Thread-Safe cu Lazy<T>)
    //
    //  Problema rezolvata: sistemul de logging trebuie sa fie unic —
    //  toti componentii scriu in acelasi log, fara sa creeze instante
    //  multiple care s-ar suprascrie sau duplica intrarile.
    //
    //  Implementare: Lazy<T> garanteaza:
    //    1. O singura instantiere (thread-safe by default)
    //    2. Initializare amanata (lazy) — instanta se creeaza doar la
    //       prima accesare, nu la incarcarea clasei.
    //    3. Constructor privat — previne instantierea directa din exterior.
    // ══════════════════════════════════════════════════════════════════

    public enum LogLevel { Info, Warning, Error }

    public class AppLogger
    {
        // ── Instanta unica, thread-safe, lazy ───────────────────────
        private static readonly Lazy<AppLogger> _instance =
            new(() => new AppLogger(), isThreadSafe: true);

        // Proprietate de acces globala
        public static AppLogger Instance => _instance.Value;

        // ── Starea interna ───────────────────────────────────────────
        private readonly List<LogEntry> _entries = new();
        private readonly object         _lock    = new();   // lock pentru multi-threading

        // ── Constructor privat (Singleton: nimeni nu poate face `new AppLogger()`) ──
        private AppLogger()
        {
            Log(LogLevel.Info, "AppLogger", "Logger initializat.");
        }

        // ── Metoda principala de logging (thread-safe) ───────────────
        public void Log(LogLevel level, string source, string message)
        {
            var entry = new LogEntry(level, source, message);
            lock (_lock)
            {
                _entries.Add(entry);
            }
            var color = level switch
            {
                LogLevel.Warning => ConsoleColor.Yellow,
                LogLevel.Error   => ConsoleColor.Red,
                _                => ConsoleColor.Gray
            };
            var prev = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine($"  [{entry.Timestamp:HH:mm:ss}] [{level}] {source}: {message}");
            Console.ForegroundColor = prev;
        }

        public void Info(string source, string message)    => Log(LogLevel.Info,    source, message);
        public void Warning(string source, string message) => Log(LogLevel.Warning, source, message);
        public void Error(string source, string message)   => Log(LogLevel.Error,   source, message);

        // ── Acces la istoricul logurilor ─────────────────────────────
        public IReadOnlyList<LogEntry> GetEntries()
        {
            lock (_lock) { return _entries.ToList().AsReadOnly(); }
        }

        public IEnumerable<LogEntry> GetByLevel(LogLevel level)
            => GetEntries().Where(e => e.Level == level);

        public int EntryCount
        {
            get { lock (_lock) { return _entries.Count; } }
        }
    }

    // ── Intrare in log ───────────────────────────────────────────────
    public record LogEntry(LogLevel Level, string Source, string Message)
    {
        public DateTime Timestamp { get; } = DateTime.UtcNow;
        public override string ToString() =>
            $"[{Timestamp:HH:mm:ss}][{Level}] {Source}: {Message}";
    }
}
