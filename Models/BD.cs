using System.Data;
using Microsoft.Data.SqlClient;

namespace Zooni.Models
{
    public static class BD
    {
        private static string connectionString =
            @"Server=Localhost;Database=Zooni;Integrated Security=True;TrustServerCertificate=True;";

        // ======================================================
        // 🔹 Devuelve conexión abierta
        // ======================================================
        public static SqlConnection GetConnection()
        {
            var conn = new SqlConnection(connectionString);
            conn.Open();
            return conn;
        }

        // ======================================================
        // 🔹 Ejecuta SELECT y devuelve DataTable
        // ======================================================
        public static DataTable ExecuteQuery(string query, Dictionary<string, object>? parameters = null)
        {
            try
            {
                using var connection = GetConnection();
                using var command = new SqlCommand(query, connection);

                if (parameters != null)
                {
                    foreach (var p in parameters)
                        command.Parameters.AddWithValue(p.Key, p.Value ?? DBNull.Value);
                }

                using var adapter = new SqlDataAdapter(command);
                var table = new DataTable();
                adapter.Fill(table);
                return table;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error en ExecuteQuery:\n{ex.Message}\nQuery: {query}");
                throw;
            }
        }

        // ======================================================
        // 🔹 Ejecuta INSERT/SELECT escalar y devuelve el primer valor
        // ======================================================
        public static object? ExecuteScalar(string query, Dictionary<string, object>? parameters = null)
        {
            try
            {
                using var connection = GetConnection();
                using var command = new SqlCommand(query, connection);

                if (parameters != null)
                {
                    foreach (var p in parameters)
                        command.Parameters.AddWithValue(p.Key, p.Value ?? DBNull.Value);
                }

                var result = command.ExecuteScalar();

                if (result == null || result == DBNull.Value)
                {
                    Console.WriteLine($"⚠️ ExecuteScalar devolvió NULL.\nQuery: {query}");
                    return null;
                }

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error en ExecuteScalar:\n{ex.Message}\nQuery: {query}");
                throw;
            }
        }

        // ======================================================
        // 🔹 Ejecuta UPDATE / DELETE / INSERT sin retorno
        // ======================================================
        public static int ExecuteNonQuery(string query, Dictionary<string, object>? parameters = null)
        {
            try
            {
                using var connection = GetConnection();
                using var command = new SqlCommand(query, connection);

                if (parameters != null)
                {
                    foreach (var p in parameters)
                        command.Parameters.AddWithValue(p.Key, p.Value ?? DBNull.Value);
                }

                int affected = command.ExecuteNonQuery();
                return affected;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error en ExecuteNonQuery:\n{ex.Message}\nQuery: {query}");
                throw;
            }
        }

        // ======================================================
        // 🔹 Ejecuta script SQL completo (múltiples comandos)
        // ======================================================
        public static void ExecuteScript(string script)
        {
            try
            {
                using var connection = GetConnection();
                // Dividir por GO y ejecutar cada comando
                var commands = script.Split(new[] { "GO", "go", "Go" }, StringSplitOptions.RemoveEmptyEntries);
                
                foreach (var cmd in commands)
                {
                    var trimmedCmd = cmd.Trim();
                    if (string.IsNullOrWhiteSpace(trimmedCmd)) continue;
                    
                    using var command = new SqlCommand(trimmedCmd, connection);
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error en ExecuteScript:\n{ex.Message}");
                throw;
            }
        }
    }
}
