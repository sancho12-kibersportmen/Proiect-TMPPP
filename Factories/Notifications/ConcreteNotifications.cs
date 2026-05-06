using FlightBooking.Interfaces;

namespace FlightBooking.Factories.Notifications
{
    // ── Produse concrete (Factory Method) ───────────────────────────────

    // Produs concret 1 – notificare Email
    public class EmailNotification : INotification
    {
        public string Channel => "Email";

        public void Send(string recipient, string subject, string body)
        {
            Console.WriteLine($"  [EMAIL] Catre: {recipient}");
            Console.WriteLine($"          Subiect: {subject}");
            Console.WriteLine($"          Mesaj: {body}");
        }
    }

    // Produs concret 2 – notificare SMS
    public class SmsNotification : INotification
    {
        public string Channel => "SMS";

        public void Send(string recipient, string subject, string body)
        {
            // SMS-ul trimite doar un rezumat scurt (max 160 caractere)
            var shortBody = body.Length > 120 ? body[..120] + "..." : body;
            Console.WriteLine($"  [SMS] Catre: {recipient} | {shortBody}");
        }
    }

    // Produs concret 3 – notificare Push (aplicatie mobila)
    public class PushNotification : INotification
    {
        public string Channel => "Push";

        public void Send(string recipient, string subject, string body)
        {
            Console.WriteLine($"  [PUSH] 🔔 {subject} → {recipient}");
        }
    }
}
