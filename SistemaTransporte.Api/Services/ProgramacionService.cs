using SistemaTransporte.Api.DAO;
using SistemaTransporte.Api.DTOs.Programacion;
using SistemaTransporte.Api.Models;
using SistemaTransporte.Api.Models.Enums;

namespace SistemaTransporte.Api.Services;

public class ProgramacionService
{
    private readonly IProgramacionDao _programacionDao;
    private readonly IBusDao _busDao;
    private readonly IConductorDao _conductorDao;

    public ProgramacionService(
        IProgramacionDao programacionDao, IBusDao busDao, IConductorDao conductorDao)
    {
        _programacionDao = programacionDao
            ?? throw new ArgumentNullException(nameof(programacionDao));
        _busDao = busDao ?? throw new ArgumentNullException(nameof(busDao));
        _conductorDao = conductorDao
            ?? throw new ArgumentNullException(nameof(conductorDao));
    }

    public Task<IEnumerable<Programacion>> GetAllAsync() =>
        _programacionDao.GetAllAsync();

    public async Task<Programacion?> GetByIdAsync(int id)
    {
        ValidarId(id);
        return await _programacionDao.GetByIdAsync(id);
    }

    public async Task<Programacion> CreateAsync(CrearProgramacionDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);
        await ValidarDatosAsync(
            dto.BusId, dto.ConductorId, dto.DiaSemana, dto.HoraEntrada, dto.HoraSalida);

        return await _programacionDao.CreateAsync(new Programacion
        {
            BusId = dto.BusId,
            ConductorId = dto.ConductorId,
            DiaSemana = dto.DiaSemana,
            HoraEntrada = dto.HoraEntrada,
            HoraSalida = dto.HoraSalida,
            Activa = dto.Activa,
            FechaCreacion = DateTime.Now
        });
    }

    public async Task<Programacion?> UpdateAsync(
        int id, ActualizarProgramacionDto dto)
    {
        ValidarId(id);
        ArgumentNullException.ThrowIfNull(dto);
        await ValidarDatosAsync(
            dto.BusId, dto.ConductorId, dto.DiaSemana, dto.HoraEntrada, dto.HoraSalida);

        return await _programacionDao.UpdateAsync(id, new Programacion
        {
            Id = id,
            BusId = dto.BusId,
            ConductorId = dto.ConductorId,
            DiaSemana = dto.DiaSemana,
            HoraEntrada = dto.HoraEntrada,
            HoraSalida = dto.HoraSalida,
            Activa = dto.Activa
        });
    }

    public async Task<bool> DeleteAsync(int id)
    {
        ValidarId(id);
        return await _programacionDao.DeleteAsync(id);
    }

    private async Task ValidarDatosAsync(
        int busId, int conductorId, DiaSemana diaSemana,
        TimeSpan horaEntrada, TimeSpan horaSalida)
    {
        if (busId <= 0 || await _busDao.GetByIdAsync(busId) is null)
        {
            throw new ArgumentException("El bus indicado no existe.");
        }

        if (conductorId <= 0 || await _conductorDao.GetByIdAsync(conductorId) is null)
        {
            throw new ArgumentException("El conductor indicado no existe.");
        }

        if (!Enum.IsDefined(diaSemana))
        {
            throw new ArgumentException("El día de la semana no es válido.");
        }

        if (horaEntrada < TimeSpan.Zero || horaEntrada >= TimeSpan.FromDays(1) ||
            horaSalida < TimeSpan.Zero || horaSalida >= TimeSpan.FromDays(1) ||
            horaSalida <= horaEntrada)
        {
            throw new ArgumentException(
                "La hora de salida debe ser posterior a la hora de entrada y ambas deben pertenecer al mismo día.");
        }
    }

    private static void ValidarId(int id)
    {
        if (id <= 0)
        {
            throw new ArgumentException("El identificador debe ser mayor que cero.", nameof(id));
        }
    }
}
