using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SistemaTransporte.Api.DTOs.ProgramacionActivaBusDia;
using SistemaTransporte.Api.Models.Enums;
using SistemaTransporte.Api.Services;

namespace SistemaTransporte.Api.Controllers;

[ApiController]
[Authorize(Roles = AutenticacionService.Administrador)]
[Route("api/[controller]")]
public class ProgramacionesActivasBusDiaController : ControllerBase
{
    private readonly ProgramacionActivaBusDiaService _service;

    public ProgramacionesActivasBusDiaController(ProgramacionActivaBusDiaService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerTodas() =>
        Ok(await _service.GetAllAsync());

    [HttpGet("{busId:int}/{diaSemana:int}")]
    public async Task<IActionResult> ObtenerPorId(int busId, DiaSemana diaSemana)
    {
        try
        {
            var item = await _service.GetByIdAsync(busId, diaSemana);
            return item is null ? NotFound() : Ok(item);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Crear(
        [FromBody] CrearProgramacionActivaBusDiaDto dto)
    {
        try
        {
            var item = await _service.CreateAsync(dto);
            return CreatedAtAction(
                nameof(ObtenerPorId),
                new { busId = item.BusId, diaSemana = (byte)item.DiaSemana },
                item);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    [HttpPut("{busId:int}/{diaSemana:int}")]
    public async Task<IActionResult> Actualizar(
        int busId,
        DiaSemana diaSemana,
        [FromBody] ActualizarProgramacionActivaBusDiaDto dto)
    {
        try
        {
            var item = await _service.UpdateAsync(busId, diaSemana, dto);
            return item is null ? NotFound() : Ok(item);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    [HttpDelete("{busId:int}/{diaSemana:int}")]
    public async Task<IActionResult> Eliminar(int busId, DiaSemana diaSemana)
    {
        try
        {
            return await _service.DeleteAsync(busId, diaSemana)
                ? NoContent()
                : NotFound();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }
}
