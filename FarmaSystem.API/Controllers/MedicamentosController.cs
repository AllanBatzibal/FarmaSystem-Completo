using AutoMapper;
using AutoMapper.QueryableExtensions;
using FarmaSystem.API.Data;
using FarmaSystem.API.DTOs;
using FarmaSystem.API.Models;
using FarmaSystem.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FarmaSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MedicamentosController : ControllerBase
{
    private readonly FarmaSystemContext _context;
    private readonly IMapper _mapper;
    private readonly IInventarioService _inventarioService;

    public MedicamentosController(
        FarmaSystemContext context,
        IMapper mapper,
        IInventarioService inventarioService)
    {
        _context = context;
        _mapper = mapper;
        _inventarioService = inventarioService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<MedicamentoDTO>>> GetAll()
    {
        var items = await _context.Medicamentos
            .Include(m => m.Categoria)
            .Include(m => m.Proveedor)
            .AsNoTracking()
            .OrderBy(m => m.Nombre)
            .ToListAsync();

        return Ok(items.Select(MapMedicamento));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<MedicamentoDTO>> GetById(int id)
    {
        var item = await _context.Medicamentos
            .Include(m => m.Categoria)
            .Include(m => m.Proveedor)
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.IdMedicamento == id);

        if (item == null)
            return NotFound(new { message = $"Medicamento con ID {id} no encontrado." });

        return Ok(MapMedicamento(item));
    }

    [HttpGet("stock-critico")]
    public async Task<ActionResult<IEnumerable<MedicamentoDTO>>> GetStockCritico()
    {
        var items = await _context.Medicamentos
            .Include(m => m.Categoria)
            .Include(m => m.Proveedor)
            .AsNoTracking()
            .Where(m => m.StockActual < m.StockMinimo)
            .OrderBy(m => m.StockActual)
            .ToListAsync();

        return Ok(items.Select(MapMedicamento));
    }

    [HttpGet("categoria/{id}")]
    public async Task<ActionResult<IEnumerable<MedicamentoDTO>>> GetByCategoria(int id)
    {
        var items = await _context.Medicamentos
            .Include(m => m.Categoria)
            .Include(m => m.Proveedor)
            .AsNoTracking()
            .Where(m => m.IdCategoria == id)
            .ToListAsync();

        return Ok(items.Select(MapMedicamento));
    }

    [HttpPost]
    public async Task<ActionResult<MedicamentoDTO>> Create([FromBody] MedicamentoCreateDTO dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Nombre))
            return BadRequest(new { message = "El nombre del medicamento es obligatorio." });

        if (dto.Precio <= 0)
            return BadRequest(new { message = "El precio debe ser mayor a cero." });

        if (dto.StockActual < 0 || dto.StockMinimo < 0)
            return BadRequest(new { message = "Los valores de stock no pueden ser negativos." });

        var idCategoria = await ResolverIdCategoriaAsync(dto);
        if (idCategoria == null)
            return BadRequest(new { message = "Debe indicar una categoría válida (IdCategoria o nombre de categoría)." });

        var idProveedor = await ResolverIdProveedorAsync(dto);
        if (idProveedor == null)
            return BadRequest(new { message = "Debe indicar un proveedor válido (IdProveedor)." });

        var medicamento = new Medicamento
        {
            IdCategoria = idCategoria.Value,
            IdProveedor = idProveedor.Value,
            Nombre = dto.Nombre.Trim(),
            Descripcion = dto.Descripcion,
            Precio = dto.Precio,
            StockActual = dto.StockActual,
            StockMinimo = dto.StockMinimo
        };

        _context.Medicamentos.Add(medicamento);
        await _context.SaveChangesAsync();

        await _context.Entry(medicamento).Reference(m => m.Categoria).LoadAsync();
        await _context.Entry(medicamento).Reference(m => m.Proveedor).LoadAsync();
        return CreatedAtAction(nameof(GetById), new { id = medicamento.IdMedicamento }, MapMedicamento(medicamento));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<MedicamentoDTO>> Update(int id, [FromBody] MedicamentoCreateDTO dto)
    {
        var medicamento = await _context.Medicamentos.FindAsync(id);
        if (medicamento == null)
            return NotFound(new { message = $"Medicamento con ID {id} no encontrado." });

        if (string.IsNullOrWhiteSpace(dto.Nombre))
            return BadRequest(new { message = "El nombre del medicamento es obligatorio." });

        var idCategoria = await ResolverIdCategoriaAsync(dto);
        if (idCategoria.HasValue)
            medicamento.IdCategoria = idCategoria.Value;

        var idProveedor = await ResolverIdProveedorAsync(dto);
        if (idProveedor.HasValue)
            medicamento.IdProveedor = idProveedor.Value;

        medicamento.Nombre = dto.Nombre.Trim();
        medicamento.Descripcion = dto.Descripcion;
        medicamento.Precio = dto.Precio;
        medicamento.StockActual = dto.StockActual;
        medicamento.StockMinimo = dto.StockMinimo;

        await _context.SaveChangesAsync();
        await _context.Entry(medicamento).Reference(m => m.Categoria).LoadAsync();
        await _context.Entry(medicamento).Reference(m => m.Proveedor).LoadAsync();
        return Ok(MapMedicamento(medicamento));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var medicamento = await _context.Medicamentos.FindAsync(id);
        if (medicamento == null)
            return NotFound(new { message = $"Medicamento con ID {id} no encontrado." });

        var enUso = await _context.DetallesVenta.AnyAsync(d => d.IdMedicamento == id)
            || await _context.DetallesCompra.AnyAsync(d => d.IdMedicamento == id);
        if (enUso)
            return BadRequest(new { message = "No se puede eliminar el medicamento porque está en ventas o compras." });

        _context.Medicamentos.Remove(medicamento);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    private MedicamentoDTO MapMedicamento(Medicamento m) => new()
    {
        Id = m.IdMedicamento,
        IdCategoria = m.IdCategoria,
        IdProveedor = m.IdProveedor,
        Categoria = m.Categoria?.Nombre,
        Proveedor = m.Proveedor?.Nombre,
        Nombre = m.Nombre,
        Descripcion = m.Descripcion,
        Precio = m.Precio,
        StockActual = m.StockActual,
        StockMinimo = m.StockMinimo,
        EstadoStock = _inventarioService.ClasificarStock(m.StockActual, m.StockMinimo)
    };

    private async Task<int?> ResolverIdCategoriaAsync(MedicamentoCreateDTO dto)
    {
        if (dto.IdCategoria > 0)
            return dto.IdCategoria;

        if (!string.IsNullOrWhiteSpace(dto.CategoriaNombre))
        {
            var cat = await _context.Categorias
                .FirstOrDefaultAsync(c => c.Nombre == dto.CategoriaNombre.Trim());
            if (cat != null)
                return cat.IdCategoria;

            var nueva = new Categoria { Nombre = dto.CategoriaNombre.Trim() };
            _context.Categorias.Add(nueva);
            await _context.SaveChangesAsync();
            return nueva.IdCategoria;
        }

        return await _context.Categorias.Select(c => c.IdCategoria).FirstOrDefaultAsync() is int id && id > 0
            ? id
            : null;
    }

    private async Task<int?> ResolverIdProveedorAsync(MedicamentoCreateDTO dto)
    {
        if (dto.IdProveedor > 0)
        {
            var existe = await _context.Proveedores.AnyAsync(p => p.IdProveedor == dto.IdProveedor);
            return existe ? dto.IdProveedor : null;
        }

        var primer = await _context.Proveedores.Select(p => p.IdProveedor).FirstOrDefaultAsync();
        return primer > 0 ? primer : null;
    }
}
