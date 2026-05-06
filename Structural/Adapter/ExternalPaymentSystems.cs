namespace FlightBooking.Structural.Adapter
{
    // ══════════════════════════════════════════════════════════════════
    //  ADAPTEE-uri — clase externe cu interfete incompatibile
    //  Simuleaza SDK-uri third-party pe care NU le putem modifica.
    //  Fiecare are propria metoda cu propria semnatura.
    // ══════════════════════════════════════════════════════════════════

    // Adaptee 1 – SDK PayPal (metoda: ExecutePayment)
    public class PayPalGateway
    {
        public string ExecutePayment(string payerEmail, double amountUsd, string description)
        {
            Console.WriteLine($"  [PayPal SDK] Platit {amountUsd:F2} USD de la {payerEmail} | {description}");
            return $"PAYPAL-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
        }

        public bool RefundTransaction(string transactionId, double amount)
        {
            Console.WriteLine($"  [PayPal SDK] Rambursare {amount:F2} USD | TxID: {transactionId}");
            return true;
        }
    }

    // Adaptee 2 – SDK Stripe (metoda: ChargeCard)
    public class StripeGateway
    {
        public StripeResult ChargeCard(long amountCents, string currency,
                                       string cardToken, string metadata)
        {
            Console.WriteLine($"  [Stripe SDK] Charged {amountCents} {currency} | Token:{cardToken} | {metadata}");
            return new StripeResult
            {
                ChargeId   = $"ch_{Guid.NewGuid().ToString("N")[..16]}",
                Success    = true,
                AmountPaid = amountCents / 100.0
            };
        }

        public bool CreateRefund(string chargeId, long amountCents)
        {
            Console.WriteLine($"  [Stripe SDK] Refund {amountCents} cents | ChargeID: {chargeId}");
            return true;
        }
    }

    public class StripeResult
    {
        public string ChargeId   { get; set; } = string.Empty;
        public bool   Success    { get; set; }
        public double AmountPaid { get; set; }
    }

    // Adaptee 3 – SDK Maib (banca locala Moldova, API SOAP-style)
    public class MaibBankGateway
    {
        public MaibResponse ProcessTransaction(MaibRequest request)
        {
            Console.WriteLine($"  [MAIB SDK] Tranzactie {request.Amount} MDL | " +
                              $"Card: {request.CardLastFour} | Ref: {request.ReferenceCode}");
            return new MaibResponse
            {
                ResponseCode  = "00",
                ApprovalCode  = $"MAIB{new Random().Next(100000, 999999)}",
                TransactionId = Guid.NewGuid().ToString("N")[..12].ToUpper()
            };
        }

        public MaibResponse ReverseTransaction(string transactionId)
        {
            Console.WriteLine($"  [MAIB SDK] Reversare tranzactie {transactionId}");
            return new MaibResponse { ResponseCode = "00", ApprovalCode = "REVERSED" };
        }
    }

    public class MaibRequest
    {
        public decimal Amount        { get; set; }
        public string  CardLastFour  { get; set; } = "0000";
        public string  ReferenceCode { get; set; } = string.Empty;
        public string  Description   { get; set; } = string.Empty;
    }

    public class MaibResponse
    {
        public string ResponseCode  { get; set; } = string.Empty;
        public string ApprovalCode  { get; set; } = string.Empty;
        public string TransactionId { get; set; } = string.Empty;
        public bool   IsSuccess     => ResponseCode == "00";
    }
}
