using MySqlConnector;
using SistemaTransporte.Api.Models;
using SistemaTransporte.Api.Data;

namespace SistemaTransporte.Api.DAO;

/// <summary>
/// Implementa las operaciones CRUD para la tabla Usuarios.
/// </summary>
public class UsuarioDao : IUsuarioDao
{
    private readonly ConexionDB _conexionDB;

    /// <summary>
    /// Inicializa el DAO con el administrador de conexiones.
    /// </summary>
    public UsuarioDao(ConexionDB conexionDB)
    {
        ArgumentNullException.ThrowIfNull(conexionDB);
        _conexionDB = conexionDB;
    }

    /// <summary>
    /// Obtiene todos los usuarios registrados sin exponer sus contraseñas.
    /// </summary>
    public async Task<IEnumerable<Usuario>> GetAllAsync()
    {
        var usuarios = new List<Usuario>();

        using MySqlConnection conexion = _conexionDB.ObtenerConexion();

        const string sql = """
            SELECT Id, NombreUsuario, Activo, RolId
            FROM Usuarios
            ORDER BY Id;
            """;

        using var comando = new MySqlCommand(sql, conexion);

        await conexion.OpenAsync();

        using var reader = await comando.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            usuarios.Add(new Usuario
            {
                Id = reader.GetInt32(0),
                NombreUsuario = reader.GetString(1),
                Activo = reader.GetBoolean(2),
                RolId = reader.GetInt32(3)
            });
        }

        return usuarios;
    }

    /// <summary>
    /// Busca un usuario por su identificador.
    /// </summary>
    public async Task<Usuario?> GetByIdAsync(int id)
    {
        using MySqlConnection conexion = _conexionDB.ObtenerConexion();

        const string sql = """
            SELECT Id, NombreUsuario, Activo, RolId
            FROM Usuarios
            WHERE Id = @Id;
            """;

        using var comando = new MySqlCommand(sql, conexion);
        comando.Parameters.AddWithValue("@Id", id);

        await conexion.OpenAsync();

        using var reader = await comando.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            return new Usuario
            {
                Id = reader.GetInt32(0),
                NombreUsuario = reader.GetString(1),
                Activo = reader.GetBoolean(2),
                RolId = reader.GetInt32(3)
            };
        }

        return null;
    }

    /// <summary>
    /// Crea un usuario y devuelve el registro insertado.
    /// PasswordHash debe contener un hash seguro, nunca la contraseña original.
    /// </summary>
    public async Task<Usuario> CreateAsync(Usuario entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        using MySqlConnection conexion = _conexionDB.ObtenerConexion();

        const string sql = """
            INSERT INTO Usuarios (NombreUsuario, PasswordHash, Activo, RolId)
            VALUES (@NombreUsuario, @PasswordHash, @Activo, @RolId);
            """;

        using var comando = new MySqlCommand(sql, conexion);

        comando.Parameters.AddWithValue("@NombreUsuario", entity.NombreUsuario);
        comando.Parameters.AddWithValue("@PasswordHash", entity.PasswordHash);
        comando.Parameters.AddWithValue("@Activo", entity.Activo);
        comando.Parameters.AddWithValue("@RolId", entity.RolId);

        await conexion.OpenAsync();

        await comando.ExecuteNonQueryAsync();

        entity.Id = Convert.ToInt32(comando.LastInsertedId);

        return entity;
    }

    /// <summary>
    /// Actualiza los datos de un usuario existente.
    /// </summary>
    public async Task<Usuario?> UpdateAsync(int id, Usuario entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        using MySqlConnection conexion = _conexionDB.ObtenerConexion();

        const string sql = """
            UPDATE Usuarios
            SET NombreUsuario = @NombreUsuario,
                PasswordHash = CASE
                    WHEN @PasswordHash <> '' THEN @PasswordHash
                    ELSE PasswordHash
                END,
                Activo = @Activo,
                RolId = @RolId
            WHERE Id = @Id;
            """;

        using var comando = new MySqlCommand(sql, conexion);

        comando.Parameters.AddWithValue("@Id", id);
        comando.Parameters.AddWithValue("@NombreUsuario", entity.NombreUsuario);
        comando.Parameters.AddWithValue("@PasswordHash", entity.PasswordHash);
        comando.Parameters.AddWithValue("@Activo", entity.Activo);
        comando.Parameters.AddWithValue("@RolId", entity.RolId);

        await conexion.OpenAsync();

        int filasAfectadas = await comando.ExecuteNonQueryAsync();

        if (filasAfectadas == 0)
        {
            return null;
        }

        entity.Id = id;
        entity.PasswordHash = string.Empty;

        return entity;
    }

    /// <summary>
    /// Elimina un usuario por su identificador.
    /// </summary>
    public async Task<bool> DeleteAsync(int id)
    {
        using MySqlConnection conexion = _conexionDB.ObtenerConexion();

        const string sql = """
            DELETE FROM Usuarios
            WHERE Id = @Id;
            """;

        using var comando = new MySqlCommand(sql, conexion);
        comando.Parameters.AddWithValue("@Id", id);

        await conexion.OpenAsync();

        int filasAfectadas = await comando.ExecuteNonQueryAsync();

        return filasAfectadas > 0;
    }
}