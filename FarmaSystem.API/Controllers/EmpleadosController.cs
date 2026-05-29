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
public class EmpleadosController : ControllerBase
{
    private readonly FarmaSystemContext _context;
    private readonly IMapper _mapper;

    public EmpleadosController(FarmaSystemContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<EmpleadoDTO>>> GetAll()
    {
        var items = await _context.Empleados
            .AsNoTracking()
            .ProjectTo<EmpleadoDTO>(_mapper.ConfigurationProvider)
            .ToListAsync();
        return Ok(items);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<EmpleadoDTO>> GetById(int id)
    {
        var item = await _context.Empleados.AsNoTracking().FirstOrDefaultAsync(e => e.IdEmpleado == id);
        if (item == null)
            return NotFound(new { message = $"Empleado con ID {id} no encontrado." });
        return Ok(_mapper.Map<EmpleadoDTO>(item));
    }

    [HttpPost]
    public async Task<ActionResult<EmpleadoDTO>> Create([FromBody] EmpleadoCreateDTO dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Nombre) || string.IsNullOrWhiteSpace(dto.Apellido))
            return BadRequest(new { message = "Nombre y apellido son obligatorios." });

        var empleado = _mapper.Map<Empleado>(dto);
        _context.Empleados.Add(empleado);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = empleado.IdEmpleado }, _mapper.Map<EmpleadoDTO>(empleado));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<EmpleadoDTO>> Update(int id, [FromBody] EmpleadoCreateDTO dto)
    {
        var empleado = await _context.Empleados.FindAsync(id);
        if (empleado == null)
            return NotFound(new { message = $"Empleado con ID {id} no encontrado." });

        _mapper.Map(dto, empleado);
        await _context.SaveChangesAsync();
        return Ok(_mapper.Map<EmpleadoDTO>(empleado));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var empleado = await _context.Empleados.FindAsync(id);
        if (empleado == null)
            return NotFound(new { message = $"Empleado con ID {id} no encontrado." });

        var tieneRegistros = await _context.Ventas.AnyAsync(v => v.IdEmpleado == id)
            || await _context.Compras.AnyAsync(c => c.IdEmpleado == id);
        if (tieneRegistros)
            return BadRequest(new { message = "No se puede eliminar el empleado porque tiene ventas o compras asociadas." });

        _context.Empleados.Remove(empleado);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
