using System.Text.Json;

namespace SistemaTransporte.Api.Middleware;

/// <summary>
/// Captura excepciones no controladas y genera respuestas HTTP uniformes.
/// </summary>
public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    /// <summary>
    /// Inicializa el middleware con el siguiente componente y el logger.
    /// </summary>
    public ExceptionMiddleware(
        RequestDelegate next,
        ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>
    /// Ejecuta el siguiente componente de la tubería HTTP y captura errores.
    /// </summary>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Ocurrió un error inesperado al procesar la petición.");

            if (context.Response.HasStarted)
                throw;

            context.Response.Clear();
            context.Response.StatusCode =
                StatusCodes.Status500InternalServerError;

            context.Response.ContentType = "application/json";

            var respuesta = new
            {
                mensaje = "Ocurrió un error interno en el servidor."
            };

            await context.Response.WriteAsync(
                JsonSerializer.Serialize(respuesta));
        }
    }
}
