using SistemaTransporte.Api.DAO;
using SistemaTransporte.Api.Services;
using SistemaTransporte.Api.Data;
using Microsoft.AspNetCore.Identity;
using SistemaTransporte.Api.Models;
using SistemaTransporte.Api.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Threading.RateLimiting;

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
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("login", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true
            }));
    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.ContentType = "application/json";
        await context.HttpContext.Response.WriteAsJsonAsync(
            new { mensaje = "Demasiados intentos. Espera un minuto antes de volver a intentar." },
            cancellationToken);
    };
});

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
app.UseRouting();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
