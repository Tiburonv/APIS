using System.Linq;
using APIS.ADO;

namespace APIS.Repositorios
{
    /// <summary>Acceso a datos de usuarios (login y migracion de clave) - Fase 2.</summary>
    public interface IUsuariosRepositorio
    {
        /// <summary>Busca por codigo de usuario o por nombre (incluye el caso '4' -> '04').</summary>
        Usuarios BuscarPorUsuarioONombre(string username);

        /// <summary>Actualiza la clave almacenada del usuario indicado.</summary>
        void GuardarClave(string codigoUsuario, string clave);
    }

    public class UsuariosRepositorio : IUsuariosRepositorio
    {
        public Usuarios BuscarPorUsuarioONombre(string username)
        {
            using (var db = new A_ZULIA_12Entities())
            {
                var usernameLower = (username ?? string.Empty).ToLower();

                // Buscar usuario por identificador de usuario o por nombre
                var usuario = db.Usuarios.FirstOrDefault(u =>
                    u.Usuario.Trim().ToLower() == usernameLower ||
                    (u.Nombre != null && u.Nombre.Trim().ToLower() == usernameLower)
                );

                // Si no se encuentra y se ingresó un número sin ceros a la izquierda (ej: '4' en lugar de '04')
                int numVal;
                if (usuario == null && int.TryParse(username, out numVal))
                {
                    var usernamePadded = username.PadLeft(2, '0');
                    usuario = db.Usuarios.FirstOrDefault(u => u.Usuario.Trim() == usernamePadded);
                }

                return usuario;
            }
        }

        public void GuardarClave(string codigoUsuario, string clave)
        {
            using (var db = new A_ZULIA_12Entities())
            {
                var usuario = db.Usuarios.FirstOrDefault(u => u.Usuario.Trim() == codigoUsuario);
                if (usuario == null)
                {
                    return;
                }

                usuario.Clave = clave;
                db.SaveChanges();
            }
        }
    }
}
