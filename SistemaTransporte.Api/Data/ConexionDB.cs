
using Microsoft.Extensions.Configuration;
using MySqlConnector;

namespace SistemaTransporte.Api.Data;

/// <summary>
/// Administra la configuración de conexión a MySQL.
/// </summary>
public sealed class ConexionDB
{
    private readonly string _cadenaConexion;

    /// <summary>
    /// Obtiene la cadena de conexión configurada para la aplicación.
    /// </summary>
    public ConexionDB(IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        _cadenaConexion = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "La cadena de conexión 'DefaultConnection' no está configurada.");

        if (string.IsNullOrWhiteSpace(_cadenaConexion))
        {
            throw new InvalidOperationException(
                "La cadena de conexión 'DefaultConnection' no puede estar vacía.");
        }
    }

    /// <summary>
    /// Crea una conexión nueva a MySQL.
    /// </summary>
    public MySqlConnection ObtenerConexion()
    {
        return new MySqlConnection(_cadenaConexion);
    }
}
