using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using APIS.ADO;

namespace APIS.Repositorios
{
    /// <summary>Acceso a datos de consultas de clientes (Fase 2).</summary>
    public interface IClienteRepositorio
    {
        List<ClienteDTO> BuscarClientes(string consulta);
    }

    public class ClienteRepositorio : IClienteRepositorio
    {
        public List<ClienteDTO> BuscarClientes(string consulta)
        {
            using (var db = new A_ZULIA_12Entities())
            {
                return db.Database.SqlQuery<ClienteDTO>(
                    "EXEC buscarCliente1 @consulta",
                    new SqlParameter("@consulta", string.IsNullOrEmpty(consulta) ? "" : consulta)
                ).ToList();
            }
        }
    }
}
