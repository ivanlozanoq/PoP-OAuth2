namespace OAuth2PoC.Domain.Configuration;

/// <summary>
/// Configuración para los parámetros de OAuth2 de un proveedor específico
/// </summary>
public class OAuth2Settings
{
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string AuthorizationEndpoint { get; set; } = string.Empty;
    public string TokenEndpoint { get; set; } = string.Empty;
    public string UserInfoEndpoint { get; set; } = string.Empty;
    public string RedirectUri { get; set; } = string.Empty;
    public string Scope { get; set; } = string.Empty;
}
