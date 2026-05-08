USE [A_ZULIA_12]
GO

-- Agrega la tasa BCV del dia en que se crea la cotizacion
IF NOT EXISTS (
    SELECT 1
    FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.ZCotizacionNB')
      AND name = 'tasa'
)
BEGIN
    ALTER TABLE dbo.ZCotizacionNB
    ADD tasa DECIMAL(18, 8) NOT NULL CONSTRAINT DF_ZCotizacionNB_tasa DEFAULT (0);
END
GO
