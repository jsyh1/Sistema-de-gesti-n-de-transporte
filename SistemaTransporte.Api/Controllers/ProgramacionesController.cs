using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SistemaTransporte.Api.DTOs.Programacion;
using SistemaTransporte.Api.Services;
using System.Security.Claims;

namespace SistemaTransporte.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class ProgramacionesController : ControllerBase
{
    private readonly ProgramacionService _programacionService;

    public ProgramacionesController(ProgramacionService programacionService)
    {
        _programacionService = programacionService;
    }

    [HttpGet]
    [Authorize(Roles = AutenticacionService.Administrador)]
    public async Task<IActionResult> ObtenerTodas() =>
        Ok(await _programacionService.GetAllAsync());

    [HttpGet("mias")]
    [Authorize(Roles = AutenticacionService.Conductor)]
    public async Task<IActionResult> ObtenerMisProgramaciones()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        return Ok(await _programacionService.GetForUserAsync(userId));
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = AutenticacionService.Administrador)]
    public async Task<IActionResult> ObtenerPorId(int id)
    {
        try
        {
            var programacion = await _programacionService.GetByIdAsync(id);
            return programacion is null ? NotFound() : Ok(programacion);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    [HttpPost]
    [Authorize(Roles = AutenticacionService.Administrador)]
    public async Task<IActionResult> Crear([FromBody] CrearProgramacionDto dto)
    {
        try
        {
            var programacion = await _programacionService.CreateAsync(dto);
            return CreatedAtAction(
                nameof(ObtenerPorId), new { id = programacion.Id }, programacion);
        }
        catch (ConflictoProgramacionException ex)
        {
            return Conflict(new { mensaje = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = AutenticacionService.Administrador)]
    public async Task<IActionResult> Actualizar(
        int id, [FromBody] ActualizarProgramacionDto dto)
    {
        try
        {
            var programacion = await _programacionService.UpdateAsync(id, dto);
            return programacion is null ? NotFound() : Ok(programacion);
        }
        catch (ConflictoProgramacionException ex)
        {
            return Conflict(new { mensaje = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = AutenticacionService.Administrador)]
    public async Task<IActionResult> Eliminar(int id)
    {
        try
        {
            return await _programacionService.DeleteAsync(id) ? NoContent() : NotFound();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }
}
