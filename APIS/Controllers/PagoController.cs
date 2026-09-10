using APIS.ADO;
using APIS.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;
using System.Data.Entity;
using Newtonsoft.Json;

public class PagoController : Controller
{
    protected override void OnActionExecuting(ActionExecutingContext filterContext)
    {
        var usuario = Session["Usuario"] as string;
        if (usuario == "999")
        {
            base.OnActionExecuting(filterContext);
            return;
        }
        if (usuario != "04" && usuario != "01" && usuario != "03")
        {
            filterContext.Result = RedirectToAction("Login", "Account");
            return;
        }
        base.OnActionExecuting(filterContext);
    }

    public ActionResult Index()
    {
        return View();
    }

    public ActionResult Pagos()
    {
        return View();
    }

    [HttpGet]
    public ActionResult DetallePago(string cob_num)
    {
        try
        {
            if (string.IsNullOrEmpty(cob_num))
            {
                return RedirectToAction("Error", new { message = "Número de pago no especificado." });
            }

            // Limpiar espacios en blanco del parámetro cob_num
            cob_num = cob_num?.Trim();

            using (var contexto = new A_ZULIA_12Entities())
            {
                List<buscarRengPGO_Result> renglones = new List<buscarRengPGO_Result>();
                
                try
                {
                    // Usar SqlDataReader para obtener los datos directamente
                    var connection = contexto.Database.Connection;
                    if (connection.State != System.Data.ConnectionState.Open)
                    {
                        connection.Open();
                    }
                    
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "EXEC buscarRengPGO @consulta";
                        command.Parameters.Add(new SqlParameter("@consulta", cob_num));
                        
                        using (var reader = command.ExecuteReader())
                        {
                            renglones = new List<buscarRengPGO_Result>();
                            
                            while (reader.Read())
                            {
                                var renglon = new buscarRengPGO_Result
                                {
                                    cob_num = reader["cob_num"]?.ToString(),
                                    co_prov = reader["co_prov"]?.ToString(),
                                    prov_des = reader["prov_des"]?.ToString(),
                                    Fecha = Convert.ToDateTime(reader["Fecha"]),
                                    Dias = reader["Dias"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["Dias"]),
                                    anulado = Convert.ToBoolean(reader["anulado"]),
                                    co_tipo_doc = reader["co_tipo_doc"]?.ToString(),
                                    nro_doc = reader["nro_doc"]?.ToString(),
                                    Total = reader["Total"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(reader["Total"]),
                                    forma_pag = reader["forma_pag"]?.ToString(),
                                    cod_cta = reader["cod_cta"]?.ToString(),
                                    num_doc = reader["num_doc"]?.ToString(),
                                    tasa = reader["tasa"] == DBNull.Value ? 0m : Convert.ToDecimal(reader["tasa"])
                                };
                                renglones.Add(renglon);
                                
                            }
                        }
                    }
                    
                }
                catch (Exception spEx)
                {
                    System.Diagnostics.Debug.WriteLine($"Error en stored procedure: {spEx.Message}");
                    System.Diagnostics.Debug.WriteLine($"StackTrace: {spEx.StackTrace}");
                    throw new Exception($"Error al ejecutar stored procedure buscarRengPGO: {spEx.Message}", spEx);
                }

                // Si no se encuentran renglones, avisar (antes se devolvian datos de PRUEBA "TEST" como si fueran reales)
                if (renglones == null || renglones.Count == 0)
                {
                    return RedirectToAction("Error", new { message = "No se encontraron renglones para el pago especificado." });
                }

                // Obtener los datos del pago principal del primer renglón
                var primerRenglon = renglones.First();

                // Crear el ViewModel con datos reales del stored procedure
                var viewModel = new DetallePagoViewModel
                {
                    cob_num = primerRenglon.cob_num,
                    co_prov = primerRenglon.co_prov,
                    prov_des = primerRenglon.prov_des,
                    Fecha = primerRenglon.Fecha,
                    Dias = primerRenglon.Dias,
                    anulado = primerRenglon.anulado,
                    tasa = primerRenglon.tasa,
                    Renglones = renglones.Select(r => new DetallePagoRenglonViewModel
                    {
                        cob_num = r.cob_num,
                        co_prov = r.co_prov,
                        prov_des = r.prov_des,
                        Fecha = r.Fecha,
                        Dias = r.Dias,
                        anulado = r.anulado,
                        co_tipo_doc = r.co_tipo_doc,
                        nro_doc = r.nro_doc,
                        Total = r.Total ?? 0,
                        forma_pag = r.forma_pag ?? "EFECTIVO",
                        cod_cta = r.cod_cta ?? "",
                        num_doc = r.num_doc ?? "",
                        tasa = r.tasa
                    }).ToList()
                };

                return View(viewModel);
            }
        }
        catch (Exception ex)
        {
            // Log del error para debugging
            System.Diagnostics.Debug.WriteLine($"Error en DetallePago: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"StackTrace: {ex.StackTrace}");
            
            return RedirectToAction("Error", new { message = "Error al consultar el detalle del pago. Intente nuevamente." });
        }
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public JsonResult ValidarPago()
    {
        try
        {
            // Leer el body de la petición
            string elementosJson;
            using (var reader = new System.IO.StreamReader(Request.InputStream))
            {
                elementosJson = reader.ReadToEnd();
            }

            if (string.IsNullOrEmpty(elementosJson))
            {
                return Json(new { success = false, message = "No hay elementos para validar." });
            }

            // Deserializar elementos
            var elementos = JsonConvert.DeserializeObject<List<ElementoPagoViewModel>>(elementosJson);
            
            if (elementos == null || elementos.Count == 0)
            {
                return Json(new { success = false, message = "No hay elementos válidos para procesar." });
            }

            // Obtener información del usuario actual
            var usuario = Session["Usuario"] as string ?? "Usuario";
            
            // Calcular totales
            var totalPago = elementos.Sum(e => e.Saldo);
            var codigoProveedor = elementos.First().CodigoProveedor;
            var nombreProveedor = elementos.First().NombreProveedor;

            // Guardar en base de datos usando la nueva estructura
            using (var contexto = new A_ZULIA_12Entities())
            {
                var connection = contexto.Database.Connection;
                if (connection.State != System.Data.ConnectionState.Open)
                {
                    connection.Open();
                }

                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Insertar un registro por cada elemento del pago
                        for (int i = 0; i < elementos.Count; i++)
                        {
                            var elemento = elementos[i];
                            
                            using (var command = connection.CreateCommand())
                            {
                                command.Transaction = transaction;
                                command.CommandText = @"
                                    INSERT INTO ZPagosValidados 
                                    (FechaValidacion, CodigoProveedor, NombreProveedor, NroProv, TipoDoc, 
                                     Monto, SaldoInicial, SaldoActual, TotalPago, UsuarioValidacion, 
                                     FechaCreacion, Activo, NotificacionEnviada, OrdenElemento)
                                    VALUES 
                                    (@FechaValidacion, @CodigoProveedor, @NombreProveedor, @NroProv, @TipoDoc,
                                     @Monto, @SaldoInicial, @SaldoActual, @TotalPago, @UsuarioValidacion,
                                     @FechaCreacion, @Activo, @NotificacionEnviada, @OrdenElemento)";

                                command.Parameters.Add(new SqlParameter("@FechaValidacion", DateTime.Now));
                                command.Parameters.Add(new SqlParameter("@CodigoProveedor", codigoProveedor));
                                command.Parameters.Add(new SqlParameter("@NombreProveedor", nombreProveedor));
                                command.Parameters.Add(new SqlParameter("@NroProv", elemento.NroProv));
                                command.Parameters.Add(new SqlParameter("@TipoDoc", elemento.TipoDoc));
                                command.Parameters.Add(new SqlParameter("@Monto", elemento.Saldo));
                                command.Parameters.Add(new SqlParameter("@SaldoInicial", elemento.Saldo)); // Saldo inicial = monto del elemento
                                command.Parameters.Add(new SqlParameter("@SaldoActual", elemento.Saldo)); // Saldo actual = monto del elemento (se actualizará después)
                                command.Parameters.Add(new SqlParameter("@TotalPago", totalPago));
                                command.Parameters.Add(new SqlParameter("@UsuarioValidacion", usuario));
                                command.Parameters.Add(new SqlParameter("@FechaCreacion", DateTime.Now));
                                command.Parameters.Add(new SqlParameter("@Activo", true));
                                command.Parameters.Add(new SqlParameter("@NotificacionEnviada", false));
                                command.Parameters.Add(new SqlParameter("@OrdenElemento", i + 1));

                                command.ExecuteNonQuery();
                            }
                        }

                        transaction.Commit();
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }

            // Obtener los IDs de los pagos recién insertados para marcar como notificado
            if (usuario != "01")
            {
                using (var contexto2 = new A_ZULIA_12Entities())
                {
                    var connection2 = contexto2.Database.Connection;
                    if (connection2.State != System.Data.ConnectionState.Open)
                    {
                        connection2.Open();
                    }

                    using (var command2 = connection2.CreateCommand())
                    {
                        // Obtener los IDs de los registros recién insertados
                        command2.CommandText = @"
                            SELECT TOP (@CantidadElementos) Id 
                            FROM ZPagosValidados 
                            WHERE CodigoProveedor = @CodigoProveedor 
                              AND UsuarioValidacion = @UsuarioValidacion
                              AND FechaCreacion >= DATEADD(MINUTE, -1, GETDATE())
                            ORDER BY Id DESC";

                        command2.Parameters.Add(new SqlParameter("@CantidadElementos", elementos.Count));
                        command2.Parameters.Add(new SqlParameter("@CodigoProveedor", codigoProveedor));
                        command2.Parameters.Add(new SqlParameter("@UsuarioValidacion", usuario));

                        using (var reader = command2.ExecuteReader())
                        {
                            var pagoIds = new List<int>();
                            while (reader.Read())
                            {
                                pagoIds.Add(Convert.ToInt32(reader["Id"]));
                            }

                            // TODO: Marcar como notificado al usuario "01" para cada pago
                            // NotificacionController no existe aún - Descomentar cuando se implemente
                            //foreach (var pagoId in pagoIds)
                            //{
                            //    APIS.Controllers.NotificacionController.MarcarPagoComoNotificado(pagoId, "01");
                            //}
                        }
                    }
                }
            }

            return Json(new { 
                success = true, 
                message = "Pago validado exitosamente.",
                totalPago = totalPago.ToString("N2"),
                cantidadElementos = elementos.Count
            });
        }
        catch (Exception ex)
        {
            APIS.Servicios.Log.Error("Error al validar pago", ex);
            return Json(new { success = false, message = "Error al validar el pago. Intente nuevamente." });
        }
    }

    [HttpGet]
    public ActionResult ResumenPagos()
    {
        try
        {
            var pagosAgrupados = new Dictionary<string, List<PagoValidadoViewModel>>();

            using (var contexto = new A_ZULIA_12Entities())
            {
                var connection = contexto.Database.Connection;
                if (connection.State != System.Data.ConnectionState.Open)
                {
                    connection.Open();
                }

                using (var command = connection.CreateCommand())
                {
                    // Obtener todos los registros individuales para poder mostrar los elementos
                    command.CommandText = @"
                        SELECT 
                            Id,
                            FechaValidacion,
                            CodigoProveedor,
                            NombreProveedor,
                            NroProv,
                            TipoDoc,
                            Monto,
                            SaldoInicial,
                            SaldoActual,
                            TotalPago,
                            UsuarioValidacion,
                            OrdenElemento
                        FROM ZPagosValidados 
                        WHERE Activo = 1 
                        ORDER BY FechaValidacion DESC, CodigoProveedor, OrdenElemento";

                    using (var reader = command.ExecuteReader())
                    {
                        var pagosTemporales = new Dictionary<string, PagoValidadoViewModel>();
                        
                        while (reader.Read())
                        {
                            var codigoUsuario = reader["UsuarioValidacion"].ToString();
                            var nombreUsuario = ObtenerNombreUsuario(codigoUsuario, contexto);
                            
                            var fechaValidacion = Convert.ToDateTime(reader["FechaValidacion"]);
                            var codigoProveedor = reader["CodigoProveedor"].ToString();
                            var usuarioValidacion = reader["UsuarioValidacion"].ToString();
                            
                            // Crear una clave única para agrupar pagos
                            var claveGrupo = $"{fechaValidacion:yyyy-MM-dd}_{codigoProveedor}_{usuarioValidacion}";
                            
                            if (!pagosTemporales.ContainsKey(claveGrupo))
                            {
                                pagosTemporales[claveGrupo] = new PagoValidadoViewModel
                                {
                                    Id = reader["Id"] == DBNull.Value ? 0 : Convert.ToInt32(reader["Id"]),
                                    FechaValidacion = fechaValidacion,
                                    CodigoProveedor = codigoProveedor,
                                    NombreProveedor = reader["NombreProveedor"].ToString(),
                                    TotalPago = reader["TotalPago"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["TotalPago"]),
                                    UsuarioValidacion = nombreUsuario,
                                    Elementos = new List<ElementoPagoViewModel>()
                                };
                            }
                            
                            // Agregar el elemento individual
                            var elemento = new ElementoPagoViewModel
                            {
                                CodigoProveedor = codigoProveedor,
                                NombreProveedor = reader["NombreProveedor"].ToString(),
                                NroProv = reader["NroProv"].ToString(),
                                TipoDoc = reader["TipoDoc"].ToString(),
                                Saldo = reader["Monto"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["Monto"]),
                                Fecha = fechaValidacion
                            };
                            
                            pagosTemporales[claveGrupo].Elementos.Add(elemento);
                        }
                        
                        // Agrupar por fecha
                        foreach (var pago in pagosTemporales.Values)
                        {
                            pago.CantidadElementos = pago.Elementos.Count;
                            
                            var fechaKey = pago.FechaSolo;
                            if (!pagosAgrupados.ContainsKey(fechaKey))
                            {
                                pagosAgrupados[fechaKey] = new List<PagoValidadoViewModel>();
                            }
                            pagosAgrupados[fechaKey].Add(pago);
                        }
                    }
                }
            }

            // Calcular estadísticas para evitar expresiones lambda complejas en la vista
            ViewBag.TotalDias = pagosAgrupados.Count;
            ViewBag.TotalPagos = pagosAgrupados.Values.Sum(pagos => pagos.Count);
            
            // Calcular totales usando bucles explícitos para mejor rendimiento
            decimal montoTotal = 0;
            int totalElementos = 0;
            
            foreach (var grupo in pagosAgrupados.Values)
            {
                foreach (var pago in grupo)
                {
                    montoTotal += pago.TotalPago;
                    totalElementos += pago.CantidadElementos;
                }
            }
            
            ViewBag.MontoTotal = montoTotal;
            ViewBag.TotalElementos = totalElementos;

            return View(pagosAgrupados);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error al cargar resumen de pagos: {ex.Message}");
            return RedirectToAction("Error", new { message = "Error al cargar el resumen de pagos. Intente nuevamente." });
        }
    }

    [HttpGet]
    public ActionResult DetallePagoValidado(int id)
    {
        try
        {
            PagoValidadoViewModel pago = null;

            using (var contexto = new A_ZULIA_12Entities())
            {
                var connection = contexto.Database.Connection;
                if (connection.State != System.Data.ConnectionState.Open)
                {
                    connection.Open();
                }

                using (var command = connection.CreateCommand())
                {
                    // Obtener el primer registro del grupo para los datos principales
                    command.CommandText = @"
                        SELECT TOP 1 
                            Id, FechaValidacion, CodigoProveedor, NombreProveedor, 
                            UsuarioValidacion, TotalPago
                        FROM ZPagosValidados 
                        WHERE Id = @Id AND Activo = 1";

                    command.Parameters.Add(new SqlParameter("@Id", id));

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var codigoUsuario = reader["UsuarioValidacion"].ToString();
                            var nombreUsuario = ObtenerNombreUsuario(codigoUsuario, contexto);

                            pago = new PagoValidadoViewModel
                            {
                                Id = reader["Id"] == DBNull.Value ? 0 : Convert.ToInt32(reader["Id"]),
                                FechaValidacion = Convert.ToDateTime(reader["FechaValidacion"]),
                                CodigoProveedor = reader["CodigoProveedor"].ToString(),
                                NombreProveedor = reader["NombreProveedor"].ToString(),
                                TotalPago = reader["TotalPago"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["TotalPago"]),
                                UsuarioValidacion = nombreUsuario,
                                Elementos = new List<ElementoPagoViewModel>()
                            };
                        }
                    }
                }

                // Si se encontró el pago, obtener todos los elementos del grupo
                if (pago != null)
                {
                    using (var command2 = connection.CreateCommand())
                    {
                        command2.CommandText = @"
                            SELECT NroProv, TipoDoc, Monto, SaldoInicial, SaldoActual, OrdenElemento
                            FROM ZPagosValidados 
                            WHERE CodigoProveedor = @CodigoProveedor 
                              AND FechaValidacion = @FechaValidacion 
                              AND UsuarioValidacion = @UsuarioValidacion
                              AND Activo = 1
                            ORDER BY OrdenElemento";

                        command2.Parameters.Add(new SqlParameter("@CodigoProveedor", pago.CodigoProveedor));
                        command2.Parameters.Add(new SqlParameter("@FechaValidacion", pago.FechaValidacion));
                        command2.Parameters.Add(new SqlParameter("@UsuarioValidacion", Session["Usuario"] as string ?? "Usuario"));

                        using (var reader2 = command2.ExecuteReader())
                        {
                            while (reader2.Read())
                            {
                                var elemento = new ElementoPagoViewModel
                                {
                                    CodigoProveedor = pago.CodigoProveedor,
                                    NombreProveedor = pago.NombreProveedor,
                                    NroProv = reader2["NroProv"].ToString(),
                                    TipoDoc = reader2["TipoDoc"].ToString(),
                                    Saldo = reader2["Monto"] == DBNull.Value ? 0 : Convert.ToDecimal(reader2["Monto"]),
                                    Fecha = pago.FechaValidacion
                                };
                                pago.Elementos.Add(elemento);
                            }
                        }
                    }
                    pago.CantidadElementos = pago.Elementos.Count;
                }
            }

            if (pago == null)
            {
                return RedirectToAction("Error", new { message = "Pago no encontrado." });
            }

            return View(pago);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error al cargar detalle de pago: {ex.Message}");
            return RedirectToAction("Error", new { message = "Error al cargar el detalle del pago. Intente nuevamente." });
        }
    }

