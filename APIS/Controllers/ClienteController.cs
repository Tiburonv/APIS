using System;
using System.Linq;
using System.Web.Mvc;
using APIS.Repositorios;

namespace APIS.Controllers
{
    public class ClienteController : Controller
    {
        private readonly IClienteRepositorio _clientes;

        public ClienteController() : this(new ClienteRepositorio())
        {
        }

        public ClienteController(IClienteRepositorio clientes)
        {
            _clientes = clientes;
        }

        [HttpGet]
        public ActionResult Buscar(string consulta)
        {
            try
            {
                var resultados = _clientes.BuscarClientes(consulta);

                // Mapear a un formato específico para el JSON
                var data = resultados.Select(x => new
                {
                    co_cli = x.co_cli ?? string.Empty,
                    cli_des = x.cli_des ?? string.Empty,
                    co_ven = x.co_ven ?? string.Empty
                }).ToList();

                return Json(data, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                APIS.Servicios.Log.Error("Error en ClienteController.Buscar", ex);
                return Json(new { error = "Error al buscar clientes. Por favor, intente nuevamente." },
                    JsonRequestBehavior.AllowGet);
            }
        }
    }
}
