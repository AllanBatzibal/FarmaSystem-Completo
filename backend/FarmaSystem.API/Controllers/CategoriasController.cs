using FarmaSystem.Core.DTOs;
using FarmaSystem.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FarmaSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriasController : ControllerBase
{
    private readonly ICategoriaService _categoriaService;

    public CategoriasController(ICategoriaService categoriaService)
    {
        _categoriaService = categoriaService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CategoriaDTO>>> GetAll()
    {
        return Ok(await _categoriaService.ObtenerTodosAsync());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CategoriaDTO>> GetById(int id)
    {
        var item = await _categoriaService.ObtenerPorIdAsync(id);
        if (item == null)
            return NotFound(new { message = $"Categoría con ID {id} no encontrada." });
        return Ok(item);
    }

    [HttpPost]
    public async Task<ActionResult<CategoriaDTO>> Create([FromBody] CategoriaDTO dto)
    {
        try
        {
            var categoria = await _categoriaService.CrearAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = categoria.Id }, categoria);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
