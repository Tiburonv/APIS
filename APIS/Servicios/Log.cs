using System;
using System.Configuration;
using System.IO;
using System.Text;
using System.Web.Hosting;

namespace APIS.Servicios
{
    /// <summary>
    /// Registro simple en archivo (sin dependencias externas) para Fase 1.
    /// Escribe un archivo diario en App_Data/logs/app-YYYYMMDD.log.
    /// Configuracion opcional en appSettings de Web.config:
    ///   log:directorio -> ruta (admite "~/App_Data/logs")
    ///   log:nivel      -> debug | info | warn | error (por defecto: error)
    /// </summary>
    public static class Log
    {
        private static readonly object _candado = new object();
        private static readonly string _directorio;
        private static readonly int _nivelMinimo; // 0=debug,1=info,2=warn,3=error

        static Log()
        {
            try
            {
                string ruta = ConfigurationManager.AppSettings["log:directorio"];
                if (string.IsNullOrWhiteSpace(ruta))
                {
                    ruta = "~/App_Data/logs";
                }
                _directorio = ResolverRuta(ruta);

                string nivel = (ConfigurationManager.AppSettings["log:nivel"] ?? "error").Trim().ToLowerInvariant();
                switch (nivel)
                {
                    case "debug": _nivelMinimo = 0; break;
                    case "info": _nivelMinimo = 1; break;
                    case "warn": _nivelMinimo = 2; break;
                    default: _nivelMinimo = 3; break; // error
                }
            }
            catch
            {
                _directorio = null;
                _nivelMinimo = 3;
            }
        }

        private static string ResolverRuta(string ruta)
        {
            try
            {
                if (ruta.StartsWith("~", StringComparison.Ordinal))
                {
                    return HostingEnvironment.MapPath(ruta);
                }
                return Path.IsPathRooted(ruta) ? ruta : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ruta);
            }
            catch
            {
                return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data\\logs");
            }
        }

        public static void Debug(string mensaje) { Escribir(0, "DEBUG", mensaje); }
        public static void Info(string mensaje) { Escribir(1, "INFO", mensaje); }
        public static void Warn(string mensaje) { Escribir(2, "WARN", mensaje); }
        public static void Error(string mensaje) { Escribir(3, "ERROR", mensaje); }
        public static void Error(string contexto, Exception ex)
        {
            if (ex == null) { Escribir(3, "ERROR", contexto); return; }
            Escribir(3, "ERROR", contexto + Environment.NewLine + ex.ToString());
        }

        private static void Escribir(int nivel, string etiqueta, string mensaje)
        {
            if (nivel < _nivelMinimo) return;
            if (_directorio == null) return;

            try
            {
                lock (_candado)
                {
                    Directory.CreateDirectory(_directorio);
                    string archivo = Path.Combine(_directorio, "app-" + DateTime.Now.ToString("yyyyMMdd") + ".log");
                    string linea = string.Format("{0:yyyy-MM-dd HH:mm:ss.fff} [{1}] {2}{3}",
                        DateTime.Now, etiqueta, mensaje, Environment.NewLine);
                    File.AppendAllText(archivo, linea, Encoding.UTF8);
                }
            }
            catch
            {
                // Nunca dejar que el logging rompa la aplicacion
            }
        }
    }
}
