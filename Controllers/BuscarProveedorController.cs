using Microsoft.AspNetCore.Mvc;
using Zooni.Models;
using Zooni.Utils;
using System;
using System.Collections.Generic;
using System.Data;

namespace Zooni.Controllers
{
    public class BuscarProveedorController : BaseController
    {
        // Método para asegurar que las tablas existan (copiado de ProveedorController)
        private void AsegurarTablasProveedores()
        {
            try
            {
                string checkTable = "SELECT COUNT(*) FROM sys.tables WHERE name = 'ProveedorServicio'";
                object? tableExists = BD.ExecuteScalar(checkTable);
                if (tableExists == null || Convert.ToInt32(tableExists) == 0)
                {
                    // Si la tabla no existe, redirigir a que se ejecute el script
                    // Por ahora solo logueamos el error
                    Console.WriteLine("⚠️ La tabla ProveedorServicio no existe. Se debe ejecutar el script SQL.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("⚠️ Error al verificar tablas: " + ex.Message);
            }
        }
        // ============================
        // GET: /BuscarProveedor
        // ============================
        [HttpGet]
        [Route("BuscarProveedor")]
        public IActionResult Index(string? ciudad, string? provincia, string? pais, string? tipoServicio, string? especie, 
            decimal? latitud, decimal? longitud, decimal? radioKm)
        {
            try
            {
                // Asegurar que las tablas existan
                AsegurarTablasProveedores();
                
                // Cargar tipos de servicio
                string tiposQuery = "SELECT Id_TipoServicio, Descripcion FROM TipoServicio ORDER BY Descripcion";
                DataTable tiposDt = BD.ExecuteQuery(tiposQuery);
                ViewBag.TiposServicio = tiposDt;

                // Construir query de búsqueda
                string query = @"
                    SELECT DISTINCT
                        P.Id_Proveedor,
                        P.NombreCompleto,
                        P.Descripcion,
                        P.FotoPerfil,
                        P.Ciudad,
                        P.Provincia,
                        P.Precio_Hora,
                        P.Calificacion_Promedio,
                        P.Cantidad_Resenas,
                        P.Experiencia_Anios,
                        P.Latitud,
                        P.Longitud,
                        P.Radio_Atencion_Km,
                        P.Tipo_Ubicacion";

                // Si hay coordenadas, calcular distancia
                if (latitud.HasValue && longitud.HasValue)
                {
                    query += @",
                        (6371 * acos(
                            cos(radians(@Latitud)) * 
                            cos(radians(P.Latitud)) * 
                            cos(radians(P.Longitud) - radians(@Longitud)) + 
                            sin(radians(@Latitud)) * 
                            sin(radians(P.Latitud))
                        )) AS Distancia_Km";
                }

                query += @"
                    FROM ProveedorServicio P
                    WHERE P.Estado = 1";
                    
                // Si hay coordenadas, solo buscar proveedores con ubicación
                if (latitud.HasValue && longitud.HasValue)
                {
                    query += @" AND P.Latitud IS NOT NULL AND P.Longitud IS NOT NULL";
                }

                var parametros = new Dictionary<string, object>();

                if (!string.IsNullOrEmpty(ciudad))
                {
                    query += " AND P.Ciudad LIKE @Ciudad";
                    parametros.Add("@Ciudad", $"%{ciudad}%");
                }

                if (!string.IsNullOrEmpty(provincia))
                {
                    query += " AND P.Provincia LIKE @Provincia";
                    parametros.Add("@Provincia", $"%{provincia}%");
                }

                if (!string.IsNullOrEmpty(pais))
                {
                    query += " AND P.Pais LIKE @Pais";
                    parametros.Add("@Pais", $"%{pais}%");
                }

                // Filtrado por ubicación (radio de búsqueda)
                if (latitud.HasValue && longitud.HasValue)
                {
                    parametros.Add("@Latitud", latitud.Value);
                    parametros.Add("@Longitud", longitud.Value);
                    
                    decimal radioBusqueda = radioKm ?? 10.0M; // Radio por defecto: 10 km
                    query += @"
                        AND (
                            (6371 * acos(
                                cos(radians(@Latitud)) * 
                                cos(radians(P.Latitud)) * 
                                cos(radians(P.Longitud) - radians(@Longitud)) + 
                                sin(radians(@Latitud)) * 
                                sin(radians(P.Latitud))
                            )) <= 
                            CASE 
                                WHEN P.Tipo_Ubicacion = 'Precisa' THEN @RadioBusqueda
                                WHEN P.Tipo_Ubicacion = 'Cobertura' THEN (P.Radio_Atencion_Km + @RadioBusqueda)
                                ELSE @RadioBusqueda
                            END
                        )";
                    parametros.Add("@RadioBusqueda", radioBusqueda);
                }

                if (!string.IsNullOrEmpty(tipoServicio))
                {
                    query += @"
                        AND EXISTS (
                            SELECT 1 FROM ProveedorServicio_TipoServicio PST
                            INNER JOIN TipoServicio TS ON PST.Id_TipoServicio = TS.Id_TipoServicio
                            WHERE PST.Id_Proveedor = P.Id_Proveedor
                            AND TS.Descripcion LIKE @TipoServicio
                        )";
                    parametros.Add("@TipoServicio", $"%{tipoServicio}%");
                }

                if (!string.IsNullOrEmpty(especie))
                {
                    query += @"
                        AND EXISTS (
                            SELECT 1 FROM ProveedorServicio_Especie PE
                            WHERE PE.Id_Proveedor = P.Id_Proveedor
                            AND PE.Especie = @Especie
                        )";
                    parametros.Add("@Especie", especie);
                }

                // Ordenar por distancia si hay coordenadas, sino por calificación
                if (latitud.HasValue && longitud.HasValue)
                {
                    query += " ORDER BY Distancia_Km ASC, P.Calificacion_Promedio DESC, P.Cantidad_Resenas DESC";
                }
                else
                {
                    query += " ORDER BY P.Calificacion_Promedio DESC, P.Cantidad_Resenas DESC";
                }

                DataTable proveedoresDt = BD.ExecuteQuery(query, parametros);
                ViewBag.Proveedores = proveedoresDt;
                ViewBag.Ciudad = ciudad;
                ViewBag.Provincia = provincia;
                ViewBag.Pais = pais;
                ViewBag.TipoServicio = tipoServicio;
                ViewBag.Especie = especie;
                ViewBag.Latitud = latitud;
                ViewBag.Longitud = longitud;
                ViewBag.RadioKm = radioKm ?? 10.0M;

                return View();
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Error en BuscarProveedor/Index: " + ex.Message);
                ViewBag.Error = "Error al buscar proveedores.";
                return View();
            }
        }

        // ============================
        // GET: /BuscarProveedor/Perfil/{id}
        // ============================
        [HttpGet]
        [Route("BuscarProveedor/Perfil/{id}")]
        public IActionResult Perfil(int id)
        {
            try
            {
                // Obtener datos del proveedor
                string proveedorQuery = @"
                    SELECT 
                        P.*,
                        U.Nombre,
                        U.Apellido,
                        M.Correo
                    FROM ProveedorServicio P
                    INNER JOIN [User] U ON P.Id_User = U.Id_User
                    INNER JOIN Mail M ON U.Id_Mail = M.Id_Mail
                    WHERE P.Id_Proveedor = @Id AND P.Estado = 1";

                DataTable proveedorDt = BD.ExecuteQuery(proveedorQuery, new Dictionary<string, object> { { "@Id", id } });

                if (proveedorDt.Rows.Count == 0)
                {
                    TempData["Error"] = "Proveedor no encontrado.";
                    return RedirectToAction("Index", "BuscarProveedor");
                }

                ViewBag.Proveedor = proveedorDt.Rows[0];

                // Obtener tipos de servicio del proveedor
                string tiposQuery = @"
                    SELECT TS.Descripcion
                    FROM ProveedorServicio_TipoServicio PST
                    INNER JOIN TipoServicio TS ON PST.Id_TipoServicio = TS.Id_TipoServicio
                    WHERE PST.Id_Proveedor = @Id";
                DataTable tiposDt = BD.ExecuteQuery(tiposQuery, new Dictionary<string, object> { { "@Id", id } });
                ViewBag.TiposServicio = tiposDt;

                // Obtener especies del proveedor
                string especiesQuery = @"
                    SELECT Especie
                    FROM ProveedorServicio_Especie
                    WHERE Id_Proveedor = @Id";
                DataTable especiesDt = BD.ExecuteQuery(especiesQuery, new Dictionary<string, object> { { "@Id", id } });
                ViewBag.Especies = especiesDt;

                // Obtener reseñas
                string resenasQuery = @"
                    SELECT 
                        R.Id_Resena,
                        R.Calificacion,
                        R.Comentario,
                        R.Fecha,
                        U.Nombre,
                        U.Apellido
                    FROM Resena R
                    INNER JOIN [User] U ON R.Id_User = U.Id_User
                    WHERE R.Id_Proveedor = @Id
                    ORDER BY R.Fecha DESC";
                DataTable resenasDt = BD.ExecuteQuery(resenasQuery, new Dictionary<string, object> { { "@Id", id } });
                ViewBag.Resenas = resenasDt;

                return View();
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Error en BuscarProveedor/Perfil: " + ex.Message);
                TempData["Error"] = "Error al cargar el perfil.";
                return RedirectToAction("Index", "BuscarProveedor");
            }
        }

        // ============================
        // POST: /BuscarProveedor/CrearResena
        // ============================
        [HttpPost]
        [Route("BuscarProveedor/CrearResena")]
        public IActionResult CrearResena(int idProveedor, int calificacion, string comentario, int? idReserva = null)
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("UserId");
                if (userId == null)
                {
                    return Json(new { success = false, message = "No autorizado" });
                }

                // Verificar que no haya una reseña previa para esta reserva
                if (idReserva.HasValue)
                {
                    string checkQuery = "SELECT COUNT(*) FROM Resena WHERE Id_Reserva = @IdReserva AND Id_User = @UserId";
                    object? existeResult = BD.ExecuteScalar(checkQuery, new Dictionary<string, object>
                    {
                        { "@IdReserva", idReserva.Value },
                        { "@UserId", userId.Value }
                    });
                    int existe = existeResult != null && existeResult != DBNull.Value ? Convert.ToInt32(existeResult) : 0;
                    
                    if (existe > 0)
                    {
                        return Json(new { success = false, message = "Ya has reseñado este servicio" });
                    }
                }

                // Crear reseña
                string insertQuery = @"
                    INSERT INTO Resena (Id_User, Id_Proveedor, Id_Reserva, Calificacion, Comentario, Fecha)
                    VALUES (@IdUser, @IdProveedor, @IdReserva, @Calificacion, @Comentario, GETDATE())";

                BD.ExecuteNonQuery(insertQuery, new Dictionary<string, object>
                {
                    { "@IdUser", userId.Value },
                    { "@IdProveedor", idProveedor },
                    { "@IdReserva", idReserva ?? (object)DBNull.Value },
                    { "@Calificacion", calificacion },
                    { "@Comentario", comentario ?? "" }
                });

                // Actualizar calificación promedio del proveedor
                string updateCalificacionQuery = @"
                    UPDATE ProveedorServicio
                    SET Calificacion_Promedio = (
                            SELECT AVG(CAST(Calificacion AS DECIMAL(4,2)))
                            FROM Resena
                            WHERE Id_Proveedor = @IdProveedor
                        ),
                        Cantidad_Resenas = (
                            SELECT COUNT(*)
                            FROM Resena
                            WHERE Id_Proveedor = @IdProveedor
                        )
                    WHERE Id_Proveedor = @IdProveedor";

                BD.ExecuteNonQuery(updateCalificacionQuery, new Dictionary<string, object>
                {
                    { "@IdProveedor", idProveedor }
                });

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Error en BuscarProveedor/CrearResena: " + ex.Message);
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ============================
        // GET: /BuscarProveedor/Contratar/{id}
        // ============================
        [HttpGet]
        [Route("BuscarProveedor/Contratar/{id}")]
        public IActionResult Contratar(int id)
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("UserId");
                if (userId == null)
                {
                    TempData["Error"] = "Debés iniciar sesión para contratar un servicio.";
                    return RedirectToAction("Login", "Auth");
                }

                // Obtener datos del proveedor
                string proveedorQuery = @"
                    SELECT P.*
                    FROM ProveedorServicio P
                    WHERE P.Id_Proveedor = @Id AND P.Estado = 1";
                
                DataTable proveedorDt = BD.ExecuteQuery(proveedorQuery, new Dictionary<string, object> { { "@Id", id } });
                if (proveedorDt.Rows.Count == 0)
                {
                    TempData["Error"] = "Proveedor no encontrado.";
                    return RedirectToAction("Index", "BuscarProveedor");
                }

                ViewBag.Proveedor = proveedorDt.Rows[0];

                // Obtener tipos de servicio del proveedor
                string tiposQuery = @"
                    SELECT TS.Id_TipoServicio, TS.Descripcion
                    FROM ProveedorServicio_TipoServicio PST
                    INNER JOIN TipoServicio TS ON PST.Id_TipoServicio = TS.Id_TipoServicio
                    WHERE PST.Id_Proveedor = @Id";
                DataTable tiposDt = BD.ExecuteQuery(tiposQuery, new Dictionary<string, object> { { "@Id", id } });
                ViewBag.TiposServicio = tiposDt;

                // Obtener mascotas del usuario
                string mascotasQuery = @"
                    SELECT Id_Mascota, Nombre, Especie, Raza
                    FROM Mascota
                    WHERE Id_User = @UserId";
                DataTable mascotasDt = BD.ExecuteQuery(mascotasQuery, new Dictionary<string, object> { { "@UserId", userId.Value } });
                ViewBag.Mascotas = mascotasDt;

                return View();
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Error en BuscarProveedor/Contratar: " + ex.Message);
                TempData["Error"] = "Error al cargar la página de contratación.";
                return RedirectToAction("Index", "BuscarProveedor");
            }
        }

