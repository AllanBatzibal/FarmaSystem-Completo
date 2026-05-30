using FarmaSystem.Core.DTOs;
using FarmaSystem.Core.Interfaces;
using FarmaSystem.Core.Views;
using FarmaSystem.Infrastructure.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace FarmaSystem.Infrastructure.Services;

public class InventarioService : IInventarioService
{
    private readonly FarmaSystemContext _context;

    public InventarioService(FarmaSystemContext context)
    {
        _context = context;
    }

    public string ClasificarStock(int stockActual, int stockMinimo)
    {
        if (stockActual < stockMinimo) return "Critico";
        if (stockActual == stockMinimo) return "Bajo";
        return "OK";
    }

    public async Task<List<InventarioItemDTO>> ObtenerEstadoStockAsync()
    {
        try
        {
            var desdeSp = await _context.Database
                .SqlQueryRaw<InventarioSpResult>("EXEC sp_ConsultarInventario")
                .ToListAsync();

            if (desdeSp.Count > 0)
                return desdeSp.Select(MapFromSp).ToList();
        }
        catch (SqlException)
        {
            // Usar vista si el SP no está disponible o falla
        }

        var desdeVista = await _context.VwInventarioActual.AsNoTracking().ToListAsync();
        return desdeVista.Select(MapFromVista).ToList();
    }

    public async Task<List<MovimientoInventarioDTO>> ObtenerMovimientosAsync()
    {
        var movimientos = await _context.VwMovimientosCompletos
            .AsNoTracking()
            .OrderByDescending(m => m.Fecha)
            .ToListAsync();

        return movimientos.Select(m => new MovimientoInventarioDTO
        {
            Id = m.IdMovimiento,
            IdMedicamento = m.IdMedicamento,
            MedicamentoNombre = m.Medicamento,
            Tipo = m.Tipo,
            Cantidad = m.Cantidad,
            Fecha = m.Fecha,
            Motivo = m.Motivo,
            Referencia = m.Motivo,
            Observacion = m.Motivo
        }).ToList();
    }

    public async Task<List<MovimientoInventarioDTO>> ObtenerMovimientosPorMedicamentoAsync(int idMedicamento)
    {
        var movimientos = await _context.VwMovimientosCompletos
            .AsNoTracking()
            .Where(m => m.IdMedicamento == idMedicamento)
            .OrderByDescending(m => m.Fecha)
            .ToListAsync();

        return movimientos.Select(m => new MovimientoInventarioDTO
        {
            Id = m.IdMovimiento,
            IdMedicamento = m.IdMedicamento,
            MedicamentoNombre = m.Medicamento,
            Tipo = m.Tipo,
            Cantidad = m.Cantidad,
            Fecha = m.Fecha,
            Motivo = m.Motivo,
            Referencia = m.Motivo,
            Observacion = m.Motivo
        }).ToList();
    }

    private InventarioItemDTO MapFromVista(VwInventarioActual v) => new()
    {
        Id = v.IdMedicamento,
        Nombre = v.Nombre,
        Categoria = v.Categoria,
        StockActual = v.StockActual,
        StockMinimo = v.StockMinimo,
        EstadoStock = v.EstadoStock ?? ClasificarStock(v.StockActual, v.StockMinimo),
        Precio = v.Precio
    };

    private InventarioItemDTO MapFromSp(InventarioSpResult r) => new()
    {
        Id = r.IdMedicamento,
        Nombre = r.Nombre ?? string.Empty,
        Categoria = r.Categoria,
        StockActual = r.StockActual,
        StockMinimo = r.StockMinimo,
        EstadoStock = r.EstadoStock ?? ClasificarStock(r.StockActual, r.StockMinimo),
        Precio = r.Precio
    };

    private class InventarioSpResult
    {
        public int IdMedicamento { get; set; }
        public string? Nombre { get; set; }
        public string? Categoria { get; set; }
        public int StockActual { get; set; }
        public int StockMinimo { get; set; }
        public string? EstadoStock { get; set; }
        public decimal Precio { get; set; }
    }
}
