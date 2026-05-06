using FlightBooking.Interfaces;
using FlightBooking.Models;

namespace FlightBooking.Services
{
    // SRP: gestioneaza exclusiv platile rezervarilor
    // DIP: depinde de IPaymentProcessor (nu stie de PayPal/Stripe/MAIB)
    public class PaymentService
    {
        private readonly IPaymentProcessor _processor;

        public PaymentService(IPaymentProcessor processor)
            => _processor = processor ?? throw new ArgumentNullException(nameof(processor));

        public string PayForReservation(Reservation reservation, string customerId)
        {
            AppLogger.Instance.Info("PaymentService",
                $"Procesare plata {reservation.TotalPrice:C} prin {_processor.ProcessorName} " +
                $"pentru rezervare #{reservation.ReservationId}");

            var txId = _processor.ProcessPayment(
                customerId,
                reservation.TotalPrice,
                $"Rezervare #{reservation.ReservationId} – {reservation.Tickets.Count} bilet(e)"
            );

            AppLogger.Instance.Info("PaymentService",
                $"Plata reusita! TxID: {txId} | Processor: {_processor.ProcessorName}");

            return txId;
        }

        public bool RefundReservation(string transactionId, decimal amount)
        {
            AppLogger.Instance.Warning("PaymentService",
                $"Initiere rambursare {amount:C} | TxID: {transactionId}");
            return _processor.Refund(transactionId, amount);
        }
    }
}
