using OAuth2PoC.Application.DTOs;
using OAuth2PoC.Application.Interfaces;
using OAuth2PoC.Domain.Entities;
using OAuth2PoC.Domain.Interfaces;
using OAuth2PoC.Domain.ValueObjects;

namespace OAuth2PoC.Application.Services;

/// <summary>
/// Servicio de aplicación que orquesta el proceso de autenticación OAuth2
/// </summary>
public class AuthenticationService : IAuthenticationService
{
    private readonly IOAuth2Provider _oAuth2Provider;
    private readonly IUserRepository _userRepository;

    public AuthenticationService(IOAuth2Provider oAuth2Provider, IUserRepository userRepository)
    {
        _oAuth2Provider = oAuth2Provider;
        _userRepository = userRepository;
    }

    /// <summary>
    /// Inicia el flujo de autenticación generando la URL de autorización
    /// </summary>
    public async Task<AuthenticationResponseDto> InitiateAuthenticationAsync(AuthenticationRequestDto request)
    {
        var state = Guid.NewGuid().ToString();
        var authorizationUrl = await _oAuth2Provider.GetAuthorizationUrlAsync(state, request.CodeChallenge);

        return new AuthenticationResponseDto
        {
            AuthorizationUrl = authorizationUrl,
            State = state
        };
    }

    /// <summary>
    /// Procesa el callback de OAuth2 y obtiene o crea el usuario en el sistema
    /// </summary>
    public async Task<UserDto> HandleCallbackAsync(CallbackRequestDto request)
    {
        if (!string.IsNullOrWhiteSpace(request.Error))
        {
            throw new InvalidOperationException($"OAuth2 error: {request.Error} - {request.ErrorDescription}");
        }

        var authorizationCode = new OAuth2AuthorizationCode(request.Code, request.State, request.CodeVerifier);
        
        if (!authorizationCode.IsValid())
        {
            throw new ArgumentException("Invalid authorization code or state");
        }

        // Intercambia el código por un token
        var token = await _oAuth2Provider.ExchangeCodeForTokenAsync(authorizationCode);
        
        // Obtiene la información del usuario del proveedor
        var userInfo = await _oAuth2Provider.GetUserInfoAsync(token.AccessToken);
        
        // Busca o crea el usuario en nuestro sistema
        var user = await GetOrCreateUserAsync(userInfo);

        return new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            Name = user.Name,
            Provider = user.Provider,
            AccessToken = token.AccessToken,
            ExpiresAt = token.ExpiresAt
        };
    }

    /// <summary>
    /// Refresca el token de acceso usando el refresh token
    /// </summary>
    public async Task<UserDto> RefreshTokenAsync(string refreshToken, string provider)
    {
        var newToken = await _oAuth2Provider.RefreshTokenAsync(refreshToken);
        var userInfo = await _oAuth2Provider.GetUserInfoAsync(newToken.AccessToken);
        
        var user = await _userRepository.GetByProviderIdAsync(userInfo.ProviderId, provider);
        if (user == null)
        {
            throw new InvalidOperationException("User not found");
        }

        return new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            Name = user.Name,
            Provider = user.Provider,
            AccessToken = newToken.AccessToken,
            ExpiresAt = newToken.ExpiresAt
        };
    }

    /// <summary>
    /// Obtiene un usuario existente o crea uno nuevo basado en la información del proveedor
    /// </summary>
    private async Task<User> GetOrCreateUserAsync(User userInfo)
    {
        var existingUser = await _userRepository.GetByProviderIdAsync(userInfo.ProviderId, userInfo.Provider);
        
        if (existingUser != null)
        {
            // Actualiza la información del usuario existente
            existingUser.Name = userInfo.Name;
            existingUser.Email = userInfo.Email;
            existingUser.UpdatedAt = DateTime.UtcNow;
            
            return await _userRepository.UpdateAsync(existingUser);
        }

        // Crea un nuevo usuario
        var newUser = new User
        {
            Id = Guid.NewGuid().ToString(),
            Email = userInfo.Email,
            Name = userInfo.Name,
            Provider = userInfo.Provider,
            ProviderId = userInfo.ProviderId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        return await _userRepository.CreateAsync(newUser);
    }
}
