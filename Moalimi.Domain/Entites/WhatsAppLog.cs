namespace Moalimi.Domain.Entites
{
    public class WhatsAppLog
    {
        public int Id { get; set; }
        public string Recipient { get; set; } = string.Empty;      // رقم المستلم
        public string Message { get; set; } = string.Empty;
        public bool IsSuccess { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
    }
}
