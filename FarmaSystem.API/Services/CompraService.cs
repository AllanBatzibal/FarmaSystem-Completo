using AutoMapper;
using FarmaSystem.API.Data;
using FarmaSystem.API.DTOs;
using FarmaSystem.API.Models;
using FarmaSystem.API.Models.Views;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace FarmaSystem.API.Services;

public class CompraService : ICompraService
{
    private readonly FarmaSystemContext _context;
    private readonly IMapper _mapper;

    public CompraService(FarmaSystemContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<CompraDTO>> ObtenerTodasAsync()
    {
        var filas = await _context.VwResumenCompras
            .AsNoTracking()
            .OrderByDescending(c => c.Fecha)
            .ThenByDescending(c => c.IdCompra)
            .ToListAsync();

        return AgruparComprasDesdeVista(filas);
    }

    public async Task<CompraDTO?> ObtenerPorIdAsync(int id)
    {
        var filas = await _context.VwResumenCompras
            .AsNoTracking()
            .Where(c => c.IdCompra == id)
            .ToListAsync();

        return AgruparComprasDesdeVista(filas).FirstOrDefault();
    }

    public async Task<CompraResumenMesDTO> ObtenerResumenMesAsync()
    {
        var inicioMes = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        var compras = await _context.Compras
            .Where(c => c.Fecha >= inicioMes)
            .AsNoTracking()
            .ToListAsync();

        return new CompraResumenMesDTO
        {
            TotalCompras = compras.Sum(c => c.Total),
            Cantidad = compras.Count
        };
    }

    public async Task<CompraDTO> RegistrarCompraAsync(CompraCreateDTO dto)
    {
        if (dto.Detalles == null || dto.Detalles.Count == 0)
            throw new InvalidOperationException("La compra debe incluir al menos un detalle.");

        if (dto.IdProveedor <= 0)
            throw new InvalidOperationException("Debe seleccionar un proveedor válido.");

        if (!await _context.Proveedores.AnyAsync(p => p.IdProveedor == dto.IdProveedor))
            throw new InvalidOperationException("El proveedor seleccionado no existe.");

        if (dto.IdEmpleado <= 0)
        {
            var primerEmpleado = await _context.Empleados.Select(e => e.IdEmpleado).FirstOrDefaultAsync();
            if (primerEmpleado == 0)
                throw new InvalidOperationException("No hay empleados registrados para asignar la compra.");
            dto.IdEmpleado = primerEmpleado;
        }

        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var idCompra = await DbStoredProcedures.RegistrarCompraAsync(
                _context, dto.IdProveedor, dto.IdEmpleado, dto.NumeroFactura);

            foreach (var detalleDto in dto.Detalles)
            {
                if (detalleDto.IdMedicamento <= 0 || detalleDto.Cantidad <= 0)
                    throw new InvalidOperationException("Cada detalle debe tener medicamento y cantidad válidos.");

                var medicamento = await _context.Medicamentos
                    .FirstOrDefaultAsync(m => m.IdMedicamento == detalleDto.IdMedicamento);

                if (medicamento == null)
                    throw new InvalidOperationException($"El medicamento con ID {detalleDto.IdMedicamento} no existe.");

                var costo = detalleDto.Costo > 0 ? detalleDto.Costo : medicamento.Precio;

                _context.DetallesCompra.Add(new DetalleCompra
                {
                    IdCompra = idCompra,
                    IdMedicamento = detalleDto.IdMedicamento,
                    Cantidad = detalleDto.Cantidad,
                    CostoUnitario = costo
                });
            }

            await _context.SaveChangesAsync();

            var compra = await _context.Compras.FindAsync(idCompra);
            if (compra != null)
            {
                var totalCalculado = await _context.DetallesCompra
                    .Where(d => d.IdCompra == idCompra)
                    .SumAsync(d => d.Subtotal);

                compra.Total = dto.Total > 0 ? dto.Total.Value : totalCalculado;
                await _context.SaveChangesAsync();
            }

            await transaction.CommitAsync();
            return (await ObtenerPorIdAsync(idCompra))!;
        }
        catch (SqlException ex)
        {
            await transaction.RollbackAsync();
            throw new InvalidOperationException($"Error al registrar la compra: {ex.Message}", ex);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    private static List<CompraDTO> AgruparComprasDesdeVista(List<VwResumenCompras> filas)
    {
        return filas
            .GroupBy(c => c.IdCompra)
            .Select(g =>
            {
                var cab = g.First();
                return new CompraDTO
                {
                    Id = cab.IdCompra,
                    IdProveedor = cab.IdProveedor,
                    ProveedorNombre = cab.ProveedorNombre,
                    IdEmpleado = cab.IdEmpleado,
                    EmpleadoNombre = cab.EmpleadoNombre,
                    NumeroFactura = cab.NFactura,
                    Fecha = cab.Fecha,
                    Total = cab.Total,
                    Detalles = g
                        .Where(x => x.IdDetalle.HasValue)
                        .Select(x => new DetalleCompraDTO
                        {
                            IdMedicamento = x.IdMedicamento ?? 0,
                            Nombre = x.Medicamento,
                            Cantidad = x.Cantidad ?? 0,
                            CostoUnitario = x.CostoUnitario ?? 0,
                            Subtotal = x.Subtotal ?? 0
                        })
                        .ToList()
                };
            })
            .OrderByDescending(c => c.Fecha)
            .ToList();
    }
}
