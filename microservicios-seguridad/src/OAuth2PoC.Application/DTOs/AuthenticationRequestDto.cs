namespace OAuth2PoC.Application.DTOs;

/// <summary>
/// DTO para la solicitud de inicio de autenticación OAuth2
/// </summary>
public class AuthenticationRequestDto
{
    public string Provider { get; set; } = string.Empty;
    public string? CodeChallenge { get; set; }
}
