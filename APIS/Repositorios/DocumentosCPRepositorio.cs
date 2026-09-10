using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using APIS.ADO;
using APIS.Models;

namespace APIS.Repositorios
{
    /// <summary>
    /// Implementacion de IDocumentosCPRepositorio (Fase 2).
    /// Todo el SQL/SP del modulo DocumentoCP vive aqui; el controlador ya no conoce la BD.
    /// </summary>
    public class DocumentosCPRepositorio : IDocumentosCPRepositorio
    {
        public List<buscarPagosCP_Result> BuscarPagosCP(string nombre)
        {
            using (var contexto = new A_ZULIA_12Entities())
            {
                return contexto.buscarPagosCP(nombre ?? string.Empty).ToList();
            }
        }

        public List<DocumentoCPViewModel> BuscarPorDocumCP(string consulta, int cantidad, string usuario)
        {
            using (var contexto = new A_ZULIA_12Entities())
            {
                return contexto.Database.SqlQuery<DocumentoCPViewModel>(
                    "EXEC buscarPorDocumCP @consulta, @cantidad, @usuario",
                    new SqlParameter("@consulta", consulta ?? string.Empty),
                    new SqlParameter("@cantidad", cantidad),
                    new SqlParameter("@usuario", usuario ?? string.Empty)
                ).ToList();
            }
        }

        public List<buscarRengCP1_Result> BuscarRenglones(string consulta)
        {
            using (var contexto = new A_ZULIA_12Entities())
            {
                return contexto.Database.SqlQuery<buscarRengCP1_Result>(
                    "EXEC buscarRengCP1 @consulta",
                    new SqlParameter("@consulta", consulta ?? string.Empty)
                ).ToList();
            }
        }

        public bool TieneImagenPago(string cobNum)
        {
            if (string.IsNullOrEmpty(cobNum))
            {
                return false;
            }

            try
            {
                using (var contexto = new A_ZULIA_12Entities())
                {
                    var existe = contexto.Database.SqlQuery<int>(
                        @"SELECT COUNT(1) 
                          FROM saPago a
                          INNER JOIN saDocumentoImagen b ON a.rowguid = b.rowguidDoc
                          WHERE a.cob_num = @p0 AND b.co_tipo_doc = 'PAGO'",
                        cobNum
                    ).FirstOrDefault();

                    return existe > 0;
                }
            }
            catch
            {
                return false;
            }
        }

        public ImagenPagoDto ObtenerImagenPago(string cobNum)
        {
            using (var contexto = new A_ZULIA_12Entities())
            {
                var connection = contexto.Database.Connection;
                if (connection.State != ConnectionState.Open)
                {
                    connection.Open();
                }

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "EXEC buscarPagostImagen @doc_num";
                    command.Parameters.Add(new SqlParameter("@doc_num", cobNum));

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new ImagenPagoDto
                            {
                                cob_num = reader["cob_num"] as string,
                                des_imag = reader["des_imag"] as string,
                                picture = reader["picture"] as byte[]
                            };
                        }
                    }
                }
            }

            return null;
        }
    }
}
