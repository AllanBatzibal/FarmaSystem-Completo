using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FarmaSystem.API.Data;
using FarmaSystem.API.DTOs;
using FarmaSystem.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace FarmaSystem.API.Services;

public class AuthService : IAuthService
{
    private readonly FarmaSystemContext _context;
    private readonly IConfiguration _configuration;

    public AuthService(FarmaSystemContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<LoginResponseDTO?> LoginAsync(LoginRequestDTO request)
    {
        if (string.IsNullOrWhiteSpace(request.Usuario) || string.IsNullOrWhiteSpace(request.Contrasena))
            return null;

        var usuario = request.Usuario.Trim().ToLowerInvariant();
        var contrasena = request.Contrasena;

        var authUsers = _configuration.GetSection("Auth:Usuarios").Get<List<UsuarioAuthConfig>>() ?? new();
        var match = authUsers.FirstOrDefault(u =>
            u.Usuario.Equals(usuario, StringComparison.OrdinalIgnoreCase) &&
            u.Contrasena == contrasena);

        Empleado? empleado = null;

        if (match != null)
        {
            empleado = await _context.Empleados
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.IdEmpleado == match.IdEmpleado && e.Activo);

            if (empleado == null)
            {
                empleado = await _context.Empleados
                    .AsNoTracking()
                    .Where(e => e.Activo)
                    .OrderBy(e => e.IdEmpleado)
                    .FirstOrDefaultAsync();
            }
        }
        else
        {
            empleado = await _context.Empleados
                .AsNoTracking()
                .FirstOrDefaultAsync(e =>
                    e.Activo &&
                    (e.Nombre.ToLower() == usuario ||
                     e.Telefono == usuario ||
                     e.IdEmpleado.ToString() == usuario));
        }

        if (empleado == null)
            return null;

        if (match == null)
        {
            var clavePorDefecto = _configuration["Auth:ContrasenaPorDefecto"] ?? "1234";
            if (contrasena != clavePorDefecto && contrasena != empleado.Telefono)
                return null;
        }

        var rol = NormalizarRol(empleado.Cargo, match?.Rol);
        var token = GenerarToken(empleado, rol);

        return new LoginResponseDTO
        {
            Token = token,
            IdEmpleado = empleado.IdEmpleado,
            Nombre = empleado.Nombre,
            Apellido = empleado.Apellido,
            NombreCompleto = $"{empleado.Nombre} {empleado.Apellido}".Trim(),
            Rol = rol,
            Cargo = empleado.Cargo
        };
    }

    public string NormalizarRol(string? cargo, string? rolOverride = null)
    {
        if (!string.IsNullOrWhiteSpace(rolOverride))
            return rolOverride.Trim();

        var c = (cargo ?? "").ToLowerInvariant();
        if (c.Contains("admin")) return "Administrador";
        if (c.Contains("invent") || c.Contains("bodega") || c.Contains("almacen")) return "Inventario";
        if (c.Contains("vend") || c.Contains("cajer")) return "Vendedor";
        return "Vendedor";
    }

    private string GenerarToken(Empleado empleado, string rol)
    {
        var key = _configuration["Jwt:Key"] ?? "FarmaSystemSecretKey2026_Minimo32Caracteres!";
        var issuer = _configuration["Jwt:Issuer"] ?? "FarmaSystem";
        var audience = _configuration["Jwt:Audience"] ?? "FarmaSystemVue";
        var expireMinutes = int.TryParse(_configuration["Jwt:ExpireMinutes"], out var m) ? m : 480;

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, empleado.IdEmpleado.ToString()),
            new Claim(ClaimTypes.Name, $"{empleado.Nombre} {empleado.Apellido}".Trim()),
            new Claim(ClaimTypes.Role, rol),
            new Claim("idEmpleado", empleado.IdEmpleado.ToString()),
            new Claim("cargo", empleado.Cargo ?? "")
        };

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer,
            audience,
            claims,
            expires: DateTime.UtcNow.AddMinutes(expireMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
