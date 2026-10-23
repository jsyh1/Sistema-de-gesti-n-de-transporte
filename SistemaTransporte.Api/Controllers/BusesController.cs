using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SistemaTransporte.Api.DTOs.Bus;
using SistemaTransporte.Api.Services;

namespace SistemaTransporte.Api.Controllers;

[ApiController]
[Authorize(Roles = AutenticacionService.Administrador)]
[Route("api/[controller]")]
public class BusesController : ControllerBase
{
    private readonly BusService _busService;

    public BusesController(BusService busService)
    {
        _busService = busService;
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerTodos() =>
        Ok(await _busService.GetAllAsync());

    [HttpGet("{id:int}")]
    public async Task<IActionResult> ObtenerPorId(int id)
    {
        try
        {
            var bus = await _busService.GetByIdAsync(id);
            return bus is null ? NotFound() : Ok(bus);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] CrearBusDto dto)
    {
        try
        {
            var bus = await _busService.CreateAsync(dto);
            return CreatedAtAction(nameof(ObtenerPorId), new { id = bus.Id }, bus);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Actualizar(int id, [FromBody] ActualizarBusDto dto)
    {
        try
        {
            var bus = await _busService.UpdateAsync(id, dto);
            return bus is null ? NotFound() : Ok(bus);
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
            return await _busService.DeleteAsync(id) ? NoContent() : NotFound();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }
}
