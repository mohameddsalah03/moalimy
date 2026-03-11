namespace Moalimi.Application.DTOs.WhatsAppDto
{
    public record SendPaymentConfirmationRequest(
     string PhoneNumber,
     string UserName,
     decimal Amount,
     string TransactionId
    );

}