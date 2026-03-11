using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moalimi.Application.DTOs.Settings;
using Moalimi.Application.DTOs.WhatsAppDto;
using Moalimi.Application.Interfaces;
using Moalimi.Domain.Entites;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Moalimi.Application.Services;

public class WhatsAppService : IWhatsAppService
{
    private readonly HttpClient _http;
    private readonly WhatsAppSettings _settings;
    private readonly IAppDbContext _db;
    private readonly ILogger<WhatsAppService> _logger;

    private string ApiUrl => $"https://graph.facebook.com/v19.0/{_settings.PhoneNumberId}/messages";

    public WhatsAppService(
        HttpClient http,
        IOptions<WhatsAppSettings> options,
        IAppDbContext db,
        ILogger<WhatsAppService> logger)
    {
        _http = http;
        _settings = options.Value;
        _db = db;
        _logger = logger;
        _http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _settings.AccessToken);
    }

    public async Task<WhatsAppResponse> SendMessageAsync(
        SendMessageRequest req, CancellationToken ct = default)
    {
        var phone = NormalizePhone(req.PhoneNumber);

        if (_settings.IsSandbox)
        {
            _logger.LogInformation("[SANDBOX] WhatsApp → {Phone}:\n{Msg}",
                MaskPhone(phone), req.Message);
            await SaveLogAsync(phone, req.Message, true, ct: ct);
            return new WhatsAppResponse(true, null);
        }

        var payload = new
        {
            messaging_product = "whatsapp",
            recipient_type = "individual",
            to = phone,
            type = "text",
            text = new { preview_url = false, body = req.Message }
        };

        return await PostAsync(phone, req.Message, payload, ct);
    }

    public Task<WhatsAppResponse> SendPaymentConfirmationAsync(
        SendPaymentConfirmationRequest req, CancellationToken ct = default) =>
        SendMessageAsync(new SendMessageRequest(req.PhoneNumber, $"""
 *تم تأكيد دفعتك!*

مرحباً {req.UserName}،
 المبلغ: {req.Amount:F2} ج.م
 رقم المعاملة: {req.TransactionId}
 {DateTime.Now:dd/MM/yyyy HH:mm}

شكراً لاختيارك منصة حصتك 
"""), ct);

    public Task<WhatsAppResponse> SendSessionReminderAsync(
        SendSessionReminderRequest req, CancellationToken ct = default) =>
        SendMessageAsync(new SendMessageRequest(req.PhoneNumber, $"""
 *تذكير بموعد حصتك!*

مرحباً {req.StudentName}،
حصتك مع *{req.TeacherName}*:
 {req.SessionTime:dd/MM/yyyy}   {req.SessionTime:HH:mm}

احرص على الانضمام في الوقت 
"""), ct);

    public Task<WhatsAppResponse> SendGiftNotificationAsync(
        SendGiftRequest req, CancellationToken ct = default) =>
        SendMessageAsync(new SendMessageRequest(req.RecipientPhone, $"""
 *لديك هدية تعليمية من حصتك!*

أهداك *{req.SenderName}* باقة:
 *{req.PackageName}*

كود الهدية: 🔑 *{req.GiftCode}*

فعّل اشتراكك على  www.hissatak.online
"""), ct);

    // ── Private ──

    private async Task<WhatsAppResponse> PostAsync(
        string phone, string logMsg, object payload, CancellationToken ct)
    {
        try
        {
            var content = new StringContent(
                JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var response = await _http.PostAsync(ApiUrl, content, ct);
            var body = await response.Content.ReadAsStringAsync(ct);
            var ok = response.IsSuccessStatusCode;

            if (!ok)
                _logger.LogWarning("WhatsApp failed: {Status} — {Body}", response.StatusCode, body);

            await SaveLogAsync(phone, logMsg, ok, ok ? null : body, ct);
            return new WhatsAppResponse(ok, ok ? null : body);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "WhatsApp exception");
            await SaveLogAsync(phone, logMsg, false, ex.Message, ct);
            return new WhatsAppResponse(false, ex.Message);
        }
    }

    private async Task SaveLogAsync(string recipient, string message,
        bool ok, string? error = null, CancellationToken ct = default)
    {
        try
        {
            _db.WhatsAppLogs.Add(new WhatsAppLog
            {
                Recipient = recipient,
                Message = message.Length > 500 ? message[..500] : message,
                IsSuccess = ok,
                ErrorMessage = error
            });
            await _db.SaveChangesAsync(ct);
        }
        catch { }
    }

    private static string NormalizePhone(string phone)
    {
        phone = phone.Replace(" ", "").Replace("-", "");
        if (phone.StartsWith("01") && phone.Length == 11) return "+20" + phone[1..];
        if (phone.StartsWith("05") && phone.Length == 10) return "+966" + phone[1..];
        if (!phone.StartsWith("+")) return "+" + phone;
        return phone;
    }

    private static string MaskPhone(string p) =>
        p.Length < 6 ? p : p[..4] + "****" + p[^3..];
}