
namespace SistemaTransporte.Api.Models;

/// <summary>
/// Representa la programación de un bus y un conductor.
/// </summary>
public class Programacion
{
    public int Id { get; set; }

    public int BusId { get; set; }

    public int ConductorId { get; set; }

    public byte DiaSemana { get; set; }

    public TimeSpan HoraEntrada { get; set; }

    public TimeSpan HoraSalida { get; set; }

    public bool Activa { get; set; } = true;

    public DateTime FechaCreacion { get; set; }
        = DateTime.Now;
}
