using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using APIS.ADO;
using APIS.Models;

namespace APIS.Repositorios
{
    /// <summary>
    /// Implementacion de IPreciosRepositorio (Fase 2 - piloto de capa de datos).
    /// La logica SQL sale del controlador y queda centralizada aqui.
    /// </summary>
    public class PreciosRepositorio : IPreciosRepositorio
    {
        public IList<CategoriaViewModel> ListarCategorias()
        {
            using (var db = new A_ZULIA_12Entities())
            {
                return db.Database.SqlQuery<CategoriaViewModel>(
                    "SELECT RTRIM(co_cat) AS co_cat, RTRIM(cat_des) AS cat_des " +
                    "FROM saCatArticulo " +
                    "ORDER BY cat_des"
                ).ToList();
            }
        }

        public IList<PreciosViewModel> BuscarPrecios(string busqueda, string coCat)
        {
            using (var db = new A_ZULIA_12Entities())
            {
                var paramBusqueda = new SqlParameter("@busqueda", (object)busqueda ?? DBNull.Value);
                var paramCoCat = new SqlParameter("@co_cat", (object)coCat ?? DBNull.Value);

                return db.Database.SqlQuery<PreciosViewModel>(
                    "EXEC buscarPrecios @busqueda, @co_cat", paramBusqueda, paramCoCat
                ).ToList();
            }
        }
    }
}
