using OAuth2PoC.Application.DTOs;

namespace OAuth2PoC.Application.Interfaces;

/// <summary>
/// Interfaz que define el contrato para el servicio de autenticación
/// </summary>
public interface IAuthenticationService
{
    /// <summary>
    /// Inicia el proceso de autenticación OAuth2 generando la URL de autorización
    /// </summary>
    Task<AuthenticationResponseDto> InitiateAuthenticationAsync(AuthenticationRequestDto request);

    /// <summary>
    /// Maneja el callback de OAuth2 y autentica al usuario
    /// </summary>
    Task<UserDto> HandleCallbackAsync(CallbackRequestDto request);

    /// <summary>
    /// Refresca el token de acceso de un usuario
    /// </summary>
    Task<UserDto> RefreshTokenAsync(string refreshToken, string provider);
}
