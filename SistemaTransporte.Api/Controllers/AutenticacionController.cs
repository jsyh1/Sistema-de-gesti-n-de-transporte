using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SistemaTransporte.Api.DTOs.Login;
using SistemaTransporte.Api.Services;

namespace SistemaTransporte.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AutenticacionController : ControllerBase
{
    private readonly AutenticacionService _autenticacionService;

    public AutenticacionController(AutenticacionService autenticacionService)
    {
        _autenticacionService = autenticacionService;
    }

    [HttpPost("login")]
    [EnableRateLimiting("login")]
    public async Task<IActionResult> IniciarSesion([FromBody] IniciarSesionDto solicitud)
    {
        var (sesion, errorStatus) =
            await _autenticacionService.IniciarSesionAsync(solicitud);
        if (sesion is not null)
        {
            return Ok(sesion);
        }

        return StatusCode(
            errorStatus ?? StatusCodes.Status401Unauthorized,
            new
            {
                mensaje = errorStatus == StatusCodes.Status403Forbidden
                    ? "La cuenta está inactiva, no tiene un rol autorizado o no está vinculada a un conductor."
                    : "Usuario o contraseña incorrectos."
            });
    }
}
