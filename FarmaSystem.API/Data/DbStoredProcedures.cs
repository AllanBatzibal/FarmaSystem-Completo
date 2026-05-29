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
        try
        {
            var idVentaParam = new SqlParameter("@idVenta", SqlDbType.Int)
            {
                Direction = ParameterDirection.Output
            };

            await context.Database.ExecuteSqlRawAsync(
                "EXEC sp_RegistrarVenta @idCliente, @idEmpleado, @idVenta OUTPUT",
                new[]
                {
                    new SqlParameter("@idCliente", idCliente),
                    new SqlParameter("@idEmpleado", idEmpleado),
                    idVentaParam
                },
                cancellationToken);

            if (idVentaParam.Value is int id)
                return id;
            if (idVentaParam.Value != null && idVentaParam.Value != DBNull.Value)
                return Convert.ToInt32(idVentaParam.Value);
        }
        catch (SqlException)
        {
            // Intentar variante con valor de retorno
        }

        var returnParam = new SqlParameter("@return", SqlDbType.Int)
        {
            Direction = ParameterDirection.ReturnValue
        };

        await context.Database.ExecuteSqlRawAsync(
            "EXEC @return = sp_RegistrarVenta @idCliente, @idEmpleado",
            new[]
            {
                returnParam,
                new SqlParameter("@idCliente", idCliente),
                new SqlParameter("@idEmpleado", idEmpleado)
            },
            cancellationToken);

        return Convert.ToInt32(returnParam.Value);
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
        try
        {
            var idCompraParam = new SqlParameter("@idCompra", SqlDbType.Int)
            {
                Direction = ParameterDirection.Output
            };

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

            if (idCompraParam.Value is int id)
                return id;
            if (idCompraParam.Value != null && idCompraParam.Value != DBNull.Value)
                return Convert.ToInt32(idCompraParam.Value);
        }
        catch (SqlException)
        {
            // Intentar variante con valor de retorno
        }

        var returnParam = new SqlParameter("@return", SqlDbType.Int)
        {
            Direction = ParameterDirection.ReturnValue
        };

        await context.Database.ExecuteSqlRawAsync(
            "EXEC @return = sp_RegistrarCompra @idProveedor, @idEmpleado, @nFactura",
            new[]
            {
                returnParam,
                new SqlParameter("@idProveedor", idProveedor),
                new SqlParameter("@idEmpleado", idEmpleado),
                new SqlParameter("@nFactura", (object?)nFactura ?? DBNull.Value)
            },
            cancellationToken);

        return Convert.ToInt32(returnParam.Value);
    }
}
