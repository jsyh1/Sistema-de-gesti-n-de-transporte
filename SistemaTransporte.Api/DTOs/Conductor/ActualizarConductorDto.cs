using System.ComponentModel.DataAnnotations;
using SistemaTransporte.Api.Models.Enums;

namespace SistemaTransporte.Api.DTOs.Conductor;

public class ActualizarConductorDto
{
    [Required]
    [StringLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [Required]
    [StringLength(30)]
    public string NumeroLicencia { get; set; } = string.Empty;

    [EnumDataType(typeof(EstadoConductor))]
    public EstadoConductor Estado { get; set; } = EstadoConductor.Disponible;

    [Range(1, int.MaxValue)]
    public int? UsuarioId { get; set; }
}
