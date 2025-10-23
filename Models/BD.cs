using System.Data;
using Microsoft.Data.SqlClient;

namespace Zooni.Models
{
    public static class BD
    {
        private static string connectionString =
            @"Server=localhost;Database=Zooni;Integrated Security=True;TrustServerCertificate=True;";

        public static SqlConnection GetConnection()
        {
            var conn = new SqlConnection(connectionString);
            conn.Open();
            return conn;
        }

        public static DataTable ExecuteQuery(string query, Dictionary<string, object>? parameters = null)
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

        public static object? ExecuteScalar(string query, Dictionary<string, object>? parameters = null)
        {
            using var connection = GetConnection();
            using var command = new SqlCommand(query, connection);

            if (parameters != null)
            {
                foreach (var p in parameters)
                    command.Parameters.AddWithValue(p.Key, p.Value ?? DBNull.Value);
            }

            return command.ExecuteScalar();
        }

        public static int ExecuteNonQuery(string query, Dictionary<string, object>? parameters = null)
        {
            using var connection = GetConnection();
            using var command = new SqlCommand(query, connection);

            if (parameters != null)
            {
                foreach (var p in parameters)
                    command.Parameters.AddWithValue(p.Key, p.Value ?? DBNull.Value);
            }

            return command.ExecuteNonQuery();
        }
    }
}
