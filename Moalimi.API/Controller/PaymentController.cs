using Microsoft.AspNetCore.Mvc;
using Moalimi.Application.DTOs.VodafoneCashDto;
using Moalimi.Application.Interfaces;

namespace Moalimi.API.Controller;

[ApiController]
[Route("api/payment")]
public class PaymentController : ControllerBase
{
    private readonly IVodafoneCashService _service;

    public PaymentController(IVodafoneCashService service) => _service = service;

    [HttpPost("initiate")]
    public async Task<IActionResult> Initiate(
        [FromBody] InitiatePaymentRequest req, CancellationToken ct)
    {
        var result = await _service.InitiateAsync(req, ct);
        return Ok(result);
    }

    [HttpPost("verify-otp")]
    public async Task<IActionResult> VerifyOtp(
        [FromBody] VerifyOtpRequest req, CancellationToken ct)
    {
        var result = await _service.VerifyOtpAsync(req, ct);
        return Ok(result);
    }
}