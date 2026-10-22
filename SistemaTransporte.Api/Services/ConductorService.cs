using SistemaTransporte.Api.DAO;
using SistemaTransporte.Api.DTOs.Conductor;
using SistemaTransporte.Api.Models;
using SistemaTransporte.Api.Models.Enums;

namespace SistemaTransporte.Api.Services;

public class ConductorService
{
    private readonly IConductorDao _conductorDao;

    public ConductorService(IConductorDao conductorDao)
    {
        _conductorDao = conductorDao
            ?? throw new ArgumentNullException(nameof(conductorDao));
    }

    public Task<IEnumerable<Conductor>> GetAllAsync() => _conductorDao.GetAllAsync();

    public async Task<Conductor?> GetByIdAsync(int id)
    {
        ValidarId(id);
        return await _conductorDao.GetByIdAsync(id);
    }

    public Task<Conductor> CreateAsync(CrearConductorDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);
        ValidarDatos(dto.Nombre, dto.NumeroLicencia, dto.Estado, dto.UsuarioId);

        return _conductorDao.CreateAsync(new Conductor
        {
            Nombre = dto.Nombre.Trim(),
            NumeroLicencia = dto.NumeroLicencia.Trim(),
            Estado = dto.Estado,
            UsuarioId = dto.UsuarioId
        });
    }

    public async Task<Conductor?> UpdateAsync(int id, ActualizarConductorDto dto)
    {
        ValidarId(id);
        ArgumentNullException.ThrowIfNull(dto);
        ValidarDatos(dto.Nombre, dto.NumeroLicencia, dto.Estado, dto.UsuarioId);

        return await _conductorDao.UpdateAsync(id, new Conductor
        {
            Id = id,
            Nombre = dto.Nombre.Trim(),
            NumeroLicencia = dto.NumeroLicencia.Trim(),
            Estado = dto.Estado,
            UsuarioId = dto.UsuarioId
        });
    }

    public async Task<bool> DeleteAsync(int id)
    {
        ValidarId(id);
        return await _conductorDao.DeleteAsync(id);
    }

    private static void ValidarId(int id)
    {
        if (id <= 0)
        {
            throw new ArgumentException("El identificador debe ser mayor que cero.", nameof(id));
        }
    }

    private static void ValidarDatos(
        string nombre, string numeroLicencia,
        EstadoConductor estado, int? usuarioId)
    {
        if (string.IsNullOrWhiteSpace(nombre))
        {
            throw new ArgumentException("El nombre del conductor es obligatorio.");
        }

        if (string.IsNullOrWhiteSpace(numeroLicencia))
        {
            throw new ArgumentException("El número de licencia es obligatorio.");
        }

        if (!Enum.IsDefined(estado))
        {
            throw new ArgumentException("El estado del conductor no es válido.");
        }

        if (usuarioId is <= 0)
        {
            throw new ArgumentException("El identificador de usuario debe ser mayor que cero.");
        }
    }
}
