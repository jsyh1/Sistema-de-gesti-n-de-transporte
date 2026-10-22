using MySqlConnector;
using SistemaTransporte.Api.Data;
using SistemaTransporte.Api.Models;
using SistemaTransporte.Api.Models.Enums;

namespace SistemaTransporte.Api.DAO;

public class BusDao : IBusDao
{
    private readonly ConexionDB _conexionDB;

    public BusDao(ConexionDB conexionDB)
    {
        _conexionDB = conexionDB ?? throw new ArgumentNullException(nameof(conexionDB));
    }

    public async Task<IEnumerable<Bus>> GetAllAsync()
    {
        var buses = new List<Bus>();
        using var conexion = _conexionDB.ObtenerConexion();
        using var comando = new MySqlCommand(
            "SELECT Id, NumeroIdentificacion, Modelo, Capacidad, Estado FROM Buses ORDER BY Id;",
            conexion);
        await conexion.OpenAsync();
        using var reader = await comando.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            buses.Add(LeerBus(reader));
        }

        return buses;
    }

    public async Task<Bus?> GetByIdAsync(int id)
    {
        using var conexion = _conexionDB.ObtenerConexion();
        using var comando = new MySqlCommand(
            "SELECT Id, NumeroIdentificacion, Modelo, Capacidad, Estado FROM Buses WHERE Id = @Id;",
            conexion);
        comando.Parameters.AddWithValue("@Id", id);
        await conexion.OpenAsync();
        using var reader = await comando.ExecuteReaderAsync();
        return await reader.ReadAsync() ? LeerBus(reader) : null;
    }

    public async Task<Bus> CreateAsync(Bus entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        using var conexion = _conexionDB.ObtenerConexion();
        using var comando = new MySqlCommand(
            """
            INSERT INTO Buses (NumeroIdentificacion, Modelo, Capacidad, Estado)
            VALUES (@NumeroIdentificacion, @Modelo, @Capacidad, @Estado);
            """,
            conexion);
        AgregarDatos(comando, entity);
        await conexion.OpenAsync();
        await comando.ExecuteNonQueryAsync();
        entity.Id = checked((int)comando.LastInsertedId);
        return entity;
    }

    public async Task<Bus?> UpdateAsync(int id, Bus entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        using var conexion = _conexionDB.ObtenerConexion();
        using var comando = new MySqlCommand(
            """
            UPDATE Buses
            SET NumeroIdentificacion = @NumeroIdentificacion,
                Modelo = @Modelo,
                Capacidad = @Capacidad,
                Estado = @Estado
            WHERE Id = @Id;
            """,
            conexion);
        AgregarDatos(comando, entity);
        comando.Parameters.AddWithValue("@Id", id);
        await conexion.OpenAsync();
        await comando.ExecuteNonQueryAsync();
        var bus = await GetByIdAsyncWithConnectionAsync(conexion, id);
        return bus;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var conexion = _conexionDB.ObtenerConexion();
        using var comando = new MySqlCommand("DELETE FROM Buses WHERE Id = @Id;", conexion);
        comando.Parameters.AddWithValue("@Id", id);
        await conexion.OpenAsync();
        return await comando.ExecuteNonQueryAsync() > 0;
    }

    private static Bus LeerBus(MySqlDataReader reader) => new()
    {
        Id = reader.GetInt32(0),
        NumeroIdentificacion = reader.GetString(1),
        Modelo = reader.GetString(2),
        Capacidad = reader.GetInt32(3),
        Estado = LeerEstadoBus(reader.GetString(4))
    };

    private static void AgregarDatos(MySqlCommand comando, Bus entity)
    {
        comando.Parameters.AddWithValue("@NumeroIdentificacion", entity.NumeroIdentificacion);
        comando.Parameters.AddWithValue("@Modelo", entity.Modelo);
        comando.Parameters.AddWithValue("@Capacidad", entity.Capacidad);
        comando.Parameters.AddWithValue("@Estado", entity.Estado.ToString());
    }

    private static EstadoBus LeerEstadoBus(string valor)
    {
        return Enum.TryParse<EstadoBus>(valor, ignoreCase: true, out var estado)
            && Enum.IsDefined(estado)
            && string.Equals(valor, estado.ToString(), StringComparison.OrdinalIgnoreCase)
            ? estado
            : throw new InvalidDataException(
                $"EstadoBus no reconocido en la base de datos: '{valor}'.");
    }

    private static async Task<Bus?> GetByIdAsyncWithConnectionAsync(
        MySqlConnection conexion, int id)
    {
        using var comando = new MySqlCommand(
            "SELECT Id, NumeroIdentificacion, Modelo, Capacidad, Estado FROM Buses WHERE Id = @Id;",
            conexion);
        comando.Parameters.AddWithValue("@Id", id);
        using var reader = await comando.ExecuteReaderAsync();
        return await reader.ReadAsync() ? LeerBus(reader) : null;
    }
}
