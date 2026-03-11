namespace Moalimi.Application.DTOs.Settings;

public class WhatsAppSettings
{
    public const string SectionName = "WhatsApp";

    public string AccessToken { get; set; } = string.Empty;
    public string PhoneNumberId { get; set; } = string.Empty;
    public bool IsSandbox { get; set; } = true;
}