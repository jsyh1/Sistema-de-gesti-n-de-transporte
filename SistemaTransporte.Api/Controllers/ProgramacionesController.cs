using Microsoft.AspNetCore.Mvc;
using SistemaTransporte.Api.DTOs.Programacion;
using SistemaTransporte.Api.Services;

namespace SistemaTransporte.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProgramacionesController : ControllerBase
{
    private readonly ProgramacionService _programacionService;

    public ProgramacionesController(ProgramacionService programacionService)
    {
        _programacionService = programacionService;
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerTodas() =>
        Ok(await _programacionService.GetAllAsync());

    [HttpGet("{id:int}")]
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
    public async Task<IActionResult> Crear([FromBody] CrearProgramacionDto dto)
    {
        try
        {
            var programacion = await _programacionService.CreateAsync(dto);
            return CreatedAtAction(
                nameof(ObtenerPorId), new { id = programacion.Id }, programacion);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Actualizar(
        int id, [FromBody] ActualizarProgramacionDto dto)
    {
        try
        {
            var programacion = await _programacionService.UpdateAsync(id, dto);
            return programacion is null ? NotFound() : Ok(programacion);
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
            return await _programacionService.DeleteAsync(id) ? NoContent() : NotFound();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }
}
