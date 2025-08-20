using Microsoft.AspNetCore.Mvc;
using OAuth2PoC.Application.DTOs;
using OAuth2PoC.Application.Interfaces;

namespace OAuth2PoC.Api.Controllers;

/// <summary>
/// Controlador que maneja las operaciones de autenticación OAuth2
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthenticationService _authenticationService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IAuthenticationService authenticationService,
        ILogger<AuthController> logger)
    {
        _authenticationService = authenticationService;
        _logger = logger;
    }

    /// <summary>
    /// Inicia el proceso de autenticación OAuth2 con el proveedor especificado
    /// </summary>
    [HttpPost("login")]
    public async Task<ActionResult<AuthenticationResponseDto>> Login(
        [FromBody] AuthenticationRequestDto request)
    {
        try
        {
            _logger.LogInformation("Starting OAuth2 authentication for provider {Provider}", request.Provider);
            
            var response = await _authenticationService.InitiateAuthenticationAsync(request);
            
            _logger.LogInformation("Generated authorization URL for state {State}", response.State);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initiate OAuth2 authentication");
            return BadRequest(new { error = "Failed to initiate authentication", message = ex.Message });
        }
    }

    /// <summary>
    /// Maneja el callback de OAuth2 después de la autorización del usuario
    /// </summary>
    [HttpGet("callback")]
    public async Task<ActionResult<UserDto>> Callback([FromQuery] CallbackRequestDto request)
    {
        try
        {
            _logger.LogInformation("Processing OAuth2 callback with state {State}", request.State);
            
            var userDto = await _authenticationService.HandleCallbackAsync(request);
            
            _logger.LogInformation("Successfully authenticated user {UserId} from provider {Provider}", 
                userDto.Id, userDto.Provider);
            
            return Ok(userDto);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Invalid callback parameters: {Message}", ex.Message);
            return BadRequest(new { error = "Invalid callback parameters", message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("OAuth2 authentication failed: {Message}", ex.Message);
            return BadRequest(new { error = "Authentication failed", message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during OAuth2 callback processing");
            return StatusCode(500, new { error = "Internal server error", message = "Authentication failed" });
        }
    }

    /// <summary>
    /// Refresca un token de acceso usando el refresh token
    /// </summary>
    [HttpPost("refresh")]
    public async Task<ActionResult<UserDto>> RefreshToken([FromBody] RefreshTokenRequestDto request)
    {
        try
        {
            _logger.LogInformation("Refreshing token for provider {Provider}", request.Provider);
            
            var userDto = await _authenticationService.RefreshTokenAsync(request.RefreshToken, request.Provider);
            
            _logger.LogInformation("Successfully refreshed token for user {UserId}", userDto.Id);
            return Ok(userDto);
        }
        catch (NotSupportedException ex)
        {
            _logger.LogWarning("Token refresh not supported: {Message}", ex.Message);
            return BadRequest(new { error = "Token refresh not supported", message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Token refresh failed: {Message}", ex.Message);
            return BadRequest(new { error = "Token refresh failed", message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during token refresh");
            return StatusCode(500, new { error = "Internal server error", message = "Token refresh failed" });
        }
    }
}
