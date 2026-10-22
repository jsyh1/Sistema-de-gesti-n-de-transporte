using SistemaTransporte.Api.DAO;
using SistemaTransporte.Api.DTOs.ProgramacionActivaBusDia;
using SistemaTransporte.Api.Models;
using SistemaTransporte.Api.Models.Enums;

namespace SistemaTransporte.Api.Services;

public class ProgramacionActivaBusDiaService
{
    private readonly IProgramacionActivaBusDiaDao _programacionDao;
    private readonly IBusDao _busDao;

    public ProgramacionActivaBusDiaService(
        IProgramacionActivaBusDiaDao programacionDao, IBusDao busDao)
    {
        _programacionDao = programacionDao
            ?? throw new ArgumentNullException(nameof(programacionDao));
        _busDao = busDao ?? throw new ArgumentNullException(nameof(busDao));
    }

    public Task<IEnumerable<ProgramacionActivaBusDia>> GetAllAsync() =>
        _programacionDao.GetAllAsync();

    public async Task<ProgramacionActivaBusDia?> GetByIdAsync(
        int busId, DiaSemana diaSemana)
    {
        ValidarClave(busId, diaSemana);
        return await _programacionDao.GetByIdAsync(busId, diaSemana);
    }

    public async Task<ProgramacionActivaBusDia> CreateAsync(
        CrearProgramacionActivaBusDiaDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);
        await ValidarDatosAsync(dto.BusId, dto.DiaSemana);
        return await _programacionDao.CreateAsync(new ProgramacionActivaBusDia
        {
            BusId = dto.BusId,
            DiaSemana = dto.DiaSemana
        });
    }

    public async Task<ProgramacionActivaBusDia?> UpdateAsync(
        int busId, DiaSemana diaSemana, ActualizarProgramacionActivaBusDiaDto dto)
    {
        ValidarClave(busId, diaSemana);
        ArgumentNullException.ThrowIfNull(dto);
        await ValidarDatosAsync(dto.BusId, dto.DiaSemana);
        return await _programacionDao.UpdateAsync(busId, diaSemana,
            new ProgramacionActivaBusDia
            {
                BusId = dto.BusId,
                DiaSemana = dto.DiaSemana
            });
    }

    public async Task<bool> DeleteAsync(int busId, DiaSemana diaSemana)
    {
        ValidarClave(busId, diaSemana);
        return await _programacionDao.DeleteAsync(busId, diaSemana);
    }

    private async Task ValidarDatosAsync(int busId, DiaSemana diaSemana)
    {
        ValidarClave(busId, diaSemana);
        if (await _busDao.GetByIdAsync(busId) is null)
        {
            throw new ArgumentException("El bus indicado no existe.");
        }
    }

    private static void ValidarClave(int busId, DiaSemana diaSemana)
    {
        if (busId <= 0)
        {
            throw new ArgumentException("El identificador del bus debe ser mayor que cero.");
        }

        if (!Enum.IsDefined(diaSemana))
        {
            throw new ArgumentException("El día de la semana no es válido.");
        }
    }
}
