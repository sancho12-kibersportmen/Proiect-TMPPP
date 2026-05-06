using FlightBooking.Interfaces;
using FlightBooking.Structural.Adapter;

namespace FlightBooking.Structural.Adapter
{
    // ══════════════════════════════════════════════════════════════════
    //  ADAPTER PATTERN
    //
    //  Problema rezolvata: aplicatia noastra trebuie sa plateasca prin
    //  PayPal, Stripe si MAIB, dar fiecare SDK are o interfata complet
    //  diferita. In loc sa scriem cod special pentru fiecare, cream
    //  adaptori care "traduc" IPaymentProcessor -> SDK-ul specific.
    //
    //  Clientul (PaymentService) nu stie de PayPal/Stripe/MAIB —
    //  lucreaza exclusiv cu IPaymentProcessor.
    // ══════════════════════════════════════════════════════════════════

    // ── Adaptor 1 – PayPal ───────────────────────────────────────────
    public class PayPalAdapter : IPaymentProcessor
    {
        private readonly PayPalGateway _payPal;
        public string ProcessorName => "PayPal";

        // Adaptee injectat prin DIP
        public PayPalAdapter(PayPalGateway payPal)
            => _payPal = payPal ?? throw new ArgumentNullException(nameof(payPal));

        // Traduce IPaymentProcessor.ProcessPayment -> PayPalGateway.ExecutePayment
        public string ProcessPayment(string customerId, decimal amount, string description)
        {
            // Adaptare: decimal -> double, customerId ca email PayPal
            return _payPal.ExecutePayment(customerId, (double)amount, description);
        }

        // Traduce IPaymentProcessor.Refund -> PayPalGateway.RefundTransaction
        public bool Refund(string transactionId, decimal amount)
            => _payPal.RefundTransaction(transactionId, (double)amount);
    }

    // ── Adaptor 2 – Stripe ───────────────────────────────────────────
    public class StripeAdapter : IPaymentProcessor
    {
        private readonly StripeGateway _stripe;
        public string ProcessorName => "Stripe";

        public StripeAdapter(StripeGateway stripe)
            => _stripe = stripe ?? throw new ArgumentNullException(nameof(stripe));

        // Traduce: decimal EUR -> long cents, adauga card token placeholder
        public string ProcessPayment(string customerId, decimal amount, string description)
        {
            var cents  = (long)(amount * 100);
            var result = _stripe.ChargeCard(cents, "EUR", $"tok_{customerId[..Math.Min(8,customerId.Length)]}", description);
            return result.Success ? result.ChargeId : throw new InvalidOperationException("Plata Stripe esuata.");
        }

        public bool Refund(string transactionId, decimal amount)
            => _stripe.CreateRefund(transactionId, (long)(amount * 100));
    }

    // ── Adaptor 3 – MAIB (banca locala Moldova) ──────────────────────
    public class MaibAdapter : IPaymentProcessor
    {
        private readonly MaibBankGateway _maib;
        public string ProcessorName => "MAIB";

        public MaibAdapter(MaibBankGateway maib)
            => _maib = maib ?? throw new ArgumentNullException(nameof(maib));

        // Traduce: IPaymentProcessor -> MaibRequest/MaibResponse
        public string ProcessPayment(string customerId, decimal amount, string description)
        {
            var request = new MaibRequest
            {
                Amount        = amount,
                CardLastFour  = customerId.Length >= 4 ? customerId[^4..] : "0000",
                ReferenceCode = $"FLT-{DateTime.UtcNow:yyyyMMddHHmmss}",
                Description   = description
            };
            var response = _maib.ProcessTransaction(request);
            if (!response.IsSuccess)
                throw new InvalidOperationException($"Plata MAIB respinsa. Cod: {response.ResponseCode}");
            return response.TransactionId;
        }

        public bool Refund(string transactionId, decimal amount)
        {
            var response = _maib.ReverseTransaction(transactionId);
            return response.IsSuccess;
        }
    }
}
