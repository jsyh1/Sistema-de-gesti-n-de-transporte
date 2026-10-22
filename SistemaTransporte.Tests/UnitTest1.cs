using SistemaTransporte.Api.DAO;
using SistemaTransporte.Api.DTOs.Bus;
using SistemaTransporte.Api.Models;
using SistemaTransporte.Api.Services;

namespace SistemaTransporte.Tests;

public class BusServiceTests
{
    [Fact]
    public async Task CreateAsync_RechazaCapacidadCero()
    {
        var service = new BusService(new FakeBusDao());

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.CreateAsync(new CrearBusDto
            {
                NumeroIdentificacion = "BUS-1",
                Modelo = "Modelo",
                Capacidad = 0
            }));
    }

    private sealed class FakeBusDao : IBusDao
    {
        public Task<IEnumerable<Bus>> GetAllAsync() =>
            Task.FromResult<IEnumerable<Bus>>([]);

        public Task<Bus?> GetByIdAsync(int id) =>
            Task.FromResult<Bus?>(null);

        public Task<Bus> CreateAsync(Bus entity) =>
            Task.FromResult(entity);

        public Task<Bus?> UpdateAsync(int id, Bus entity) =>
            Task.FromResult<Bus?>(null);

        public Task<bool> DeleteAsync(int id) =>
            Task.FromResult(false);
    }
}
