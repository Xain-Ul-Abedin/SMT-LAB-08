-- Database Initialization Script for Enterprise Product Management System (SMT-LAB-08)

IF NOT EXISTS (SELECT 1 FROM sys.databases WHERE name = 'SMT_Inventory_DB')
BEGIN
    CREATE DATABASE SMT_Inventory_DB;
END
GO

USE SMT_Inventory_DB;
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Products' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE [dbo].[Products] (
        [Id]            INT            IDENTITY(1,1) NOT NULL,
        [Sku]           NVARCHAR(30)   NOT NULL,
        [Title]         NVARCHAR(120)  NOT NULL,
        [Category]      NVARCHAR(60)   NOT NULL,
        [Description]   NVARCHAR(500)  NOT NULL,
        [ImageUrl]      NVARCHAR(255)  NOT NULL,
        [Price]         DECIMAL(18,2)  NOT NULL,
        [StockQuantity] INT            NOT NULL,
        [Status]        NVARCHAR(40)   NOT NULL,
        [CreatedAt]     DATETIME2      NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT [PK_Products] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [UQ_Products_Sku] UNIQUE NONCLUSTERED ([Sku] ASC)
    );

    CREATE NONCLUSTERED INDEX [IX_Products_Category] ON [dbo].[Products] ([Category] ASC);
    CREATE NONCLUSTERED INDEX [IX_Products_Status] ON [dbo].[Products] ([Status] ASC);
END
GO

-- Seed Baseline Catalog Data
IF NOT EXISTS (SELECT 1 FROM [dbo].[Products])
BEGIN
    INSERT INTO [dbo].[Products] ([Sku], [Title], [Category], [Description], [ImageUrl], [Price], [StockQuantity], [Status], [CreatedAt])
    VALUES
    ('PRD-1001', 'Logitech MX Master 3S', 'Peripherals', 'Ergonomic performance wireless mouse with 8K DPI track-on-glass sensor and quiet click mechanisms.', 'uploads/catalog/sample-mxmaster.jpg', 99.99, 14, 'Active', GETUTCDATE()),
    ('PRD-1002', 'Keychron Q1 Pro Mechanical Keyboard', 'Peripherals', 'Custom 75% QMK/VIA wireless mechanical keyboard with CNC aluminum body and hot-swappable switches.', 'uploads/catalog/sample-keychron.jpg', 199.00, 8, 'Active', GETUTCDATE()),
    ('PRD-2001', 'Sony WH-1000XM5 Wireless Headphones', 'Audio', 'Industry-leading noise canceling over-ear headphones with 30-hour battery life and multi-point Bluetooth pairing.', 'uploads/catalog/sample-sony.jpg', 349.99, 5, 'Active', GETUTCDATE()),
    ('PRD-3001', 'Dell UltraSharp 34 Curved Monitor', 'Monitors', '34.14-inch WQHD IPS curved USB-C hub display featuring 98% DCI-P3 color reproduction.', 'uploads/catalog/sample-dell.jpg', 729.50, 3, 'Active', GETUTCDATE()),
    ('PRD-4001', 'Samsung 990 PRO 2TB NVMe SSD', 'Components', 'PCIe 4.0 NVMe M.2 internal solid state drive achieving sequential read speeds up to 7,450 MB/s.', 'uploads/catalog/sample-samsung.jpg', 169.99, 22, 'Active', GETUTCDATE());
END
GO
