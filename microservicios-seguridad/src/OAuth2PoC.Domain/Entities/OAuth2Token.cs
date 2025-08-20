namespace OAuth2PoC.Domain.Entities;

/// <summary>
/// Entidad que representa un token de OAuth2 con sus metadatos
/// </summary>
public class OAuth2Token
{
    public string AccessToken { get; set; } = string.Empty;
    public string? RefreshToken { get; set; }
    public string TokenType { get; set; } = "Bearer";
    public int ExpiresIn { get; set; }
    public string? Scope { get; set; }
    public DateTime IssuedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
}
