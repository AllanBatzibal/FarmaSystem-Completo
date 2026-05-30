using FarmaSystem.Core.DTOs;
using FarmaSystem.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FarmaSystem.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpGet("estado")]
    public async Task<IActionResult> Estado()
    {
        return Ok(await _authService.ObtenerEstadoAsync());
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
