using MySqlConnector;
using SistemaTransporte.Api.Data;
using SistemaTransporte.Api.Models;
using SistemaTransporte.Api.Models.Enums;

namespace SistemaTransporte.Api.DAO;

public class ProgramacionActivaBusDiaDao : IProgramacionActivaBusDiaDao
{
    private readonly ConexionDB _conexionDB;

    public ProgramacionActivaBusDiaDao(ConexionDB conexionDB)
    {
        _conexionDB = conexionDB ?? throw new ArgumentNullException(nameof(conexionDB));
    }

    public async Task<IEnumerable<ProgramacionActivaBusDia>> GetAllAsync()
    {
        var programaciones = new List<ProgramacionActivaBusDia>();
        using var conexion = _conexionDB.ObtenerConexion();
        using var comando = new MySqlCommand(
            "SELECT BusId, DiaSemana FROM ProgramacionesActivasBusDia ORDER BY BusId, DiaSemana;",
            conexion);
        await conexion.OpenAsync();
        using var reader = await comando.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            programaciones.Add(Leer(reader));
        }

        return programaciones;
    }

    public async Task<ProgramacionActivaBusDia?> GetByIdAsync(
        int busId, DiaSemana diaSemana)
    {
        using var conexion = _conexionDB.ObtenerConexion();
        using var comando = CrearConsulta(conexion, busId, diaSemana);
        await conexion.OpenAsync();
        using var reader = await comando.ExecuteReaderAsync();
        return await reader.ReadAsync() ? Leer(reader) : null;
    }

    public async Task<ProgramacionActivaBusDia> CreateAsync(ProgramacionActivaBusDia entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        using var conexion = _conexionDB.ObtenerConexion();
        using var comando = new MySqlCommand(
            "INSERT INTO ProgramacionesActivasBusDia (BusId, DiaSemana) VALUES (@BusId, @DiaSemana);",
            conexion);
        comando.Parameters.AddWithValue("@BusId", entity.BusId);
        comando.Parameters.AddWithValue("@DiaSemana", (byte)entity.DiaSemana);
        await conexion.OpenAsync();
        await comando.ExecuteNonQueryAsync();
        return entity;
    }

    public async Task<ProgramacionActivaBusDia?> UpdateAsync(
        int busId, DiaSemana diaSemana, ProgramacionActivaBusDia entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        using var conexion = _conexionDB.ObtenerConexion();
        await conexion.OpenAsync();
        using (var comprobar = CrearConsulta(conexion, busId, diaSemana))
        using (var lectorActual = await comprobar.ExecuteReaderAsync())
        {
            if (!await lectorActual.ReadAsync())
            {
                return null;
            }
        }

        if (busId == entity.BusId && diaSemana == entity.DiaSemana)
        {
            return entity;
        }

        using var comando = new MySqlCommand(
            """
            UPDATE ProgramacionesActivasBusDia
            SET BusId = @NuevoBusId, DiaSemana = @NuevoDiaSemana
            WHERE BusId = @BusId AND DiaSemana = @DiaSemana;
            """,
            conexion);
        comando.Parameters.AddWithValue("@NuevoBusId", entity.BusId);
        comando.Parameters.AddWithValue("@NuevoDiaSemana", (byte)entity.DiaSemana);
        comando.Parameters.AddWithValue("@BusId", busId);
        comando.Parameters.AddWithValue("@DiaSemana", diaSemana);
        await comando.ExecuteNonQueryAsync();
        using var consulta = CrearConsulta(conexion, entity.BusId, entity.DiaSemana);
        using var reader = await consulta.ExecuteReaderAsync();
        return await reader.ReadAsync() ? Leer(reader) : null;
    }

    public async Task<bool> DeleteAsync(int busId, DiaSemana diaSemana)
    {
        using var conexion = _conexionDB.ObtenerConexion();
        using var comando = new MySqlCommand(
            "DELETE FROM ProgramacionesActivasBusDia WHERE BusId = @BusId AND DiaSemana = @DiaSemana;",
            conexion);
        comando.Parameters.AddWithValue("@BusId", busId);
        comando.Parameters.AddWithValue("@DiaSemana", (byte)diaSemana);
        await conexion.OpenAsync();
        return await comando.ExecuteNonQueryAsync() > 0;
    }

    private static MySqlCommand CrearConsulta(
        MySqlConnection conexion, int busId, DiaSemana diaSemana)
    {
        var comando = new MySqlCommand(
            """
            SELECT BusId, DiaSemana
            FROM ProgramacionesActivasBusDia
            WHERE BusId = @BusId AND DiaSemana = @DiaSemana;
            """,
            conexion);
        comando.Parameters.AddWithValue("@BusId", busId);
        comando.Parameters.AddWithValue("@DiaSemana", (byte)diaSemana);
        return comando;
    }

    private static ProgramacionActivaBusDia Leer(MySqlDataReader reader) => new()
    {
        BusId = reader.GetInt32(0),
        DiaSemana = LeerDiaSemana(reader.GetByte(1))
    };

    private static DiaSemana LeerDiaSemana(byte valor)
    {
        var dia = (DiaSemana)valor;
        return Enum.IsDefined(dia)
            ? dia
            : throw new InvalidDataException(
                $"DiaSemana no reconocido en la base de datos: {valor}.");
    }
}
