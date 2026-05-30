using FarmaSystem.Core.DTOs;
using FarmaSystem.Core.Interfaces;
using FarmaSystem.Core.Models;
using FarmaSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FarmaSystem.Infrastructure.Services;

public class MedicamentoService : IMedicamentoService
{
    private readonly FarmaSystemContext _context;
    private readonly IInventarioService _inventarioService;

    public MedicamentoService(FarmaSystemContext context, IInventarioService inventarioService)
    {
        _context = context;
        _inventarioService = inventarioService;
    }

    public async Task<List<MedicamentoDTO>> ObtenerTodosAsync()
    {
        var items = await _context.Medicamentos
            .Include(m => m.Categoria)
            .Include(m => m.Proveedor)
            .AsNoTracking()
            .OrderBy(m => m.Nombre)
            .ToListAsync();

        return items.Select(MapMedicamento).ToList();
    }

    public async Task<MedicamentoDTO?> ObtenerPorIdAsync(int id)
    {
        var item = await _context.Medicamentos
            .Include(m => m.Categoria)
            .Include(m => m.Proveedor)
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.IdMedicamento == id);

        return item == null ? null : MapMedicamento(item);
    }

    public async Task<List<MedicamentoDTO>> ObtenerStockCriticoAsync()
    {
        var items = await _context.Medicamentos
            .Include(m => m.Categoria)
            .Include(m => m.Proveedor)
            .AsNoTracking()
            .Where(m => m.StockActual < m.StockMinimo)
            .OrderBy(m => m.StockActual)
            .ToListAsync();

        return items.Select(MapMedicamento).ToList();
    }

    public async Task<List<MedicamentoDTO>> ObtenerPorCategoriaAsync(int idCategoria)
    {
        var items = await _context.Medicamentos
            .Include(m => m.Categoria)
            .Include(m => m.Proveedor)
            .AsNoTracking()
            .Where(m => m.IdCategoria == idCategoria)
            .ToListAsync();

        return items.Select(MapMedicamento).ToList();
    }

    public async Task<MedicamentoDTO> CrearAsync(MedicamentoCreateDTO dto)
    {
        ValidarMedicamento(dto, esCreacion: true);

        var idCategoria = await ResolverIdCategoriaAsync(dto);
        if (idCategoria == null)
            throw new InvalidOperationException("Debe indicar una categoría válida (IdCategoria o nombre de categoría).");

        var idProveedor = await ResolverIdProveedorAsync(dto);
        if (idProveedor == null)
            throw new InvalidOperationException("Debe indicar un proveedor válido (IdProveedor).");

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
        return MapMedicamento(medicamento);
    }

    public async Task<MedicamentoDTO?> ActualizarAsync(int id, MedicamentoCreateDTO dto)
    {
        var medicamento = await _context.Medicamentos.FindAsync(id);
        if (medicamento == null)
            return null;

        ValidarMedicamento(dto, esCreacion: false);

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
        return MapMedicamento(medicamento);
    }

    public async Task EliminarAsync(int id)
    {
        var medicamento = await _context.Medicamentos.FindAsync(id);
        if (medicamento == null)
            throw new InvalidOperationException($"Medicamento con ID {id} no encontrado.");

        var enUso = await _context.DetallesVenta.AnyAsync(d => d.IdMedicamento == id)
            || await _context.DetallesCompra.AnyAsync(d => d.IdMedicamento == id);
        if (enUso)
            throw new InvalidOperationException("No se puede eliminar el medicamento porque está en ventas o compras.");

        _context.Medicamentos.Remove(medicamento);
        await _context.SaveChangesAsync();
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

    private static void ValidarMedicamento(MedicamentoCreateDTO dto, bool esCreacion)
    {
        if (string.IsNullOrWhiteSpace(dto.Nombre))
            throw new InvalidOperationException("El nombre del medicamento es obligatorio.");

        if (esCreacion)
        {
            if (dto.Precio <= 0)
                throw new InvalidOperationException("El precio debe ser mayor a cero.");

            if (dto.StockActual < 0 || dto.StockMinimo < 0)
                throw new InvalidOperationException("Los valores de stock no pueden ser negativos.");
        }
    }

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
