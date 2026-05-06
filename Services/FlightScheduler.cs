using FlightBooking.Interfaces;
using FlightBooking.Models;

namespace FlightBooking.Services
{
    // SRP: genereaza zboruri regulate pe baza templateurilor (Prototype)
    // DIP: stocheaza templateuri si repo prin interfete
    public class FlightScheduler
    {
        private readonly List<FlightTemplate>  _templates   = new();
        private readonly IFlightRepository     _flightRepo;

        public FlightScheduler(IFlightRepository flightRepo)
        {
            _flightRepo = flightRepo ?? throw new ArgumentNullException(nameof(flightRepo));
        }

        // Inregistreaza un template de zbor
        public void RegisterTemplate(FlightTemplate template)
            => _templates.Add(template);

        // Genereaza zboruri pentru o perioada (clonam template-ul pentru fiecare zi)
        public IEnumerable<Flight> GenerateSchedule(DateTime from, DateTime to)
        {
            var generated = new List<Flight>();

            foreach (var template in _templates)
            {
                for (var date = from.Date; date <= to.Date; date = date.AddDays(1))
                {
                    // Prototype: clonam template-ul si instantiem un zbor concret
                    var cloned = template.DeepCopy();
                    var flight = cloned.CreateFlightForDate(date);
                    _flightRepo.Add(flight);
                    generated.Add(flight);
                }
            }

            return generated;
        }

        public IReadOnlyList<FlightTemplate> Templates => _templates.AsReadOnly();
    }
}
