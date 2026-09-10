using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using APIS.ADO;
using APIS.Models;

namespace APIS.Repositorios
{
    /// <summary>Acceso a datos de la configuracion de empresa (Fase 2).</summary>
    public interface IConfigEmpresaRepositorio
    {
        ConfigEmpresaViewModel Obtener();
        void Guardar(string rif, string direccion, string telefono, byte[] logo, string logoMime, bool actualizarLogo, string usuario);
    }

    public class ConfigEmpresaRepositorio : IConfigEmpresaRepositorio
    {
        public ConfigEmpresaViewModel Obtener()
        {
            using (var db = new A_ZULIA_12Entities())
            {
                return db.Database.SqlQuery<ConfigEmpresaViewModel>(
                    "EXEC obtenerZConfigEmpresa"
                ).FirstOrDefault();
            }
        }

        public void Guardar(string rif, string direccion, string telefono, byte[] logo, string logoMime, bool actualizarLogo, string usuario)
        {
            using (var db = new A_ZULIA_12Entities())
            {
                var connection = db.Database.Connection;
                if (connection.State != ConnectionState.Open)
                {
                    connection.Open();
                }

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "EXEC guardarZConfigEmpresa @RIF, @Direccion, @Telefono, @Logo, @LogoMimeType, @ActualizarLogo, @Co_us_mo";
                    command.CommandType = CommandType.Text;

                    command.Parameters.Add(new SqlParameter("@RIF", SqlDbType.VarChar, 20) { Value = rif });
                    command.Parameters.Add(new SqlParameter("@Direccion", SqlDbType.NVarChar, 250) { Value = direccion });
                    command.Parameters.Add(new SqlParameter("@Telefono", SqlDbType.VarChar, 50) { Value = telefono });
                    command.Parameters.Add(new SqlParameter("@Logo", SqlDbType.VarBinary, -1)
                    {
                        Value = (object)logo ?? DBNull.Value
                    });
                    command.Parameters.Add(new SqlParameter("@LogoMimeType", SqlDbType.VarChar, 50)
                    {
                        Value = (object)logoMime ?? DBNull.Value
                    });
                    command.Parameters.Add(new SqlParameter("@ActualizarLogo", SqlDbType.Bit) { Value = actualizarLogo });
                    command.Parameters.Add(new SqlParameter("@Co_us_mo", SqlDbType.VarChar, 10) { Value = usuario });

                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
