using System.Data;
using Microsoft.Data.SqlClient;
using Dapper;
namespace Zooni.Models
{
    public class bd
    {
        // 🔹 Cadena de conexión a tu base de datos "Zooni"
        // 💡 Cambiá el Server si tu instancia no es (localdb)\MSSQLLocalDB
        private static string connectionString =
            "Server=(localdb)\\MSSQLLocalDB;Database=Zooni;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";

        // Si usás SQL Server Express o un servidor remoto, sería por ejemplo:
        // "Server=localhost\\SQLEXPRESS;Database=Zooni;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true";
        //
        // O si usás autenticación SQL:
        // "Server=localhost;Database=Zooni;User Id=sa;Password=tu_contraseña;TrustServerCertificate=True;MultipleActiveResultSets=true";

        // 🔹 Método para obtener conexión abierta
        public static SqlConnection ObtenerConexion()
        {
            var conexion = new SqlConnection(connectionString);
            conexion.Open();
            return conexion;
        }

        // 🔹 Método auxiliar: ejecutar un comando rápido
        public static int EjecutarComando(string query, params SqlParameter[] parametros)
        {
            using (var conexion = new SqlConnection(connectionString))
            {
                conexion.Open();
                using (var comando = new SqlCommand(query, conexion))
                {
                    if (parametros != null)
                        comando.Parameters.AddRange(parametros);

                    return comando.ExecuteNonQuery();
                }
            }
        }

        // 🔹 Método auxiliar: obtener datos en DataTable
        public static DataTable EjecutarConsulta(string query, params SqlParameter[] parametros)
        {
            using (var conexion = new SqlConnection(connectionString))
            {
                conexion.Open();
                using (var comando = new SqlCommand(query, conexion))
                {
                    if (parametros != null)
                        comando.Parameters.AddRange(parametros);

                    using (var adaptador = new SqlDataAdapter(comando))
                    {
                        DataTable tabla = new DataTable();
                        adaptador.Fill(tabla);
                        return tabla;
                    }
                }
            }
        }
    }
}
