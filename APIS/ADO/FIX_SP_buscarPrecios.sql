USE [A_ZULIA_12]
GO

/****** 
Script para corregir el SP buscarPrecios
Fecha: 26/05/2026
Problemas corregidos:
  1. Precedencia del WHERE: "AND a.anulado = 0 OR a.co_cat = @co_cat"
     devolvia articulos anulados si coincidia la categoria.
  2. @busqueda vacio con LIKE '%%' devolvia todos los articulos.
  3. Si llegan texto y categoria, PRIORIZA el texto e IGNORA la categoria.
  4. Comparacion de categoria con RTRIM por espacios en campos CHAR.
  5. Columna tasa: tasa USD del dia desde tabla satasa (o la mas reciente).
******/

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE [dbo].[buscarPrecios]
    @busqueda VARCHAR(100) = NULL,
    @co_cat   VARCHAR(10)  = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @busquedaLimpia VARCHAR(100) = NULLIF(LTRIM(RTRIM(@busqueda)), '');
    DECLARE @coCatLimpia     VARCHAR(10)  = NULLIF(LTRIM(RTRIM(@co_cat)), '');

    -- Tasa USD del dia (o la mas reciente anterior si no hay registro hoy)
    DECLARE @tasaDia DECIMAL(18, 8) = (
        SELECT TOP 1 t.tasa_v
        FROM satasa t
        WHERE RTRIM(t.co_mone) = 'USD'
          AND CAST(t.fecha AS DATE) <= CAST(GETDATE() AS DATE)
        ORDER BY CAST(t.fecha AS DATE) DESC, t.fecha DESC
    );

    -- Evitar devolver todo el catalogo sin filtros
    IF @busquedaLimpia IS NULL AND @coCatLimpia IS NULL
        RETURN;

    WITH UltimosPrecios AS (
        SELECT
            b.co_art,
            b.co_precio,
            b.monto,
            b.fe_us_mo,
            ROW_NUMBER() OVER (
                PARTITION BY b.co_art, b.co_precio
                ORDER BY b.fe_us_mo DESC
            ) AS rn
        FROM saArtPrecio b
    )
    SELECT
        a.co_art,
        a.art_des,
        a.co_cat,
        b.cat_des,
        a.tipo_imp,
        IIF(a.tipo_imp = '1', '16%', '') AS iva,
        MAX(IIF(up.co_precio = '01', up.monto, 0)) AS Precio1,
        MAX(IIF(up.co_precio = '02', up.monto, 0)) AS Precio2,
        MAX(IIF(up.co_precio = '03', up.monto, 0)) AS Precio3,
        MAX(IIF(up.co_precio = '04', up.monto, 0)) AS Precio4,
        MAX(IIF(up.co_precio = '05', up.monto, 0)) AS Precio5,
        ISNULL(@tasaDia, 0) AS tasa
    FROM saArticulo a
    LEFT JOIN (
        SELECT * FROM UltimosPrecios WHERE rn = 1
    ) up ON a.co_art = up.co_art
    INNER JOIN saCatArticulo AS b ON a.co_cat = b.co_cat
    WHERE a.anulado = 0
      AND (
            -- Hay texto: buscar por codigo o descripcion (ignora categoria)
            (
                @busquedaLimpia IS NOT NULL
                AND (
                    RTRIM(a.co_art) = @busquedaLimpia
                    OR a.art_des LIKE '%' + @busquedaLimpia + '%'
                )
            )
            OR
            -- Sin texto: filtrar solo por categoria
            (
                @busquedaLimpia IS NULL
                AND @coCatLimpia IS NOT NULL
                AND RTRIM(a.co_cat) = @coCatLimpia
            )
          )
    GROUP BY
        a.co_art,
        a.art_des,
        a.co_cat,
        b.cat_des,
        a.tipo_imp;
END
GO
