namespace OAuth2PoC.Application.DTOs;

/// <summary>
/// DTO para la solicitud de refresco de token OAuth2
/// </summary>
public class RefreshTokenRequestDto
{
    public string RefreshToken { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
}
