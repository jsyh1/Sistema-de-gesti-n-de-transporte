using Microsoft.AspNetCore.Identity;
using SistemaTransporte.Api.DAO;
using SistemaTransporte.Api.DTOs.Login;
using SistemaTransporte.Api.Models;

namespace SistemaTransporte.Api.Services;

/// <summary>
/// Gestiona la lógica de negocio de los usuarios.
/// </summary>
public class UsuarioService
{
    private readonly IUsuarioDao _usuarioDao;
    private readonly IRolDao _rolDao;
    private readonly IPasswordHasher<Usuario> _passwordHasher;

    /// <summary>
    /// Inicializa el servicio con sus dependencias.
    /// </summary>
    public UsuarioService(
        IUsuarioDao usuarioDao,
        IRolDao rolDao,
        IPasswordHasher<Usuario> passwordHasher)
    {
        ArgumentNullException.ThrowIfNull(usuarioDao);
        ArgumentNullException.ThrowIfNull(rolDao);
        ArgumentNullException.ThrowIfNull(passwordHasher);

        _usuarioDao = usuarioDao;
        _rolDao = rolDao;
        _passwordHasher = passwordHasher;
    }

    /// <summary>
    /// Obtiene todos los usuarios registrados.
    /// </summary>
    public async Task<IEnumerable<Usuario>> GetAllAsync()
    {
        return await _usuarioDao.GetAllAsync();
    }

    /// <summary>
    /// Busca un usuario por su identificador.
    /// </summary>
    public async Task<Usuario?> GetByIdAsync(int id)
    {
        if (id <= 0)
        {
            throw new ArgumentException(
                "El identificador debe ser mayor que cero.", nameof(id));
        }

        return await _usuarioDao.GetByIdAsync(id);
    }

    /// <summary>
    /// Registra un usuario y almacena su contraseña como hash.
    /// </summary>
    public async Task<Usuario> CreateAsync(CrearUsuarioDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        if (string.IsNullOrWhiteSpace(dto.NombreUsuario))
        {
            throw new ArgumentException("El nombre de usuario es obligatorio.");
        }

        if (string.IsNullOrWhiteSpace(dto.Password) || dto.Password.Length < 8)
        {
            throw new ArgumentException(
                "La contraseña debe tener al menos 8 caracteres.");
        }

        var rol = await _rolDao.GetByIdAsync(dto.RolId);

        if (rol is null)
        {
            throw new ArgumentException("El rol indicado no existe.");
        }

        var usuario = new Usuario
        {
            NombreUsuario = dto.NombreUsuario.Trim(),
            Activo = true,
            RolId = dto.RolId
        };

        usuario.PasswordHash = _passwordHasher.HashPassword(
            usuario, dto.Password);

        var usuarioCreado = await _usuarioDao.CreateAsync(usuario);

        // No devolver el hash al cliente.
        usuarioCreado.PasswordHash = string.Empty;

        return usuarioCreado;
    }

    /// <summary>
    /// Actualiza los datos de un usuario existente.
    /// </summary>
    public async Task<Usuario?> UpdateAsync(
        int id, ActualizarUsuarioDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        if (id <= 0)
        {
            throw new ArgumentException(
                "El identificador debe ser mayor que cero.", nameof(id));
        }

        var usuarioExistente = await _usuarioDao.GetByIdAsync(id);

        if (usuarioExistente is null)
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(dto.NombreUsuario))
        {
            throw new ArgumentException("El nombre de usuario es obligatorio.");
        }

        if (!string.IsNullOrEmpty(dto.Password) && dto.Password.Length < 8)
        {
            throw new ArgumentException(
                "La contraseña debe tener al menos 8 caracteres.");
        }

        var rol = await _rolDao.GetByIdAsync(dto.RolId);

        if (rol is null)
        {
            throw new ArgumentException("El rol indicado no existe.");
        }

        usuarioExistente.NombreUsuario = dto.NombreUsuario.Trim();
        usuarioExistente.RolId = dto.RolId;
        usuarioExistente.Activo = dto.Activo;

        // Si se proporciona una contraseña nueva, generar un hash.
        if (!string.IsNullOrWhiteSpace(dto.Password))
        {
            usuarioExistente.PasswordHash =
                _passwordHasher.HashPassword(
                    usuarioExistente, dto.Password);
        }

        var usuarioActualizado =
            await _usuarioDao.UpdateAsync(id, usuarioExistente);

        if (usuarioActualizado is not null)
        {
            usuarioActualizado.PasswordHash = string.Empty;
        }

        return usuarioActualizado;
    }

    /// <summary>
    /// Elimina un usuario por su identificador.
    /// </summary>
    public async Task<bool> DeleteAsync(int id)
    {
        if (id <= 0)
        {
            throw new ArgumentException(
                "El identificador debe ser mayor que cero.", nameof(id));
        }

        return await _usuarioDao.DeleteAsync(id);
    }
}