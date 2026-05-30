-- =============================================================================
-- FarmaSystemDB — SETUP COMPLETO
-- Script maestro idempotente: BD + tablas + vistas + SP + datos iniciales
-- =============================================================================

-- =============================================================================
-- 1. CREAR BASE DE DATOS
-- =============================================================================
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'FarmaSystemDB')
BEGIN
    CREATE DATABASE [FarmaSystemDB];
END
GO

USE [FarmaSystemDB];
GO

SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO

-- =============================================================================
-- 2. CREAR TABLAS
-- =============================================================================

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name = N'Categoria' AND xtype = 'U')
CREATE TABLE [dbo].[Categoria](
    [idCategoria] [int] IDENTITY(1,1) NOT NULL,
    [nombre] [varchar](80) NOT NULL,
    [descripcion] [varchar](200) NULL,
    CONSTRAINT [PK_Categoria] PRIMARY KEY CLUSTERED ([idCategoria] ASC),
    CONSTRAINT [UQ_Categoria_nombre] UNIQUE NONCLUSTERED ([nombre] ASC)
);
GO

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name = N'Proveedor' AND xtype = 'U')
CREATE TABLE [dbo].[Proveedor](
    [idProveedor] [int] IDENTITY(1,1) NOT NULL,
    [nombre] [varchar](100) NOT NULL,
    [contacto] [varchar](100) NULL,
    [telefono] [varchar](20) NULL,
    [correo] [varchar](100) NULL,
    [direccion] [varchar](200) NULL,
    [activo] [bit] NOT NULL CONSTRAINT [DF_Proveedor_activo] DEFAULT ((1)),
    CONSTRAINT [PK_Proveedor] PRIMARY KEY CLUSTERED ([idProveedor] ASC)
);
GO

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name = N'Empleado' AND xtype = 'U')
CREATE TABLE [dbo].[Empleado](
    [idEmpleado] [int] IDENTITY(1,1) NOT NULL,
    [nombre] [varchar](80) NOT NULL,
    [apellido] [varchar](80) NOT NULL,
    [cargo] [varchar](50) NOT NULL,
    [salario] [decimal](10, 2) NULL,
    [telefono] [varchar](20) NULL,
    [fechaIngreso] [date] NOT NULL CONSTRAINT [DF_Empleado_fechaIngreso] DEFAULT (GETDATE()),
    [activo] [bit] NOT NULL CONSTRAINT [DF_Empleado_activo] DEFAULT ((1)),
    CONSTRAINT [PK_Empleado] PRIMARY KEY CLUSTERED ([idEmpleado] ASC),
    CONSTRAINT [CK_Empleado_cargo] CHECK ([cargo] = N'Cajero' OR [cargo] = N'Vendedor' OR [cargo] = N'Administrador'),
    CONSTRAINT [CK_Empleado_sal] CHECK ([salario] >= (0))
);
GO

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name = N'Rol' AND xtype = 'U')
CREATE TABLE [dbo].[Rol](
    [idRol] [int] IDENTITY(1,1) NOT NULL,
    [nombre] [varchar](50) NOT NULL,
    [descripcion] [varchar](200) NULL,
    [activo] [bit] NOT NULL CONSTRAINT [DF_Rol_activo] DEFAULT ((1)),
    CONSTRAINT [PK_Rol] PRIMARY KEY CLUSTERED ([idRol] ASC),
    CONSTRAINT [UQ_Rol_nombre] UNIQUE NONCLUSTERED ([nombre] ASC)
);
GO

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name = N'Cliente' AND xtype = 'U')
CREATE TABLE [dbo].[Cliente](
    [idCliente] [int] IDENTITY(1,1) NOT NULL,
    [nombre] [varchar](80) NOT NULL,
    [apellido] [varchar](80) NOT NULL,
    [telefono] [varchar](20) NULL,
    [correo] [varchar](100) NULL,
    [direccion] [varchar](200) NULL,
    [fechaRegistro] [date] NOT NULL CONSTRAINT [DF_Cliente_fechaRegistro] DEFAULT (GETDATE()),
    CONSTRAINT [PK_Cliente] PRIMARY KEY CLUSTERED ([idCliente] ASC)
);
GO

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name = N'Medicamento' AND xtype = 'U')
CREATE TABLE [dbo].[Medicamento](
    [idMedicamento] [int] IDENTITY(1,1) NOT NULL,
    [nombre] [varchar](100) NOT NULL,
    [descripcion] [varchar](300) NULL,
    [precio] [decimal](10, 2) NOT NULL,
    [stockActual] [int] NOT NULL CONSTRAINT [DF_Medicamento_stockActual] DEFAULT ((0)),
    [stockMinimo] [int] NOT NULL CONSTRAINT [DF_Medicamento_stockMinimo] DEFAULT ((5)),
    [idCategoria] [int] NOT NULL,
    [idProveedor] [int] NOT NULL,
    CONSTRAINT [PK_Medicamento] PRIMARY KEY CLUSTERED ([idMedicamento] ASC),
    CONSTRAINT [CK_Med_precio] CHECK ([precio] > (0)),
    CONSTRAINT [CK_Med_stockActual] CHECK ([stockActual] >= (0)),
    CONSTRAINT [CK_Med_stockMinimo] CHECK ([stockMinimo] >= (0))
);
GO

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name = N'Venta' AND xtype = 'U')
CREATE TABLE [dbo].[Venta](
    [idVenta] [int] IDENTITY(1,1) NOT NULL,
    [fecha] [datetime] NOT NULL CONSTRAINT [DF_Venta_fecha] DEFAULT (GETDATE()),
    [total] [decimal](10, 2) NOT NULL CONSTRAINT [DF_Venta_total] DEFAULT ((0)),
    [descuento] [decimal](5, 2) NOT NULL CONSTRAINT [DF_Venta_descuento] DEFAULT ((0)),
    [idCliente] [int] NOT NULL,
    [idEmpleado] [int] NOT NULL,
    CONSTRAINT [PK_Venta] PRIMARY KEY CLUSTERED ([idVenta] ASC),
    CONSTRAINT [CK_Venta_desc] CHECK ([descuento] >= (0) AND [descuento] <= (100)),
    CONSTRAINT [CK_Venta_total] CHECK ([total] >= (0))
);
GO

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name = N'DetalleVenta' AND xtype = 'U')
CREATE TABLE [dbo].[DetalleVenta](
    [idDetalle] [int] IDENTITY(1,1) NOT NULL,
    [idVenta] [int] NOT NULL,
    [idMedicamento] [int] NOT NULL,
    [cantidad] [int] NOT NULL,
    [precioUnitario] [decimal](10, 2) NOT NULL,
    [subtotal] AS ([cantidad] * [precioUnitario]) PERSISTED,
    CONSTRAINT [PK_DetalleVenta] PRIMARY KEY CLUSTERED ([idDetalle] ASC),
    CONSTRAINT [CK_DV_cantidad] CHECK ([cantidad] > (0)),
    CONSTRAINT [CK_DV_precio] CHECK ([precioUnitario] > (0))
);
GO

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name = N'Compra' AND xtype = 'U')
CREATE TABLE [dbo].[Compra](
    [idCompra] [int] IDENTITY(1,1) NOT NULL,
    [fecha] [datetime] NOT NULL CONSTRAINT [DF_Compra_fecha] DEFAULT (GETDATE()),
    [total] [decimal](10, 2) NOT NULL CONSTRAINT [DF_Compra_total] DEFAULT ((0)),
    [nFactura] [varchar](30) NULL,
    [idProveedor] [int] NOT NULL,
    [idEmpleado] [int] NOT NULL,
    CONSTRAINT [PK_Compra] PRIMARY KEY CLUSTERED ([idCompra] ASC),
    CONSTRAINT [CK_Compra_total] CHECK ([total] >= (0))
);
GO

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name = N'DetalleCompra' AND xtype = 'U')
CREATE TABLE [dbo].[DetalleCompra](
    [idDetalle] [int] IDENTITY(1,1) NOT NULL,
    [idCompra] [int] NOT NULL,
    [idMedicamento] [int] NOT NULL,
    [cantidad] [int] NOT NULL,
    [costoUnitario] [decimal](10, 2) NOT NULL,
    [subtotal] AS ([cantidad] * [costoUnitario]) PERSISTED,
    CONSTRAINT [PK_DetalleCompra] PRIMARY KEY CLUSTERED ([idDetalle] ASC),
    CONSTRAINT [CK_DC_cantidad] CHECK ([cantidad] > (0)),
    CONSTRAINT [CK_DC_costo] CHECK ([costoUnitario] > (0))
);
GO

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name = N'MovimientoInventario' AND xtype = 'U')
CREATE TABLE [dbo].[MovimientoInventario](
    [idMovimiento] [int] IDENTITY(1,1) NOT NULL,
    [idMedicamento] [int] NOT NULL,
    [tipo] [varchar](10) NOT NULL,
    [cantidad] [int] NOT NULL,
    [fecha] [datetime] NOT NULL CONSTRAINT [DF_MovimientoInventario_fecha] DEFAULT (GETDATE()),
    [motivo] [varchar](200) NULL,
    CONSTRAINT [PK_MovimientoInventario] PRIMARY KEY CLUSTERED ([idMovimiento] ASC),
    CONSTRAINT [CK_Mov_cantidad] CHECK ([cantidad] > (0)),
    CONSTRAINT [CK_Mov_tipo] CHECK ([tipo] = N'Salida' OR [tipo] = N'Entrada')
);
GO

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name = N'Usuario' AND xtype = 'U')
CREATE TABLE [dbo].[Usuario](
    [idUsuario] [int] IDENTITY(1,1) NOT NULL,
    [usuario] [varchar](50) NOT NULL,
    [contrasena] [varchar](256) NOT NULL,
    [idEmpleado] [int] NOT NULL,
    [idRol] [int] NOT NULL,
    [activo] [bit] NOT NULL CONSTRAINT [DF_Usuario_activo] DEFAULT ((1)),
    [fechaCreacion] [datetime] NOT NULL CONSTRAINT [DF_Usuario_fechaCreacion] DEFAULT (GETDATE()),
    [ultimoAcceso] [datetime] NULL,
    CONSTRAINT [PK_Usuario] PRIMARY KEY CLUSTERED ([idUsuario] ASC),
    CONSTRAINT [UQ_Usuario_empleado] UNIQUE NONCLUSTERED ([idEmpleado] ASC),
    CONSTRAINT [UQ_Usuario_usuario] UNIQUE NONCLUSTERED ([usuario] ASC)
);
GO

