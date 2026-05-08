USE [A_ZULIA_12]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID(N'dbo.obtenerZConfigEmpresa', N'P') IS NOT NULL
    DROP PROCEDURE dbo.obtenerZConfigEmpresa;
GO

CREATE PROCEDURE dbo.obtenerZConfigEmpresa
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP 1
        Id,
        RIF,
        Direccion,
        Telefono,
        Logo,
        LogoMimeType,
        Fe_us_mo,
        Co_us_mo
    FROM dbo.ZConfigEmpresa
    ORDER BY Id;
END
GO

IF OBJECT_ID(N'dbo.guardarZConfigEmpresa', N'P') IS NOT NULL
    DROP PROCEDURE dbo.guardarZConfigEmpresa;
GO

CREATE PROCEDURE dbo.guardarZConfigEmpresa
    @RIF           VARCHAR(20),
    @Direccion     NVARCHAR(250),
    @Telefono      VARCHAR(50),
    @Logo          VARBINARY(MAX) = NULL,
    @LogoMimeType  VARCHAR(50)  = NULL,
    @ActualizarLogo BIT         = 0,
    @Co_us_mo      VARCHAR(10)  = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM dbo.ZConfigEmpresa)
    BEGIN
        UPDATE dbo.ZConfigEmpresa
        SET RIF          = @RIF,
            Direccion    = @Direccion,
            Telefono     = @Telefono,
            Fe_us_mo     = GETDATE(),
            Co_us_mo     = @Co_us_mo,
            Logo         = CASE WHEN @ActualizarLogo = 1 THEN @Logo ELSE Logo END,
            LogoMimeType = CASE WHEN @ActualizarLogo = 1 THEN @LogoMimeType ELSE LogoMimeType END
        WHERE Id = (SELECT TOP 1 Id FROM dbo.ZConfigEmpresa ORDER BY Id);
    END
    ELSE
    BEGIN
        INSERT INTO dbo.ZConfigEmpresa (RIF, Direccion, Telefono, Logo, LogoMimeType, Co_us_mo)
        VALUES (@RIF, @Direccion, @Telefono, @Logo, @LogoMimeType, @Co_us_mo);
    END
END
GO
