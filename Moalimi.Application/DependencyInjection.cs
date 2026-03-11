using Microsoft.Extensions.DependencyInjection;


namespace Moalimi.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Application layer — لو ضفنا MediatR بعدين هنا مكانه

        
        return services;
    }
}