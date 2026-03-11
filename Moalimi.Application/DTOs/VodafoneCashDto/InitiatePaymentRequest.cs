namespace Moalimi.Application.DTOs.VodafoneCashDto
{
    public record InitiatePaymentRequest(
     string MsisdnNumber,
     decimal Amount,
     string Description
 );
}
