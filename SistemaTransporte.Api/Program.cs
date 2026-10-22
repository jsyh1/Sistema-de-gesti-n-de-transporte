using SistemaTransporte.Api.DAO;
using SistemaTransporte.Api.Services;
using SistemaTransporte.Api.Data;

var builder = WebApplication.CreateBuilder(args);

// Servicios de la aplicación.
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Inyección de dependencias.
builder.Services.AddSingleton<ConexionDB>();

builder.Services.AddScoped<IRolDao, RolDao>();
builder.Services.AddScoped<RolService>();

var app = builder.Build();

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
