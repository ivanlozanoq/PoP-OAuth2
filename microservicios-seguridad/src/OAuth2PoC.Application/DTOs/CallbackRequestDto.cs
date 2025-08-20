namespace OAuth2PoC.Application.DTOs;

/// <summary>
/// DTO para manejar el callback de OAuth2 después de la autorización
/// </summary>
public class CallbackRequestDto
{
    public string Code { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string? CodeVerifier { get; set; }
    public string? Error { get; set; }
    public string? ErrorDescription { get; set; }
}
