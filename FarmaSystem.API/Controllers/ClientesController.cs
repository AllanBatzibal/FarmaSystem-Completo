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
public class ClientesController : ControllerBase
{
    private readonly FarmaSystemContext _context;
    private readonly IMapper _mapper;

    public ClientesController(FarmaSystemContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    [HttpGet("test-conexion")]
    public async Task<IActionResult> TestConexion()
    {
        try
        {
            var totalClientes = await _context.Clientes.CountAsync();
            var totalMedicamentos = await _context.Medicamentos.CountAsync();
            return Ok(new
            {
                mensaje = "Conexión exitosa a FarmaSystemDB",
                api = "http://localhost:5280",
                clientes = totalClientes,
                medicamentos = totalMedicamentos
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ClienteDTO>>> GetAll()
    {
        var clientes = await _context.Clientes
            .AsNoTracking()
            .ProjectTo<ClienteDTO>(_mapper.ConfigurationProvider)
            .ToListAsync();
        return Ok(clientes);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ClienteDTO>> GetById(int id)
    {
        var cliente = await _context.Clientes.AsNoTracking().FirstOrDefaultAsync(c => c.IdCliente == id);
        if (cliente == null)
            return NotFound(new { message = $"Cliente con ID {id} no encontrado." });
        return Ok(_mapper.Map<ClienteDTO>(cliente));
    }

    [HttpPost]
    public async Task<ActionResult<ClienteDTO>> Create([FromBody] ClienteCreateDTO dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Nombre) || string.IsNullOrWhiteSpace(dto.Apellido))
            return BadRequest(new { message = "Nombre y apellido son obligatorios." });

        var cliente = _mapper.Map<Cliente>(dto);
        _context.Clientes.Add(cliente);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = cliente.IdCliente }, _mapper.Map<ClienteDTO>(cliente));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ClienteDTO>> Update(int id, [FromBody] ClienteCreateDTO dto)
    {
        var cliente = await _context.Clientes.FindAsync(id);
        if (cliente == null)
            return NotFound(new { message = $"Cliente con ID {id} no encontrado." });

        if (string.IsNullOrWhiteSpace(dto.Nombre) || string.IsNullOrWhiteSpace(dto.Apellido))
            return BadRequest(new { message = "Nombre y apellido son obligatorios." });

        _mapper.Map(dto, cliente);
        await _context.SaveChangesAsync();
        return Ok(_mapper.Map<ClienteDTO>(cliente));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var cliente = await _context.Clientes.FindAsync(id);
        if (cliente == null)
            return NotFound(new { message = $"Cliente con ID {id} no encontrado." });

        var tieneVentas = await _context.Ventas.AnyAsync(v => v.IdCliente == id);
        if (tieneVentas)
            return BadRequest(new { message = "No se puede eliminar el cliente porque tiene ventas asociadas." });

        _context.Clientes.Remove(cliente);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
