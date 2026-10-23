namespace SistemaTransporte.Api.DTOs.Login;

public sealed class IniciarSesionDto
{
    public string NombreUsuario { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
