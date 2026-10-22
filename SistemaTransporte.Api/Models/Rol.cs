
namespace SistemaTransporte.Api.Models;

/// <summary>
/// Representa un rol dentro del sistema.
/// </summary>
public class Rol
{
    public int Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public static implicit operator bool(Rol? v)
    {
        throw new NotImplementedException();
    }
}
