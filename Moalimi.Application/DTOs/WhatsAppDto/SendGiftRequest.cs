namespace Moalimi.Application.DTOs.WhatsAppDto
{
    public record SendGiftRequest(
     string RecipientPhone,
     string SenderName,
     string PackageName,
     string GiftCode
    );
}