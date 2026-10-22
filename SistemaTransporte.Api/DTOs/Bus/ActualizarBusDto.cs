using System.ComponentModel.DataAnnotations;
using SistemaTransporte.Api.Models.Enums;

namespace SistemaTransporte.Api.DTOs.Bus;

public class ActualizarBusDto
{
    [Required]
    [StringLength(30)]
    public string NumeroIdentificacion { get; set; } = string.Empty;

    [Required]
    [StringLength(80)]
    public string Modelo { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int Capacidad { get; set; }

    [EnumDataType(typeof(EstadoBus))]
    public EstadoBus Estado { get; set; } = EstadoBus.Disponible;
}
