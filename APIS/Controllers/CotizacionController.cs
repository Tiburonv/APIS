using APIS.ADO;
using APIS.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;
using System.Data.Entity.Validation;
using Newtonsoft.Json;
namespace APIS.Controllers
{
    public class CotizacionController : Controller
    {
        public ActionResult Cotizacion()
        {
            var usuario = Session["Usuario"] as string;
            if (usuario != "999" && usuario != "04" && usuario != "05")
            {
                // Redirige al inicio o muestra acceso denegado
                return RedirectToAction("Index", "Home");
                // O: return new HttpStatusCodeResult(403);
            }
            return View();
        }

        [HttpGet]
        public ActionResult buscarCliente1(string consulta)
        {
            try
            {
                using (var db = new A_ZULIA_12Entities())
                {
                    var resultados = db.Database.SqlQuery<ClienteDTO>(
                        "EXEC buscarCliente1 @consulta",
                        new SqlParameter("@consulta", consulta ?? "")
                    ).ToList();

                    // Para depuración
                    foreach (var item in resultados)
                    {
                        System.Diagnostics.Debug.WriteLine($"co_cli: {item.co_cli}, tip_cli: {item.tip_cli}");
                    }

                    return new JsonResult
                    {
                        Data = resultados,
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                        MaxJsonLength = int.MaxValue
                    };
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en buscarCliente1: {ex.Message}");
                return new JsonResult
                {
                    Data = new { error = "Error al buscar clientes." },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
            }
        }

        [HttpGet]
        public ActionResult ObtenerTransportes()
        {
            try
            {
                var transportes = new List<TransporteViewModel>();
                using (var db = new A_ZULIA_12Entities())
                {
                    transportes = db.Database.SqlQuery<TransporteViewModel>("EXEC buscarTransporte1").ToList();
                }
                return Json(transportes, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en ObtenerTransportes: {ex.Message}");
                return new JsonResult
                {
                    Data = new { error = "Error al obtener los transportes." },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
            }
        }

        [HttpGet]
        public JsonResult ObtenerTasaDia()
        {
            try
            {
                using (var db = new A_ZULIA_12Entities())
                {
                    var tasa = db.Database.SqlQuery<decimal?>(
                        @"SELECT TOP 1 t.tasa_v
                          FROM satasa t
                          WHERE RTRIM(t.co_mone) = 'USD'
                            AND CAST(t.fecha AS DATE) <= CAST(GETDATE() AS DATE)
                          ORDER BY CAST(t.fecha AS DATE) DESC, t.fecha DESC"
                    ).FirstOrDefault();

                    return Json(new { tasa = tasa ?? 0m }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en ObtenerTasaDia: {ex.ToString()}");
                return Json(new { tasa = 0m, error = "Error al obtener la tasa del día." }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public JsonResult Buscar(string busqueda)
        {
            var resultados = new List<PreciosViewModel>();

            if (string.IsNullOrWhiteSpace(busqueda))
                return Json(resultados, JsonRequestBehavior.AllowGet);

            using (var db = new A_ZULIA_12Entities())
            {
                var paramBusqueda = new SqlParameter("@busqueda", busqueda);
                var paramCoCat = new SqlParameter("@co_cat", DBNull.Value);
                resultados = db.Database.SqlQuery<PreciosViewModel>(
                    "EXEC buscarPrecios @busqueda, @co_cat", paramBusqueda, paramCoCat
                ).ToList();
            }

            var data = resultados.Select(x => new
            {
                co_art = x.co_art,
                art_des = x.art_des,
                tipo_imp = x.tipo_imp,
                iva = x.iva,
                precio1 = x.Precio1,
                precio2 = x.Precio2,
                precio3 = x.Precio3,
                precio4 = x.Precio4,
                precio5 = x.Precio5,
                tasa = x.tasa
            });

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        private decimal ObtenerTasaDelDia(A_ZULIA_12Entities db)
        {
            var tasa = db.Database.SqlQuery<decimal?>(
                @"SELECT TOP 1 t.tasa_v
                  FROM satasa t
                  WHERE RTRIM(t.co_mone) = 'USD'
                    AND CAST(t.fecha AS DATE) <= CAST(GETDATE() AS DATE)
                  ORDER BY CAST(t.fecha AS DATE) DESC, t.fecha DESC"
            ).FirstOrDefault();

            return tasa ?? 0m;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult GuardarCotizacion(ZCotizacionNBViewModel model)
        {
            using (var db = new A_ZULIA_12Entities())
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    // Validar que los campos requeridos no sean nulos
                    if (model == null || model.renglones == null || !model.renglones.Any())
                    {
                        return Json(new { success = false, error = "No se han proporcionado datos válidos para la cotización" });
                    }

                    // Obtener el último doc_num existente
                    var ultimo = db.ZCotizacionNB
                        .OrderByDescending(c => c.doc_num)
                        .Select(c => c.doc_num)
                        .FirstOrDefault();

                    // Si no hay registros, empieza en 9000000001
                    long nuevoNum = 9000000001;
                    if (!string.IsNullOrEmpty(ultimo) && long.TryParse(ultimo, out long num))
                    {
                        // Si el último número es menor a 9000000001, empezar desde 9000000001
                        if (num < 9000000001)
                        {
                            nuevoNum = 9000000001;
                        }
                        else
                        {
                            nuevoNum = num + 1;
                        }
                    }

                    // Formatea el número con ceros a la izquierda (10 dígitos)
                    string docNumGenerado = nuevoNum.ToString("D10");

                    var tasaCotizacion = model.tasa > 0 ? model.tasa : ObtenerTasaDelDia(db);

                    // Crear la cotización
                    var cotizacion = new ZCotizacionNB
                    {
                        doc_num = docNumGenerado.PadRight(20), // Asegurar longitud fija
                        co_cli = (model.co_cli ?? string.Empty).PadRight(16), // Asegurar longitud fija
                        tip_cli = (model.tip_cli ?? string.Empty).PadRight(6), // Asegurar longitud fija
                        co_tran = (model.co_tran ?? string.Empty).PadRight(6), // Asegurar longitud fija
                        fec_emis = DateTime.Now,
                        total_bruto = model.total_bruto,
                        monto_imp = model.monto_imp,
                        total_neto = model.total_neto,
                        tasa = tasaCotizacion
                    };

                    db.ZCotizacionNB.Add(cotizacion);

                    // Guardar los renglones de la cotización
                    int rengNum = 1;
                    foreach (var renglon in model.renglones)
                    {
                        if (string.IsNullOrEmpty(renglon.co_art))
                            continue;

                        decimal montoImp = renglon.tipo_imp == "1"
                            ? renglon.total_art * renglon.prec_vta * 0.16M
                            : 0;

                        var detalle = new ZCotizacionRengNB
                        {
                            reng_num = rengNum++,
                            doc_num = docNumGenerado.PadRight(20), // Asegurar longitud fija
                            co_art = renglon.co_art.PadRight(30), // Asegurar longitud fija
                            total_art = renglon.total_art,
                            prec_vta = renglon.prec_vta,
                            tipo_imp = int.TryParse(renglon.tipo_imp, out int tipoImp) ? tipoImp : 0,
                            reng_neto = renglon.reng_neto,
                            monto_imp = montoImp,
                            
                        };
                        db.ZCotizacionRengNB.Add(detalle);
                    }

                    // Guardar todos los cambios en una sola transacción
                    db.SaveChanges();
                    transaction.Commit();

                    return Json(new { success = true, doc_num = docNumGenerado, tasa = tasaCotizacion });
                }
                catch (DbEntityValidationException ex)
                {
                    var errores = ex.EntityValidationErrors
                        .SelectMany(e => e.ValidationErrors)
                        .Select(e => e.PropertyName + ": " + e.ErrorMessage)
                        .ToList();
                    return Json(new { success = false, error = string.Join("; ", errores) });
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error en GuardarCotizacion: {ex.ToString()}");

                    // En modo Debug se expone el detalle tecnico para desarrollo; en produccion mensaje generico
                    if (HttpContext != null && HttpContext.IsDebuggingEnabled)
                    {
                        string inner = "";
                        Exception innerEx = ex;
                        while (innerEx.InnerException != null)
                        {
                            innerEx = innerEx.InnerException;
                            inner += innerEx.Message + " ";
                        }
                        return Json(new { success = false, error = ex.Message + " " + inner });
                    }

                    return Json(new { success = false, error = "Error al guardar la cotización. Intente nuevamente." });
                }
            }
        }
            

        public ActionResult Vercotizacion(DateTime? fechaInicio = null, DateTime? fechaFin = null)
        {
            using (var db = new A_ZULIA_12Entities())
            {
                IQueryable<ZCotizacionNB> query = db.ZCotizacionNB;
                
                // Aplicar filtro de fechas si se proporcionan
                if (fechaInicio.HasValue)
                {
                    query = query.Where(c => c.fec_emis >= fechaInicio.Value);
                }
                
                if (fechaFin.HasValue)
                {
                    // Añadir un día para incluir todo el día de la fecha final
                    var fechaFinInclusive = fechaFin.Value.AddDays(1);
                    query = query.Where(c => c.fec_emis < fechaFinInclusive);
                }
                
                var cotizaciones = query.OrderByDescending(c => c.fec_emis).ToList();
                var renglones = db.ZCotizacionRengNB.ToList();
                
                // Obtener códigos de clientes únicos
                var codigosClientes = cotizaciones.Select(c => c.co_cli).Distinct().ToList();
                var clientes = new Dictionary<string, string>();

                // Obtener descripciones de clientes
                foreach (var codigo in codigosClientes)
                {
                    try
                    {
                        var resultado = db.Database.SqlQuery<ClienteDTO>(
                            "EXEC buscarCliente1 @consulta",
                            new SqlParameter("@consulta", codigo)
                        ).FirstOrDefault();

                        if (resultado != null)
                        {
                            clientes[resultado.co_cli] = resultado.cli_des;
                        }
                    }
                    catch (Exception ex)
                    {
                        // En caso de error, guardar el código como descripción
                        clientes[codigo] = codigo;
                        System.Diagnostics.Debug.WriteLine($"Error al obtener cliente {codigo}: {ex.Message}");
                    }
                }

                var model = new APIS.Models.VercotizacionViewModel
                {
                    Cotizaciones = cotizaciones,
                    Renglones = renglones,
                    Clientes = clientes,
                    FechaInicio = fechaInicio,
                    FechaFin = fechaFin
                };
                
                ViewBag.FechaInicio = fechaInicio?.ToString("yyyy-MM-dd");
                ViewBag.FechaFin = fechaFin?.ToString("yyyy-MM-dd");
                
                return View(model);
            }
        }

        [HttpGet]
        public ActionResult DetalleCotizacion(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return new HttpStatusCodeResult(400, "ID de cotización no proporcionado");
            }

            // Registrar el ID recibido para depuración
            System.Diagnostics.Debug.WriteLine($"ID de cotización recibido: '{id}'");
            System.Diagnostics.Debug.WriteLine($"Longitud del ID: {id.Length}");

            using (var db = new A_ZULIA_12Entities())
            {
                // Obtener la cotización (más tolerante con espacios)
                var idLimpio = id.Trim();
                System.Diagnostics.Debug.WriteLine($"Buscando cotización con ID limpio: '{idLimpio}'");
                
                // Intentar con diferentes formatos si es necesario
                var cotizacion = db.ZCotizacionNB.FirstOrDefault(c => c.doc_num.Trim() == idLimpio);
                
                if (cotizacion == null)
                {
                    // Intentar sin ceros a la izquierda
                    var idSinCeros = idLimpio.TrimStart('0');
                    if (idSinCeros != idLimpio)
                    {
                        cotizacion = db.ZCotizacionNB.FirstOrDefault(c => c.doc_num.Trim() == idSinCeros);
                        System.Diagnostics.Debug.WriteLine($"Buscando sin ceros a la izquierda: '{idSinCeros}'. Encontrado: {cotizacion != null}");
                    }
                    
                    if (cotizacion == null)
                    {
                        System.Diagnostics.Debug.WriteLine($"Cotización no encontrada para ID: '{id}'", "Error");
                        return HttpNotFound("Cotización no encontrada");
                    }
                }

                // Obtener los renglones de la cotización
                var renglones = db.ZCotizacionRengNB
                    .Where(r => r.doc_num.Trim() == id.Trim())
                    .ToList();

                // Obtener las descripciones de los artículos
                var descripcionesArticulos = new Dictionary<string, string>();
                foreach (var renglon in renglones)
                {
                    if (!string.IsNullOrEmpty(renglon.co_art) && !descripcionesArticulos.ContainsKey(renglon.co_art.Trim()))
                    {
                        try
                        {
                            var resultado = db.Database.SqlQuery<PreciosViewModel>(
                                "EXEC buscarPrecios @busqueda, @co_cat",
                                new SqlParameter("@busqueda", renglon.co_art.Trim()),
                                new SqlParameter("@co_cat", DBNull.Value)
                            ).FirstOrDefault();

                            if (resultado != null && !string.IsNullOrEmpty(resultado.art_des))
                            {
                                descripcionesArticulos[renglon.co_art.Trim()] = resultado.art_des;
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error al obtener descripción para artículo {renglon.co_art}: {ex.Message}");
                            descripcionesArticulos[renglon.co_art.Trim()] = renglon.co_art.Trim();
                        }
                    }
                }

                // Obtener la descripción del cliente
                string descripcionCliente = string.Empty;
                try
                {
                    var cliente = db.Database.SqlQuery<ClienteDTO>(
                        "EXEC buscarCliente1 @consulta",
                        new SqlParameter("@consulta", cotizacion.co_cli)
                    ).FirstOrDefault();

                    if (cliente != null)
                    {
                        descripcionCliente = cliente.cli_des;
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error al obtener cliente: {ex.Message}");
                }

                // Asignar las descripciones a los renglones
                foreach (var renglon in renglones)
                {
                    if (!string.IsNullOrEmpty(renglon.co_art) && descripcionesArticulos.ContainsKey(renglon.co_art.Trim()))
                    {
                        // Usar reflexión para asignar la descripción si la propiedad existe
                        var property = renglon.GetType().GetProperty("art_des");
                        if (property != null && property.CanWrite)
                        {
                            property.SetValue(renglon, descripcionesArticulos[renglon.co_art.Trim()]);
                        }
                    }
                }

                // Crear el modelo de vista
                var model = new DetalleCotizacionViewModel
                {
                    Cotizacion = cotizacion,
                    Renglones = renglones,
                    DescripcionCliente = descripcionCliente,
                    DescripcionesArticulos = descripcionesArticulos
                };

                return View(model);
            }
        }
    }
}

// Modelo para mapear los resultados del SP buscarTransporte1
public class TransporteViewModel
{
    public string co_tran { get; set; }
    public string des_tran { get; set; }
}

//// Modelo para mapear los resultados del SP buscarArticulo1
//public class ArticuloViewModel
//{
//    public string co_art { get; set; }
//    public string art_des { get; set; }
//    public string iva { get; set; }
//}

public class ZCotizacionNBViewModel
{
    public string doc_num { get; set; }
    public string co_cli { get; set; }
    public string tip_cli { get; set; }
    public string co_tran { get; set; }
    public DateTime fec_emis { get; set; }
    public decimal total_bruto { get; set; }
    public decimal monto_imp { get; set; }
    public decimal total_neto { get; set; }
    public decimal tasa { get; set; }
    public List<ZCotizacionRengNBViewModel> renglones { get; set; }
}

public class ZCotizacionRengNBViewModel
{
    public int reng_num { get; set; }
    public string co_art { get; set; }
    public decimal total_art { get; set; }
    public decimal prec_vta { get; set; }
    public string tipo_imp { get; set; }
    public decimal reng_neto { get; set; }
}

public class VercotizacionViewModel
{
    public List<ZCotizacionNB> Cotizaciones { get; set; }
    public List<ZCotizacionRengNB> Renglones { get; set; }
}