using Microsoft.Extensions.DependencyInjection;
using OAuth2PoC.Domain.Interfaces;
using OAuth2PoC.Infrastructure.Providers;
using OAuth2PoC.Infrastructure.Repositories;

namespace OAuth2PoC.Infrastructure.Extensions;

/// <summary>
/// Extensiones para la configuración de servicios de la capa de infraestructura
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registra todos los servicios de infraestructura en el contenedor de DI
    /// </summary>
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        services.AddHttpClient<IOAuth2Provider, GitHubOAuth2Provider>();
        services.AddSingleton<IUserRepository, InMemoryUserRepository>();
        
        return services;
    }
}
