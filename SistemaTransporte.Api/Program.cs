using SistemaTransporte.Api.DAO;
using SistemaTransporte.Api.Services;
using SistemaTransporte.Api.Data;
using Microsoft.AspNetCore.Identity;
using SistemaTransporte.Api.Models;
using SistemaTransporte.Api.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Servicios de la aplicación.
builder.Services.AddControllers();
builder.Services.AddOpenApi();

var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrWhiteSpace(jwtKey) || Encoding.UTF8.GetByteCount(jwtKey) < 32)
{
    throw new InvalidOperationException(
        "Configure Jwt:Key con una clave de al menos 32 bytes mediante una variable de entorno.");
}

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = "SistemaTransporte",
            ValidateAudience = true,
            ValidAudience = "SistemaTransporte.Client",
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30),
            RoleClaimType = System.Security.Claims.ClaimTypes.Role,
            NameClaimType = System.Security.Claims.ClaimTypes.Name
        };
    });
builder.Services.AddAuthorization();

// Inyección de dependencias.
builder.Services.AddSingleton<ConexionDB>();

builder.Services.AddScoped<IRolDao, RolDao>();
builder.Services.AddScoped<RolService>();

builder.Services.AddScoped<IUsuarioDao, UsuarioDao>();
builder.Services.AddScoped<UsuarioService>();
builder.Services.AddScoped<AutenticacionService>();

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
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
