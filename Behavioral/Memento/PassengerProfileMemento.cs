using FlightBooking.Services;
namespace FlightBooking.Behavioral.Memento
{
    // ══════════════════════════════════════════════════════════════════
    //  MEMENTO PATTERN
    //
    //  Problema rezolvata: un pasager isi editeaza profilul de calatorie
    //  (preferinte de loc, masa, documente). Vrem sa putem anula
    //  modificarile (Undo) si a restabili orice versiune anterioara
    //  (Save/Load), fara a expune starea interna a obiectului.
    //
    //  Originator  : PassengerProfile (obiectul care isi salveaza starea)
    //  Memento     : ProfileSnapshot (captura imutabila a starii)
    //  Caretaker   : ProfileHistory (gestioneaza lista de snapshot-uri)
    // ══════════════════════════════════════════════════════════════════

    // ── Memento: captura imutabila a starii ──────────────────────────
    public sealed class ProfileSnapshot
    {
        public string   Label         { get; }
        public string   FirstName     { get; }
        public string   LastName      { get; }
        public string   Email         { get; }
        public string   PassportNo    { get; }
        public string   SeatPreference{ get; }
        public string   MealPreference{ get; }
        public string   FrequentFlyer { get; }
        public bool     NewsletterSub { get; }
        public DateTime SavedAt       { get; }

        // Constructor intern – doar PassengerProfile poate crea snapshot-uri
        internal ProfileSnapshot(string label, string firstName, string lastName,
            string email, string passportNo, string seatPref, string mealPref,
            string frequentFlyer, bool newsletter)
        {
            Label          = label;
            FirstName      = firstName;
            LastName       = lastName;
            Email          = email;
            PassportNo     = passportNo;
            SeatPreference = seatPref;
            MealPreference = mealPref;
            FrequentFlyer  = frequentFlyer;
            NewsletterSub  = newsletter;
            SavedAt        = DateTime.UtcNow;
        }

        public override string ToString() =>
            $"[{SavedAt:HH:mm:ss}] '{Label}' | {FirstName} {LastName} | " +
            $"Loc:{SeatPreference} | Masa:{MealPreference} | FF:{FrequentFlyer}";
    }

    // ── Originator: profilul pasagerului ─────────────────────────────
    public class PassengerProfile
    {
        public string FirstName      { get; set; } = string.Empty;
        public string LastName       { get; set; } = string.Empty;
        public string Email          { get; set; } = string.Empty;
        public string PassportNo     { get; set; } = string.Empty;
        public string SeatPreference { get; set; } = "Window";
        public string MealPreference { get; set; } = "Standard";
        public string FrequentFlyer  { get; set; } = string.Empty;
        public bool   NewsletterSub  { get; set; } = false;

        // Salveaza starea curenta intr-un Memento
        public ProfileSnapshot Save(string label = "")
        {
            var snapshot = new ProfileSnapshot(
                string.IsNullOrEmpty(label) ? $"Save_{DateTime.UtcNow:HHmmss}" : label,
                FirstName, LastName, Email, PassportNo,
                SeatPreference, MealPreference, FrequentFlyer, NewsletterSub);
            AppLogger.Instance.Info("PassengerProfile",
                $"Profil salvat: '{snapshot.Label}'");
            return snapshot;
        }

        // Restaureaza starea dintr-un Memento
        public void Restore(ProfileSnapshot snapshot)
        {
            FirstName      = snapshot.FirstName;
            LastName       = snapshot.LastName;
            Email          = snapshot.Email;
            PassportNo     = snapshot.PassportNo;
            SeatPreference = snapshot.SeatPreference;
            MealPreference = snapshot.MealPreference;
            FrequentFlyer  = snapshot.FrequentFlyer;
            NewsletterSub  = snapshot.NewsletterSub;
            AppLogger.Instance.Info("PassengerProfile",
                $"Profil restaurat din: '{snapshot.Label}'");
        }

        public void Print(string prefix = "")
            => Console.WriteLine(
                $"  {prefix}Profil: {FirstName} {LastName} | {Email} | " +
                $"Loc:{SeatPreference} | Masa:{MealPreference} | FF:{FrequentFlyer}");
    }

    // ── Caretaker: gestioneaza istoricul de snapshot-uri ─────────────
    // SRP: stocheaza si ofera acces la snapshot-uri, nu le interpreteaza
    public class ProfileHistory
    {
        private readonly List<ProfileSnapshot>  _history  = new();
        private int                             _position = -1;

        // Adauga un nou snapshot (taie Redo-ul)
        public void Push(ProfileSnapshot snapshot)
        {
            // Stergem orice snapshot "viitor" (dupa pozitia curenta)
            if (_position < _history.Count - 1)
                _history.RemoveRange(_position + 1, _history.Count - _position - 1);

            _history.Add(snapshot);
            _position = _history.Count - 1;
            Console.WriteLine($"  [History] Saved: {snapshot}");
        }

        // Undo: pas inapoi in istoric
        public ProfileSnapshot? Undo()
        {
            if (_position <= 0)
            {
                Console.WriteLine("  [History] Nu exista versiune anterioara.");
                return null;
            }
            _position--;
            Console.WriteLine($"  [History] Undo la: {_history[_position].Label}");
            return _history[_position];
        }

        // Redo: pas inainte in istoric
        public ProfileSnapshot? Redo()
        {
            if (_position >= _history.Count - 1)
            {
                Console.WriteLine("  [History] Nu exista versiune urmatoare.");
                return null;
            }
            _position++;
            Console.WriteLine($"  [History] Redo la: {_history[_position].Label}");
            return _history[_position];
        }

        // Incarca un snapshot dupa eticheta
        public ProfileSnapshot? LoadByLabel(string label)
            => _history.FirstOrDefault(s => s.Label == label);

        public void PrintHistory()
        {
            Console.WriteLine($"  [History] {_history.Count} versiuni salvate:");
            for (int i = 0; i < _history.Count; i++)
            {
                var marker = i == _position ? " <-- CURRENT" : "";
                Console.WriteLine($"    [{i}] {_history[i]}{marker}");
            }
        }

        public int Count    => _history.Count;
        public int Position => _position;
    }
}

