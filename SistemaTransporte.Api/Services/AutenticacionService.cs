using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using SistemaTransporte.Api.DAO;
using SistemaTransporte.Api.DTOs.Login;
using SistemaTransporte.Api.Models;

namespace SistemaTransporte.Api.Services;

public sealed class AutenticacionService
{
    public const string Administrador = "Administrador";
    public const string Conductor = "Conductor";
    private static readonly TimeSpan DuracionToken = TimeSpan.FromMinutes(30);

    private readonly IUsuarioDao _usuarioDao;
    private readonly IRolDao _rolDao;
    private readonly IConductorDao _conductorDao;
    private readonly IPasswordHasher<Usuario> _passwordHasher;
    private readonly SigningCredentials _credenciales;

    public AutenticacionService(
        IUsuarioDao usuarioDao,
        IRolDao rolDao,
        IConductorDao conductorDao,
        IPasswordHasher<Usuario> passwordHasher,
        IConfiguration configuration)
    {
        _usuarioDao = usuarioDao ?? throw new ArgumentNullException(nameof(usuarioDao));
        _rolDao = rolDao ?? throw new ArgumentNullException(nameof(rolDao));
        _conductorDao = conductorDao ?? throw new ArgumentNullException(nameof(conductorDao));
        _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));

        var key = configuration["Jwt:Key"];
        if (string.IsNullOrWhiteSpace(key) || Encoding.UTF8.GetByteCount(key) < 32)
        {
            throw new InvalidOperationException(
                "Configure Jwt:Key con una clave de al menos 32 bytes mediante una variable de entorno.");
        }

        _credenciales = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            SecurityAlgorithms.HmacSha256);
    }

    public async Task<(SesionDto? Sesion, int? ErrorStatus)> IniciarSesionAsync(
        IniciarSesionDto solicitud)
    {
        ArgumentNullException.ThrowIfNull(solicitud);
        if (string.IsNullOrWhiteSpace(solicitud.NombreUsuario) ||
            string.IsNullOrEmpty(solicitud.Password))
        {
            return (null, StatusCodes.Status401Unauthorized);
        }

        var usuario = await _usuarioDao.GetByUsernameWithPasswordAsync(
            solicitud.NombreUsuario.Trim());
        if (usuario is null ||
            _passwordHasher.VerifyHashedPassword(
                usuario, usuario.PasswordHash, solicitud.Password) == PasswordVerificationResult.Failed)
        {
            return (null, StatusCodes.Status401Unauthorized);
        }

        if (!usuario.Activo)
        {
            return (null, StatusCodes.Status403Forbidden);
        }

        if (usuario.RolId is not (1 or 2))
        {
            return (null, StatusCodes.Status403Forbidden);
        }

        var rol = await _rolDao.GetByIdAsync(usuario.RolId);
        var nombreRol = usuario.RolId == 1 ? Administrador : Conductor;
        if (rol is null || !string.Equals(rol.Nombre, nombreRol, StringComparison.Ordinal))
        {
            return (null, StatusCodes.Status403Forbidden);
        }

        if (usuario.RolId == 2 &&
            !(await _conductorDao.GetAllAsync()).Any(c => c.UsuarioId == usuario.Id))
        {
            return (null, StatusCodes.Status403Forbidden);
        }

        var expira = DateTime.UtcNow.Add(DuracionToken);
        var token = new JwtSecurityToken(
            issuer: "SistemaTransporte",
            audience: "SistemaTransporte.Client",
            claims:
            [
                new Claim(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Name, usuario.NombreUsuario),
                new Claim(ClaimTypes.Role, nombreRol),
                new Claim("role_id", usuario.RolId.ToString())
            ],
            expires: expira,
            signingCredentials: _credenciales);

        return (new SesionDto
        {
            AccessToken = new JwtSecurityTokenHandler().WriteToken(token),
            ExpiresAt = expira,
            UserId = usuario.Id,
            NombreUsuario = usuario.NombreUsuario,
            RolId = usuario.RolId,
            Rol = nombreRol
        }, null);
    }
}
