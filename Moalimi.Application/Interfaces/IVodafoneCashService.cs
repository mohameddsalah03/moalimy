using Moalimi.Application.DTOs.VodafoneCashDto;

namespace Moalimi.Application.Interfaces;

public interface IVodafoneCashService
{
    Task<InitiatePaymentResponse> InitiateAsync(InitiatePaymentRequest req, CancellationToken ct = default);
    Task<VerifyOtpResponse> VerifyOtpAsync(VerifyOtpRequest req, CancellationToken ct = default);
}