        // ============================
        // POST: /BuscarProveedor/CrearReserva
        // ============================
        [HttpPost]
        [Route("BuscarProveedor/CrearReserva")]
        public IActionResult CrearReserva(int idProveedor, int idMascota, int idTipoServicio, DateTime fechaInicio, TimeSpan horaInicio, decimal? duracionHoras, string? notas)
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("UserId");
                if (userId == null)
                {
                    return Json(new { success = false, message = "No autorizado" });
                }

                // Obtener precio del proveedor
                string precioQuery = "SELECT Precio_Hora FROM ProveedorServicio WHERE Id_Proveedor = @IdProveedor";
                DataTable precioDt = BD.ExecuteQuery(precioQuery, new Dictionary<string, object> { { "@IdProveedor", idProveedor } });
                decimal precioHora = 0;
                if (precioDt.Rows.Count > 0 && precioDt.Rows[0]["Precio_Hora"] != DBNull.Value)
                {
                    precioHora = Convert.ToDecimal(precioDt.Rows[0]["Precio_Hora"]);
                }

                decimal duracion = duracionHoras ?? 1.0M;
                decimal precioTotal = precioHora * duracion;

                // Crear reserva
                string insertQuery = @"
                    INSERT INTO ReservaProveedor 
                    (Id_User, Id_Proveedor, Id_Mascota, Id_TipoServicio, Fecha_Inicio, Hora_Inicio, Duracion_Horas, Precio_Total, Id_EstadoReserva, Notas, Fecha_Creacion)
                    VALUES 
                    (@IdUser, @IdProveedor, @IdMascota, @IdTipoServicio, @FechaInicio, @HoraInicio, @DuracionHoras, @PrecioTotal, 1, @Notas, GETDATE())";
                
