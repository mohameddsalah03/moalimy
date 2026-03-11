using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moalimi.Application.DTOs.Settings;
using Moalimi.Application.DTOs.VodafoneCashDto;
using Moalimi.Application.Interfaces;
using Moalimi.Domain.Entites;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace Moalimi.Application.Services;

public class VodafoneCashService : IVodafoneCashService
{
    private readonly HttpClient _http;
    private readonly PaymobSettings _settings;
    private readonly IAppDbContext _db;
    private readonly ILogger<VodafoneCashService> _logger;

    private const string BaseUrl = "https://accept.paymob.com/api";

    public VodafoneCashService(
        HttpClient http,
        IOptions<PaymobSettings> options,
        IAppDbContext db,
        ILogger<VodafoneCashService> logger)
    {
        _http = http;
        _settings = options.Value;
        _db = db;
        _logger = logger;
    }

    public async Task<InitiatePaymentResponse> InitiateAsync(
        InitiatePaymentRequest req, CancellationToken ct = default)
    {
        var orderId = $"ORD-{DateTime.UtcNow:yyyyMMddHHmmss}-{Random.Shared.Next(1000, 9999)}";

        var record = new PaymentRecord
        {
            OrderId = orderId,
            MsisdnNumber = req.MsisdnNumber,
            Amount = req.Amount,
            Status = "Pending"
        };
        _db.Payments.Add(record);
        await _db.SaveChangesAsync(ct);

        if (_settings.IsSandbox)
        {
            var fakeTxId = $"SANDBOX-{DateTime.UtcNow:yyyyMMddHHmmss}";
            record.TransactionId = fakeTxId;
            record.Status = "Processing";
            await _db.SaveChangesAsync(ct);
            _logger.LogInformation("[SANDBOX] Payment initiated. TxId: {TxId}", fakeTxId);
            return new InitiatePaymentResponse(true, record.Id, fakeTxId,
                $"OTP sent to {MaskPhone(req.MsisdnNumber)}", null);
        }

        try
        {
            var token = await GetAuthTokenAsync(ct);
            if (token is null) return await FailAsync(record, "فشل الاتصال بـ Paymob", ct);

            var paymobOrderId = await RegisterOrderAsync(token, orderId, req.Amount, ct);
            if (paymobOrderId is null) return await FailAsync(record, "فشل إنشاء الطلب", ct);

            var paymentKey = await GetPaymentKeyAsync(token, paymobOrderId, req.Amount, req.MsisdnNumber, ct);
            if (paymentKey is null) return await FailAsync(record, "فشل الحصول على مفتاح الدفع", ct);

            var (txId, redirectUrl, error) = await PayWithWalletAsync(paymentKey, req.MsisdnNumber, ct);
            if (txId is null) return await FailAsync(record, error ?? "فشل الدفع", ct);

            record.TransactionId = txId;
            record.Status = "Processing";
            await _db.SaveChangesAsync(ct);

            return new InitiatePaymentResponse(true, record.Id, txId,
                redirectUrl ?? $"OTP sent to {MaskPhone(req.MsisdnNumber)}", null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Payment initiate failed");
            return await FailAsync(record, ex.Message, ct);
        }
    }

    public async Task<VerifyOtpResponse> VerifyOtpAsync(
        VerifyOtpRequest req, CancellationToken ct = default)
    {
        var record = await _db.Payments.FindAsync(new object[] { req.PaymentId }, ct);
        if (record is null)
            return new VerifyOtpResponse(false, "Payment not found");

        if (_settings.IsSandbox)
        {
            if (req.OtpCode == "000000")
            {
                record.Status = "Failed";
                record.FailureReason = "Invalid OTP (sandbox)";
                await _db.SaveChangesAsync(ct);
                return new VerifyOtpResponse(false, "كود OTP غير صحيح");
            }

            record.Status = "Completed";
            record.PaidAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);
            _logger.LogInformation("[SANDBOX] Payment verified. TxId: {TxId}", req.TransactionId);
            return new VerifyOtpResponse(true, null);
        }

        try
        {
            var response = await _http.GetAsync(
                $"{BaseUrl}/acceptance/transactions/{req.TransactionId}", ct);
            var raw = await response.Content.ReadAsStringAsync(ct);
            var data = JsonSerializer.Deserialize<JsonElement>(raw);

            var success = data.TryGetProperty("success", out var s) && s.GetBoolean();
            var pending = data.TryGetProperty("pending", out var p) && p.GetBoolean();

            if (success)
            {
                record.Status = "Completed";
                record.PaidAt = DateTime.UtcNow;
                await _db.SaveChangesAsync(ct);
                return new VerifyOtpResponse(true, null);
            }

            if (pending)
                return new VerifyOtpResponse(false, "العملية لا تزال قيد الانتظار");

            record.Status = "Failed";
            record.FailureReason = "فشلت العملية";
            await _db.SaveChangesAsync(ct);
            return new VerifyOtpResponse(false, "فشلت عملية الدفع");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "OTP verify failed");
            return new VerifyOtpResponse(false, ex.Message);
        }
    }

    // ── Paymob Steps ──

    private async Task<string?> GetAuthTokenAsync(CancellationToken ct)
    {
        var res = await _http.PostAsJsonAsync($"{BaseUrl}/auth/tokens",
            new { api_key = _settings.ApiKey }, ct);
        var data = JsonSerializer.Deserialize<JsonElement>(
            await res.Content.ReadAsStringAsync(ct));
        return data.TryGetProperty("token", out var t) ? t.GetString() : null;
    }

    private async Task<string?> RegisterOrderAsync(string token,
        string merchantOrderId, decimal amount, CancellationToken ct)
    {
        var payload = new
        {
            auth_token = token,
            delivery_needed = false,
            amount_cents = (int)(amount * 100),
            currency = "EGP",
            merchant_order_id = merchantOrderId,
            items = Array.Empty<object>()
        };
        var res = await _http.PostAsJsonAsync($"{BaseUrl}/ecommerce/orders", payload, ct);
        var data = JsonSerializer.Deserialize<JsonElement>(
            await res.Content.ReadAsStringAsync(ct));
        return data.TryGetProperty("id", out var id) ? id.GetInt32().ToString() : null;
    }

    private async Task<string?> GetPaymentKeyAsync(string token, string orderId,
        decimal amount, string phone, CancellationToken ct)
    {
        var payload = new
        {
            auth_token = token,
            amount_cents = (int)(amount * 100),
            expiration = 3600,
            order_id = orderId,
            billing_data = new
            {
                first_name = "Customer",
                last_name = "Hissatak",
                email = "customer@hissatak.online",
                phone_number = phone,
                apartment = "NA",
                floor = "NA",
                street = "NA",
                building = "NA",
                shipping_method = "NA",
                postal_code = "NA",
                city = "Cairo",
                country = "EG",
                state = "Cairo"
            },
            currency = "EGP",
            integration_id = int.Parse(_settings.WalletIntegrationId),
            lock_order_when_paid = true
        };
        var res = await _http.PostAsJsonAsync($"{BaseUrl}/acceptance/payment_keys", payload, ct);
        var data = JsonSerializer.Deserialize<JsonElement>(
            await res.Content.ReadAsStringAsync(ct));
        return data.TryGetProperty("token", out var t) ? t.GetString() : null;
    }

    private async Task<(string? txId, string? redirectUrl, string? error)> PayWithWalletAsync(
        string paymentKey, string phone, CancellationToken ct)
    {
        var payload = new
        {
            source = new { identifier = phone, subtype = "WALLET" },
            payment_token = paymentKey
        };
        var content = new StringContent(
            JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        var res = await _http.PostAsync($"{BaseUrl}/acceptance/payments/pay", content, ct);
        var raw = await res.Content.ReadAsStringAsync(ct);
        _logger.LogInformation("Paymob response: {Json}", raw);

        var data = JsonSerializer.Deserialize<JsonElement>(raw);
        var txId = data.TryGetProperty("id", out var id) ? id.GetInt32().ToString() : null;
        var redirectUrl = data.TryGetProperty("redirect_url", out var r) ? r.GetString() : null;
        var pending = data.TryGetProperty("pending", out var p) && p.GetBoolean();

        if (txId != null && (pending || redirectUrl != null))
            return (txId, redirectUrl, null);

        var err = data.TryGetProperty("detail", out var d) ? d.GetString() : "خطأ غير معروف";
        return (null, null, err);
    }

    private async Task<InitiatePaymentResponse> FailAsync(
        PaymentRecord record, string error, CancellationToken ct)
    {
        record.Status = "Failed";
        record.FailureReason = error;
        await _db.SaveChangesAsync(ct);
        return new InitiatePaymentResponse(false, record.Id, null, null, error);
    }

    private static string MaskPhone(string p) =>
        p.Length < 5 ? p : p[..3] + "****" + p[^3..];
}