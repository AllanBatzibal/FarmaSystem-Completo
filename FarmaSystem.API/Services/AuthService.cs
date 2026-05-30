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
        if (string.IsNullOrWhiteSpace(request.Usuario) || 
            string.IsNullOrWhiteSpace(request.Contrasena))
            return null;

        var usuarioNombre = request.Usuario.Trim().ToLowerInvariant();
        var contrasena = request.Contrasena;

        // Buscar usuario en la tabla Usuario de la base de datos
        var usuarioDB = await _context.Usuarios
            .Include(u => u.Empleado)
            .Include(u => u.Rol)
            .AsNoTracking()
            .FirstOrDefaultAsync(u =>
                u.UsuarioNombre.ToLower() == usuarioNombre &&
                u.Contrasena == contrasena &&
                u.Activo &&
                u.Empleado.Activo);

        if (usuarioDB == null)
            return null;

        // Actualizar ultimo acceso
        var usuarioUpdate = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.IdUsuario == usuarioDB.IdUsuario);
        if (usuarioUpdate != null)
        {
            usuarioUpdate.UltimoAcceso = DateTime.Now;
            await _context.SaveChangesAsync();
        }

        var rol = usuarioDB.Rol.Nombre;
        var token = GenerarToken(usuarioDB.Empleado, rol);

        return new LoginResponseDTO
        {
            Token = token,
            IdEmpleado = usuarioDB.Empleado.IdEmpleado,
            Nombre = usuarioDB.Empleado.Nombre,
            Apellido = usuarioDB.Empleado.Apellido,
            NombreCompleto = $"{usuarioDB.Empleado.Nombre} {usuarioDB.Empleado.Apellido}".Trim(),
            Rol = rol,
            Cargo = usuarioDB.Empleado.Cargo
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