                BD.ExecuteNonQuery(insertQuery, new Dictionary<string, object>
                {
                    { "@IdUser", userId.Value },
                    { "@IdProveedor", idProveedor },
                    { "@IdMascota", idMascota },
                    { "@IdTipoServicio", idTipoServicio },
                    { "@FechaInicio", fechaInicio },
                    { "@HoraInicio", horaInicio },
                    { "@DuracionHoras", duracion },
                    { "@PrecioTotal", precioTotal },
                    { "@Notas", notas ?? "" }
                });

                // Obtener información del proveedor para la notificación
                string qProveedor = @"
                    SELECT PS.Id_User, PS.NombreCompleto, TS.Descripcion as TipoServicio
                    FROM ProveedorServicio PS
                    INNER JOIN TipoServicio TS ON @IdTipoServicio = TS.Id_TipoServicio
                    WHERE PS.Id_Proveedor = @IdProveedor";
                
                DataTable dtProv = BD.ExecuteQuery(qProveedor, new Dictionary<string, object>
                {
                    { "@IdProveedor", idProveedor },
                    { "@IdTipoServicio", idTipoServicio }
                });

                if (dtProv.Rows.Count > 0)
                {
                    int idProveedorUser = Convert.ToInt32(dtProv.Rows[0]["Id_User"]);
                    string tipoServicio = dtProv.Rows[0]["TipoServicio"]?.ToString() ?? "servicio";
                    string fechaFormateada = fechaInicio.ToString("dd/MM/yyyy");

                    NotificacionController.CrearNotificacion(
                        idProveedorUser,
                        "NuevaReserva",
                        "Nueva reserva recibida",
                        $"Tienes una nueva reserva de {tipoServicio} para el {fechaFormateada}",
                        idProveedor,
                        "/Proveedor/Reservas"
                    );
                }

