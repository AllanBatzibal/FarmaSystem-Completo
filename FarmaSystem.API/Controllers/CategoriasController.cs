using AutoMapper;
using AutoMapper.QueryableExtensions;
using FarmaSystem.API.Data;
using FarmaSystem.API.DTOs;
using FarmaSystem.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FarmaSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriasController : ControllerBase
{
    private readonly FarmaSystemContext _context;
    private readonly IMapper _mapper;

    public CategoriasController(FarmaSystemContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CategoriaDTO>>> GetAll()
    {
        var items = await _context.Categorias
            .AsNoTracking()
            .ProjectTo<CategoriaDTO>(_mapper.ConfigurationProvider)
            .ToListAsync();
        return Ok(items);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CategoriaDTO>> GetById(int id)
    {
        var item = await _context.Categorias.AsNoTracking().FirstOrDefaultAsync(c => c.IdCategoria == id);
        if (item == null)
            return NotFound(new { message = $"Categoría con ID {id} no encontrada." });
        return Ok(_mapper.Map<CategoriaDTO>(item));
    }

    [HttpPost]
    public async Task<ActionResult<CategoriaDTO>> Create([FromBody] CategoriaDTO dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Nombre))
            return BadRequest(new { message = "El nombre de la categoría es obligatorio." });

        var categoria = new Categoria { Nombre = dto.Nombre, Descripcion = dto.Descripcion };
        _context.Categorias.Add(categoria);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = categoria.IdCategoria }, _mapper.Map<CategoriaDTO>(categoria));
    }
}
