using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using APIS.Repositorios;

namespace APIS
{
    /// <summary>
    /// Contenedor de dependencias liviano SIN paquetes externos (Fase 1).
    /// Sigue el patron oficial de MVC 5: al registrar un IDependencyResolver,
    /// los controladores se resuelven por constructor cuando hay dependencias.
    /// Los tipos NO registrados devuelven null y MVC usa su activacion por defecto,
    /// por lo que la aplicacion funciona igual aunque no haya registros todavia.
    /// (Si mas adelante se prefiere Autofac/SimpleInjector, solo se sustituye
    ///  el interior de DependencyConfig sin tocar los controladores.)
    /// </summary>
    public static class DependencyConfig
    {
        private static readonly Dictionary<Type, Type> _registros = new Dictionary<Type, Type>();
        private static readonly Dictionary<Type, object> _singletons = new Dictionary<Type, object>();
        private static readonly object _candado = new object();
        private static bool _inicializado = false;

        /// <summary>Registra TInterface -> TImplementacion (instancia unica, perezosa).</summary>
        public static void RegistrarSingleton<TInterface, TImplementacion>() where TImplementacion : TInterface
        {
            lock (_candado)
            {
                _registros[typeof(TInterface)] = typeof(TImplementacion);
            }
        }

        public static T Obtener<T>()
        {
            return (T)Resolver(typeof(T));
        }

        public static void Inicializar()
        {
            if (_inicializado) return;
            _inicializado = true;

            // Ejemplos cuando existan servicios (Fase 2+):
            RegistrarSingleton<IPreciosRepositorio, PreciosRepositorio>();
            RegistrarSingleton<IDocumentosCPRepositorio, DocumentosCPRepositorio>();
            RegistrarSingleton<IClienteRepositorio, ClienteRepositorio>();
            RegistrarSingleton<IImagenesRepositorio, ImagenesRepositorio>();
            RegistrarSingleton<IConfigEmpresaRepositorio, ConfigEmpresaRepositorio>();
            RegistrarSingleton<IUsuariosRepositorio, UsuariosRepositorio>();
            RegistrarSingleton<IPedidosRepositorio, PedidosRepositorio>();

            DependencyResolver.SetResolver(new ResolvedorDependencias());
        }

        private static object Resolver(Type tipo)
        {
            lock (_candado)
            {
                object instancia;
                if (_singletons.TryGetValue(tipo, out instancia))
                {
                    return instancia;
                }

                Type implementacion;
                if (!_registros.TryGetValue(tipo, out implementacion))
                {
                    return null;
                }

                instancia = CrearInstancia(implementacion);
                _singletons[tipo] = instancia;
                return instancia;
            }
        }

        private static object CrearInstancia(Type tipo)
        {
            // Elige el constructor publico; resuelve sus parametros recursivamente
            var constructor = tipo.GetConstructors()
                .OrderByDescending(c => c.GetParameters().Length)
                .FirstOrDefault();

            if (constructor == null)
            {
                return Activator.CreateInstance(tipo);
            }

            var parametros = constructor.GetParameters();
            var argumentos = new object[parametros.Length];
            for (int i = 0; i < parametros.Length; i++)
            {
                argumentos[i] = Resolver(parametros[i].ParameterType);
                if (argumentos[i] == null)
                {
                    throw new InvalidOperationException(
                        "No se puede resolver la dependencia '" + parametros[i].ParameterType.FullName +
                        "' al construir '" + tipo.FullName + "'. Registrela en DependencyConfig.Inicializar().");
                }
            }

            return constructor.Invoke(argumentos);
        }

        private class ResolvedorDependencias : IDependencyResolver
        {
            public object GetService(Type serviceType)
            {
                return Resolver(serviceType);
            }

            public IEnumerable<object> GetServices(Type serviceType)
            {
                var servicio = Resolver(serviceType);
                return servicio != null ? new object[] { servicio } : Enumerable.Empty<object>();
            }
        }
    }
}
