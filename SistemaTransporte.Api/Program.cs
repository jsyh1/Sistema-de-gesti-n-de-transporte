using SistemaTransporte.Api.DAO;
using SistemaTransporte.Api.Services;
using SistemaTransporte.Api.Data;
using Microsoft.AspNetCore.Identity;
using SistemaTransporte.Api.Models;
using SistemaTransporte.Api.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Servicios de la aplicación.
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Inyección de dependencias.
builder.Services.AddSingleton<ConexionDB>();

builder.Services.AddScoped<IRolDao, RolDao>();
builder.Services.AddScoped<RolService>();

builder.Services.AddScoped<IUsuarioDao, UsuarioDao>();
builder.Services.AddScoped<UsuarioService>();

builder.Services.AddScoped<IPasswordHasher<Usuario>, PasswordHasher<Usuario>>();

builder.Services.AddScoped<IBusDao, BusDao>();
builder.Services.AddScoped<BusService>();

builder.Services.AddScoped<IConductorDao, ConductorDao>();
builder.Services.AddScoped<ConductorService>();

builder.Services.AddScoped<IProgramacionDao, ProgramacionDao>();
builder.Services.AddScoped<ProgramacionService>();

builder.Services.AddScoped<IProgramacionActivaBusDiaDao, ProgramacionActivaBusDiaDao>();
builder.Services.AddScoped<ProgramacionActivaBusDiaService>();

var app = builder.Build();

app.UseMiddleware<ExceptionMiddleware>();
// Configuración de Swagger en desarrollo.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint(
            "/openapi/v1.json",
            "SistemaTransporte API v1");
    });
}

// Configuración del flujo HTTP.
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
