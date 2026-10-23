using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SistemaTransporte.Api.DTOs.Conductor;
using SistemaTransporte.Api.Services;

namespace SistemaTransporte.Api.Controllers;

[ApiController]
[Authorize(Roles = AutenticacionService.Administrador)]
[Route("api/[controller]")]
public class ConductoresController : ControllerBase
{
    private readonly ConductorService _conductorService;

    public ConductoresController(ConductorService conductorService)
    {
        _conductorService = conductorService;
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerTodos() =>
        Ok(await _conductorService.GetAllAsync());

    [HttpGet("{id:int}")]
    public async Task<IActionResult> ObtenerPorId(int id)
    {
        try
        {
            var conductor = await _conductorService.GetByIdAsync(id);
            return conductor is null ? NotFound() : Ok(conductor);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] CrearConductorDto dto)
    {
        try
        {
            var conductor = await _conductorService.CreateAsync(dto);
            return CreatedAtAction(
                nameof(ObtenerPorId), new { id = conductor.Id }, conductor);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Actualizar(
        int id, [FromBody] ActualizarConductorDto dto)
    {
        try
        {
            var conductor = await _conductorService.UpdateAsync(id, dto);
            return conductor is null ? NotFound() : Ok(conductor);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Eliminar(int id)
    {
        try
        {
            return await _conductorService.DeleteAsync(id) ? NoContent() : NotFound();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }
}