                return Json(new { success = true, message = "Reserva creada exitosamente" });
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Error en BuscarProveedor/CrearReserva: " + ex.Message);
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ============================
        // POST: /BuscarProveedor/ObtenerOCrearChat
        // ============================
        [HttpPost]
        [Route("BuscarProveedor/ObtenerOCrearChat")]
        public IActionResult ObtenerOCrearChat([FromBody] int idProveedorUser)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return Json(new { success = false, message = "No autenticado" });

            try
            {
                // Buscar chat existente entre estos dos usuarios
                string qChatExistente = @"
                    SELECT TOP 1 c.Id_Chat
                    FROM Chat c
                    INNER JOIN ParticipanteChat pc1 ON c.Id_Chat = pc1.Id_Chat AND pc1.Id_User = @UserId
                    INNER JOIN ParticipanteChat pc2 ON c.Id_Chat = pc2.Id_Chat AND pc2.Id_User = @ProveedorUserId
                    WHERE c.EsGrupo = 0";

                var dtChat = BD.ExecuteQuery(qChatExistente, new Dictionary<string, object>
                {
                    { "@UserId", userId.Value },
                    { "@ProveedorUserId", idProveedorUser }
                });

                int chatId;
                if (dtChat.Rows.Count > 0)
                {
                    chatId = Convert.ToInt32(dtChat.Rows[0]["Id_Chat"]);
                }
                else
                {
                    // Crear nuevo chat individual
                    string qCrearChat = @"
                        INSERT INTO Chat (Nombre, EsGrupo, FechaCreacion)
                        VALUES (NULL, 0, GETDATE());
                        SELECT CAST(SCOPE_IDENTITY() AS INT);";
                    chatId = Convert.ToInt32(BD.ExecuteScalar(qCrearChat, new Dictionary<string, object>()));

                    // Agregar participantes
                    BD.ExecuteNonQuery(@"
                        INSERT INTO ParticipanteChat (Id_Chat, Id_User, Administrador, FechaIngreso)
                        VALUES (@ChatId, @UserId, 0, GETDATE())",
                        new Dictionary<string, object> { { "@ChatId", chatId }, { "@UserId", userId.Value } });

                    BD.ExecuteNonQuery(@"
                        INSERT INTO ParticipanteChat (Id_Chat, Id_User, Administrador, FechaIngreso)
                        VALUES (@ChatId, @ProveedorUserId, 0, GETDATE())",
                        new Dictionary<string, object> { { "@ChatId", chatId }, { "@ProveedorUserId", idProveedorUser } });
                }

                return Json(new { success = true, chatId = chatId });
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Error en ObtenerOCrearChat: " + ex.Message);
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ============================
        // GET: Obtener todos los proveedores (para planificador)
        // ============================
        [HttpGet]
        public IActionResult ObtenerProveedores()
        {
            try
            {
                AsegurarTablasProveedores();
                
                string query = @"
                    SELECT 
                        P.Id_Proveedor,
                        P.NombreCompleto,
                        P.Precio_Hora,
                        P.Estado
                    FROM ProveedorServicio P
                    WHERE P.Estado = 1
                    ORDER BY P.NombreCompleto ASC";
                
                DataTable dt = BD.ExecuteQuery(query, new Dictionary<string, object>());
                var proveedores = new List<object>();
                
                foreach (DataRow row in dt.Rows)
                {
                    proveedores.Add(new
                    {
                        id = Convert.ToInt32(row["Id_Proveedor"]),
                        nombre = EncryptionHelper.Decrypt(row["NombreCompleto"].ToString() ?? ""),
                        precioHora = row["Precio_Hora"] != DBNull.Value ? Convert.ToDecimal(row["Precio_Hora"]) : 0
                    });
                }
                
                return Json(new { success = true, proveedores = proveedores });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error ObtenerProveedores: " + ex.Message);
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ============================
        // GET: Obtener tipos de servicio de un proveedor
        // ============================
        [HttpGet]
        [Route("BuscarProveedor/ObtenerTiposServicio/{idProveedor}")]
        public IActionResult ObtenerTiposServicio(int idProveedor)
        {
            try
            {
                string query = @"
                    SELECT TS.Id_TipoServicio, TS.Descripcion
                    FROM ProveedorServicio_TipoServicio PSTS
                    INNER JOIN TipoServicio TS ON PSTS.Id_TipoServicio = TS.Id_TipoServicio
                    WHERE PSTS.Id_Proveedor = @IdProveedor
                    ORDER BY TS.Descripcion ASC";
                
                DataTable dt = BD.ExecuteQuery(query, new Dictionary<string, object> { { "@IdProveedor", idProveedor } });
                var tipos = new List<object>();
                
                foreach (DataRow row in dt.Rows)
                {
                    tipos.Add(new
                    {
                        id = Convert.ToInt32(row["Id_TipoServicio"]),
                        descripcion = row["Descripcion"].ToString()
                    });
                }
                
                return Json(new { success = true, tipos = tipos });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error ObtenerTiposServicio: " + ex.Message);
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}

