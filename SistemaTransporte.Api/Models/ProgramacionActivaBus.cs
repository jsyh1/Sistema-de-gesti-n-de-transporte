
namespace SistemaTransporte.Api.Models;

/// <summary>
/// Representa un bus registrado para un día de la semana
/// en la tabla auxiliar de programaciones activas.
/// </summary>
public class ProgramacionActivaBusDia
{
    public int BusId { get; set; }

    public byte DiaSemana { get; set; }
}
