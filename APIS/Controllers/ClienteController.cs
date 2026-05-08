using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Web.Mvc;
using System.Linq;
using APIS.ADO;

namespace APIS.Controllers
{
    public class ClienteController : Controller
    {
        [HttpGet]
        public ActionResult Buscar(string consulta)
        {
            try
            {
                // Usar el contexto de EF y ejecutar el SP con parámetro tipado.
                // ANTES se abria un SqlConnection con la connectionString de Entity Framework
                // (metadata=res://...) lo cual NO es válido para SqlConnection y fallaba en runtime.
                using (var db = new A_ZULIA_12Entities())
                {
                    var resultados = db.Database.SqlQuery<ClienteDTO>(
                        "EXEC buscarCliente1 @consulta",
                        new SqlParameter("@consulta", string.IsNullOrEmpty(consulta) ? "" : consulta)
                    ).ToList();

                    // Mapear a un formato específico para el JSON
                    var data = resultados.Select(x => new
                    {
                        co_cli = x.co_cli ?? string.Empty,
                        cli_des = x.cli_des ?? string.Empty,
                        co_ven = x.co_ven ?? string.Empty
                    }).ToList();

                    return Json(data, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en Buscar cliente: {ex.Message}");
                return Json(new { error = "Error al buscar clientes. Por favor, intente nuevamente." },
                    JsonRequestBehavior.AllowGet);
            }
        }
    }
}
