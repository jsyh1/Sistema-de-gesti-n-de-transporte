
using SistemaTransporte.Api.Models.Enums;

namespace SistemaTransporte.Api.Models;

/// <summary>
/// Representa un conductor registrado en el sistema.
/// </summary>
public class Conductor
{
    public int Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string NumeroLicencia { get; set; } = string.Empty;

    public EstadoConductor Estado { get; set; } = EstadoConductor.Disponible;

    public int? UsuarioId { get; set; }
}
