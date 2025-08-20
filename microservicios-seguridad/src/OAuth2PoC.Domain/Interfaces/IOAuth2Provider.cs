using OAuth2PoC.Domain.Entities;
using OAuth2PoC.Domain.ValueObjects;

namespace OAuth2PoC.Domain.Interfaces;

/// <summary>
/// Interfaz que define el contrato para un proveedor de OAuth2
/// </summary>
public interface IOAuth2Provider
{
    /// <summary>
    /// Genera la URL de autorización para iniciar el flujo OAuth2
    /// </summary>
    Task<string> GetAuthorizationUrlAsync(string state, string? codeChallenge = null);

    /// <summary>
    /// Intercambia el código de autorización por un token de acceso
    /// </summary>
    Task<OAuth2Token> ExchangeCodeForTokenAsync(OAuth2AuthorizationCode authorizationCode);

    /// <summary>
    /// Obtiene la información del usuario usando el token de acceso
    /// </summary>
    Task<User> GetUserInfoAsync(string accessToken);

    /// <summary>
    /// Refresca un token de acceso usando el refresh token
    /// </summary>
    Task<OAuth2Token> RefreshTokenAsync(string refreshToken);
}
