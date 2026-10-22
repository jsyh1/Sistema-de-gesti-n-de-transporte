
namespace SistemaTransporte.Api.Models;

/// <summary>
/// Representa un bus registrado en el sistema.
/// </summary>
public class Bus
{
    public int Id { get; set; }

    public string NumeroIdentificacion { get; set; } = string.Empty;

    public string Modelo { get; set; } = string.Empty;

    public int Capacidad { get; set; }

    public int Estado { get; set; } = 0;
}
