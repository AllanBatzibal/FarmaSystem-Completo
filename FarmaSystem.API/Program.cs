using System.Text;
using FarmaSystem.API.Data;
using FarmaSystem.API.Mapping;
using FarmaSystem.API.Services;
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

builder.Services.AddDbContext<FarmaSystemContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("FarmaSystemDB")));

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

builder.Services.AddAutoMapper(typeof(MappingProfile));

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IVentaService, VentaService>();
builder.Services.AddScoped<ICompraService, CompraService>();
builder.Services.AddScoped<IInventarioService, InventarioService>();

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
