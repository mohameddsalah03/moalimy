namespace Moalimi.Application.DTOs.VodafoneCashDto
{
    public record VerifyOtpRequest(
    int PaymentId,
    string TransactionId,
    string OtpCode
);

}
