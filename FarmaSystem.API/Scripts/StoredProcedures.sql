-- =============================================================================
-- FarmaSystemDB — Stored Procedures (script idempotente)
-- Ejecutar en SSMS o con:
--   sqlcmd -S localhost -d FarmaSystemDB -E -i StoredProcedures.sql
-- =============================================================================

USE [FarmaSystemDB];
GO

SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO

-- =============================================================================
-- sp_RegistrarVenta
-- Usado por: VentaService.RegistrarVentaAsync (SqlConnection + transacción C#)
-- Sin BEGIN/COMMIT TRANSACTION interno.
-- =============================================================================
IF OBJECT_ID(N'[dbo].[sp_RegistrarVenta]', N'P') IS NOT NULL
    DROP PROCEDURE [dbo].[sp_RegistrarVenta];
GO

CREATE PROCEDURE [dbo].[sp_RegistrarVenta]
    @idCliente  INT,
    @idEmpleado INT,
    @descuento  DECIMAL(18, 2) = 0,
    @total      DECIMAL(18, 2)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @idVenta INT;

    INSERT INTO [dbo].[Venta] (idCliente, idEmpleado, descuento, total, fecha)
    VALUES (@idCliente, @idEmpleado, @descuento, @total, GETDATE());

    SET @idVenta = SCOPE_IDENTITY();
    SELECT @idVenta AS idVenta;
END;
GO

-- =============================================================================
-- sp_RegistrarDetalleVenta
-- Usado por: VentaService.RegistrarVentaAsync
-- Sin transacción interna; el stock se valida en la misma transacción C#.
-- =============================================================================
IF OBJECT_ID(N'[dbo].[sp_RegistrarDetalleVenta]', N'P') IS NOT NULL
    DROP PROCEDURE [dbo].[sp_RegistrarDetalleVenta];
GO

CREATE PROCEDURE [dbo].[sp_RegistrarDetalleVenta]
    @idVenta         INT,
    @idMedicamento   INT,
    @cantidad        INT,
    @precioUnitario  DECIMAL(18, 2)
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO [dbo].[DetalleVenta] (idVenta, idMedicamento, cantidad, precioUnitario)
    VALUES (@idVenta, @idMedicamento, @cantidad, @precioUnitario);

    UPDATE [dbo].[Medicamento]
    SET stockActual = stockActual - @cantidad
    WHERE idMedicamento = @idMedicamento;
END;
GO

-- =============================================================================
-- sp_AgregarDetalleVenta
-- Usado por: DbStoredProcedures.AgregarDetalleVentaAsync
-- Parámetros: @idVenta, @idMedicamento, @cantidad (precio desde Medicamento)
-- =============================================================================
IF OBJECT_ID(N'[dbo].[sp_AgregarDetalleVenta]', N'P') IS NOT NULL
    DROP PROCEDURE [dbo].[sp_AgregarDetalleVenta];
GO

CREATE PROCEDURE [dbo].[sp_AgregarDetalleVenta]
    @idVenta        INT,
    @idMedicamento  INT,
    @cantidad       INT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @precio DECIMAL(10, 2);

    SELECT @precio = precio
    FROM [dbo].[Medicamento]
    WHERE idMedicamento = @idMedicamento;

    IF @precio IS NULL
    BEGIN
        RAISERROR(N'Medicamento no encontrado.', 16, 1);
        RETURN;
    END;

    EXEC [dbo].[sp_RegistrarDetalleVenta]
        @idVenta        = @idVenta,
        @idMedicamento  = @idMedicamento,
        @cantidad       = @cantidad,
        @precioUnitario = @precio;
END;
GO

-- =============================================================================
-- sp_RegistrarCompra
-- Usado por: DbStoredProcedures.RegistrarCompraAsync
-- =============================================================================
IF OBJECT_ID(N'[dbo].[sp_RegistrarCompra]', N'P') IS NOT NULL
    DROP PROCEDURE [dbo].[sp_RegistrarCompra];
GO

CREATE PROCEDURE [dbo].[sp_RegistrarCompra]
    @idProveedor INT,
    @idEmpleado  INT,
    @nFactura    VARCHAR(30) = NULL,
    @idCompra    INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO [dbo].[Compra] (fecha, total, nFactura, idProveedor, idEmpleado)
    VALUES (GETDATE(), 0, @nFactura, @idProveedor, @idEmpleado);

    SET @idCompra = SCOPE_IDENTITY();
END;
GO

-- =============================================================================
-- sp_RegistrarDetalleCompra
-- Detalle de compra con actualización de stock
-- =============================================================================
IF OBJECT_ID(N'[dbo].[sp_RegistrarDetalleCompra]', N'P') IS NOT NULL
    DROP PROCEDURE [dbo].[sp_RegistrarDetalleCompra];
GO

CREATE PROCEDURE [dbo].[sp_RegistrarDetalleCompra]
    @idCompra        INT,
    @idMedicamento   INT,
    @cantidad        INT,
    @costoUnitario   DECIMAL(10, 2)
AS
BEGIN
    SET NOCOUNT ON;

    IF @cantidad <= 0
    BEGIN
        RAISERROR(N'La cantidad debe ser mayor a cero.', 16, 1);
        RETURN;
    END;

    IF NOT EXISTS (SELECT 1 FROM [dbo].[Medicamento] WHERE idMedicamento = @idMedicamento)
    BEGIN
        RAISERROR(N'Medicamento no encontrado.', 16, 1);
        RETURN;
    END;

    INSERT INTO [dbo].[DetalleCompra] (idCompra, idMedicamento, cantidad, costoUnitario)
    VALUES (@idCompra, @idMedicamento, @cantidad, @costoUnitario);

    UPDATE [dbo].[Medicamento]
    SET stockActual = stockActual + @cantidad
    WHERE idMedicamento = @idMedicamento;

    UPDATE [dbo].[Compra]
    SET total = (
        SELECT ISNULL(SUM(subtotal), 0)
        FROM [dbo].[DetalleCompra]
        WHERE idCompra = @idCompra
    )
    WHERE idCompra = @idCompra;

    INSERT INTO [dbo].[MovimientoInventario] (idMedicamento, tipo, cantidad, fecha, motivo)
    VALUES (@idMedicamento, N'Entrada', @cantidad, GETDATE(),
            CONCAT(N'Compra #', @idCompra));
END;
GO

-- =============================================================================
-- sp_ConsultarInventario
-- Usado por: InventarioService.ObtenerEstadoStockAsync
-- =============================================================================
IF OBJECT_ID(N'[dbo].[sp_ConsultarInventario]', N'P') IS NOT NULL
    DROP PROCEDURE [dbo].[sp_ConsultarInventario];
GO

CREATE PROCEDURE [dbo].[sp_ConsultarInventario]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        m.idMedicamento,
        m.nombre,
        m.descripcion,
        m.precio,
        m.stockActual,
        m.stockMinimo,
        c.nombre AS categoria,
        p.nombre AS proveedor,
        CASE
            WHEN m.stockActual <= 0 THEN N'Sin stock'
            WHEN m.stockActual <= m.stockMinimo THEN N'Stock bajo'
            ELSE N'Normal'
        END AS estado
    FROM [dbo].[Medicamento] m
    INNER JOIN [dbo].[Categoria] c ON m.idCategoria = c.idCategoria
    INNER JOIN [dbo].[Proveedor] p ON m.idProveedor = p.idProveedor
    ORDER BY m.nombre;
END;
GO

-- =============================================================================
-- sp_ReporteVentasPorPeriodo
-- Usado por: VentaService.ObtenerPorFechaAsync
-- =============================================================================
IF OBJECT_ID(N'[dbo].[sp_ReporteVentasPorPeriodo]', N'P') IS NOT NULL
    DROP PROCEDURE [dbo].[sp_ReporteVentasPorPeriodo];
GO

CREATE PROCEDURE [dbo].[sp_ReporteVentasPorPeriodo]
    @fechaInicio DATE,
    @fechaFin    DATE
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        v.idVenta,
        v.fecha,
        v.total,
        v.descuento,
        v.idCliente,
        CONCAT(c.nombre, N' ', c.apellido) AS clienteNombre,
        v.idEmpleado,
        CONCAT(e.nombre, N' ', e.apellido) AS empleadoNombre
    FROM [dbo].[Venta] v
    INNER JOIN [dbo].[Cliente]  c ON v.idCliente  = c.idCliente
    INNER JOIN [dbo].[Empleado] e ON v.idEmpleado = e.idEmpleado
    WHERE CAST(v.fecha AS DATE) BETWEEN @fechaInicio AND @fechaFin
    ORDER BY v.fecha DESC, v.idVenta DESC;
END;
GO

-- =============================================================================
-- sp_BuscarMedicamentos
-- Búsqueda por nombre o descripción con stock disponible
-- =============================================================================
IF OBJECT_ID(N'[dbo].[sp_BuscarMedicamentos]', N'P') IS NOT NULL
    DROP PROCEDURE [dbo].[sp_BuscarMedicamentos];
GO

CREATE PROCEDURE [dbo].[sp_BuscarMedicamentos]
    @busqueda VARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        idMedicamento,
        nombre,
        descripcion,
        precio,
        stockActual
    FROM [dbo].[Medicamento]
    WHERE (
            nombre LIKE N'%' + @busqueda + N'%'
            OR descripcion LIKE N'%' + @busqueda + N'%'
          )
      AND stockActual > 0
    ORDER BY nombre;
END;
GO

-- =============================================================================
-- sp_BuscarClientes
-- Búsqueda por nombre, apellido, teléfono o correo
-- =============================================================================
IF OBJECT_ID(N'[dbo].[sp_BuscarClientes]', N'P') IS NOT NULL
    DROP PROCEDURE [dbo].[sp_BuscarClientes];
GO

CREATE PROCEDURE [dbo].[sp_BuscarClientes]
    @busqueda VARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        idCliente,
        nombre,
        apellido,
        telefono,
        correo,
        direccion,
        fechaRegistro
    FROM [dbo].[Cliente]
    WHERE nombre LIKE N'%' + @busqueda + N'%'
       OR apellido LIKE N'%' + @busqueda + N'%'
       OR telefono LIKE N'%' + @busqueda + N'%'
       OR correo LIKE N'%' + @busqueda + N'%'
    ORDER BY apellido, nombre;
END;
GO

-- =============================================================================
-- sp_ObtenerDashboard
-- Resumen general para panel de control
-- =============================================================================
IF OBJECT_ID(N'[dbo].[sp_ObtenerDashboard]', N'P') IS NOT NULL
    DROP PROCEDURE [dbo].[sp_ObtenerDashboard];
GO

CREATE PROCEDURE [dbo].[sp_ObtenerDashboard]
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @hoy DATE = CAST(GETDATE() AS DATE);
    DECLARE @inicioMes DATE = DATEFROMPARTS(YEAR(@hoy), MONTH(@hoy), 1);

    SELECT
        ISNULL((
            SELECT SUM(total)
            FROM [dbo].[Venta]
            WHERE CAST(fecha AS DATE) = @hoy
        ), 0) AS ventasHoy,
        ISNULL((
            SELECT COUNT(*)
            FROM [dbo].[Venta]
            WHERE CAST(fecha AS DATE) = @hoy
        ), 0) AS cantidadVentasHoy,
        ISNULL((
            SELECT SUM(total)
            FROM [dbo].[Compra]
            WHERE CAST(fecha AS DATE) >= @inicioMes
        ), 0) AS comprasMes,
        ISNULL((
            SELECT COUNT(*)
            FROM [dbo].[Compra]
            WHERE CAST(fecha AS DATE) >= @inicioMes
        ), 0) AS cantidadComprasMes,
        ISNULL((
            SELECT COUNT(*)
            FROM [dbo].[Medicamento]
            WHERE stockActual <= stockMinimo
        ), 0) AS stockCritico,
        ISNULL((
            SELECT COUNT(*)
            FROM [dbo].[Cliente]
        ), 0) AS totalClientes;
END;
GO

PRINT N'Stored procedures de FarmaSystem creados correctamente.';
GO
