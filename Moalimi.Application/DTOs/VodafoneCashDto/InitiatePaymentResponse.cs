namespace Moalimi.Application.DTOs.VodafoneCashDto
{
    public record InitiatePaymentResponse(
     bool IsSuccess,
     int PaymentId,
     string? TransactionId,
     string? OtpHint,
     string? Error
    );

}
