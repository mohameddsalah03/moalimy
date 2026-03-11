namespace Moalimi.Application.DTOs.Settings;

public class PaymobSettings
{
    public const string SectionName = "Paymob";

    public string ApiKey { get; set; } = string.Empty;
    public string WalletIntegrationId { get; set; } = string.Empty;
    public bool IsSandbox { get; set; } = true;
}