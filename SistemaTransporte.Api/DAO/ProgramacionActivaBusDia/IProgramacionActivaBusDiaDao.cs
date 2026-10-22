using SistemaTransporte.Api.Models;
using SistemaTransporte.Api.Models.Enums;

namespace SistemaTransporte.Api.DAO;

public interface IProgramacionActivaBusDiaDao
{
    Task<IEnumerable<ProgramacionActivaBusDia>> GetAllAsync();
    Task<ProgramacionActivaBusDia?> GetByIdAsync(int busId, DiaSemana diaSemana);
    Task<ProgramacionActivaBusDia> CreateAsync(ProgramacionActivaBusDia entity);
    Task<ProgramacionActivaBusDia?> UpdateAsync(
        int busId, DiaSemana diaSemana, ProgramacionActivaBusDia entity);
    Task<bool> DeleteAsync(int busId, DiaSemana diaSemana);
}
