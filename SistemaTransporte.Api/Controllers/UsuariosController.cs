using Microsoft.AspNetCore.Mvc;
using SistemaTransporte.Api.DTOs;
using SistemaTransporte.Api.DTOs.Login;
using SistemaTransporte.Api.Services;

namespace SistemaTransporte.Api.Controllers;

/// <summary>
/// Gestiona las peticiones HTTP relacionadas con los usuarios.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class UsuariosController : ControllerBase
{
    private readonly UsuarioService _usuarioService;

    /// <summary>
    /// Inicializa el controlador con el servicio de usuarios.
    /// </summary>
    public UsuariosController(UsuarioService usuarioService)
    {
        _usuarioService = usuarioService;
    }

    /// <summary>
    /// Obtiene todos los usuarios.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> ObtenerTodos()
    {
        var usuarios = await _usuarioService.GetAllAsync();
        return Ok(usuarios);
    }

    /// <summary>
    /// Obtiene un usuario por su identificador.
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> ObtenerPorId(int id)
    {
        try
        {
            var usuario = await _usuarioService.GetByIdAsync(id);

            if (usuario is null)
                return NotFound(new { mensaje = "El usuario no existe." });

            return Ok(usuario);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    /// <summary>
    /// Crea un usuario nuevo.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] CrearUsuarioDto dto)
    {
        try
        {
            var usuario = await _usuarioService.CreateAsync(dto);

            return CreatedAtAction(
                nameof(ObtenerPorId),
                new { id = usuario.Id },
                usuario
            );
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    /// <summary>
    /// Actualiza los datos de un usuario existente.
    /// </summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Actualizar(
        int id,
        [FromBody] ActualizarUsuarioDto dto)
    {
        try
        {
            var usuario = await _usuarioService.UpdateAsync(id, dto);

            if (usuario is null)
                return NotFound(new { mensaje = "El usuario no existe." });

            return Ok(usuario);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    /// <summary>
    /// Elimina un usuario por su identificador.
    /// </summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Eliminar(int id)
    {
        try
        {
            var eliminado = await _usuarioService.DeleteAsync(id);

            if (!eliminado)
                return NotFound(new { mensaje = "El usuario no existe." });

            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }
}
