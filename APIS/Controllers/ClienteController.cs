using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.Mvc;
using System.Linq;

namespace APIS.Controllers
{
    public class ClienteController : Controller
    {
        [HttpGet]
        public ActionResult Buscar(string consulta)
        {
            var resultados = new List<Dictionary<string, string>>();
            string connectionString = ConfigurationManager.ConnectionStrings["A_ZULIA_12Entities"].ConnectionString;

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    using (SqlCommand command = new SqlCommand("buscarCliente1", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@consulta", string.IsNullOrEmpty(consulta) ? "" : consulta);

                        connection.Open();
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var fila = new Dictionary<string, string>();
                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    string nombreCampo = reader.GetName(i);
                                    string valor = reader[i] != DBNull.Value ? reader[i].ToString() : string.Empty;
                                    fila[nombreCampo] = valor;
                                }
                                resultados.Add(fila);
                            }
                        }
                    }
                }

                // Mapear a un formato específico para el JSON
                var data = resultados.Select(x => new
                {
                    co_cli = x.ContainsKey("co_cli") ? x["co_cli"] : string.Empty,
                    cli_des = x.ContainsKey("cli_des") ? x["cli_des"] : string.Empty,
                    co_ven = x.ContainsKey("co_ven") ? x["co_ven"] : string.Empty
                }).ToList();

                return Json(data, JsonRequestBehavior.AllowGet);
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
