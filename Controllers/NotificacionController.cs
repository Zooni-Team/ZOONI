using Microsoft.AspNetCore.Mvc;
using Zooni.Models;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Data;

namespace Zooni.Controllers
{
    public class NotificacionController : BaseController
    {
        private readonly IMemoryCache _cache;

        public NotificacionController(IMemoryCache cache)
        {
            _cache = cache;
        }

        // ============================
        // Asegurar tabla de notificaciones
        // ============================
        private void AsegurarTablaNotificaciones()
        {
            try
            {
                string checkTable = "SELECT COUNT(*) FROM sys.tables WHERE name = 'Notificacion'";
                object? tableExists = BD.ExecuteScalar(checkTable);
                if (tableExists == null || Convert.ToInt32(tableExists) == 0)
                {
                    string createTable = @"
                        CREATE TABLE [dbo].[Notificacion](
                            [Id_Notificacion] [int] IDENTITY(1,1) NOT NULL,
                            [Id_User] [int] NOT NULL,
                            [Tipo] [nvarchar](50) NOT NULL,
                            [Titulo] [nvarchar](200) NOT NULL,
                            [Mensaje] [nvarchar](1000) NOT NULL,
                            [Id_Referencia] [int] NULL,
                            [Url] [nvarchar](500) NULL,
                            [Leida] [bit] NOT NULL DEFAULT 0,
                            [Fecha] [datetime2](7) NOT NULL DEFAULT GETDATE(),
                            [Eliminada] [bit] NOT NULL DEFAULT 0,
                            CONSTRAINT [PK_Notificacion] PRIMARY KEY CLUSTERED ([Id_Notificacion] ASC),
                            CONSTRAINT [FK_Notificacion_User] FOREIGN KEY ([Id_User]) REFERENCES [dbo].[User]([Id_User])
                        )";
                    BD.ExecuteNonQuery(createTable);
                }
                else
                {
                    // Verificar y agregar columnas faltantes si la tabla ya existe
                    AsegurarColumnasNotificacion();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al crear tabla Notificacion: " + ex.Message);
            }
        }

        // ============================
        // Asegurar columnas de notificaciones
        // ============================
        private void AsegurarColumnasNotificacion()
        {
            try
            {
                // Verificar y agregar columna Tipo
                string checkTipo = @"
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Notificacion]') AND name = 'Tipo')
                    BEGIN
                        ALTER TABLE [dbo].[Notificacion] ADD [Tipo] [nvarchar](50) NOT NULL DEFAULT 'General';
                    END";
                BD.ExecuteNonQuery(checkTipo);

                // Verificar y agregar columna Id_Referencia
                string checkIdReferencia = @"
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Notificacion]') AND name = 'Id_Referencia')
                    BEGIN
                        ALTER TABLE [dbo].[Notificacion] ADD [Id_Referencia] [int] NULL;
                    END";
                BD.ExecuteNonQuery(checkIdReferencia);

                // Verificar y agregar columna Url
                string checkUrl = @"
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Notificacion]') AND name = 'Url')
                    BEGIN
                        ALTER TABLE [dbo].[Notificacion] ADD [Url] [nvarchar](500) NULL;
                    END";
                BD.ExecuteNonQuery(checkUrl);

