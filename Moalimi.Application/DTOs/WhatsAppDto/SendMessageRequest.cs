namespace Moalimi.Application.DTOs.WhatsAppDto
{
    public record SendMessageRequest(
     string PhoneNumber,
     string Message
    );
}
