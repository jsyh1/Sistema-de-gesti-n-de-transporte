using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;
using SistemaTransporte.Api.Controllers;
using SistemaTransporte.Api.DAO;
using SistemaTransporte.Api.DTOs;
using SistemaTransporte.Api.DTOs.Bus;
using SistemaTransporte.Api.DTOs.Conductor;
using SistemaTransporte.Api.DTOs.Programacion;
using SistemaTransporte.Api.DTOs.ProgramacionActivaBusDia;
using SistemaTransporte.Api.DTOs.Login;
using SistemaTransporte.Api.Models;
using SistemaTransporte.Api.Models.Enums;
using SistemaTransporte.Api.Services;

namespace SistemaTransporte.Tests;

public class CrudControllersTests
{
    [Fact]
    public async Task Controladores_ExponenCrudCompletoParaTodosLosModelos()
    {
        var busDao = new MemoryBusDao();
        var conductorDao = new MemoryConductorDao();
        var rolDao = new MemoryRolDao();
        var usuarioDao = new MemoryUsuarioDao();
        var programacionDao = new MemoryProgramacionDao();
        var programacionActivaDao = new MemoryProgramacionActivaDao();

        using var host = await CrearAplicacionAsync(
            busDao, conductorDao, rolDao, usuarioDao,
            programacionDao, programacionActivaDao);
        using var client = host.GetTestClient();

        var rolResponse = await client.PostAsJsonAsync(
            "/api/Roles", new CrearRolDto { Nombre = "Administrador" });
        Assert.Equal(HttpStatusCode.Created, rolResponse.StatusCode);
        var rol = await rolResponse.Content.ReadFromJsonAsync<Rol>();
        Assert.NotNull(rol);

        var busDto = new CrearBusDto
        {
            NumeroIdentificacion = "BUS-1",
            Modelo = "Volvo",
            Capacidad = 40
        };
        var busResponse = await client.PostAsJsonAsync("/api/Buses", busDto);
        Assert.Equal(HttpStatusCode.Created, busResponse.StatusCode);
        var bus = await busResponse.Content.ReadFromJsonAsync<Bus>();
        Assert.NotNull(bus);
        Assert.Equal(EstadoBus.Disponible, bus.Estado);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/Buses")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync($"/api/Buses/{bus.Id}")).StatusCode);
        var busUpdate = await client.PutAsJsonAsync($"/api/Buses/{bus.Id}",
            new ActualizarBusDto
            {
                NumeroIdentificacion = "BUS-1",
                Modelo = "Volvo actualizado",
                Capacidad = 45,
                Estado = EstadoBus.Asignado
            });
        Assert.Equal(HttpStatusCode.OK, busUpdate.StatusCode);

        var conductorResponse = await client.PostAsJsonAsync(
            "/api/Conductores",
            new CrearConductorDto
            {
                Nombre = "Ana Pérez",
                NumeroLicencia = "LIC-1"
            });
        Assert.Equal(HttpStatusCode.Created, conductorResponse.StatusCode);
        var conductor = await conductorResponse.Content.ReadFromJsonAsync<Conductor>();
        Assert.NotNull(conductor);
        Assert.Equal(EstadoConductor.Disponible, conductor.Estado);
        Assert.Equal(HttpStatusCode.OK,
            (await client.GetAsync($"/api/Conductores/{conductor.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.OK,
            (await client.PutAsJsonAsync($"/api/Conductores/{conductor.Id}",
                new ActualizarConductorDto
                {
                    Nombre = "Ana Pérez",
                    NumeroLicencia = "LIC-2",
                    Estado = EstadoConductor.Asignado
                })).StatusCode);

        var programacionResponse = await client.PostAsJsonAsync(
            "/api/Programaciones",
            new CrearProgramacionDto
            {
                BusId = bus.Id,
                ConductorId = conductor.Id,
                DiaSemana = DiaSemana.Lunes,
                HoraEntrada = new TimeSpan(8, 0, 0),
                HoraSalida = new TimeSpan(16, 0, 0)
            });
        Assert.Equal(HttpStatusCode.Created, programacionResponse.StatusCode);
        var programacion = await programacionResponse.Content.ReadFromJsonAsync<Programacion>();
        Assert.NotNull(programacion);
        Assert.Equal(DiaSemana.Lunes, programacion.DiaSemana);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/Programaciones")).StatusCode);
        Assert.Equal(HttpStatusCode.OK,
            (await client.GetAsync($"/api/Programaciones/{programacion.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.OK,
            (await client.PutAsJsonAsync($"/api/Programaciones/{programacion.Id}",
                new ActualizarProgramacionDto
                {
                    BusId = bus.Id,
                    ConductorId = conductor.Id,
                    DiaSemana = DiaSemana.Martes,
                    HoraEntrada = new TimeSpan(9, 0, 0),
                    HoraSalida = new TimeSpan(17, 0, 0),
                    Activa = false
                })).StatusCode);

        var activaResponse = await client.PostAsJsonAsync(
            "/api/ProgramacionesActivasBusDia",
            new CrearProgramacionActivaBusDiaDto
            {
                BusId = bus.Id,
                DiaSemana = DiaSemana.Lunes
            });
        Assert.Equal(HttpStatusCode.Created, activaResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK,
            (await client.GetAsync("/api/ProgramacionesActivasBusDia")).StatusCode);
        Assert.Equal(HttpStatusCode.OK,
            (await client.GetAsync($"/api/ProgramacionesActivasBusDia/{bus.Id}/1")).StatusCode);
        Assert.Equal(HttpStatusCode.OK,
            (await client.PutAsJsonAsync(
                $"/api/ProgramacionesActivasBusDia/{bus.Id}/1",
                new ActualizarProgramacionActivaBusDiaDto
                {
                    BusId = bus.Id,
                    DiaSemana = DiaSemana.Martes
                })).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent,
            (await client.DeleteAsync($"/api/ProgramacionesActivasBusDia/{bus.Id}/2")).StatusCode);

        var usuarioResponse = await client.PostAsJsonAsync(
            "/api/Usuarios",
            new CrearUsuarioDto
            {
                NombreUsuario = "ana",
                Password = "contrasena-segura",
                RolId = rol.Id
            });
        Assert.Equal(HttpStatusCode.Created, usuarioResponse.StatusCode);
        var usuario = await usuarioResponse.Content.ReadFromJsonAsync<Usuario>();
        Assert.NotNull(usuario);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/Usuarios")).StatusCode);
        var usuarioJson = await (await client.GetAsync($"/api/Usuarios/{usuario.Id}"))
            .Content.ReadAsStringAsync();
        Assert.DoesNotContain("passwordHash", usuarioJson, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(HttpStatusCode.OK,
            (await client.PutAsJsonAsync($"/api/Usuarios/{usuario.Id}",
                new ActualizarUsuarioDto
                {
                    NombreUsuario = "ana2",
                    RolId = rol.Id,
                    Activo = false,
                    Password = "nueva-contrasena-segura"
                })).StatusCode);

        Assert.Equal(HttpStatusCode.NoContent,
            (await client.DeleteAsync($"/api/Usuarios/{usuario.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent,
            (await client.DeleteAsync($"/api/Programaciones/{programacion.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent,
            (await client.DeleteAsync($"/api/Conductores/{conductor.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent,
            (await client.DeleteAsync($"/api/Buses/{bus.Id}")).StatusCode);
    }

    [Fact]
    public async Task Programaciones_RechazaHorasInvalidasYReferenciasInexistentes()
    {
        var busDao = new MemoryBusDao();
        var conductorDao = new MemoryConductorDao();
        var programacionDao = new MemoryProgramacionDao();
        var host = await CrearAplicacionAsync(
            busDao,
            conductorDao,
            new MemoryRolDao(),
            new MemoryUsuarioDao(),
            programacionDao,
            new MemoryProgramacionActivaDao());
        using (host)
        using (var client = host.GetTestClient())
        {
            var badTime = await client.PostAsJsonAsync(
                "/api/Programaciones",
                new CrearProgramacionDto
                {
                    BusId = 1,
                    ConductorId = 1,
                    DiaSemana = DiaSemana.Lunes,
                    HoraEntrada = new TimeSpan(17, 0, 0),
                    HoraSalida = new TimeSpan(8, 0, 0)
                });
            Assert.Equal(HttpStatusCode.BadRequest, badTime.StatusCode);

            var missingBus = await client.PostAsJsonAsync(
                "/api/Programaciones",
                new CrearProgramacionDto
                {
                    BusId = 99,
                    ConductorId = 1,
                    DiaSemana = DiaSemana.Lunes,
                    HoraEntrada = new TimeSpan(8, 0, 0),
                    HoraSalida = new TimeSpan(16, 0, 0)
                });
            Assert.Equal(HttpStatusCode.BadRequest, missingBus.StatusCode);
        }
    }

    [Fact]
    public async Task Programaciones_Devuelve409CuandoHayConflictosDeBusODelConductor()
    {
        var busDao = new MemoryBusDao();
        var conductorDao = new MemoryConductorDao();
        var primerBus = await busDao.CreateAsync(new Bus
        {
            NumeroIdentificacion = "BUS-1",
            Modelo = "Modelo",
            Capacidad = 30
        });
        var segundoBus = await busDao.CreateAsync(new Bus
        {
            NumeroIdentificacion = "BUS-2",
            Modelo = "Modelo",
            Capacidad = 30
        });
        var tercerBus = await busDao.CreateAsync(new Bus
        {
            NumeroIdentificacion = "BUS-3",
            Modelo = "Modelo",
            Capacidad = 30
        });
        var primerConductor = await conductorDao.CreateAsync(new Conductor
        {
            Nombre = "Conductor 1",
            NumeroLicencia = "LIC-1"
        });
        var segundoConductor = await conductorDao.CreateAsync(new Conductor
        {
            Nombre = "Conductor 2",
            NumeroLicencia = "LIC-2"
        });

        using var host = await CrearAplicacionAsync(
            busDao,
            conductorDao,
            new MemoryRolDao(),
            new MemoryUsuarioDao(),
            new MemoryProgramacionDao(),
            new MemoryProgramacionActivaDao());
        using var client = host.GetTestClient();

        var primera = await client.PostAsJsonAsync("/api/Programaciones",
            CrearHorario(primerBus.Id, primerConductor.Id, 8, 12));
        Assert.Equal(HttpStatusCode.Created, primera.StatusCode);

        var busOcupado = await client.PostAsJsonAsync("/api/Programaciones",
            CrearHorario(primerBus.Id, segundoConductor.Id, 13, 15));
        Assert.Equal(HttpStatusCode.Conflict, busOcupado.StatusCode);
        Assert.Contains("programación activa", await busOcupado.Content.ReadAsStringAsync());

        var conductorOcupado = await client.PostAsJsonAsync("/api/Programaciones",
            CrearHorario(segundoBus.Id, primerConductor.Id, 11, 13));
        Assert.Equal(HttpStatusCode.Conflict, conductorOcupado.StatusCode);
        Assert.Contains("se solapa", await conductorOcupado.Content.ReadAsStringAsync());

        var tercera = await client.PostAsJsonAsync("/api/Programaciones",
            CrearHorario(tercerBus.Id, segundoConductor.Id, 8, 10));
        Assert.Equal(HttpStatusCode.Created, tercera.StatusCode);
        var terceraProgramacion = await tercera.Content.ReadFromJsonAsync<Programacion>();
        Assert.NotNull(terceraProgramacion);

        var actualizacionConflictiva = await client.PutAsJsonAsync(
            $"/api/Programaciones/{terceraProgramacion.Id}",
            new ActualizarProgramacionDto
            {
                BusId = primerBus.Id,
                ConductorId = segundoConductor.Id,
                DiaSemana = DiaSemana.Lunes,
                HoraEntrada = new TimeSpan(8, 0, 0),
                HoraSalida = new TimeSpan(10, 0, 0),
                Activa = true
            });
        Assert.Equal(HttpStatusCode.Conflict, actualizacionConflictiva.StatusCode);
    }

    private static CrearProgramacionDto CrearHorario(
        int busId, int conductorId, int horaEntrada, int horaSalida) => new()
    {
        BusId = busId,
        ConductorId = conductorId,
        DiaSemana = DiaSemana.Lunes,
        HoraEntrada = new TimeSpan(horaEntrada, 0, 0),
        HoraSalida = new TimeSpan(horaSalida, 0, 0),
        Activa = true
    };

    [Fact]
    public async Task Enums_RechazanValoresNoDefinidosEnSolicitudes()
    {
        using var host = await CrearAplicacionAsync(
            new MemoryBusDao(),
            new MemoryConductorDao(),
            new MemoryRolDao(),
            new MemoryUsuarioDao(),
            new MemoryProgramacionDao(),
            new MemoryProgramacionActivaDao());
        using var client = host.GetTestClient();

        var busResponse = await client.PostAsJsonAsync(
            "/api/Buses",
            new CrearBusDto
            {
                NumeroIdentificacion = "BUS-INVALID",
                Modelo = "Modelo",
                Capacidad = 30,
                Estado = (EstadoBus)99
            });
        Assert.Equal(HttpStatusCode.BadRequest, busResponse.StatusCode);

        var conductorResponse = await client.PostAsJsonAsync(
            "/api/Conductores",
            new CrearConductorDto
            {
                Nombre = "Conductor",
                NumeroLicencia = "LIC-INVALID",
                Estado = (EstadoConductor)99
            });
        Assert.Equal(HttpStatusCode.BadRequest, conductorResponse.StatusCode);

        var diaResponse = await client.PostAsJsonAsync(
            "/api/ProgramacionesActivasBusDia",
            new CrearProgramacionActivaBusDiaDto
            {
                BusId = 1,
                DiaSemana = (DiaSemana)0
            });
        Assert.Equal(HttpStatusCode.BadRequest, diaResponse.StatusCode);
    }

    private static async Task<IHost> CrearAplicacionAsync(
        IBusDao busDao,
        IConductorDao conductorDao,
        IRolDao rolDao,
        IUsuarioDao usuarioDao,
        IProgramacionDao programacionDao,
        IProgramacionActivaBusDiaDao programacionActivaDao)
    {
        var host = new HostBuilder()
            .ConfigureWebHost(webHost => webHost
                .UseTestServer()
                .ConfigureServices(services =>
                {
                    services.AddAuthentication(options =>
                    {
                        options.DefaultAuthenticateScheme = TestAuthenticationHandler.SchemeName;
                        options.DefaultChallengeScheme = TestAuthenticationHandler.SchemeName;
                    }).AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>(
                        TestAuthenticationHandler.SchemeName, _ => { });
                    services.AddAuthorization();
                    services.AddControllers()
                        .AddApplicationPart(typeof(BusesController).Assembly);
                    services.AddSingleton(busDao);
                    services.AddSingleton(conductorDao);
                    services.AddSingleton(rolDao);
                    services.AddSingleton(usuarioDao);
                    services.AddSingleton(programacionDao);
                    services.AddSingleton(programacionActivaDao);
                    services.AddScoped<BusService>();
                    services.AddScoped<ConductorService>();
                    services.AddScoped<RolService>();
                    services.AddScoped<UsuarioService>();
                    services.AddScoped<ProgramacionService>();
                    services.AddScoped<ProgramacionActivaBusDiaService>();
                    services.AddScoped<IPasswordHasher<Usuario>, PasswordHasher<Usuario>>();
                })
                .Configure(app =>
                {
                    app.UseRouting();
                    app.UseAuthentication();
                    app.UseAuthorization();
                    app.UseEndpoints(endpoints => endpoints.MapControllers());
                }))
            .Build();

        await host.StartAsync();
        return host;
    }

    private class MemoryDao<TEntity> : IDAO<TEntity>
        where TEntity : class
    {
        private readonly List<TEntity> _items = [];
        private readonly Func<TEntity, int> _getId;
        private readonly Action<TEntity, int> _setId;
        private int _nextId = 1;

        public MemoryDao(Func<TEntity, int> getId, Action<TEntity, int> setId)
        {
            _getId = getId;
            _setId = setId;
        }

        public Task<IEnumerable<TEntity>> GetAllAsync() =>
            Task.FromResult<IEnumerable<TEntity>>(_items.ToArray());

        public Task<TEntity?> GetByIdAsync(int id) =>
            Task.FromResult(_items.FirstOrDefault(item => _getId(item) == id));

        public Task<TEntity> CreateAsync(TEntity entity)
        {
            _setId(entity, _nextId++);
            _items.Add(entity);
            return Task.FromResult(entity);
        }

        public Task<TEntity?> UpdateAsync(int id, TEntity entity)
        {
            var index = _items.FindIndex(item => _getId(item) == id);
            if (index < 0)
            {
                return Task.FromResult<TEntity?>(null);
            }

            _setId(entity, id);
            _items[index] = entity;
            return Task.FromResult<TEntity?>(entity);
        }

        public Task<bool> DeleteAsync(int id)
        {
            var item = _items.FirstOrDefault(candidate => _getId(candidate) == id);
            return Task.FromResult(item is not null && _items.Remove(item));
        }
    }

    private sealed class MemoryBusDao : MemoryDao<Bus>, IBusDao
    {
        public MemoryBusDao() : base(bus => bus.Id, (bus, id) => bus.Id = id) { }
    }

    private sealed class MemoryConductorDao : MemoryDao<Conductor>, IConductorDao
    {
        public MemoryConductorDao()
            : base(conductor => conductor.Id, (conductor, id) => conductor.Id = id) { }
    }

    private sealed class MemoryRolDao : MemoryDao<Rol>, IRolDao
    {
        public MemoryRolDao() : base(rol => rol.Id, (rol, id) => rol.Id = id) { }
    }

    private sealed class MemoryUsuarioDao : MemoryDao<Usuario>, IUsuarioDao
    {
        public MemoryUsuarioDao()
            : base(usuario => usuario.Id, (usuario, id) => usuario.Id = id) { }

        public async Task<Usuario?> GetByUsernameWithPasswordAsync(string nombreUsuario) =>
            (await GetAllAsync()).SingleOrDefault(
                usuario => usuario.NombreUsuario == nombreUsuario);
    }

    private sealed class MemoryProgramacionDao : MemoryDao<Programacion>, IProgramacionDao
    {
        public MemoryProgramacionDao()
            : base(programacion => programacion.Id, (programacion, id) => programacion.Id = id) { }
    }

    private sealed class MemoryProgramacionActivaDao : IProgramacionActivaBusDiaDao
    {
        private readonly Dictionary<(int BusId, DiaSemana DiaSemana), ProgramacionActivaBusDia>
            _items = [];

        public Task<IEnumerable<ProgramacionActivaBusDia>> GetAllAsync() =>
            Task.FromResult<IEnumerable<ProgramacionActivaBusDia>>(_items.Values.ToArray());

        public Task<ProgramacionActivaBusDia?> GetByIdAsync(
            int busId, DiaSemana diaSemana) =>
            Task.FromResult(_items.GetValueOrDefault((busId, diaSemana)));

        public Task<ProgramacionActivaBusDia> CreateAsync(ProgramacionActivaBusDia entity)
        {
            _items.Add((entity.BusId, entity.DiaSemana), entity);
            return Task.FromResult(entity);
        }

        public Task<ProgramacionActivaBusDia?> UpdateAsync(
            int busId, DiaSemana diaSemana, ProgramacionActivaBusDia entity)
        {
            if (!_items.Remove((busId, diaSemana)))
            {
                return Task.FromResult<ProgramacionActivaBusDia?>(null);
            }

            _items[(entity.BusId, entity.DiaSemana)] = entity;
            return Task.FromResult<ProgramacionActivaBusDia?>(entity);
        }

        public Task<bool> DeleteAsync(int busId, DiaSemana diaSemana) =>
            Task.FromResult(_items.Remove((busId, diaSemana)));
    }

    private sealed class TestAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public const string SchemeName = "Test";

        public TestAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder)
            : base(options, logger, encoder)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var identity = new ClaimsIdentity(
                [
                    new Claim(ClaimTypes.NameIdentifier, "1"),
                    new Claim(ClaimTypes.Role, AutenticacionService.Administrador)
                ],
                Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);
            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}
