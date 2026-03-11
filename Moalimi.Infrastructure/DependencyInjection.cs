using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moalimi.Application.DTOs.Settings;
using Moalimi.Application.Interfaces;
using Moalimi.Application.Services;
using Moalimi.Infrastructure.Data;

namespace Moalimi.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<AppDbContext>(opts =>
            opts.UseSqlite("Data Source=moalimi.db"));
        services.AddScoped<IAppDbContext, AppDbContext>();

        
        services.Configure<CloudinarySettings>(
            config.GetSection(CloudinarySettings.SectionName));
        services.Configure<PaymobSettings>(
            config.GetSection(PaymobSettings.SectionName));
        services.Configure<WhatsAppSettings>(
            config.GetSection(WhatsAppSettings.SectionName));

        services.AddScoped<ICloudinaryService, CloudinaryService>();
        services.AddHttpClient<IVodafoneCashService, VodafoneCashService>();
        services.AddHttpClient<IWhatsAppService, WhatsAppService>();

        return services;
    }
}
