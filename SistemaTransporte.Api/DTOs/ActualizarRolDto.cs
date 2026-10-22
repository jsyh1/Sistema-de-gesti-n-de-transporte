
namespace SistemaTransporte.Api.DTOs;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Contiene los datos necesarios para actualizar un rol.
/// </summary>
public class ActualizarRolDto
{
    [Required(ErrorMessage = "El nombre del rol es obligatorio.")]
    [StringLength(30, ErrorMessage = "El nombre no puede superar los 30 caracteres.")]
    public string Nombre { get; set; } = string.Empty;
}
