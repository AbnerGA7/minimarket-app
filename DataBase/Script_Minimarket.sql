-- 1. CREACIÓN DE LA BASE DE DATOS
CREATE DATABASE [minimarket-app];
GO
USE [minimarket-app];
GO

-- =============================================
-- MÓDULO DE SEGURIDAD Y USUARIOS
-- =============================================
CREATE TABLE Roles (
    IdRol INT PRIMARY KEY IDENTITY(1,1),
    NombreRol VARCHAR(30) NOT NULL -- 'Administrador', 'Cajero'
);

CREATE TABLE Usuarios (
    IdUsuario INT PRIMARY KEY IDENTITY(1,1),
    NombreCompleto VARCHAR(100) NOT NULL,
    Username VARCHAR(50) UNIQUE NOT NULL,
    PasswordHash VARCHAR(255) NOT NULL, -- Contraseña encriptada
    IdRol INT FOREIGN KEY REFERENCES Roles(IdRol),
    Activo BIT DEFAULT 1
);

-- =============================================
-- MÓDULO DE INVENTARIO (CORE)
-- =============================================
CREATE TABLE Categorias (
    IdCategoria INT PRIMARY KEY IDENTITY(1,1),
    Nombre VARCHAR(50) NOT NULL,
    Descripcion VARCHAR(200)
);

CREATE TABLE Proveedores (
    IdProveedor INT PRIMARY KEY IDENTITY(1,1),
    RUC_DNI VARCHAR(20),
    RazonSocial VARCHAR(100) NOT NULL,
    Telefono VARCHAR(20),
    Email VARCHAR(100)
);

CREATE TABLE Productos (
    IdProducto INT PRIMARY KEY IDENTITY(1,1),
    CodigoBarras VARCHAR(50) UNIQUE, -- Puede ser nulo si es a granel, pero idealmente único
    Nombre VARCHAR(100) NOT NULL,
    IdCategoria INT FOREIGN KEY REFERENCES Categorias(IdCategoria),
    StockMinimo INT DEFAULT 5,
    PrecioVenta DECIMAL(10,2) NOT NULL, -- Precio al público actual
    UnidadMedida VARCHAR(20) DEFAULT 'UNIDAD' -- UNIDAD, KG, LT
);

-- TABLA CRÍTICA PARA FIFO Y VENCIMIENTOS
CREATE TABLE Lotes (
    IdLote INT PRIMARY KEY IDENTITY(1,1),
    IdProducto INT FOREIGN KEY REFERENCES Productos(IdProducto),
    IdProveedor INT FOREIGN KEY REFERENCES Proveedores(IdProveedor),
    NumeroLote VARCHAR(50), -- Lote del fabricante
    FechaVencimiento DATE NOT NULL,
    FechaRecepcion DATETIME DEFAULT GETDATE(),
    CostoCompraUnitario DECIMAL(10,2) NOT NULL, -- Cuánto nos costó cada unidad de este lote
    CantidadInicial INT NOT NULL,
    StockActual INT NOT NULL, -- ESTE ES EL QUE BAJA CON LAS VENTAS
    Estado BIT DEFAULT 1 -- 1: Activo, 0: Agotado/Vencido
);

-- =============================================
-- MÓDULO DE VENTAS Y CAJA
-- =============================================
CREATE TABLE MediosPago (
    IdMedioPago INT PRIMARY KEY IDENTITY(1,1),
    Nombre VARCHAR(30) NOT NULL -- 'Efectivo', 'Yape', 'Tarjeta', 'Plin'
);

CREATE TABLE Ventas (
    IdVenta INT PRIMARY KEY IDENTITY(1,1),
    FechaVenta DATETIME DEFAULT GETDATE(),
    IdUsuario INT FOREIGN KEY REFERENCES Usuarios(IdUsuario), -- Quién vendió
    IdMedioPago INT FOREIGN KEY REFERENCES MediosPago(IdMedioPago),
    TotalVenta DECIMAL(10,2) NOT NULL,
    MontoPagado DECIMAL(10,2), -- Cuánto entregó el cliente
    Vuelto DECIMAL(10,2)       -- Cuánto se devolvió
);

-- DETALLE VENTA: Aquí ocurre la magia del FIFO
-- Se relaciona con LOTE, no solo con Producto, para saber de qué lote salió y calcular ganancia real
CREATE TABLE DetalleVenta (
    IdDetalle INT PRIMARY KEY IDENTITY(1,1),
    IdVenta INT FOREIGN KEY REFERENCES Ventas(IdVenta),
    IdLote INT FOREIGN KEY REFERENCES Lotes(IdLote), 
    Cantidad INT NOT NULL,
    PrecioVentaUnitario DECIMAL(10,2) NOT NULL, -- Precio al que se vendió en ese momento
    SubTotal AS (Cantidad * PrecioVentaUnitario)
);

-- CONTROL DE CAJA CHICA
CREATE TABLE CajaMovimientos (
    IdMovimiento INT PRIMARY KEY IDENTITY(1,1),
    IdUsuario INT FOREIGN KEY REFERENCES Usuarios(IdUsuario),
    FechaMovimiento DATETIME DEFAULT GETDATE(),
    TipoMovimiento VARCHAR(20) NOT NULL, -- 'APERTURA', 'CIERRE', 'INGRESO', 'RETIRO'
    Monto DECIMAL(10,2) NOT NULL,
    Observacion VARCHAR(200)
);

-- Insertar datos semilla (necesarios para empezar)
INSERT INTO Roles VALUES ('Administrador'), ('Cajero');
INSERT INTO MediosPago VALUES ('Efectivo'), ('Yape'), ('Plin'), ('Tarjeta');
INSERT INTO Categorias VALUES ('General', 'Productos varios');