using MySqlConnector;
using SistemaTransporte.Api.Data;
using SistemaTransporte.Api.Models;
using SistemaTransporte.Api.Models.Enums;

namespace SistemaTransporte.Api.DAO;

public class ProgramacionDao : IProgramacionDao
{
    private readonly ConexionDB _conexionDB;

    public ProgramacionDao(ConexionDB conexionDB)
    {
        _conexionDB = conexionDB ?? throw new ArgumentNullException(nameof(conexionDB));
    }

    public async Task<IEnumerable<Programacion>> GetAllAsync()
    {
        var programaciones = new List<Programacion>();
        using var conexion = _conexionDB.ObtenerConexion();
        using var comando = new MySqlCommand(
            """
            SELECT Id, BusId, ConductorId, DiaSemana, HoraEntrada, HoraSalida, Activa, FechaCreacion
            FROM Programaciones
            ORDER BY Id;
            """,
            conexion);
        await conexion.OpenAsync();
        using var reader = await comando.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            programaciones.Add(LeerProgramacion(reader));
        }

        return programaciones;
    }

    public async Task<Programacion?> GetByIdAsync(int id)
    {
        using var conexion = _conexionDB.ObtenerConexion();
        using var comando = CrearConsultaPorId(conexion, id);
        await conexion.OpenAsync();
        using var reader = await comando.ExecuteReaderAsync();
        return await reader.ReadAsync() ? LeerProgramacion(reader) : null;
    }

    public async Task<Programacion> CreateAsync(Programacion entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        using var conexion = _conexionDB.ObtenerConexion();
        using var comando = new MySqlCommand(
            """
            INSERT INTO Programaciones
                (BusId, ConductorId, DiaSemana, HoraEntrada, HoraSalida, Activa, FechaCreacion)
            VALUES
                (@BusId, @ConductorId, @DiaSemana, @HoraEntrada, @HoraSalida, @Activa, @FechaCreacion);
            """,
            conexion);
        AgregarDatos(comando, entity);
        await conexion.OpenAsync();
        await comando.ExecuteNonQueryAsync();
        entity.Id = checked((int)comando.LastInsertedId);
        return entity;
    }

    public async Task<Programacion?> UpdateAsync(int id, Programacion entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        using var conexion = _conexionDB.ObtenerConexion();
        using var comando = new MySqlCommand(
            """
            UPDATE Programaciones
            SET BusId = @BusId,
                ConductorId = @ConductorId,
                DiaSemana = @DiaSemana,
                HoraEntrada = @HoraEntrada,
                HoraSalida = @HoraSalida,
                Activa = @Activa
            WHERE Id = @Id;
            """,
            conexion);
        AgregarDatos(comando, entity, incluirFechaCreacion: false);
        comando.Parameters.AddWithValue("@Id", id);
        await conexion.OpenAsync();
        await comando.ExecuteNonQueryAsync();
        using var consulta = CrearConsultaPorId(conexion, id);
        using var reader = await consulta.ExecuteReaderAsync();
        return await reader.ReadAsync() ? LeerProgramacion(reader) : null;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var conexion = _conexionDB.ObtenerConexion();
        using var comando = new MySqlCommand("DELETE FROM Programaciones WHERE Id = @Id;", conexion);
        comando.Parameters.AddWithValue("@Id", id);
        await conexion.OpenAsync();
        return await comando.ExecuteNonQueryAsync() > 0;
    }

    private static MySqlCommand CrearConsultaPorId(MySqlConnection conexion, int id)
    {
        var comando = new MySqlCommand(
            """
            SELECT Id, BusId, ConductorId, DiaSemana, HoraEntrada, HoraSalida, Activa, FechaCreacion
            FROM Programaciones
            WHERE Id = @Id;
            """,
            conexion);
        comando.Parameters.AddWithValue("@Id", id);
        return comando;
    }

    private static Programacion LeerProgramacion(MySqlDataReader reader) => new()
    {
        Id = reader.GetInt32(0),
        BusId = reader.GetInt32(1),
        ConductorId = reader.GetInt32(2),
        DiaSemana = LeerDiaSemana(reader.GetByte(3)),
        HoraEntrada = reader.GetTimeSpan(4),
        HoraSalida = reader.GetTimeSpan(5),
        Activa = reader.GetBoolean(6),
        FechaCreacion = reader.GetDateTime(7)
    };

    private static void AgregarDatos(
        MySqlCommand comando, Programacion entity, bool incluirFechaCreacion = true)
    {
        comando.Parameters.AddWithValue("@BusId", entity.BusId);
        comando.Parameters.AddWithValue("@ConductorId", entity.ConductorId);
        comando.Parameters.AddWithValue("@DiaSemana", (byte)entity.DiaSemana);
        comando.Parameters.AddWithValue("@HoraEntrada", entity.HoraEntrada);
        comando.Parameters.AddWithValue("@HoraSalida", entity.HoraSalida);
        comando.Parameters.AddWithValue("@Activa", entity.Activa);
        if (incluirFechaCreacion)
        {
            comando.Parameters.AddWithValue("@FechaCreacion", entity.FechaCreacion);
        }
    }

    private static DiaSemana LeerDiaSemana(byte valor)
    {
        var dia = (DiaSemana)valor;
        return Enum.IsDefined(dia)
            ? dia
            : throw new InvalidDataException(
                $"DiaSemana no reconocido en la base de datos: {valor}.");
    }
}
