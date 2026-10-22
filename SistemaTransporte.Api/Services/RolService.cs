
using SistemaTransporte.Api.DAO;
using SistemaTransporte.Api.Models;

namespace SistemaTransporte.Api.Services;

/// <summary>
/// Contiene la lógica de negocio relacionada con los roles.
/// </summary>
public class RolService
{
    private readonly IRolDao _rolDao;

    /// <summary>
    /// Recibe el DAO encargado del acceso a los datos de los roles.
    /// </summary>
    public RolService(IRolDao rolDao)
    {
        _rolDao = rolDao;
    }

    /// <summary>
    /// Obtiene todos los roles registrados.
    /// </summary>
    public async Task<IEnumerable<Rol>> ObtenerTodosAsync()
    {
        return await _rolDao.GetAllAsync();
    }

    /// <summary>
    /// Obtiene un rol por su identificador.
    /// </summary>
    public async Task<Rol?> ObtenerPorIdAsync(int id)
    {
        if (id <= 0)
        {
            throw new ArgumentException(
                "El identificador debe ser mayor que cero.",
                nameof(id));
        }

        return await _rolDao.GetByIdAsync(id);
    }

    /// <summary>
    /// Registra un nuevo rol.
    /// </summary>
    public async Task<Rol> CrearAsync(Rol rol)
    {
        ArgumentNullException.ThrowIfNull(rol);

        if (string.IsNullOrWhiteSpace(rol.Nombre))
        {
            throw new ArgumentException(
                "El nombre del rol es obligatorio.");
        }

        rol.Nombre = rol.Nombre.Trim();

        if (rol.Nombre.Length > 30)
        {
            throw new ArgumentException(
                "El nombre del rol no puede superar los 30 caracteres.");
        }

        return await _rolDao.CreateAsync(rol);
    }

    /// <summary>
    /// Actualiza un rol existente.
    /// </summary>
    public async Task<Rol?> ActualizarAsync(int id, Rol rol)
    {
        ArgumentNullException.ThrowIfNull(rol);

        if (id <= 0)
        {
            throw new ArgumentException(
                "El identificador debe ser mayor que cero.",
                nameof(id));
        }

        if (string.IsNullOrWhiteSpace(rol.Nombre))
        {
            throw new ArgumentException(
                "El nombre del rol es obligatorio.");
        }

        rol.Nombre = rol.Nombre.Trim();

        if (rol.Nombre.Length > 30)
        {
            throw new ArgumentException(
                "El nombre del rol no puede superar los 30 caracteres.");
        }

        rol.Id = id;

        return await _rolDao.UpdateAsync(id, rol);
    }

    /// <summary>
    /// Elimina un rol por su identificador.
    /// </summary>
    public async Task<bool> EliminarAsync(int id)
    {
        if (id <= 0)
        {
            throw new ArgumentException(
                "El identificador debe ser mayor que cero.",
                nameof(id));
        }

        return await _rolDao.DeleteAsync(id);
    }
}
