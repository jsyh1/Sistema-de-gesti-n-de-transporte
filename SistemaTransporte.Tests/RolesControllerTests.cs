using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SistemaTransporte.Api.Controllers;
using SistemaTransporte.Api.DAO;
using SistemaTransporte.Api.Models;
using SistemaTransporte.Api.Services;

namespace SistemaTransporte.Tests;

public class RolesControllerTests
{
    [Fact]
    public async Task GetRoles_DevuelveListaDeRoles()
    {
        var dao = new FakeRolDao(
        [
            new Rol { Id = 1, Nombre = "Administrador" },
            new Rol { Id = 2, Nombre = "Conductor" }
        ]);
        using var host = await CrearAplicacionAsync(dao);
        using var client = host.GetTestClient();

        using var response = await client.GetAsync("/api/Roles");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var roles = await response.Content.ReadFromJsonAsync<List<Rol>>();
        Assert.NotNull(roles);
        Assert.Collection(
            roles,
            rol =>
            {
                Assert.Equal(1, rol.Id);
                Assert.Equal("Administrador", rol.Nombre);
            },
            rol =>
            {
                Assert.Equal(2, rol.Id);
                Assert.Equal("Conductor", rol.Nombre);
            });
    }

    [Fact]
    public async Task GetRol_CuandoExiste_DevuelveElRol()
    {
        using var host = await CrearAplicacionAsync(new FakeRolDao(
            [new Rol { Id = 1, Nombre = "Administrador" }]));
        using var client = host.GetTestClient();

        using var response = await client.GetAsync("/api/Roles/1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var rol = await response.Content.ReadFromJsonAsync<Rol>();
        Assert.NotNull(rol);
        Assert.Equal(1, rol.Id);
        Assert.Equal("Administrador", rol.Nombre);
    }

    [Fact]
    public async Task GetRol_CuandoNoExiste_DevuelveNotFound()
    {
        using var host = await CrearAplicacionAsync(new FakeRolDao([]));
        using var client = host.GetTestClient();

        using var response = await client.GetAsync("/api/Roles/99");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task PostRol_CreaRolYDevuelveUbicacionParaConsultarlo()
    {
        using var host = await CrearAplicacionAsync(new FakeRolDao([]));
        using var client = host.GetTestClient();

        using var response = await client.PostAsJsonAsync(
            "/api/Roles",
            new Rol { Nombre = "Supervisor" });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);

        var rolCreado = await response.Content.ReadFromJsonAsync<Rol>();
        Assert.NotNull(rolCreado);
        Assert.Equal(1, rolCreado.Id);
        Assert.Equal("Supervisor", rolCreado.Nombre);

        using var consulta = await client.GetAsync(response.Headers.Location);
        Assert.Equal(HttpStatusCode.OK, consulta.StatusCode);
        var rolConsultado = await consulta.Content.ReadFromJsonAsync<Rol>();
        Assert.NotNull(rolConsultado);
        Assert.Equal(rolCreado.Id, rolConsultado.Id);
        Assert.Equal(rolCreado.Nombre, rolConsultado.Nombre);
    }

    [Fact]
    public async Task PutRol_ActualizaRolYDevuelveNoContent()
    {
        using var host = await CrearAplicacionAsync(new FakeRolDao(
            [new Rol { Id = 1, Nombre = "Conductor" }]));
        using var client = host.GetTestClient();

        using var response = await client.PutAsJsonAsync(
            "/api/Roles/1",
            new Rol { Nombre = "Supervisor" });

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        using var consulta = await client.GetAsync("/api/Roles/1");
        var rolActualizado = await consulta.Content.ReadFromJsonAsync<Rol>();
        Assert.NotNull(rolActualizado);
        Assert.Equal("Supervisor", rolActualizado.Nombre);
    }

    [Fact]
    public async Task PutRol_CuandoNoExiste_DevuelveNotFound()
    {
        using var host = await CrearAplicacionAsync(new FakeRolDao([]));
        using var client = host.GetTestClient();

        using var response = await client.PutAsJsonAsync(
            "/api/Roles/99",
            new Rol { Nombre = "Supervisor" });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteRol_EliminaRolYLuegoDevuelveNotFound()
    {
        using var host = await CrearAplicacionAsync(new FakeRolDao(
            [new Rol { Id = 1, Nombre = "Administrador" }]));
        using var client = host.GetTestClient();

        using var response = await client.DeleteAsync("/api/Roles/1");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        using var consulta = await client.GetAsync("/api/Roles/1");
        Assert.Equal(HttpStatusCode.NotFound, consulta.StatusCode);
    }

    [Fact]
    public async Task DeleteRol_CuandoNoExiste_DevuelveNotFound()
    {
        using var host = await CrearAplicacionAsync(new FakeRolDao([]));
        using var client = host.GetTestClient();

        using var response = await client.DeleteAsync("/api/Roles/99");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private static async Task<IHost> CrearAplicacionAsync(FakeRolDao dao)
    {
        var host = new HostBuilder()
            .ConfigureWebHost(webHost => webHost
                .UseTestServer()
                .ConfigureServices(services =>
                {
                    services
                        .AddControllers()
                        .AddApplicationPart(typeof(RolesController).Assembly);
                    services.AddSingleton<IRolDao>(dao);
                    services.AddScoped<RolService>();
                })
                .Configure(app =>
                {
                    app.UseRouting();
                    app.UseEndpoints(endpoints => endpoints.MapControllers());
                }))
            .Build();

        await host.StartAsync();
        return host;
    }

    private sealed class FakeRolDao : IRolDao
    {
        private readonly List<Rol> _roles;
        private int _siguienteId;

        public FakeRolDao(IEnumerable<Rol> roles)
        {
            _roles = roles.ToList();
            _siguienteId = _roles.Select(rol => rol.Id).DefaultIfEmpty().Max() + 1;
        }

        public Task<IEnumerable<Rol>> GetAllAsync()
        {
            return Task.FromResult<IEnumerable<Rol>>(_roles);
        }

        public Task<Rol?> GetByIdAsync(int id)
        {
            return Task.FromResult(_roles.FirstOrDefault(rol => rol.Id == id));
        }

        public Task<Rol> CreateAsync(Rol entity)
        {
            entity.Id = _siguienteId++;
            _roles.Add(entity);
            return Task.FromResult(entity);
        }

        public Task<Rol?> UpdateAsync(int id, Rol entity)
        {
            var indice = _roles.FindIndex(rol => rol.Id == id);
            if (indice < 0)
            {
                return Task.FromResult<Rol?>(null);
            }

            entity.Id = id;
            _roles[indice] = entity;
            return Task.FromResult<Rol?>(entity);
        }

        public Task<bool> DeleteAsync(int id)
        {
            var rol = _roles.FirstOrDefault(rol => rol.Id == id);
            return Task.FromResult(rol is not null && _roles.Remove(rol));
        }
    }
}
