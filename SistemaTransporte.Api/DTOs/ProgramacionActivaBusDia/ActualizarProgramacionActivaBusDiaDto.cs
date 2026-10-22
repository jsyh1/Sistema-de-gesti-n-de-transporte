using System.ComponentModel.DataAnnotations;
using SistemaTransporte.Api.Models.Enums;

namespace SistemaTransporte.Api.DTOs.ProgramacionActivaBusDia;

public class ActualizarProgramacionActivaBusDiaDto
{
    [Range(1, int.MaxValue)]
    public int BusId { get; set; }

    [EnumDataType(typeof(DiaSemana))]
    public DiaSemana DiaSemana { get; set; } = DiaSemana.Lunes;
}
