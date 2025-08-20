using Microsoft.Extensions.DependencyInjection;
using OAuth2PoC.Application.Interfaces;
using OAuth2PoC.Application.Services;

namespace OAuth2PoC.Application.Extensions;

/// <summary>
/// Extensiones para la configuración de servicios de la capa de aplicación
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registra todos los servicios de la capa de aplicación en el contenedor de DI
    /// </summary>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        
        return services;
    }
}
