namespace OAuth2PoC.Domain.ValueObjects;

/// <summary>
/// Objeto de valor que encapsula el código de autorización de OAuth2
/// </summary>
public record OAuth2AuthorizationCode(string Code, string State, string? CodeVerifier = null)
{
    /// <summary>
    /// Valida que el código de autorización no esté vacío
    /// </summary>
    public bool IsValid() => !string.IsNullOrWhiteSpace(Code) && !string.IsNullOrWhiteSpace(State);
}
