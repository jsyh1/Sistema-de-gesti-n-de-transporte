using System.ComponentModel.DataAnnotations;
using SistemaTransporte.Api.Models.Enums;

namespace SistemaTransporte.Api.DTOs.Programacion;

public class ActualizarProgramacionDto
{
    [Range(1, int.MaxValue)]
    public int BusId { get; set; }

    [Range(1, int.MaxValue)]
    public int ConductorId { get; set; }

    [EnumDataType(typeof(DiaSemana))]
    public DiaSemana DiaSemana { get; set; } = DiaSemana.Lunes;

    public TimeSpan HoraEntrada { get; set; }

    public TimeSpan HoraSalida { get; set; }

    public bool Activa { get; set; }
}
