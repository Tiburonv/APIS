using System.Collections.Generic;
using APIS.Models;

namespace APIS.Repositorios
{
    /// <summary>Acceso a datos del modulo de Precios (solo lectura).</summary>
    public interface IPreciosRepositorio
    {
        IList<CategoriaViewModel> ListarCategorias();
        IList<PreciosViewModel> BuscarPrecios(string busqueda, string coCat);
    }
}
