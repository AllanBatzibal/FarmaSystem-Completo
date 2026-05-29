using AutoMapper;
using AutoMapper.QueryableExtensions;
using FarmaSystem.API.Data;
using FarmaSystem.API.DTOs;
using FarmaSystem.API.Models;
using FarmaSystem.API.Models.Views;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace FarmaSystem.API.Services;

public class VentaService : IVentaService
{
    private readonly FarmaSystemContext _context;
    private readonly IMapper _mapper;

    public VentaService(FarmaSystemContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<VentaDTO>> ObtenerTodasAsync(int? limit = null, DateTime? fecha = null)
    {
        var query = _context.VwResumenVentas.AsNoTracking();

        if (fecha.HasValue)
            query = query.Where(v => v.Fecha.Date == fecha.Value.Date);

        var filas = await query.OrderByDescending(v => v.Fecha).ThenByDescending(v => v.IdVenta).ToListAsync();
        var agrupadas = AgruparVentasDesdeVista(filas);

        if (limit.HasValue)
            agrupadas = agrupadas.Take(limit.Value).ToList();

        return agrupadas;
    }

    public async Task<VentaDTO?> ObtenerPorIdAsync(int id)
    {
        var filas = await _context.VwResumenVentas
            .AsNoTracking()
            .Where(v => v.IdVenta == id)
            .ToListAsync();

        return AgruparVentasDesdeVista(filas).FirstOrDefault();
    }

    public async Task<List<VentaDTO>> ObtenerPorFechaAsync(DateTime inicio, DateTime fin)
    {
        try
        {
            var reporte = await _context.Database
                .SqlQueryRaw<VentaReporteSpResult>(
                    "EXEC sp_ReporteVentasPorPeriodo @fechaInicio, @fechaFin",
                    new SqlParameter("@fechaInicio", inicio.Date),
                    new SqlParameter("@fechaFin", fin.Date))
                .ToListAsync();

            return reporte
                .GroupBy(r => r.IdVenta)
                .Select(g =>
                {
                    var first = g.First();
                    return new VentaDTO
                    {
                        Id = first.IdVenta,
                        IdCliente = first.IdCliente,
                        ClienteNombre = first.ClienteNombre,
                        IdEmpleado = first.IdEmpleado,
                        EmpleadoNombre = first.EmpleadoNombre,
                        Fecha = first.Fecha,
                        Descuento = first.Descuento,
                        Total = first.Total
                    };
                })
                .OrderByDescending(v => v.Fecha)
                .ToList();
        }
        catch (SqlException)
        {
            var filas = await _context.VwResumenVentas
                .AsNoTracking()
                .Where(v => v.Fecha.Date >= inicio.Date && v.Fecha.Date <= fin.Date)
                .ToListAsync();

            return AgruparVentasDesdeVista(filas);
        }
    }

    public async Task<List<VentaDTO>> ObtenerVentasHoyAsync()
    {
        return await ObtenerTodasAsync(fecha: DateTime.Today);
    }

    public async Task<VentaResumenDTO> ObtenerResumenDiaAsync()
    {
        var hoy = DateTime.Today;
        var ventas = await _context.Ventas
            .Where(v => v.Fecha.Date == hoy)
            .AsNoTracking()
            .ToListAsync();

        return new VentaResumenDTO
        {
            TotalVentas = ventas.Sum(v => v.Total),
            Cantidad = ventas.Count
        };
    }

    public async Task<VentaDTO> RegistrarVentaAsync(VentaCreateDTO dto)
    {
        if (dto.Detalles == null || dto.Detalles.Count == 0)
            throw new InvalidOperationException("La venta debe incluir al menos un detalle.");

        if (dto.IdCliente <= 0)
        {
            if (string.IsNullOrWhiteSpace(dto.NombreClienteNuevo))
                throw new InvalidOperationException("Seleccione un cliente o ingrese un nombre nuevo.");

            var nombreCompleto = dto.NombreClienteNuevo.Trim();
            var partes = nombreCompleto.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
            var clienteNuevo = new Cliente
            {
                Nombre = partes[0],
                Apellido = partes.Length > 1 ? partes[1] : "Cliente",
                FechaRegistro = DateTime.Now
            };
            _context.Clientes.Add(clienteNuevo);
            await _context.SaveChangesAsync();
            dto.IdCliente = clienteNuevo.IdCliente;
        }

        if (dto.IdEmpleado <= 0)
            throw new InvalidOperationException("Debe seleccionar un empleado válido.");

        if (!await _context.Clientes.AnyAsync(c => c.IdCliente == dto.IdCliente))
            throw new InvalidOperationException("El cliente seleccionado no existe.");

        if (!await _context.Empleados.AnyAsync(e => e.IdEmpleado == dto.IdEmpleado))
            throw new InvalidOperationException("El empleado seleccionado no existe.");

        foreach (var det in dto.Detalles)
        {
            if (det.IdMedicamento <= 0 || det.Cantidad <= 0)
                throw new InvalidOperationException("Cada detalle debe tener medicamento y cantidad válidos.");
        }

        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var idVenta = await DbStoredProcedures.RegistrarVentaAsync(
                _context, dto.IdCliente, dto.IdEmpleado);

            foreach (var detalle in dto.Detalles)
            {
                await DbStoredProcedures.AgregarDetalleVentaAsync(
                    _context, idVenta, detalle.IdMedicamento, detalle.Cantidad);
            }

            if (dto.Descuento > 0)
            {
                var venta = await _context.Ventas.FindAsync(idVenta);
                if (venta != null)
                {
                    venta.Descuento = dto.Descuento;
                    var subtotal = await _context.DetallesVenta
                        .Where(d => d.IdVenta == idVenta)
                        .SumAsync(d => d.Subtotal);
                    venta.Total = Math.Max(0, subtotal - venta.Descuento);
                    await _context.SaveChangesAsync();
                }
            }

            await transaction.CommitAsync();
            return (await ObtenerPorIdAsync(idVenta))!;
        }
        catch (SqlException ex)
        {
            await transaction.RollbackAsync();
            throw new InvalidOperationException($"Error al registrar la venta: {ex.Message}", ex);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    private static List<VentaDTO> AgruparVentasDesdeVista(List<VwResumenVentas> filas)
    {
        return filas
            .GroupBy(v => v.IdVenta)
            .Select(g =>
            {
                var cab = g.First();
                return new VentaDTO
                {
                    Id = cab.IdVenta,
                    IdCliente = cab.IdCliente,
                    ClienteNombre = cab.ClienteNombre,
                    IdEmpleado = cab.IdEmpleado,
                    EmpleadoNombre = cab.EmpleadoNombre,
                    Fecha = cab.Fecha,
                    Descuento = cab.Descuento,
                    Total = cab.Total,
                    Detalles = g
                        .Where(x => x.IdDetalle.HasValue)
                        .Select(x => new DetalleVentaDTO
                        {
                            IdMedicamento = x.IdMedicamento ?? 0,
                            Nombre = x.Medicamento,
                            Cantidad = x.Cantidad ?? 0,
                            PrecioUnitario = x.PrecioUnitario ?? 0,
                            Subtotal = x.Subtotal ?? 0
                        })
                        .ToList()
                };
            })
            .OrderByDescending(v => v.Fecha)
            .ToList();
    }

    private class VentaReporteSpResult
    {
        public int IdVenta { get; set; }
        public DateTime Fecha { get; set; }
        public decimal Total { get; set; }
        public decimal Descuento { get; set; }
        public int IdCliente { get; set; }
        public string? ClienteNombre { get; set; }
        public int IdEmpleado { get; set; }
        public string? EmpleadoNombre { get; set; }
    }
}
