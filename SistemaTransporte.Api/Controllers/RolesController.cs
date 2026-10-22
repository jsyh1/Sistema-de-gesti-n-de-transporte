
using Microsoft.AspNetCore.Mvc;
using SistemaTransporte.Api.DTOs;
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
        try
        {
            var rol = await _rolService.ObtenerPorIdAsync(id);

            if (rol is null)
            {
                return NotFound(new { mensaje = "El rol no existe." });
            }

            return Ok(rol);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    /// <summary>
    /// Registra un nuevo rol.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] CrearRolDto dto)
    {
        try
        {
            var creado = await _rolService.CrearAsync(dto);

            return CreatedAtAction(
                nameof(ObtenerPorId),
                new { id = creado.Id },
                creado
            );
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }


    /// <summary>
    /// Actualiza un rol existente.
    /// </summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Actualizar(int id, [FromBody] ActualizarRolDto dto)
    {
        try
        {
            var actualizado = await _rolService.ActualizarAsync(id, dto);

            if (actualizado is null)
            {
                return NotFound(new { mensaje = "El rol no existe." });
            }

            return Ok(actualizado);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }


    /// <summary>
    /// Elimina un rol por su identificador.
    /// </summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Eliminar(int id)
    {
        try
        {
            var eliminado = await _rolService.EliminarAsync(id);

            if (!eliminado)
            {
                return NotFound(new { mensaje = "El rol no existe." });
            }

            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }
}
