using System.Collections.Generic;
using System.Linq;
using APIS.ADO;

namespace APIS.Repositorios
{
    /// <summary>Acceso a datos del modulo de Pedidos (Fase 2).</summary>
    public interface IPedidosRepositorio
    {
        List<buscarPedidosActivos_Result> BuscarPedidosActivos(string consulta);
        List<buscarRengPE2_Result> BuscarRenglones(string docNum);
    }

    public class PedidosRepositorio : IPedidosRepositorio
    {
        public List<buscarPedidosActivos_Result> BuscarPedidosActivos(string consulta)
        {
            using (var db = new A_ZULIA_12Entities())
            {
                return db.buscarPedidosActivos(consulta).ToList();
            }
        }

        public List<buscarRengPE2_Result> BuscarRenglones(string docNum)
        {
            using (var db = new A_ZULIA_12Entities())
            {
                return db.buscarRengPE2(docNum).ToList();
            }
        }
    }
}
