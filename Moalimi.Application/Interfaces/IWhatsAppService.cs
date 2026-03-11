using Moalimi.Application.DTOs.WhatsAppDto;

namespace Moalimi.Application.Interfaces;

public interface IWhatsAppService
{
    Task<WhatsAppResponse> SendMessageAsync(SendMessageRequest req, CancellationToken ct = default);
    Task<WhatsAppResponse> SendPaymentConfirmationAsync(SendPaymentConfirmationRequest req, CancellationToken ct = default);
    Task<WhatsAppResponse> SendSessionReminderAsync(SendSessionReminderRequest req, CancellationToken ct = default);
    Task<WhatsAppResponse> SendGiftNotificationAsync(SendGiftRequest req, CancellationToken ct = default);
}