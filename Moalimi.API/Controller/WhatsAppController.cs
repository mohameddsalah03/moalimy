using Microsoft.AspNetCore.Mvc;
using Moalimi.Application.DTOs.WhatsAppDto;
using Moalimi.Application.Interfaces;

namespace Moalimi.API.Controller;

[ApiController]
[Route("api/whatsapp")]
public class WhatsAppController : ControllerBase
{
    private readonly IWhatsAppService _service;

    public WhatsAppController(IWhatsAppService service) => _service = service;

    /// <summary>رسالة نصية حرة</summary>
    [HttpPost("send")]
    public async Task<IActionResult> Send(
        [FromBody] SendMessageRequest req, CancellationToken ct)
    {
        var result = await _service.SendMessageAsync(req, ct);
        return Ok(result);
    }

    /// <summary>تأكيد دفع</summary>
    [HttpPost("payment-confirmation")]
    public async Task<IActionResult> PaymentConfirmation(
        [FromBody] SendPaymentConfirmationRequest req, CancellationToken ct)
    {
        var result = await _service.SendPaymentConfirmationAsync(req, ct);
        return Ok(result);
    }

    /// <summary>تذكير بموعد حصة</summary>
    [HttpPost("session-reminder")]
    public async Task<IActionResult> SessionReminder(
        [FromBody] SendSessionReminderRequest req, CancellationToken ct)
    {
        var result = await _service.SendSessionReminderAsync(req, ct);
        return Ok(result);
    }

    /// <summary>إشعار هدية تعليمية</summary>
    [HttpPost("gift")]
    public async Task<IActionResult> Gift(
        [FromBody] SendGiftRequest req, CancellationToken ct)
    {
        var result = await _service.SendGiftNotificationAsync(req, ct);
        return Ok(result);
    }
}