    [HttpGet]
    public ActionResult Error(string message = null)
    {
        ViewBag.ErrorMessage = message ?? "Ha ocurrido un error inesperado.";
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public JsonResult EliminarPago()
    {
        try
        {
            // Leer el body de la petición
            string requestBody;
            using (var reader = new System.IO.StreamReader(Request.InputStream))
            {
                requestBody = reader.ReadToEnd();
            }

            if (string.IsNullOrEmpty(requestBody))
            {
                return Json(new { success = false, message = "No se especificó el ID del pago a eliminar." });
            }

            // Deserializar el ID
            var requestData = JsonConvert.DeserializeObject<dynamic>(requestBody);
            int pagoId = requestData.id;

            if (pagoId <= 0)
            {
                return Json(new { success = false, message = "ID de pago inválido." });
            }

            // Eliminar de la base de datos (marcar como inactivo)
            using (var contexto = new A_ZULIA_12Entities())
            {
                var connection = contexto.Database.Connection;
                if (connection.State != System.Data.ConnectionState.Open)
                {
                    connection.Open();
                }

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        UPDATE ZPagosValidados 
                        SET Activo = 0 
                        WHERE Id = @Id";

                    command.Parameters.Add(new SqlParameter("@Id", pagoId));

                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        return Json(new { success = true, message = "Validación eliminada exitosamente." });
                    }
                    else
                    {
                        return Json(new { success = false, message = "No se encontró la validación especificada." });
                    }
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error al eliminar pago: {ex.Message}");
            return Json(new { success = false, message = "Error al eliminar la validación. Intente nuevamente." });
        }
    }

    /// <summary>
    /// Elimina pagos validados que ya no existen en el SP buscarPorDocumCP (ya se pagaron)
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public JsonResult LimpiarPagosYaPagados()
    {
        try
        {
            using (var contexto = new A_ZULIA_12Entities())
            {
                var connection = contexto.Database.Connection;
                if (connection.State != System.Data.ConnectionState.Open)
                {
                    connection.Open();
                }

                using (var command = connection.CreateCommand())
                {
                    // Con la nueva estructura, es más simple comparar directamente
                    command.CommandText = @"
                        ;WITH DocumentosActuales AS (
                            SELECT DISTINCT 
                                a.co_prov,
                                a.co_tipo_doc,
                                a.nro_doc
                            FROM saDocumentoCP a
                            LEFT JOIN saProveedor b ON a.co_prov = b.co_prov
                            LEFT JOIN saRetencionCP c ON a.co_tipo_doc = c.co_tipo_doc AND a.nro_doc = c.nro_doc
                            WHERE a.saldo > 0
                        )
                        DELETE FROM ZPagosValidados 
                        WHERE Activo = 1 
                        AND NOT EXISTS (
                            SELECT 1 FROM DocumentosActuales da
                            WHERE da.co_prov = ZPagosValidados.CodigoProveedor
                            AND da.co_tipo_doc = ZPagosValidados.TipoDoc
                            AND da.nro_doc = LTRIM(RTRIM(ZPagosValidados.NroProv))
                        )";

                    int rowsAffected = command.ExecuteNonQuery();

                    return Json(new { 
                        success = true, 
                        message = $"Se eliminaron {rowsAffected} registros de pagos que ya fueron pagados de la tabla ZPagosValidados." 
                    });
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error al limpiar pagos ya pagados: {ex.Message}");
            return Json(new { success = false, message = "Error al limpiar pagos ya pagados. Intente nuevamente." });
        }
    }

    /// <summary>
    /// Actualiza los saldos de los pagos validados y los desactiva si ya fueron pagados
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public JsonResult ActualizarSaldosPagos()
    {
        try
        {
            using (var contexto = new A_ZULIA_12Entities())
            {
                var connection = contexto.Database.Connection;
                if (connection.State != System.Data.ConnectionState.Open)
                {
                    connection.Open();
                }

                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Actualizar saldos actuales desde saDocumentoCP
                        using (var command = connection.CreateCommand())
                        {
                            command.Transaction = transaction;
                            command.CommandText = @"
                                UPDATE zpv 
                                SET SaldoActual = ISNULL(da.saldo, 0)
                                FROM ZPagosValidados zpv
                                LEFT JOIN saDocumentoCP da ON da.nro_doc = zpv.NroProv 
                                                           AND da.co_tipo_doc = zpv.TipoDoc
                                                           AND da.co_prov = zpv.CodigoProveedor
                                WHERE zpv.Activo = 1";

                            int saldosActualizados = command.ExecuteNonQuery();

                            // Desactivar pagos donde el saldo actual es 0 o menor al saldo inicial
                            command.CommandText = @"
                                UPDATE ZPagosValidados 
                                SET Activo = 0,
                                    FechaNotificacion = GETDATE()
                                WHERE Activo = 1 
                                  AND (SaldoActual <= 0 OR SaldoActual <= SaldoInicial)";

                            int pagosDesactivados = command.ExecuteNonQuery();

                            transaction.Commit();

                            return Json(new { 
                                success = true, 
                                message = $"Proceso completado exitosamente. Saldos actualizados: {saldosActualizados}, Pagos desactivados: {pagosDesactivados}",
                                saldosActualizados = saldosActualizados,
                                pagosDesactivados = pagosDesactivados
                            });
                        }
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error al actualizar saldos: {ex.Message}");
            return Json(new { success = false, message = "Error al actualizar saldos. Intente nuevamente." });
        }
    }

    /// <summary>
    /// Obtiene el nombre del usuario basado en su código
    /// </summary>
    /// <param name="codigoUsuario">Código del usuario (ej: "04")</param>
    /// <param name="contexto">Contexto de la base de datos</param>
    /// <returns>Nombre del usuario o el código si no se encuentra</returns>
    private string ObtenerNombreUsuario(string codigoUsuario, A_ZULIA_12Entities contexto)
    {
        try
        {
            var usuario = contexto.Usuarios
                .FirstOrDefault(u => u.Usuario == codigoUsuario);
            
            return usuario?.Nombre ?? codigoUsuario; // Retorna el nombre o el código si no se encuentra
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error al obtener nombre de usuario {codigoUsuario}: {ex.Message}");
            return codigoUsuario; // En caso de error, retorna el código
        }
    }
}
