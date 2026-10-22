namespace SistemaTransporte.Api.DTOs.Login;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Contiene los datos necesarios para actualizar un usuario.
/// </summary>
public class ActualizarUsuarioDto
{
    /// <summary>
    /// Nombre de usuario para iniciar sesión.
    /// </summary>
    [Required(ErrorMessage = "El nombre de usuario es obligatorio.")]
    [StringLength(50, ErrorMessage = "El nombre de usuario no puede superar los 50 caracteres.")]
    public string NombreUsuario { get; set; } = string.Empty;

    /// <summary>
    /// Contraseña en texto plano recibida durante el registro.
    /// Debe convertirse en un hash antes de almacenarse.
    /// </summary>
    [StringLength(100, MinimumLength = 8,
        ErrorMessage = "La contraseña debe tener entre 8 y 100 caracteres.")]
    public string? Password { get; set; }

    public bool Activo { get; set; } = true;

    /// <summary>
    /// Identificador del rol asignado al usuario.
    /// 1: Administrador; 2: Conductor.
    /// </summary>
    [Range(1, int.MaxValue,
        ErrorMessage = "Debe seleccionar un rol válido.")]
    public int RolId { get; set; }
}
