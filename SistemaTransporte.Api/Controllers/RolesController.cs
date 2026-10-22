
using Microsoft.AspNetCore.Mvc;
using SistemaTransporte.Api.Models;
using SistemaTransporte.Api.Services;

namespace SistemaTransporte.Api.Controllers;

/// <summary>
/// Gestiona las operaciones HTTP relacionadas con los roles.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class RolesController : ControllerBase
{
    private readonly RolService _rolService;

    public RolesController(RolService rolService)
    {
        _rolService = rolService;
    }

    /// <summary>
    /// Consulta todos los roles registrados.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> ObtenerTodos()
    {
        var roles = await _rolService.ObtenerTodosAsync();

        return Ok(roles);
    }

    /// <summary>
    /// Consulta un rol por su identificador.
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> ObtenerPorId(int id)
    {
        var rol = await _rolService.ObtenerPorIdAsync(id);

        if (rol is null)
        {
            return NotFound(new { mensaje = "El rol no existe." });
        }

        return Ok(rol);
    }

    /// <summary>
    /// Registra un nuevo rol.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] Rol rol)
    {
        var creado = await _rolService.CrearAsync(rol);

        return CreatedAtAction(
            nameof(ObtenerPorId),
            new { id = creado.Id },
            creado
        );
    }


    /// <summary>
    /// Actualiza un rol existente.
    /// </summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Actualizar(int id, [FromBody] Rol rol)
    {
        var actualizado = await _rolService.ActualizarAsync(id, rol);

        if (actualizado is null)
        {
            return NotFound(new { mensaje = "El rol no existe." });
        }

        return NoContent();
    }


    /// <summary>
    /// Elimina un rol por su identificador.
    /// </summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Eliminar(int id)
    {
        var eliminado = await _rolService.EliminarAsync(id);

        if (!eliminado)
        {
            return NotFound(new { mensaje = "El rol no existe." });
        }

        return NoContent();
    }
}
