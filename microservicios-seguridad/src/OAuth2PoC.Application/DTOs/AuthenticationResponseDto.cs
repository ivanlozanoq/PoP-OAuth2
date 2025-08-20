namespace OAuth2PoC.Application.DTOs;

/// <summary>
/// DTO para la respuesta de autenticación OAuth2
/// </summary>
public class AuthenticationResponseDto
{
    public string AuthorizationUrl { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
}
