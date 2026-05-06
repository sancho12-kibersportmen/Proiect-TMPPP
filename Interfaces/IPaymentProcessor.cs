namespace FlightBooking.Interfaces
{
    // ── Interfata Tinta (Target) ─────────────────────────────────────
    // Aplicatia noastra cunoaste DOAR aceasta interfata.
    // Adaptoarele o implementeaza si traduc catre SDK-urile externe.
    public interface IPaymentProcessor
    {
        string  ProcessPayment(string customerId, decimal amount, string description);
        bool    Refund(string transactionId, decimal amount);
        string  ProcessorName { get; }
    }
}
