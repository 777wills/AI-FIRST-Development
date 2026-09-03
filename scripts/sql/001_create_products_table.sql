-- =============================================================================
-- Script: 001_create_products_table.sql
-- Descripción: Crea la tabla Products y carga 30 registros de prueba.
-- Entidad: Olimpia.Domain.Entities.Product (hereda de BaseEntity)
-- =============================================================================

-- Crear tabla Products
IF NOT EXISTS (
    SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Products]') AND type = N'U'
)
BEGIN
    CREATE TABLE [dbo].[Products] (
        [Id]          INT             NOT NULL IDENTITY(1,1)  PRIMARY KEY,
        [Name]        NVARCHAR(200)   NOT NULL,
        [Description] NVARCHAR(1000)  NOT NULL,
        [Price]       DECIMAL(18,2)   NOT NULL,
        [Stock]       INT             NOT NULL,
        [CreatedAt]   DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt]   DATETIME2       NULL
    );

    PRINT 'Tabla Products creada correctamente.';
END
ELSE
BEGIN
    PRINT 'La tabla Products ya existe.';
END
GO

-- =============================================================================
-- Datos de prueba (30 registros)
-- =============================================================================
SET IDENTITY_INSERT [dbo].[Products] OFF;

INSERT INTO [dbo].[Products] ([Name], [Description], [Price], [Stock], [CreatedAt])
VALUES
    ('Laptop Pro 15',          'Laptop profesional 15" Intel Core i7, 16GB RAM, 512GB SSD',          1299.99,  45, DATEADD(DAY, -30, GETUTCDATE())),
    ('Laptop Air 13',          'Laptop ultradelgada 13" con 8GB RAM y 256GB SSD',                     899.99,  60, DATEADD(DAY, -29, GETUTCDATE())),
    ('MacBook Clone 14',       'Portátil premium 14" con pantalla IPS y teclado retro-iluminado',    1099.00,  20, DATEADD(DAY, -28, GETUTCDATE())),
    ('Monitor 27" 4K',         'Monitor UHD 4K 27 pulgadas con panel IPS y 144Hz',                    549.99,  35, DATEADD(DAY, -27, GETUTCDATE())),
    ('Monitor 24" Full HD',    'Monitor Full HD 24 pulgadas 75Hz con entrada HDMI y VGA',             199.99,  80, DATEADD(DAY, -26, GETUTCDATE())),
    ('Teclado Mecánico RGB',   'Teclado mecánico con switches Blue y retroiluminación RGB',            89.99, 120, DATEADD(DAY, -25, GETUTCDATE())),
    ('Teclado Inalámbrico',    'Teclado inalámbrico Bluetooth con batería de larga duración',          49.99, 200, DATEADD(DAY, -24, GETUTCDATE())),
    ('Mouse Gaming 12000 DPI', 'Mouse gaming con sensor óptico 12000 DPI y 7 botones programables',   59.99, 150, DATEADD(DAY, -23, GETUTCDATE())),
    ('Mouse Inalámbrico',      'Mouse inalámbrico ergonómico con receptor USB nano',                   29.99, 300, DATEADD(DAY, -22, GETUTCDATE())),
    ('Auriculares Noise Cancel','Auriculares over-ear con cancelación activa de ruido y 30h batería', 249.99,  55, DATEADD(DAY, -21, GETUTCDATE())),
    ('Auriculares Gaming',     'Auriculares gaming 7.1 virtual surround con micrófono desmontable',    79.99,  90, DATEADD(DAY, -20, GETUTCDATE())),
    ('Webcam 1080p',           'Cámara web Full HD 1080p con micrófono integrado y corrección de luz', 69.99,  70, DATEADD(DAY, -19, GETUTCDATE())),
    ('SSD Externo 1TB',        'Unidad SSD portátil 1TB USB-C con velocidad de lectura 1050 MB/s',    109.99,  40, DATEADD(DAY, -18, GETUTCDATE())),
    ('SSD Externo 500GB',      'Unidad SSD portátil 500GB USB 3.2 con carcasa resistente a golpes',    64.99,  65, DATEADD(DAY, -17, GETUTCDATE())),
    ('Hub USB-C 7 en 1',       'Hub multipuerto USB-C con HDMI 4K, USB 3.0 x3, SD, microSD y PD',     44.99, 180, DATEADD(DAY, -16, GETUTCDATE())),
    ('Cargador GaN 65W',       'Cargador compacto GaN 65W con puerto USB-C y USB-A',                   35.99, 250, DATEADD(DAY, -15, GETUTCDATE())),
    ('Cable USB-C a USB-C 2m', 'Cable USB-C trenzado 2m con soporte para carga 100W y datos 10Gbps',  14.99, 400, DATEADD(DAY, -14, GETUTCDATE())),
    ('Soporte Laptop Ajustable','Soporte ergonómico de aluminio ajustable en altura para laptops',     39.99, 110, DATEADD(DAY, -13, GETUTCDATE())),
    ('Alfombrilla XL Gaming',  'Alfombrilla de ratón extra grande 900x400mm con base antideslizante',  24.99, 160, DATEADD(DAY, -12, GETUTCDATE())),
    ('Impresora Láser',        'Impresora láser monocromo con WiFi, duplex automático y 30ppm',       279.99,  25, DATEADD(DAY, -11, GETUTCDATE())),
    ('Impresora Multifunción', 'Impresora de tinta a color multifunción con escáner y fax',           149.99,  30, DATEADD(DAY, -10, GETUTCDATE())),
    ('Tablet 10" Android',     'Tablet Android 10 pulgadas 4GB RAM 64GB con pantalla FHD',            229.99,  50, DATEADD(DAY,  -9, GETUTCDATE())),
    ('iPad Mini Clone',        'Tablet 8 pulgadas con stylus incluido y batería de 7000mAh',          179.99,  35, DATEADD(DAY,  -8, GETUTCDATE())),
    ('Mochila Laptop 15.6"',   'Mochila antirrobo para laptop 15.6" con puerto USB de carga',         54.99, 130, DATEADD(DAY,  -7, GETUTCDATE())),
    ('Funda Neopreno 14"',     'Funda de neopreno impermeable para laptop 14" con bolsillo extra',     18.99, 220, DATEADD(DAY,  -6, GETUTCDATE())),
    ('Switch HDMI 4K',         'Selector HDMI 4 entradas 1 salida compatible con 4K@60Hz',             26.99,  95, DATEADD(DAY,  -5, GETUTCDATE())),
    ('Regleta con Protección', 'Regleta de 6 tomas con protección contra sobretensiones y 4 USB',      32.99, 140, DATEADD(DAY,  -4, GETUTCDATE())),
    ('Cooler Laptop 17"',      'Base refrigeradora para laptop hasta 17" con 2 ventiladores silenciosos',22.99, 100, DATEADD(DAY, -3, GETUTCDATE())),
    ('Pasta Térmica',          'Pasta térmica de alto rendimiento para CPU y GPU, jeringa 4g',           7.99, 500, DATEADD(DAY,  -2, GETUTCDATE())),
    ('Kit Limpieza Pantallas', 'Kit de limpieza para pantallas incluye spray, paño microfibra y esponja',11.99, 350, DATEADD(DAY,  -1, GETUTCDATE()));

PRINT CONCAT('Insertados ', @@ROWCOUNT, ' registros de prueba en Products.');
GO
