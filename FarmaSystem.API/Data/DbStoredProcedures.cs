using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace FarmaSystem.API.Data;

public static class DbStoredProcedures
{
    public static async Task<int> RegistrarVentaAsync(
        FarmaSystemContext context,
        int idCliente,
        int idEmpleado,
        CancellationToken cancellationToken = default)
    {
        var idVentaParam = new SqlParameter("@idVenta", SqlDbType.Int)
        {
            Direction = ParameterDirection.Output
        };

        try
        {
            await context.Database.ExecuteSqlRawAsync(
                "EXEC sp_RegistrarVenta @idCliente, @idEmpleado, @idVenta OUTPUT",
                new[]
                {
                    new SqlParameter("@idCliente", idCliente),
                    new SqlParameter("@idEmpleado", idEmpleado),
                    idVentaParam
                },
                cancellationToken);

            if (TryGetIntParameterValue(idVentaParam, out var idVenta))
                return idVenta;
        }
        catch (SqlException)
        {
            // SP antiguo con solo 2 parámetros y RETURN
        }

        var returnParam = new SqlParameter("@returnValue", SqlDbType.Int)
        {
            Direction = ParameterDirection.ReturnValue
        };

        await context.Database.ExecuteSqlRawAsync(
            "EXEC sp_RegistrarVenta @idCliente, @idEmpleado",
            new[]
            {
                returnParam,
                new SqlParameter("@idCliente", idCliente),
                new SqlParameter("@idEmpleado", idEmpleado)
            },
            cancellationToken);

        if (TryGetIntParameterValue(returnParam, out var idFromReturn))
            return idFromReturn;

        throw new InvalidOperationException(
            "sp_RegistrarVenta no devolvió un identificador de venta válido.");
    }

    public static async Task AgregarDetalleVentaAsync(
        FarmaSystemContext context,
        int idVenta,
        int idMedicamento,
        int cantidad,
        CancellationToken cancellationToken = default)
    {
        await context.Database.ExecuteSqlRawAsync(
            "EXEC sp_AgregarDetalleVenta @idVenta, @idMedicamento, @cantidad",
            new[]
            {
                new SqlParameter("@idVenta", idVenta),
                new SqlParameter("@idMedicamento", idMedicamento),
                new SqlParameter("@cantidad", cantidad)
            },
            cancellationToken);
    }

    public static async Task<int> RegistrarCompraAsync(
        FarmaSystemContext context,
        int idProveedor,
        int idEmpleado,
        string? nFactura,
        CancellationToken cancellationToken = default)
    {
        var idCompraParam = new SqlParameter("@idCompra", SqlDbType.Int)
        {
            Direction = ParameterDirection.Output
        };

        try
        {
            await context.Database.ExecuteSqlRawAsync(
                "EXEC sp_RegistrarCompra @idProveedor, @idEmpleado, @nFactura, @idCompra OUTPUT",
                new[]
                {
                    new SqlParameter("@idProveedor", idProveedor),
                    new SqlParameter("@idEmpleado", idEmpleado),
                    new SqlParameter("@nFactura", (object?)nFactura ?? DBNull.Value),
                    idCompraParam
                },
                cancellationToken);

            if (TryGetIntParameterValue(idCompraParam, out var idCompra))
                return idCompra;
        }
        catch (SqlException)
        {
            // SP antiguo con solo 3 parámetros y RETURN
        }

        var returnParam = new SqlParameter("@returnValue", SqlDbType.Int)
        {
            Direction = ParameterDirection.ReturnValue
        };

        await context.Database.ExecuteSqlRawAsync(
            "EXEC sp_RegistrarCompra @idProveedor, @idEmpleado, @nFactura",
            new[]
            {
                returnParam,
                new SqlParameter("@idProveedor", idProveedor),
                new SqlParameter("@idEmpleado", idEmpleado),
                new SqlParameter("@nFactura", (object?)nFactura ?? DBNull.Value)
            },
            cancellationToken);

        if (TryGetIntParameterValue(returnParam, out var idFromReturn))
            return idFromReturn;

        throw new InvalidOperationException(
            "sp_RegistrarCompra no devolvió un identificador de compra válido.");
    }

    private static bool TryGetIntParameterValue(SqlParameter parameter, out int value)
    {
        value = 0;
        if (parameter.Value is int intValue)
        {
            value = intValue;
            return value > 0;
        }

        if (parameter.Value == null || parameter.Value == DBNull.Value)
            return false;

        value = Convert.ToInt32(parameter.Value);
        return value > 0;
    }
}
