using SistemaTransporte.Api.DAO;
using SistemaTransporte.Api.Services;
using SistemaTransporte.Api.Data;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();


//inyecion de rutas de dependencias
builder.Services.AddScoped<IRolDao, RolDao>();
builder.Services.AddScoped<RolService>();

builder.Services.AddSingleton<ConexionDB>(
    ConexionDB.Instancia);

builder.Services.AddScoped<IRolDao, RolDao>();
builder.Services.AddScoped<RolService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
