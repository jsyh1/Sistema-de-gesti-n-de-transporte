using SistemaTransporte.Api.Models;

namespace SistemaTransporte.Api.DAO;

/// <summary>
/// Define las operaciones de acceso a datos para los usuarios.
/// </summary>
public interface IUsuarioDao : IDAO<Usuario>
{
    Task<Usuario?> GetByUsernameWithPasswordAsync(string nombreUsuario);
}