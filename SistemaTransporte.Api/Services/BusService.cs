using SistemaTransporte.Api.DAO;
using SistemaTransporte.Api.DTOs.Bus;
using SistemaTransporte.Api.Models;
using SistemaTransporte.Api.Models.Enums;

namespace SistemaTransporte.Api.Services;

public class BusService
{
    private readonly IBusDao _busDao;

    public BusService(IBusDao busDao)
    {
        _busDao = busDao ?? throw new ArgumentNullException(nameof(busDao));
    }

    public Task<IEnumerable<Bus>> GetAllAsync() => _busDao.GetAllAsync();

    public async Task<Bus?> GetByIdAsync(int id)
    {
        ValidarId(id);
        return await _busDao.GetByIdAsync(id);
    }

    public Task<Bus> CreateAsync(CrearBusDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);
        ValidarDatos(dto.NumeroIdentificacion, dto.Modelo, dto.Capacidad, dto.Estado);

        return _busDao.CreateAsync(new Bus
        {
            NumeroIdentificacion = dto.NumeroIdentificacion.Trim(),
            Modelo = dto.Modelo.Trim(),
            Capacidad = dto.Capacidad,
            Estado = dto.Estado
        });
    }

    public async Task<Bus?> UpdateAsync(int id, ActualizarBusDto dto)
    {
        ValidarId(id);
        ArgumentNullException.ThrowIfNull(dto);
        ValidarDatos(dto.NumeroIdentificacion, dto.Modelo, dto.Capacidad, dto.Estado);

        return await _busDao.UpdateAsync(id, new Bus
        {
            Id = id,
            NumeroIdentificacion = dto.NumeroIdentificacion.Trim(),
            Modelo = dto.Modelo.Trim(),
            Capacidad = dto.Capacidad,
            Estado = dto.Estado
        });
    }

    public async Task<bool> DeleteAsync(int id)
    {
        ValidarId(id);
        return await _busDao.DeleteAsync(id);
    }

    private static void ValidarId(int id)
    {
        if (id <= 0)
        {
            throw new ArgumentException("El identificador debe ser mayor que cero.", nameof(id));
        }
    }

    private static void ValidarDatos(
        string numeroIdentificacion, string modelo, int capacidad, EstadoBus estado)
    {
        if (string.IsNullOrWhiteSpace(numeroIdentificacion))
        {
            throw new ArgumentException("El número de identificación es obligatorio.");
        }

        if (string.IsNullOrWhiteSpace(modelo))
        {
            throw new ArgumentException("El modelo del bus es obligatorio.");
        }

        if (capacidad <= 0)
        {
            throw new ArgumentException("La capacidad debe ser mayor que cero.");
        }

        if (!Enum.IsDefined(estado))
        {
            throw new ArgumentException("El estado del bus no es válido.");
        }
    }
}
