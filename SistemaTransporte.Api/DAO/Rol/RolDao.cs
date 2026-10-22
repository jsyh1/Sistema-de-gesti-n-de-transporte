
using MySqlConnector;
using SistemaTransporte.Api.Models;
using SistemaTransporte.Api.Data;

namespace SistemaTransporte.Api.DAO;

/// <summary>
/// Implementa las operaciones CRUD para la tabla Roles.
/// </summary>
public class RolDao : IRolDao
{
    private readonly ConexionDB _conexionDB;

    public RolDao(ConexionDB conexionDB)
    {
        _conexionDB = conexionDB;
    }

    /// <summary>
    /// Obtiene todos los roles registrados.
    /// </summary>
    public async Task<IEnumerable<Rol>> GetAllAsync()
    {
        var roles = new List<Rol>();

        using MySqlConnection conexion = _conexionDB.ObtenerConexion();

        const string sql = """
            SELECT Id, Nombre
            FROM Roles
            ORDER BY Id;
            """;

        using var comando = new MySqlCommand(sql, conexion);

        await conexion.OpenAsync();

        using var reader = await comando.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            roles.Add(new Rol
            {
                Id = reader.GetInt32(0),
                Nombre = reader.GetString(1)
            });
        }

        return roles;
    }

    /// <summary>
    /// Busca un rol por su identificador.
    /// </summary>
    public async Task<Rol?> GetByIdAsync(int id)
    {
        using MySqlConnection conexion = _conexionDB.ObtenerConexion();

        const string sql = """
            SELECT Id, Nombre
            FROM Roles
            WHERE Id = @Id;
            """;

        using var comando = new MySqlCommand(sql, conexion);
        comando.Parameters.AddWithValue("@Id", id);

        await conexion.OpenAsync();

        using var reader = await comando.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            return new Rol
            {
                Id = reader.GetInt32(0),
                Nombre = reader.GetString(1)
            };
        }

        return null;
    }

    /// <summary>
    /// Crea un rol y devuelve el registro insertado.
    /// </summary>
    public async Task<Rol> CreateAsync(Rol entity)
    {
        using MySqlConnection conexion = _conexionDB.ObtenerConexion();

        const string sql = """
            INSERT INTO Roles (Nombre)
            VALUES (@Nombre);
            """;

        using var comando = new MySqlCommand(sql, conexion);
        comando.Parameters.AddWithValue("@Nombre", entity.Nombre);

        await conexion.OpenAsync();

        await comando.ExecuteNonQueryAsync();

        using var comandoId = new MySqlCommand("SELECT LAST_INSERT_ID();", conexion);
        entity.Id = Convert.ToInt32(await comandoId.ExecuteScalarAsync());

        return entity;
    }

    /// <summary>
    /// Actualiza un rol existente.
    /// </summary>
    public async Task<Rol?> UpdateAsync(int id, Rol entity)
    {
        using MySqlConnection conexion = _conexionDB.ObtenerConexion();

        const string sql = """
            UPDATE Roles
            SET Nombre = @Nombre
            WHERE Id = @Id;
            """;

        using var comando = new MySqlCommand(sql, conexion);
        comando.Parameters.AddWithValue("@Id", id);
        comando.Parameters.AddWithValue("@Nombre", entity.Nombre);

        await conexion.OpenAsync();

        int filasAfectadas = await comando.ExecuteNonQueryAsync();

        if (filasAfectadas == 0)
        {
            return null;
        }

        entity.Id = id;

        return entity;
    }

    /// <summary>
    /// Elimina un rol por su identificador.
    /// </summary>
    public async Task<bool> DeleteAsync(int id)
    {
        using MySqlConnection conexion = _conexionDB.ObtenerConexion();

        const string sql = """
            DELETE FROM Roles
            WHERE Id = @Id;
            """;

        using var comando = new MySqlCommand(sql, conexion);
        comando.Parameters.AddWithValue("@Id", id);

        await conexion.OpenAsync();

        int filasAfectadas = await comando.ExecuteNonQueryAsync();

        return filasAfectadas > 0;
    }
}
