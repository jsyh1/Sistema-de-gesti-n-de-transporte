namespace SistemaTransporte.Api.DTOs.Login;

public sealed class SesionDto
{
    public string AccessToken { get; init; } = string.Empty;
    public DateTime ExpiresAt { get; init; }
    public int UserId { get; init; }
    public string NombreUsuario { get; init; } = string.Empty;
    public int RolId { get; init; }
    public string Rol { get; init; } = string.Empty;
}
