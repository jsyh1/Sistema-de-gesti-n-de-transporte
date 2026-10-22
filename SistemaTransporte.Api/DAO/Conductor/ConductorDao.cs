using MySqlConnector;
using SistemaTransporte.Api.Data;
using SistemaTransporte.Api.Models;
using SistemaTransporte.Api.Models.Enums;

namespace SistemaTransporte.Api.DAO;

public class ConductorDao : IConductorDao
{
    private readonly ConexionDB _conexionDB;

    public ConductorDao(ConexionDB conexionDB)
    {
        _conexionDB = conexionDB ?? throw new ArgumentNullException(nameof(conexionDB));
    }

    public async Task<IEnumerable<Conductor>> GetAllAsync()
    {
        var conductores = new List<Conductor>();
        using var conexion = _conexionDB.ObtenerConexion();
        using var comando = new MySqlCommand(
            "SELECT Id, Nombre, NumeroLicencia, Estado, UsuarioId FROM Conductores ORDER BY Id;",
            conexion);
        await conexion.OpenAsync();
        using var reader = await comando.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            conductores.Add(LeerConductor(reader));
        }

        return conductores;
    }

    public async Task<Conductor?> GetByIdAsync(int id)
    {
        using var conexion = _conexionDB.ObtenerConexion();
        using var comando = new MySqlCommand(
            "SELECT Id, Nombre, NumeroLicencia, Estado, UsuarioId FROM Conductores WHERE Id = @Id;",
            conexion);
        comando.Parameters.AddWithValue("@Id", id);
        await conexion.OpenAsync();
        using var reader = await comando.ExecuteReaderAsync();
        return await reader.ReadAsync() ? LeerConductor(reader) : null;
    }

    public async Task<Conductor> CreateAsync(Conductor entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        using var conexion = _conexionDB.ObtenerConexion();
        using var comando = new MySqlCommand(
            """
            INSERT INTO Conductores (Nombre, NumeroLicencia, Estado, UsuarioId)
            VALUES (@Nombre, @NumeroLicencia, @Estado, @UsuarioId);
            """,
            conexion);
        AgregarDatos(comando, entity);
        await conexion.OpenAsync();
        await comando.ExecuteNonQueryAsync();
        entity.Id = checked((int)comando.LastInsertedId);
        return entity;
    }

    public async Task<Conductor?> UpdateAsync(int id, Conductor entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        using var conexion = _conexionDB.ObtenerConexion();
        using var comando = new MySqlCommand(
            """
            UPDATE Conductores
            SET Nombre = @Nombre,
                NumeroLicencia = @NumeroLicencia,
                Estado = @Estado,
                UsuarioId = @UsuarioId
            WHERE Id = @Id;
            """,
            conexion);
        AgregarDatos(comando, entity);
        comando.Parameters.AddWithValue("@Id", id);
        await conexion.OpenAsync();
        await comando.ExecuteNonQueryAsync();
        return await GetByIdAsyncWithConnectionAsync(conexion, id);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var conexion = _conexionDB.ObtenerConexion();
        using var comando = new MySqlCommand("DELETE FROM Conductores WHERE Id = @Id;", conexion);
        comando.Parameters.AddWithValue("@Id", id);
        await conexion.OpenAsync();
        return await comando.ExecuteNonQueryAsync() > 0;
    }

    private static Conductor LeerConductor(MySqlDataReader reader) => new()
    {
        Id = reader.GetInt32(0),
        Nombre = reader.GetString(1),
        NumeroLicencia = reader.GetString(2),
        Estado = LeerEstadoConductor(reader.GetString(3)),
        UsuarioId = reader.IsDBNull(4) ? null : reader.GetInt32(4)
    };

    private static void AgregarDatos(MySqlCommand comando, Conductor entity)
    {
        comando.Parameters.AddWithValue("@Nombre", entity.Nombre);
        comando.Parameters.AddWithValue("@NumeroLicencia", entity.NumeroLicencia);
        comando.Parameters.AddWithValue("@Estado", entity.Estado.ToString());
        comando.Parameters.AddWithValue("@UsuarioId", entity.UsuarioId is null
            ? DBNull.Value
            : entity.UsuarioId.Value);
    }

    private static async Task<Conductor?> GetByIdAsyncWithConnectionAsync(
        MySqlConnection conexion, int id)
    {
        using var comando = new MySqlCommand(
            "SELECT Id, Nombre, NumeroLicencia, Estado, UsuarioId FROM Conductores WHERE Id = @Id;",
            conexion);
        comando.Parameters.AddWithValue("@Id", id);
        using var reader = await comando.ExecuteReaderAsync();
        return await reader.ReadAsync() ? LeerConductor(reader) : null;
    }

    private static EstadoConductor LeerEstadoConductor(string valor)
    {
        return Enum.TryParse<EstadoConductor>(valor, ignoreCase: true, out var estado)
            && Enum.IsDefined(estado)
            && string.Equals(valor, estado.ToString(), StringComparison.OrdinalIgnoreCase)
            ? estado
            : throw new InvalidDataException(
                $"EstadoConductor no reconocido en la base de datos: '{valor}'.");
    }
}
