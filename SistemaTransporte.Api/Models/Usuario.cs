
using System.Text.Json.Serialization;

namespace SistemaTransporte.Api.Models;

/// <summary>
/// Representa un usuario del sistema.
/// </summary>
public class Usuario
{
    public int Id { get; set; }

    public string NombreUsuario { get; set; } = string.Empty;

    [JsonIgnore]//importante para que no se muestre en las respuestas de la API
    public string PasswordHash { get; set; } = string.Empty;

    public bool Activo { get; set; } = true;

    public int RolId { get; set; }
}
