using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OAuth2PoC.Domain.Configuration;
using OAuth2PoC.Domain.Entities;
using OAuth2PoC.Domain.Interfaces;
using OAuth2PoC.Domain.ValueObjects;
using System.Text;
using System.Text.Json;

namespace OAuth2PoC.Infrastructure.Providers;

/// <summary>
/// Implementación del proveedor OAuth2 para GitHub
/// </summary>
public class GitHubOAuth2Provider : IOAuth2Provider
{
    private readonly HttpClient _httpClient;
    private readonly OAuth2Settings _settings;
    private readonly ILogger<GitHubOAuth2Provider> _logger;

    public GitHubOAuth2Provider(
        HttpClient httpClient, 
        IConfiguration configuration,
        ILogger<GitHubOAuth2Provider> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        
        _settings = new OAuth2Settings
        {
            ClientId = configuration["OAuth2:GitHub:ClientId"] ?? string.Empty,
            ClientSecret = configuration["OAuth2:GitHub:ClientSecret"] ?? string.Empty,
            AuthorizationEndpoint = "https://github.com/login/oauth/authorize",
            TokenEndpoint = "https://github.com/login/oauth/access_token",
            UserInfoEndpoint = "https://api.github.com/user",
            RedirectUri = configuration["OAuth2:GitHub:RedirectUri"] ?? "https://localhost:7001/api/auth/callback",
            Scope = "user:email"
        };
    }

    /// <summary>
    /// Construye la URL de autorización de GitHub con los parámetros necesarios
    /// </summary>
    public Task<string> GetAuthorizationUrlAsync(string state, string? codeChallenge = null)
    {
        var queryParams = new Dictionary<string, string>
        {
            ["client_id"] = _settings.ClientId,
            ["redirect_uri"] = _settings.RedirectUri,
            ["scope"] = _settings.Scope,
            ["state"] = state,
            ["response_type"] = "code"
        };

        if (!string.IsNullOrWhiteSpace(codeChallenge))
        {
            queryParams["code_challenge"] = codeChallenge;
            queryParams["code_challenge_method"] = "S256";
        }

        var queryString = string.Join("&", queryParams.Select(kvp => 
            $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));

        var authorizationUrl = $"{_settings.AuthorizationEndpoint}?{queryString}";
        
        _logger.LogInformation("Generated GitHub authorization URL");
        return Task.FromResult(authorizationUrl);
    }

    /// <summary>
    /// Intercambia el código de autorización por un token de acceso con GitHub
    /// </summary>
    public async Task<OAuth2Token> ExchangeCodeForTokenAsync(OAuth2AuthorizationCode authorizationCode)
    {
        var tokenRequest = new
        {
            client_id = _settings.ClientId,
            client_secret = _settings.ClientSecret,
            code = authorizationCode.Code,
            redirect_uri = _settings.RedirectUri
        };

        var json = JsonSerializer.Serialize(tokenRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        var request = new HttpRequestMessage(HttpMethod.Post, _settings.TokenEndpoint)
        {
            Content = content
        };
        request.Headers.Add("Accept", "application/json");
        request.Headers.Add("User-Agent", "OAuth2PoC");

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadAsStringAsync();
        var tokenData = JsonSerializer.Deserialize<JsonElement>(responseContent);

        var issuedAt = DateTime.UtcNow;
        var expiresIn = tokenData.TryGetProperty("expires_in", out var expiresInElement) 
            ? expiresInElement.GetInt32() 
            : 28800; // GitHub tokens typically expire in 8 hours

        var token = new OAuth2Token
        {
            AccessToken = tokenData.GetProperty("access_token").GetString() ?? string.Empty,
            TokenType = tokenData.TryGetProperty("token_type", out var tokenTypeElement) 
                ? tokenTypeElement.GetString() ?? "Bearer" 
                : "Bearer",
            ExpiresIn = expiresIn,
            Scope = tokenData.TryGetProperty("scope", out var scopeElement) 
                ? scopeElement.GetString() 
                : null,
            RefreshToken = tokenData.TryGetProperty("refresh_token", out var refreshTokenElement)
                ? refreshTokenElement.GetString()
                : null,
            IssuedAt = issuedAt,
            ExpiresAt = issuedAt.AddSeconds(expiresIn)
        };

        _logger.LogInformation("Successfully exchanged code for GitHub access token");
        return token;
    }

    /// <summary>
    /// Obtiene la información del usuario desde la API de GitHub
    /// </summary>
    public async Task<User> GetUserInfoAsync(string accessToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, _settings.UserInfoEndpoint);
        request.Headers.Add("Authorization", $"Bearer {accessToken}");
        request.Headers.Add("User-Agent", "OAuth2PoC");

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadAsStringAsync();
        var userData = JsonSerializer.Deserialize<JsonElement>(responseContent);

        // Obtiene emails del usuario (GitHub requiere una llamada separada para emails)
        var email = await GetUserEmailAsync(accessToken);

        var user = new User
        {
            ProviderId = userData.GetProperty("id").GetInt32().ToString(),
            Provider = "GitHub",
            Name = userData.TryGetProperty("name", out var nameElement) && !nameElement.ValueKind.Equals(JsonValueKind.Null)
                ? nameElement.GetString() ?? userData.GetProperty("login").GetString() ?? string.Empty
                : userData.GetProperty("login").GetString() ?? string.Empty,
            Email = email
        };

        _logger.LogInformation("Successfully retrieved GitHub user information for user {ProviderId}", user.ProviderId);
        return user;
    }

    /// <summary>
    /// Refresca el token de acceso (GitHub no soporta refresh tokens en el flujo básico)
    /// </summary>
    public Task<OAuth2Token> RefreshTokenAsync(string refreshToken)
    {
        _logger.LogWarning("GitHub OAuth2 does not support refresh tokens in the basic flow");
        throw new NotSupportedException("GitHub OAuth2 does not support refresh tokens in the basic flow");
    }

    /// <summary>
    /// Obtiene el email principal del usuario desde la API de emails de GitHub
    /// </summary>
    private async Task<string> GetUserEmailAsync(string accessToken)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "https://api.github.com/user/emails");
            request.Headers.Add("Authorization", $"Bearer {accessToken}");
            request.Headers.Add("User-Agent", "OAuth2PoC");

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var emails = JsonSerializer.Deserialize<JsonElement[]>(responseContent);

            if (emails == null || emails.Length == 0)
            {
                return string.Empty;
            }

            var primaryEmail = emails.FirstOrDefault(e => 
                e.TryGetProperty("primary", out var primaryElement) && primaryElement.GetBoolean());

            if (primaryEmail.ValueKind != JsonValueKind.Undefined)
            {
                return primaryEmail.GetProperty("email").GetString() ?? string.Empty;
            }

            // Si no hay email primario, toma el primero disponible
            if (emails.Length > 0)
            {
                return emails[0].GetProperty("email").GetString() ?? string.Empty;
            }

            return string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to retrieve user email from GitHub");
            return string.Empty;
        }
    }
}
