using FlightBooking.Interfaces;
using FlightBooking.Models;

namespace FlightBooking.Models
{
    // ══════════════════════════════════════════════════════════════════
    //  PROTOTYPE PATTERN
    //
    //  Problema rezolvata: compania aeriana opereaza zboruri regulate
    //  (ex. KIV→OTP in fiecare zi la 08:00). In loc sa construim de la
    //  zero fiecare zbor, clonam un "template" si ajustam doar data.
    //
    //  ShallowCopy: copiaza campurile primitive + refoloseste referintele
    //               la Airport (imutabile — safe).
    //  DeepCopy:    creeaza Airports noi — utila daca Airport ar deveni
    //               mutabil in viitor.
    // ══════════════════════════════════════════════════════════════════

    // SRP: reprezinta un sablon de zbor clonabil
    public class FlightTemplate : IPrototype<FlightTemplate>
    {
        public string    FlightNumber  { get; set; }
        public Airport   Origin        { get; set; }
        public Airport   Destination   { get; set; }
        public TimeSpan  Duration      { get; set; }      // durata fixa a zborului
        public TimeOnly  DepartureTime { get; set; }      // ora de decolare (fara data)
        public SeatClass Class         { get; set; }
        public decimal   BasePrice     { get; set; }
        public int       TotalSeats    { get; set; }

        public FlightTemplate(
            string flightNumber, Airport origin, Airport destination,
            TimeOnly departureTime, TimeSpan duration,
            SeatClass seatClass, decimal basePrice, int totalSeats)
        {
            FlightNumber  = flightNumber;
            Origin        = origin;
            Destination   = destination;
            DepartureTime = departureTime;
            Duration      = duration;
            Class         = seatClass;
            BasePrice     = basePrice;
            TotalSeats    = totalSeats;
        }

        // ── Instantiaza un Flight real dintr-un template + data concreta ──
        public Flight CreateFlightForDate(DateTime date)
        {
            var departure = date.Date
                .AddHours(DepartureTime.Hour)
                .AddMinutes(DepartureTime.Minute);
            var arrival = departure + Duration;

            // Numarul de zbor include data (ex. MV101-20260110)
            var flightNumber = $"{FlightNumber}-{date:yyyyMMdd}";

            return new Flight(flightNumber, Origin, Destination,
                              departure, arrival, Class, BasePrice, TotalSeats);
        }

        // ── Shallow Copy ─────────────────────────────────────────────
        // Campurile Airport sunt imutabile → safe sa le reutilizam
        public FlightTemplate ShallowCopy()
        {
            return (FlightTemplate)MemberwiseClone();
        }

        // ── Deep Copy ────────────────────────────────────────────────
        // Creeaza obiecte Airport noi (util daca Airport devine mutabil)
        public FlightTemplate DeepCopy()
        {
            return new FlightTemplate(
                FlightNumber,
                new Airport(Origin.Code, Origin.Name, Origin.City, Origin.Country),
                new Airport(Destination.Code, Destination.Name, Destination.City, Destination.Country),
                DepartureTime,
                Duration,
                Class,
                BasePrice,
                TotalSeats
            );
        }

        public override string ToString() =>
            $"[Template] {FlightNumber} | {Origin.Code}→{Destination.Code} | " +
            $"{DepartureTime:HH:mm} | {Duration:hh\\:mm}h | {Class} | {BasePrice:C}";
    }
}
