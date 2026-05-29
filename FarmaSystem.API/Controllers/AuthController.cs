using FarmaSystem.API.Data;
using FarmaSystem.API.DTOs;
using FarmaSystem.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FarmaSystem.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly FarmaSystemContext _context;

    public AuthController(IAuthService authService, FarmaSystemContext context)
    {
        _authService = authService;
        _context = context;
    }

    [HttpGet("estado")]
    public async Task<IActionResult> Estado()
    {
        try
        {
            var empleados = await _context.Empleados.CountAsync(e => e.Activo);
            var clientes = await _context.Clientes.CountAsync();
            return Ok(new
            {
                api = "conectada",
                baseDatos = "conectada",
                url = "http://localhost:5280/api",
                empleados,
                clientes
            });
        }
        catch (Exception ex)
        {
            return Ok(new
            {
                api = "conectada",
                baseDatos = "error",
                mensaje = ex.Message
            });
        }
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponseDTO>> Login([FromBody] LoginRequestDTO request)
    {
        try
        {
            var result = await _authService.LoginAsync(request);
            if (result == null)
                return Unauthorized(new { message = "Usuario o contraseña incorrectos." });

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"Error al iniciar sesión: {ex.Message}" });
        }
    }
}
