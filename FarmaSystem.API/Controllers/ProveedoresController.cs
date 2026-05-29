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
public class ProveedoresController : ControllerBase
{
    private readonly FarmaSystemContext _context;
    private readonly IMapper _mapper;

    public ProveedoresController(FarmaSystemContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProveedorDTO>>> GetAll()
    {
        var items = await _context.Proveedores
            .AsNoTracking()
            .ProjectTo<ProveedorDTO>(_mapper.ConfigurationProvider)
            .ToListAsync();
        return Ok(items);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProveedorDTO>> GetById(int id)
    {
        var item = await _context.Proveedores.AsNoTracking().FirstOrDefaultAsync(p => p.IdProveedor == id);
        if (item == null)
            return NotFound(new { message = $"Proveedor con ID {id} no encontrado." });
        return Ok(_mapper.Map<ProveedorDTO>(item));
    }

    [HttpPost]
    public async Task<ActionResult<ProveedorDTO>> Create([FromBody] ProveedorCreateDTO dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Nombre))
            return BadRequest(new { message = "El nombre del proveedor es obligatorio." });

        var proveedor = _mapper.Map<Proveedor>(dto);
        _context.Proveedores.Add(proveedor);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = proveedor.IdProveedor }, _mapper.Map<ProveedorDTO>(proveedor));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ProveedorDTO>> Update(int id, [FromBody] ProveedorCreateDTO dto)
    {
        var proveedor = await _context.Proveedores.FindAsync(id);
        if (proveedor == null)
            return NotFound(new { message = $"Proveedor con ID {id} no encontrado." });

        _mapper.Map(dto, proveedor);
        await _context.SaveChangesAsync();
        return Ok(_mapper.Map<ProveedorDTO>(proveedor));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var proveedor = await _context.Proveedores.FindAsync(id);
        if (proveedor == null)
            return NotFound(new { message = $"Proveedor con ID {id} no encontrado." });

        var tieneCompras = await _context.Compras.AnyAsync(c => c.IdProveedor == id);
        if (tieneCompras)
            return BadRequest(new { message = "No se puede eliminar el proveedor porque tiene compras asociadas." });

        _context.Proveedores.Remove(proveedor);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
