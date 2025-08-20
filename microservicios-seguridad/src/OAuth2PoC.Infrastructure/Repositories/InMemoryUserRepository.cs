using Microsoft.Extensions.Logging;
using OAuth2PoC.Domain.Entities;
using OAuth2PoC.Domain.Interfaces;
using System.Collections.Concurrent;

namespace OAuth2PoC.Infrastructure.Repositories;

/// <summary>
/// Implementación en memoria del repositorio de usuarios para el PoC
/// </summary>
public class InMemoryUserRepository : IUserRepository
{
    private readonly ConcurrentDictionary<string, User> _users;
    private readonly ILogger<InMemoryUserRepository> _logger;

    public InMemoryUserRepository(ILogger<InMemoryUserRepository> logger)
    {
        _users = new ConcurrentDictionary<string, User>();
        _logger = logger;
    }

    /// <summary>
    /// Busca un usuario por su ID de proveedor OAuth2 y el proveedor
    /// </summary>
    public Task<User?> GetByProviderIdAsync(string providerId, string provider)
    {
        var user = _users.Values.FirstOrDefault(u => 
            u.ProviderId == providerId && u.Provider == provider);
        
        _logger.LogInformation("Searched user by provider ID {ProviderId} and provider {Provider}. Found: {Found}", 
            providerId, provider, user != null);
        
        return Task.FromResult(user);
    }

    /// <summary>
    /// Crea un nuevo usuario en el repositorio
    /// </summary>
    public Task<User> CreateAsync(User user)
    {
        if (string.IsNullOrWhiteSpace(user.Id))
        {
            user.Id = Guid.NewGuid().ToString();
        }

        user.CreatedAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;

        if (!_users.TryAdd(user.Id, user))
        {
            throw new InvalidOperationException($"User with ID {user.Id} already exists");
        }

        _logger.LogInformation("Created new user with ID {UserId} from provider {Provider}", 
            user.Id, user.Provider);
        
        return Task.FromResult(user);
    }

    /// <summary>
    /// Actualiza la información de un usuario existente
    /// </summary>
    public Task<User> UpdateAsync(User user)
    {
        if (!_users.ContainsKey(user.Id))
        {
            throw new KeyNotFoundException($"User with ID {user.Id} not found");
        }

        user.UpdatedAt = DateTime.UtcNow;
        _users[user.Id] = user;

        _logger.LogInformation("Updated user with ID {UserId}", user.Id);
        
        return Task.FromResult(user);
    }

    /// <summary>
    /// Busca un usuario por su dirección de email
    /// </summary>
    public Task<User?> GetByEmailAsync(string email)
    {
        var user = _users.Values.FirstOrDefault(u => 
            string.Equals(u.Email, email, StringComparison.OrdinalIgnoreCase));
        
        _logger.LogInformation("Searched user by email {Email}. Found: {Found}", 
            email, user != null);
        
        return Task.FromResult(user);
    }
}
