namespace Moalimi.Domain.Entites
{
    public class PaymentRecord
    {
        public int Id { get; set; }
        public string OrderId { get; set; } = string.Empty;
        public string MsisdnNumber { get; set; } = string.Empty;   // رقم فودافون كاش
        public decimal Amount { get; set; }
        public string Status { get; set; } = "Pending";            // Pending / Processing / Completed / Failed
        public string? TransactionId { get; set; }                 // يرجع من فودافون
        public string? FailureReason { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? PaidAt { get; set; }
    }
}