                // Verificar y agregar columna Eliminada
                string checkEliminada = @"
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Notificacion]') AND name = 'Eliminada')
                    BEGIN
                        ALTER TABLE [dbo].[Notificacion] ADD [Eliminada] [bit] NOT NULL DEFAULT 0;
                    END";
                BD.ExecuteNonQuery(checkEliminada);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al asegurar columnas de Notificacion: " + ex.Message);
            }
        }

        // ============================
        // GET: Obtener notificaciones del usuario
        // ============================
        [HttpGet]
        public IActionResult ObtenerNotificaciones()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return Json(new { success = false, message = "No autenticado" });

            try
            {
                AsegurarTablaNotificaciones();

                string cacheKey = $"notificaciones_{userId}";
                if (!_cache.TryGetValue(cacheKey, out List<object>? notificaciones))
                {
                    string query = @"
                        SELECT TOP 50
                            Id_Notificacion,
                            Tipo,
                            Titulo,
                            Mensaje,
                            Id_Referencia,
                            Url,
                            Leida,
                            Fecha
                        FROM Notificacion
                        WHERE Id_User = @UserId 
                          AND Eliminada = 0
                        ORDER BY Fecha DESC";

                    DataTable dt = BD.ExecuteQuery(query, new Dictionary<string, object> { { "@UserId", userId.Value } });
                    notificaciones = new List<object>();

                    foreach (DataRow row in dt.Rows)
                    {
                        notificaciones.Add(new
                        {
                            id = Convert.ToInt32(row["Id_Notificacion"]),
                            tipo = row["Tipo"].ToString(),
                            titulo = row["Titulo"].ToString(),
                            mensaje = row["Mensaje"].ToString(),
                            idReferencia = row["Id_Referencia"] != DBNull.Value ? Convert.ToInt32(row["Id_Referencia"]) : (int?)null,
                            url = row["Url"] != DBNull.Value ? row["Url"].ToString() : null,
                            leida = Convert.ToBoolean(row["Leida"]),
                            fecha = Convert.ToDateTime(row["Fecha"]).ToString("yyyy-MM-dd HH:mm:ss")
                        });
                    }

                    _cache.Set(cacheKey, notificaciones, TimeSpan.FromMinutes(1));
                }

                int noLeidas = 0;
                if (notificaciones != null)
                {
                    foreach (var notif in notificaciones)
                    {
                        var props = notif.GetType().GetProperty("leida");
                        if (props != null && !Convert.ToBoolean(props.GetValue(notif)))
                            noLeidas++;
                    }
                }

                return Json(new { success = true, notificaciones = notificaciones, noLeidas = noLeidas });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error ObtenerNotificaciones: " + ex.Message);
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ============================
        // POST: Marcar notificación como leída
        // ============================
        [HttpPost]
        public IActionResult MarcarLeida([FromBody] dynamic request)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return Json(new { success = false, message = "No autenticado" });

            try
            {
                int idNotificacion = Convert.ToInt32(request.idNotificacion);

                // Verificar que la notificación pertenece al usuario
                string verificarQuery = "SELECT Id_User FROM Notificacion WHERE Id_Notificacion = @IdNotificacion";
                object? idUserResult = BD.ExecuteScalar(verificarQuery, new Dictionary<string, object> { { "@IdNotificacion", idNotificacion } });

                if (idUserResult == null || Convert.ToInt32(idUserResult) != userId.Value)
                {
                    return Json(new { success = false, message = "No autorizado" });
                }

                string updateQuery = "UPDATE Notificacion SET Leida = 1 WHERE Id_Notificacion = @IdNotificacion";
                BD.ExecuteNonQuery(updateQuery, new Dictionary<string, object> { { "@IdNotificacion", idNotificacion } });

                // Invalidar caché
                string cacheKey = $"notificaciones_{userId}";
                _cache.Remove(cacheKey);

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error MarcarLeida: " + ex.Message);
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ============================
        // POST: Eliminar notificación
        // ============================
        [HttpPost]
        public IActionResult EliminarNotificacion([FromBody] dynamic request)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return Json(new { success = false, message = "No autenticado" });

            try
            {
                int idNotificacion = Convert.ToInt32(request.idNotificacion);

                // Verificar que la notificación pertenece al usuario
                string verificarQuery = "SELECT Id_User FROM Notificacion WHERE Id_Notificacion = @IdNotificacion";
                object? idUserResult = BD.ExecuteScalar(verificarQuery, new Dictionary<string, object> { { "@IdNotificacion", idNotificacion } });

                if (idUserResult == null || Convert.ToInt32(idUserResult) != userId.Value)
                {
                    return Json(new { success = false, message = "No autorizado" });
                }

                string deleteQuery = "UPDATE Notificacion SET Eliminada = 1 WHERE Id_Notificacion = @IdNotificacion";
                BD.ExecuteNonQuery(deleteQuery, new Dictionary<string, object> { { "@IdNotificacion", idNotificacion } });

                // Invalidar caché
                string cacheKey = $"notificaciones_{userId}";
                _cache.Remove(cacheKey);

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error EliminarNotificacion: " + ex.Message);
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ============================
        // POST: Marcar todas como leídas
        // ============================
        [HttpPost]
        public IActionResult MarcarTodasLeidas()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return Json(new { success = false, message = "No autenticado" });

            try
            {
                string updateQuery = "UPDATE Notificacion SET Leida = 1 WHERE Id_User = @UserId AND Eliminada = 0";
                BD.ExecuteNonQuery(updateQuery, new Dictionary<string, object> { { "@UserId", userId.Value } });

                // Invalidar caché
                string cacheKey = $"notificaciones_{userId}";
                _cache.Remove(cacheKey);

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error MarcarTodasLeidas: " + ex.Message);
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ============================
        // Método estático para crear notificación (llamado desde otros controladores)
        // ============================
        public static void CrearNotificacion(int idUser, string tipo, string titulo, string mensaje, int? idReferencia = null, string? url = null)
        {
            try
            {
                string insertQuery = @"
                    INSERT INTO Notificacion (Id_User, Tipo, Titulo, Mensaje, Id_Referencia, Url, Fecha)
                    VALUES (@IdUser, @Tipo, @Titulo, @Mensaje, @IdReferencia, @Url, GETDATE())";

                var parametros = new Dictionary<string, object>
                {
                    { "@IdUser", idUser },
                    { "@Tipo", tipo },
                    { "@Titulo", titulo },
                    { "@Mensaje", mensaje },
                    { "@IdReferencia", idReferencia ?? (object)DBNull.Value },
                    { "@Url", url ?? (object)DBNull.Value }
                };

                BD.ExecuteNonQuery(insertQuery, parametros);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error CrearNotificacion: " + ex.Message);
            }
        }
    }
}

