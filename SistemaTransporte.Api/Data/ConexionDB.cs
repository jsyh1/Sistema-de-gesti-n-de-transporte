
using Microsoft.Data.SqlClient;

namespace SistemaTransporte.Api.Data;

/// <summary>
/// Administra la configuración de conexión a SQL Server
/// mediante el patrón Singleton.
/// </summary>
public sealed class ConexionDB
{
    private static readonly Lazy<ConexionDB> _instancia =
        new(() => new ConexionDB());

    private readonly string _cadenaConexion;

    /// <summary>
    /// Obtiene la instancia única de ConexionDB.
    /// </summary>
    public static ConexionDB Instancia => _instancia.Value;

    /// <summary>
    /// Constructor privado para impedir instancias externas.
    /// </summary>
    private ConexionDB()
    {
        _cadenaConexion =
            @"Server=(localdb)\MSSQLLocalDB;Database=ProgramacionBuses;Integrated Security=True;TrustServerCertificate=True;";
    }

    /// <summary>
    /// Crea una conexión nueva a SQL Server.
    /// </summary>
    public SqlConnection ObtenerConexion()
    {
        return new SqlConnection(_cadenaConexion);
    }
}
