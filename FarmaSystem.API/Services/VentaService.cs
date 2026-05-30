using AutoMapper;
using AutoMapper.QueryableExtensions;
using FarmaSystem.API.Data;
using FarmaSystem.API.DTOs;
using FarmaSystem.API.Models;
using FarmaSystem.API.Models.Views;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;

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

        var connectionString = _context.Database.GetConnectionString()
            ?? throw new InvalidOperationException("No se configuró la cadena de conexión a la base de datos.");

        decimal totalEstimado = 0;
        foreach (var det in dto.Detalles)
        {
            var precio = det.PrecioUnitario;
            if (!precio.HasValue || precio.Value <= 0)
            {
                precio = await _context.Medicamentos
                    .AsNoTracking()
                    .Where(m => m.IdMedicamento == det.IdMedicamento)
                    .Select(m => m.Precio)
                    .FirstOrDefaultAsync();
            }

            if (precio is null or <= 0)
                throw new InvalidOperationException($"No se pudo obtener el precio del medicamento {det.IdMedicamento}.");

            totalEstimado += precio.Value * det.Cantidad;
        }

        if (dto.Descuento > 0)
            totalEstimado = Math.Max(0, totalEstimado - dto.Descuento);

        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();
        await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync();

        int idVenta;

        try
        {
            idVenta = await EjecutarRegistrarVentaAsync(
                connection,
                transaction,
                dto.IdCliente,
                dto.IdEmpleado,
                dto.Descuento,
                totalEstimado);

            foreach (var detalle in dto.Detalles)
            {
                var precioUnitario = detalle.PrecioUnitario;
                if (!precioUnitario.HasValue || precioUnitario.Value <= 0)
                {
                    precioUnitario = await ObtenerPrecioMedicamentoAsync(
                        connection,
                        transaction,
                        detalle.IdMedicamento);
                }

                await EjecutarRegistrarDetalleVentaAsync(
                    connection,
                    transaction,
                    idVenta,
                    detalle.IdMedicamento,
                    detalle.Cantidad,
                    precioUnitario.Value);
            }

            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            try
            {
                await transaction.RollbackAsync();
            }
            catch
            {
                // La transacción puede haber quedado abortada por el motor SQL.
            }

            if (ex is InvalidOperationException)
                throw;

            throw new InvalidOperationException($"Error al registrar la venta: {ex.Message}", ex);
        }

        var ventaRegistrada = await ObtenerPorIdAsync(idVenta);
        if (ventaRegistrada == null)
            throw new InvalidOperationException("La venta se registró pero no pudo recuperarse.");

        return ventaRegistrada;
    }

    private static async Task<int> EjecutarRegistrarVentaAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        int idCliente,
        int idEmpleado,
        decimal descuento,
        decimal total)
    {
        await using var command = new SqlCommand("sp_RegistrarVenta", connection, transaction)
        {
            CommandType = CommandType.StoredProcedure
        };

        command.Parameters.Add(new SqlParameter("@idCliente", SqlDbType.Int) { Value = idCliente });
        command.Parameters.Add(new SqlParameter("@idEmpleado", SqlDbType.Int) { Value = idEmpleado });
        command.Parameters.Add(new SqlParameter("@descuento", SqlDbType.Decimal)
        {
            Precision = 18,
            Scale = 2,
            Value = descuento
        });
        command.Parameters.Add(new SqlParameter("@total", SqlDbType.Decimal)
        {
            Precision = 18,
            Scale = 2,
            Value = total
        });

        var result = await command.ExecuteScalarAsync();
        if (result == null || result == DBNull.Value)
            throw new InvalidOperationException("sp_RegistrarVenta no devolvió un identificador de venta.");

        var idVenta = Convert.ToInt32(result);
        if (idVenta <= 0)
            throw new InvalidOperationException("No se pudo obtener el identificador de la venta.");

        return idVenta;
    }

    private static async Task EjecutarRegistrarDetalleVentaAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        int idVenta,
        int idMedicamento,
        int cantidad,
        decimal precioUnitario)
    {
        await using var command = new SqlCommand("sp_RegistrarDetalleVenta", connection, transaction)
        {
            CommandType = CommandType.StoredProcedure
        };

        command.Parameters.Add(new SqlParameter("@idVenta", SqlDbType.Int) { Value = idVenta });
        command.Parameters.Add(new SqlParameter("@idMedicamento", SqlDbType.Int) { Value = idMedicamento });
        command.Parameters.Add(new SqlParameter("@cantidad", SqlDbType.Int) { Value = cantidad });
        command.Parameters.Add(new SqlParameter("@precioUnitario", SqlDbType.Decimal)
        {
            Precision = 18,
            Scale = 2,
            Value = precioUnitario
        });

        await command.ExecuteNonQueryAsync();
    }

    private static async Task<decimal> ObtenerPrecioMedicamentoAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        int idMedicamento)
    {
        await using var command = new SqlCommand(
            "SELECT precio FROM Medicamento WHERE idMedicamento = @idMedicamento",
            connection,
            transaction);

        command.Parameters.Add(new SqlParameter("@idMedicamento", SqlDbType.Int) { Value = idMedicamento });

        var result = await command.ExecuteScalarAsync();
        if (result == null || result == DBNull.Value)
            throw new InvalidOperationException($"El medicamento con ID {idMedicamento} no existe.");

        return Convert.ToDecimal(result);
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
