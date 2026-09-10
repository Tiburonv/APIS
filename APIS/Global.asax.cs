using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace APIS
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
#if !DEBUG
            // En Release: combinar y minificar bundles (CSS/JS) para produccion
            BundleTable.EnableOptimizations = true;
#else
            // En Debug: sin optimizacion para facilitar la depuracion
            BundleTable.EnableOptimizations = false;
#endif

            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            // Inyeccion de dependencias (Fase 1) y registro de inicio
            APIS.DependencyConfig.Inicializar();
            APIS.Servicios.Log.Info("APIS iniciado (Application_Start).");
        }

        protected void Application_BeginRequest()
        {
            Response.ContentEncoding = System.Text.Encoding.UTF8;
            Response.HeaderEncoding = System.Text.Encoding.UTF8;
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            var ex = Server.GetLastError();
            if (ex != null)
            {
                APIS.Servicios.Log.Error("Excepcion no controlada", ex);
            }
        }

        protected void Application_PostAcquireRequestState(object sender, EventArgs e)
        {
            if (HttpContext.Current != null && HttpContext.Current.Session != null)
            {
                if (User != null && User.Identity != null && User.Identity.IsAuthenticated)
                {
                    if (Session["Usuario"] == null && !string.IsNullOrEmpty(User.Identity.Name))
                    {
                        Session["Usuario"] = User.Identity.Name;
                    }
                }
            }
        }
    }
}
