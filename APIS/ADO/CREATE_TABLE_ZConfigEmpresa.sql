USE [A_ZULIA_12]
GO

/****** 
Tabla de configuracion de datos de empresa para encabezados PDF
Fecha: 25/05/2026
Solo debe existir un registro activo (configuracion unica).
******/

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID(N'dbo.ZConfigEmpresa', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ZConfigEmpresa (
        Id            INT IDENTITY(1,1) PRIMARY KEY,
        RIF           VARCHAR(20)   NOT NULL,
        Direccion     NVARCHAR(250) NOT NULL,
        Telefono      VARCHAR(50)   NOT NULL,
        Logo          VARBINARY(MAX) NULL,
        LogoMimeType  VARCHAR(50)   NULL,
        Fe_us_mo      DATETIME      NOT NULL CONSTRAINT DF_ZConfigEmpresa_Fe_us_mo DEFAULT GETDATE(),
        Co_us_mo      VARCHAR(10)   NULL
    );
END
GO
