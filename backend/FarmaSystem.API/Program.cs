using System.Text;
using FarmaSystem.Infrastructure;
using FarmaSystem.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "FarmaSystem API", Version = "v1" });
});

// ── Verificación de conexión a base de datos ──────────────────────────────
string connectionString = builder.Configuration.GetConnectionString("FarmaSystemDB")
    ?? throw new InvalidOperationException("No se encontró la cadena de conexión 'FarmaSystemDB'.");

bool usarLocalDB = false;

// Probar la conexión al SQL Server configurado
try
{
    using var testConnection = new Microsoft.Data.SqlClient.SqlConnection(connectionString);
    await testConnection.OpenAsync();
    // Conexión exitosa — continuar normalmente
}
catch
{
    // Conexión fallida — mostrar alerta interactiva en consola
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine();
    Console.WriteLine("╔══════════════════════════════════════════════════════════╗");
    Console.WriteLine("║         FARMASYSTEM — ALERTA DE BASE DE DATOS            ║");
    Console.WriteLine("╠══════════════════════════════════════════════════════════╣");
    Console.WriteLine("║  No se puede conectar al SQL Server configurado.         ║");

    // Mostrar el servidor configurado
    var serverName = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(connectionString).DataSource;
    Console.WriteLine($"║  Servidor: {serverName,-47}║");
    Console.WriteLine("║                                                          ║");
    Console.WriteLine("║  ¿Desea utilizar una base de datos local (LocalDB)?      ║");
    Console.WriteLine("║                                                          ║");
    Console.WriteLine("║  [S] Sí — usar base de datos local                       ║");
    Console.WriteLine("║  [N] No — salir de la aplicación                         ║");
    Console.WriteLine("╚══════════════════════════════════════════════════════════╝");
    Console.ResetColor();
    Console.Write("Ingrese su opción (S/N): ");

    string? respuesta = null;
    while (respuesta != "S" && respuesta != "N")
    {
        respuesta = Console.ReadLine()?.Trim().ToUpper();
        if (respuesta != "S" && respuesta != "N")
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Opción inválida. Por favor ingrese S o N.");
            Console.ResetColor();
            Console.Write("Ingrese su opción (S/N): ");
        }
    }

    if (respuesta == "N")
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("Aplicación detenida por el usuario.");
        Console.ResetColor();
        Environment.Exit(0);
    }

    // Usuario eligió S — cambiar a LocalDB
    usarLocalDB = true;
    connectionString = "Server=(localdb)\\MSSQLLocalDB;Database=FarmaSystemDB;Trusted_Connection=True;TrustServerCertificate=True;";
}

builder.Services.AddInfrastructure(builder.Configuration, connectionString);
// ── Fin verificación ──────────────────────────────────────────────────────

var jwtKey = builder.Configuration["Jwt:Key"] ?? "FarmaSystemSecretKey2026_Minimo32Caracteres!";
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();

var corsOrigins = new[]
{
    builder.Configuration["Cors:AllowedOrigin"] ?? "http://localhost:5173",
    "http://127.0.0.1:5173"
};
builder.Services.AddCors(options =>
{
    options.AddPolicy("VuePolicy", policy =>
    {
        policy.WithOrigins(corsOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

// ── Inicializar base de datos local si el usuario eligió esa opción ───────
if (usarLocalDB)
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<FarmaSystemContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    try
    {
        var scriptPath = Path.Combine(AppContext.BaseDirectory, "database", "schema", "FarmaSystemDB.sql");

        if (!File.Exists(scriptPath))
        {
            // Buscar también relativo al directorio de trabajo (útil en desarrollo)
            scriptPath = Path.Combine(Directory.GetCurrentDirectory(),
                "..", "database", "schema", "FarmaSystemDB.sql");
        }

        if (!File.Exists(scriptPath))
        {
            scriptPath = Path.GetFullPath(Path.Combine(
                Directory.GetCurrentDirectory(), "..", "..", "database", "schema", "FarmaSystemDB.sql"));
        }

        if (File.Exists(scriptPath))
        {
            logger.LogInformation("Ejecutando script de inicialización de base de datos local...");
            var script = await File.ReadAllTextAsync(scriptPath);

            // Dividir por GO (separador de batches de SQL Server)
            var batches = script.Split(
                new[] { "\nGO\n", "\nGO\r\n", "\r\nGO\r\n", "\r\nGO\n", "\nGO", "\r\nGO" },
                StringSplitOptions.RemoveEmptyEntries
            );

            foreach (var batch in batches)
            {
                var trimmed = batch.Trim();
                if (!string.IsNullOrWhiteSpace(trimmed))
                {
                    await context.Database.ExecuteSqlRawAsync(trimmed);
                }
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("✓ Base de datos local inicializada correctamente. Continuando...");
            Console.ResetColor();
        }
        else
        {
            logger.LogError("No se encontró el script SQL en: {Path}", scriptPath);
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"✗ No se encontró el script SQL. Ruta buscada: {scriptPath}");
            Console.ResetColor();
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error al inicializar la base de datos local.");
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"✗ Error al inicializar la base de datos: {ex.Message}");
        Console.ResetColor();
    }
}
// ── Fin inicialización local ──────────────────────────────────────────────

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "FarmaSystem API v1"));
}

app.UseCors("VuePolicy");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
