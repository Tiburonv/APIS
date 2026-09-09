using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using APIS.Models;

namespace APIS.ADO
{
    /// <summary>
    /// Consultas compartidas de Cuentas por Cobrar.
    /// Centraliza la ejecucion del SP buscarPorDocumCC1 (con su variante segun version del SP)
    /// que antes estaba duplicada en HomeController e ImagenesController.
    /// </summary>
    public static class ConsultasCuentasCobrar
    {
        /// <summary>
        /// Ejecuta buscarPorDocumCC1. Usa @incluirImagen si el SP esta actualizado; si no, cae al SP de 3 parametros.
        /// </summary>
        public static List<DocumentoCC1ViewModel> BuscarPorDocumCC1(
            A_ZULIA_12Entities contexto,
            string consulta,
            int cantidad,
            string usuario,
            bool incluirImagen)
        {
            var consultaParam = new SqlParameter("@consulta", (object)(consulta ?? string.Empty) ?? DBNull.Value);
            var cantidadParam = new SqlParameter("@cantidad", cantidad);
            var usuarioParam = new SqlParameter("@usuario", (object)(usuario ?? string.Empty) ?? DBNull.Value);

            try
            {
                var incluirParam = new SqlParameter("@incluirImagen", incluirImagen);
                return contexto.Database.SqlQuery<DocumentoCC1ViewModel>(
                    "EXEC buscarPorDocumCC1 @consulta, @cantidad, @usuario, @incluirImagen",
                    consultaParam,
                    cantidadParam,
                    usuarioParam,
                    incluirParam
                ).ToList();
            }
            catch (SqlException)
            {
                // El SP desplegado no soporta @incluirImagen: reintentar con la version de 3 parametros
                return contexto.Database.SqlQuery<DocumentoCC1ViewModel>(
                    "EXEC buscarPorDocumCC1 @consulta, @cantidad, @usuario",
                    consultaParam,
                    cantidadParam,
                    usuarioParam
                ).ToList();
            }
        }
    }
}
