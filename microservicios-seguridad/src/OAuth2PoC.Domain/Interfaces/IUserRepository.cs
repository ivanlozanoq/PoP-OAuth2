using OAuth2PoC.Domain.Entities;

namespace OAuth2PoC.Domain.Interfaces;

/// <summary>
/// Interfaz que define el contrato para el repositorio de usuarios
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Busca un usuario por su ID del proveedor OAuth2
    /// </summary>
    Task<User?> GetByProviderIdAsync(string providerId, string provider);

    /// <summary>
    /// Crea un nuevo usuario en el repositorio
    /// </summary>
    Task<User> CreateAsync(User user);

    /// <summary>
    /// Actualiza la información de un usuario existente
    /// </summary>
    Task<User> UpdateAsync(User user);

    /// <summary>
    /// Busca un usuario por su email
    /// </summary>
    Task<User?> GetByEmailAsync(string email);
}