-- Foreign keys (idempotentes)
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Med_Categoria')
    ALTER TABLE [dbo].[Medicamento] WITH CHECK ADD CONSTRAINT [FK_Med_Categoria]
        FOREIGN KEY([idCategoria]) REFERENCES [dbo].[Categoria]([idCategoria]) ON UPDATE CASCADE;
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Med_Proveedor')
    ALTER TABLE [dbo].[Medicamento] WITH CHECK ADD CONSTRAINT [FK_Med_Proveedor]
        FOREIGN KEY([idProveedor]) REFERENCES [dbo].[Proveedor]([idProveedor]) ON UPDATE CASCADE;
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Venta_Cliente')
    ALTER TABLE [dbo].[Venta] WITH CHECK ADD CONSTRAINT [FK_Venta_Cliente]
        FOREIGN KEY([idCliente]) REFERENCES [dbo].[Cliente]([idCliente]) ON UPDATE CASCADE;
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Venta_Empleado')
    ALTER TABLE [dbo].[Venta] WITH CHECK ADD CONSTRAINT [FK_Venta_Empleado]
        FOREIGN KEY([idEmpleado]) REFERENCES [dbo].[Empleado]([idEmpleado]) ON UPDATE CASCADE;
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_DV_Venta')
    ALTER TABLE [dbo].[DetalleVenta] WITH CHECK ADD CONSTRAINT [FK_DV_Venta]
        FOREIGN KEY([idVenta]) REFERENCES [dbo].[Venta]([idVenta]) ON UPDATE CASCADE ON DELETE CASCADE;
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_DV_Medicamento')
    ALTER TABLE [dbo].[DetalleVenta] WITH CHECK ADD CONSTRAINT [FK_DV_Medicamento]
        FOREIGN KEY([idMedicamento]) REFERENCES [dbo].[Medicamento]([idMedicamento]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Compra_Proveedor')
    ALTER TABLE [dbo].[Compra] WITH CHECK ADD CONSTRAINT [FK_Compra_Proveedor]
        FOREIGN KEY([idProveedor]) REFERENCES [dbo].[Proveedor]([idProveedor]) ON UPDATE CASCADE;
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Compra_Empleado')
    ALTER TABLE [dbo].[Compra] WITH CHECK ADD CONSTRAINT [FK_Compra_Empleado]
        FOREIGN KEY([idEmpleado]) REFERENCES [dbo].[Empleado]([idEmpleado]) ON UPDATE CASCADE;
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_DC_Compra')
    ALTER TABLE [dbo].[DetalleCompra] WITH CHECK ADD CONSTRAINT [FK_DC_Compra]
        FOREIGN KEY([idCompra]) REFERENCES [dbo].[Compra]([idCompra]) ON UPDATE CASCADE ON DELETE CASCADE;
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_DC_Medicamento')
    ALTER TABLE [dbo].[DetalleCompra] WITH CHECK ADD CONSTRAINT [FK_DC_Medicamento]
        FOREIGN KEY([idMedicamento]) REFERENCES [dbo].[Medicamento]([idMedicamento]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Mov_Medicamento')
    ALTER TABLE [dbo].[MovimientoInventario] WITH CHECK ADD CONSTRAINT [FK_Mov_Medicamento]
        FOREIGN KEY([idMedicamento]) REFERENCES [dbo].[Medicamento]([idMedicamento]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Usuario_Empleado')
    ALTER TABLE [dbo].[Usuario] WITH CHECK ADD CONSTRAINT [FK_Usuario_Empleado]
        FOREIGN KEY([idEmpleado]) REFERENCES [dbo].[Empleado]([idEmpleado]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Usuario_Rol')
    ALTER TABLE [dbo].[Usuario] WITH CHECK ADD CONSTRAINT [FK_Usuario_Rol]
        FOREIGN KEY([idRol]) REFERENCES [dbo].[Rol]([idRol]);
GO

-- =============================================================================
-- 3. CREAR VISTAS
-- =============================================================================

IF OBJECT_ID(N'dbo.vw_ResumenVentas', N'V') IS NOT NULL
    DROP VIEW dbo.vw_ResumenVentas;
GO

CREATE VIEW dbo.vw_ResumenVentas AS
SELECT
    v.idVenta,
    v.fecha,
    v.total,
    v.descuento,
    v.idCliente,
    (c.nombre + N' ' + c.apellido) AS clienteNombre,
    v.idEmpleado,
    (e.nombre + N' ' + e.apellido) AS empleadoNombre,
    dv.idDetalle,
    dv.idMedicamento,
    m.nombre AS medicamento,
    dv.cantidad,
    dv.precioUnitario,
    dv.subtotal
FROM dbo.Venta v
INNER JOIN dbo.Cliente c ON v.idCliente = c.idCliente
INNER JOIN dbo.Empleado e ON v.idEmpleado = e.idEmpleado
LEFT JOIN dbo.DetalleVenta dv ON v.idVenta = dv.idVenta
LEFT JOIN dbo.Medicamento m ON dv.idMedicamento = m.idMedicamento;
GO

IF OBJECT_ID(N'dbo.vw_InventarioActual', N'V') IS NOT NULL
    DROP VIEW dbo.vw_InventarioActual;
GO

CREATE VIEW dbo.vw_InventarioActual AS
SELECT
    m.idMedicamento,
    m.nombre,
    m.descripcion,
    m.precio,
    m.stockActual,
    m.stockMinimo,
    m.idCategoria,
    cat.nombre AS categoria,
    m.idProveedor,
    prov.nombre AS proveedor,
    CASE
        WHEN m.stockActual <= m.stockMinimo THEN N'BAJO'
        ELSE N'OK'
    END AS estadoStock
FROM dbo.Medicamento m
LEFT JOIN dbo.Categoria cat ON m.idCategoria = cat.idCategoria
LEFT JOIN dbo.Proveedor prov ON m.idProveedor = prov.idProveedor;
GO

IF OBJECT_ID(N'dbo.vw_MovimientosCompletos', N'V') IS NOT NULL
    DROP VIEW dbo.vw_MovimientosCompletos;
GO

CREATE VIEW dbo.vw_MovimientosCompletos AS
SELECT
    mi.idMovimiento,
    mi.idMedicamento,
    m.nombre AS medicamento,
    mi.tipo,
    mi.cantidad,
    mi.fecha,
    mi.motivo
FROM dbo.MovimientoInventario mi
LEFT JOIN dbo.Medicamento m ON mi.idMedicamento = m.idMedicamento;
GO

IF OBJECT_ID(N'dbo.vw_ResumenCompras', N'V') IS NOT NULL
    DROP VIEW dbo.vw_ResumenCompras;
GO

CREATE VIEW dbo.vw_ResumenCompras AS
SELECT
    c.idCompra,
    c.fecha,
    c.total,
    c.nFactura,
    c.idProveedor,
    p.nombre AS proveedorNombre,
    c.idEmpleado,
    (e.nombre + N' ' + e.apellido) AS empleadoNombre,
    dc.idDetalle,
    dc.idMedicamento,
    m.nombre AS medicamento,
    dc.cantidad,
    dc.costoUnitario,
    dc.subtotal
FROM dbo.Compra c
LEFT JOIN dbo.Proveedor p ON c.idProveedor = p.idProveedor
LEFT JOIN dbo.Empleado e ON c.idEmpleado = e.idEmpleado
LEFT JOIN dbo.DetalleCompra dc ON c.idCompra = dc.idCompra
LEFT JOIN dbo.Medicamento m ON dc.idMedicamento = m.idMedicamento;
GO

-- =============================================================================
-- 4. CREAR STORED PROCEDURES
-- =============================================================================

IF OBJECT_ID(N'dbo.sp_RegistrarVenta', N'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_RegistrarVenta;
GO

CREATE PROCEDURE dbo.sp_RegistrarVenta
    @idCliente  INT,
    @idEmpleado INT,
    @descuento  DECIMAL(18, 2) = 0,
    @total      DECIMAL(18, 2)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @idVenta INT;

    INSERT INTO Venta (idCliente, idEmpleado, descuento, total, fecha)
    VALUES (@idCliente, @idEmpleado, @descuento, @total, GETDATE());

    SET @idVenta = SCOPE_IDENTITY();
    SELECT @idVenta AS idVenta;
END;
GO

IF OBJECT_ID(N'dbo.sp_RegistrarDetalleVenta', N'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_RegistrarDetalleVenta;
GO

CREATE PROCEDURE dbo.sp_RegistrarDetalleVenta
    @idVenta        INT,
    @idMedicamento  INT,
    @cantidad       INT,
    @precioUnitario DECIMAL(18, 2)
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO DetalleVenta (idVenta, idMedicamento, cantidad, precioUnitario)
    VALUES (@idVenta, @idMedicamento, @cantidad, @precioUnitario);

    UPDATE Medicamento
    SET stockActual = stockActual - @cantidad
    WHERE idMedicamento = @idMedicamento;
END;
GO

IF OBJECT_ID(N'dbo.sp_AgregarDetalleVenta', N'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_AgregarDetalleVenta;
GO

CREATE PROCEDURE dbo.sp_AgregarDetalleVenta
    @idVenta        INT,
    @idMedicamento  INT,
    @cantidad       INT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @precio DECIMAL(10, 2);

    SELECT @precio = precio
    FROM Medicamento
    WHERE idMedicamento = @idMedicamento;

    IF @precio IS NULL
    BEGIN
        RAISERROR(N'Medicamento no encontrado.', 16, 1);
        RETURN;
    END;

    EXEC dbo.sp_RegistrarDetalleVenta
        @idVenta        = @idVenta,
        @idMedicamento  = @idMedicamento,
        @cantidad       = @cantidad,
        @precioUnitario = @precio;
END;
GO

IF OBJECT_ID(N'dbo.sp_RegistrarCompra', N'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_RegistrarCompra;
GO

CREATE PROCEDURE dbo.sp_RegistrarCompra
    @idProveedor INT,
    @idEmpleado  INT,
    @nFactura    VARCHAR(30) = NULL,
    @idCompra    INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO Compra (fecha, total, nFactura, idProveedor, idEmpleado)
    VALUES (GETDATE(), 0, @nFactura, @idProveedor, @idEmpleado);

    SET @idCompra = SCOPE_IDENTITY();
END;
GO

IF OBJECT_ID(N'dbo.sp_RegistrarDetalleCompra', N'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_RegistrarDetalleCompra;
GO

CREATE PROCEDURE dbo.sp_RegistrarDetalleCompra
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

    IF NOT EXISTS (SELECT 1 FROM Medicamento WHERE idMedicamento = @idMedicamento)
    BEGIN
        RAISERROR(N'Medicamento no encontrado.', 16, 1);
        RETURN;
    END;

    INSERT INTO DetalleCompra (idCompra, idMedicamento, cantidad, costoUnitario)
    VALUES (@idCompra, @idMedicamento, @cantidad, @costoUnitario);

    UPDATE Medicamento
    SET stockActual = stockActual + @cantidad
    WHERE idMedicamento = @idMedicamento;

    UPDATE Compra
    SET total = (
        SELECT ISNULL(SUM(subtotal), 0)
        FROM DetalleCompra
        WHERE idCompra = @idCompra
    )
    WHERE idCompra = @idCompra;

    INSERT INTO MovimientoInventario (idMedicamento, tipo, cantidad, fecha, motivo)
    VALUES (@idMedicamento, N'Entrada', @cantidad, GETDATE(), CONCAT(N'Compra #', @idCompra));
END;
GO

IF OBJECT_ID(N'dbo.sp_ConsultarInventario', N'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_ConsultarInventario;
GO

CREATE PROCEDURE dbo.sp_ConsultarInventario
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
    FROM Medicamento m
    INNER JOIN Categoria c ON m.idCategoria = c.idCategoria
    INNER JOIN Proveedor p ON m.idProveedor = p.idProveedor
    ORDER BY m.nombre;
END;
GO

IF OBJECT_ID(N'dbo.sp_ReporteVentasPorPeriodo', N'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_ReporteVentasPorPeriodo;
GO

CREATE PROCEDURE dbo.sp_ReporteVentasPorPeriodo
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
    FROM Venta v
    INNER JOIN Cliente c ON v.idCliente = c.idCliente
    INNER JOIN Empleado e ON v.idEmpleado = e.idEmpleado
    WHERE CAST(v.fecha AS DATE) BETWEEN @fechaInicio AND @fechaFin
    ORDER BY v.fecha DESC, v.idVenta DESC;
END;
GO

IF OBJECT_ID(N'dbo.sp_BuscarMedicamentos', N'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_BuscarMedicamentos;
GO

CREATE PROCEDURE dbo.sp_BuscarMedicamentos
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
    FROM Medicamento
    WHERE (
            nombre LIKE N'%' + @busqueda + N'%'
            OR descripcion LIKE N'%' + @busqueda + N'%'
          )
      AND stockActual > 0
    ORDER BY nombre;
END;
GO

IF OBJECT_ID(N'dbo.sp_BuscarClientes', N'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_BuscarClientes;
GO

CREATE PROCEDURE dbo.sp_BuscarClientes
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
    FROM Cliente
    WHERE nombre LIKE N'%' + @busqueda + N'%'
       OR apellido LIKE N'%' + @busqueda + N'%'
       OR telefono LIKE N'%' + @busqueda + N'%'
       OR correo LIKE N'%' + @busqueda + N'%'
    ORDER BY apellido, nombre;
END;
GO

IF OBJECT_ID(N'dbo.sp_ObtenerDashboard', N'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_ObtenerDashboard;
GO

CREATE PROCEDURE dbo.sp_ObtenerDashboard
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @hoy DATE = CAST(GETDATE() AS DATE);
    DECLARE @inicioMes DATE = DATEFROMPARTS(YEAR(@hoy), MONTH(@hoy), 1);

    SELECT
        ISNULL((SELECT SUM(total) FROM Venta WHERE CAST(fecha AS DATE) = @hoy), 0) AS ventasHoy,
        ISNULL((SELECT COUNT(*) FROM Venta WHERE CAST(fecha AS DATE) = @hoy), 0) AS cantidadVentasHoy,
        ISNULL((SELECT SUM(total) FROM Compra WHERE CAST(fecha AS DATE) >= @inicioMes), 0) AS comprasMes,
        ISNULL((SELECT COUNT(*) FROM Compra WHERE CAST(fecha AS DATE) >= @inicioMes), 0) AS cantidadComprasMes,
        ISNULL((SELECT COUNT(*) FROM Medicamento WHERE stockActual <= stockMinimo), 0) AS stockCritico,
        ISNULL((SELECT COUNT(*) FROM Cliente), 0) AS totalClientes;
END;
GO

-- =============================================================================
-- 5. DATOS INICIALES — ROLES, EMPLEADOS Y USUARIOS
-- =============================================================================

IF NOT EXISTS (SELECT 1 FROM Rol WHERE nombre = N'Administrador')
    INSERT INTO Rol (nombre, descripcion, activo)
    VALUES (N'Administrador', N'Acceso completo al sistema', 1);
GO

IF NOT EXISTS (SELECT 1 FROM Rol WHERE nombre = N'Vendedor')
    INSERT INTO Rol (nombre, descripcion, activo)
    VALUES (N'Vendedor', N'Ventas y clientes', 1);
GO

IF NOT EXISTS (SELECT 1 FROM Rol WHERE nombre = N'Inventario')
    INSERT INTO Rol (nombre, descripcion, activo)
    VALUES (N'Inventario', N'Inventario, compras y proveedores', 1);
GO

IF NOT EXISTS (SELECT 1 FROM Empleado WHERE idEmpleado = 1)
BEGIN
    SET IDENTITY_INSERT Empleado ON;
    INSERT INTO Empleado (idEmpleado, nombre, apellido, cargo, salario, telefono, fechaIngreso, activo)
    VALUES (1, N'Carlos', N'Administrador', N'Administrador', 8500.00, N'555-0001', CAST(GETDATE() AS DATE), 1);
    SET IDENTITY_INSERT Empleado OFF;
END
GO

IF NOT EXISTS (SELECT 1 FROM Empleado WHERE idEmpleado = 2)
BEGIN
    SET IDENTITY_INSERT Empleado ON;
    INSERT INTO Empleado (idEmpleado, nombre, apellido, cargo, salario, telefono, fechaIngreso, activo)
    VALUES (2, N'María', N'Vendedora', N'Vendedor', 4500.00, N'555-0002', CAST(GETDATE() AS DATE), 1);
    SET IDENTITY_INSERT Empleado OFF;
END
GO

IF NOT EXISTS (SELECT 1 FROM Empleado WHERE idEmpleado = 3)
BEGIN
    SET IDENTITY_INSERT Empleado ON;
    INSERT INTO Empleado (idEmpleado, nombre, apellido, cargo, salario, telefono, fechaIngreso, activo)
    VALUES (3, N'Luis', N'Bodega', N'Cajero', 4200.00, N'555-0003', CAST(GETDATE() AS DATE), 1);
    SET IDENTITY_INSERT Empleado OFF;
END
GO

DECLARE @idRolAdmin INT = (SELECT idRol FROM Rol WHERE nombre = N'Administrador');
DECLARE @idRolVendedor INT = (SELECT idRol FROM Rol WHERE nombre = N'Vendedor');
DECLARE @idRolInventario INT = (SELECT idRol FROM Rol WHERE nombre = N'Inventario');

IF NOT EXISTS (SELECT 1 FROM Usuario WHERE usuario = N'admin')
    INSERT INTO Usuario (usuario, contrasena, idEmpleado, idRol, activo, fechaCreacion)
    VALUES (N'admin', N'admin123', 1, @idRolAdmin, 1, GETDATE());
GO

DECLARE @idRolVendedor2 INT = (SELECT idRol FROM Rol WHERE nombre = N'Vendedor');

IF NOT EXISTS (SELECT 1 FROM Usuario WHERE usuario = N'vendedor')
    INSERT INTO Usuario (usuario, contrasena, idEmpleado, idRol, activo, fechaCreacion)
    VALUES (N'vendedor', N'vendedor123', 2, @idRolVendedor2, 1, GETDATE());
GO

DECLARE @idRolInventario2 INT = (SELECT idRol FROM Rol WHERE nombre = N'Inventario');

IF NOT EXISTS (SELECT 1 FROM Usuario WHERE usuario = N'inventario')
    INSERT INTO Usuario (usuario, contrasena, idEmpleado, idRol, activo, fechaCreacion)
    VALUES (N'inventario', N'inventario123', 3, @idRolInventario2, 1, GETDATE());
GO

PRINT N'FarmaSystemDB — SETUP COMPLETO ejecutado correctamente.';
GO

-- ============================================
-- INSTRUCCIONES DE EJECUCION:
-- 1. Abre SSMS
-- 2. Conectate a tu servidor
-- 3. Abre este archivo
-- 4. Presiona F5 para ejecutar
-- 5. Debe mostrar "Command(s) completed successfully"
-- 6. Reinicia el backend: dotnet run
-- ============================================
