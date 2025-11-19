using Microsoft.AspNetCore.Mvc;
using Zooni.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text.Json;

namespace Zooni.Controllers
{
    public class PaseoController : BaseController
    {
        // Método para asegurar que las tablas existan
        private void AsegurarTablasProveedores()
        {
            try
            {
                string checkTable = "SELECT COUNT(*) FROM sys.tables WHERE name = 'ProveedorServicio'";
                object? tableExists = BD.ExecuteScalar(checkTable);
                if (tableExists == null || Convert.ToInt32(tableExists) == 0)
                {
                    Console.WriteLine("⚠️ La tabla ProveedorServicio no existe. Se debe ejecutar el script SQL.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("⚠️ Error al verificar tablas: " + ex.Message);
            }
        }

        // ============================
        // GET: /Paseo/MapaActivos
        // Mapa con proveedores activos y sus mascotas
        // ============================
        [HttpGet]
        [Route("Paseo/MapaActivos")]
        public IActionResult MapaActivos()
        {
            AsegurarTablasProveedores();
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            return View();
        }

        // ============================
        // GET: /Paseo/ProveedoresActivos
        // API para obtener proveedores con servicios activos
        // ============================
        [HttpGet]
        [Route("Paseo/ProveedoresActivos")]
        public IActionResult ProveedoresActivos()
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("UserId");
                if (userId == null)
                {
                    return Json(new { success = false, message = "No autorizado" });
                }

                // Obtener proveedores con servicios activos (Estado = 3 = EnCurso)
                // Mostrar todos los proveedores activos, no solo los del usuario
                string query = @"
                    SELECT DISTINCT
                        P.Id_Proveedor,
                        P.NombreCompleto,
                        P.Latitud,
                        P.Longitud,
                        PS.Id_Reserva,
                        PS.Id_Mascota,
                        M.Nombre AS MascotaNombre,
                        M.Foto AS MascotaFoto,
                        M.Especie AS MascotaEspecie,
                        U.Nombre AS DuenioNombre,
                        U.Apellido AS DuenioApellido,
                        TS.Descripcion AS TipoServicio
                    FROM ProveedorServicio P
                    INNER JOIN ReservaProveedor PS ON P.Id_Proveedor = PS.Id_Proveedor
                    INNER JOIN Mascota M ON PS.Id_Mascota = M.Id_Mascota
                    INNER JOIN [User] U ON PS.Id_User = U.Id_User
                    INNER JOIN TipoServicio TS ON PS.Id_TipoServicio = TS.Id_TipoServicio
                    WHERE PS.Id_EstadoReserva = 3 -- EnCurso
                      AND P.Latitud IS NOT NULL
                      AND P.Longitud IS NOT NULL";

                DataTable dt = BD.ExecuteQuery(query, new Dictionary<string, object>());

                // Agrupar por proveedor
                var proveedores = new Dictionary<int, object>();
                foreach (DataRow row in dt.Rows)
                {
                    int idProveedor = Convert.ToInt32(row["Id_Proveedor"]);
                    if (!proveedores.ContainsKey(idProveedor))
                    {
                        proveedores[idProveedor] = new
                        {
                            IdProveedor = idProveedor,
                            Nombre = row["NombreCompleto"].ToString(),
                            Latitud = Convert.ToDecimal(row["Latitud"]),
                            Longitud = Convert.ToDecimal(row["Longitud"]),
                            TipoServicio = row["TipoServicio"].ToString(),
                            Mascotas = new List<object>()
                        };
                    }

                    var proveedor = proveedores[idProveedor] as dynamic;
                    var mascotas = proveedor.Mascotas as List<object>;
                    mascotas.Add(new
                    {
                        IdReserva = Convert.ToInt32(row["Id_Reserva"]),
                        IdMascota = Convert.ToInt32(row["Id_Mascota"]),
                        Nombre = row["MascotaNombre"].ToString(),
                        Foto = row["MascotaFoto"]?.ToString() ?? "",
                        Especie = row["MascotaEspecie"].ToString(),
                        DuenioNombre = row["DuenioNombre"].ToString(),
                        DuenioApellido = row["DuenioApellido"].ToString()
                    });
                }

                return Json(new { success = true, proveedores = proveedores.Values });
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Error en Paseo/ProveedoresActivos: " + ex.Message);
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ============================
        // GET: /Paseo/Tracking/{idReserva}
        // Vista de tracking en tiempo real del paseo
        // ============================
        [HttpGet]
        [Route("Paseo/Tracking/{idReserva}")]
        public IActionResult Tracking(int idReserva)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            // Verificar que la reserva pertenece al usuario o es el proveedor
            try
            {
                string query = @"
                    SELECT 
                        RP.*,
                        P.NombreCompleto AS ProveedorNombre,
                        M.Nombre AS MascotaNombre,
                        M.Foto AS MascotaFoto,
                        TS.Descripcion AS TipoServicio
                    FROM ReservaProveedor RP
                    INNER JOIN ProveedorServicio P ON RP.Id_Proveedor = P.Id_Proveedor
                    INNER JOIN Mascota M ON RP.Id_Mascota = M.Id_Mascota
                    INNER JOIN TipoServicio TS ON RP.Id_TipoServicio = TS.Id_TipoServicio
                    WHERE RP.Id_Reserva = @IdReserva
                      AND (RP.Id_User = @UserId OR P.Id_User = @UserId)";

                DataTable dt = BD.ExecuteQuery(query, new Dictionary<string, object> 
                { 
                    { "@IdReserva", idReserva },
                    { "@UserId", userId.Value }
                });

                if (dt.Rows.Count == 0)
                {
                    TempData["Error"] = "Reserva no encontrada o no autorizada.";
                    return RedirectToAction("Index", "Home");
                }

                ViewBag.Reserva = dt.Rows[0];
                ViewBag.IdReserva = idReserva;
                return View();
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Error en Paseo/Tracking: " + ex.Message);
                TempData["Error"] = "Error al cargar el tracking.";
                return RedirectToAction("Index", "Home");
            }
        }

        // ============================
        // POST: /Paseo/GuardarUbicacion
        // Guardar ubicación durante el paseo
        // ============================
        [HttpPost]
        [Route("Paseo/GuardarUbicacion")]
        public IActionResult GuardarUbicacion(int idReserva, decimal latitud, decimal longitud)
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("UserId");
                if (userId == null)
                {
                    return Json(new { success = false, message = "No autorizado" });
                }

                // Verificar que la reserva está activa
                string checkQuery = @"
                    SELECT RP.*, P.Id_User AS IdProveedorUser
                    FROM ReservaProveedor RP
                    INNER JOIN ProveedorServicio P ON RP.Id_Proveedor = P.Id_Proveedor
                    WHERE RP.Id_Reserva = @IdReserva
                      AND RP.Id_EstadoReserva = 3
                      AND (RP.Id_User = @UserId OR P.Id_User = @UserId)";

                DataTable dt = BD.ExecuteQuery(checkQuery, new Dictionary<string, object>
                {
                    { "@IdReserva", idReserva },
                    { "@UserId", userId.Value }
                });

                if (dt.Rows.Count == 0)
                {
                    return Json(new { success = false, message = "Reserva no encontrada o no activa" });
                }

                var reserva = dt.Rows[0];
                int idProveedor = Convert.ToInt32(reserva["Id_Proveedor"]);

                // Obtener última ubicación para calcular distancia
                decimal distanciaAcumulada = 0;
                int tiempoTranscurrido = 0;
                DateTime? fechaInicioReal = null;

                string ultimaUbicacionQuery = @"
                    SELECT TOP 1 
                        Latitud,
                        Longitud,
                        Distancia_Acumulada_Metros,
                        Tiempo_Transcurrido_Segundos,
                        Fecha_Hora
                    FROM UbicacionServicio
                    WHERE Id_Reserva = @IdReserva
                    ORDER BY Fecha_Hora DESC";

                DataTable ultimaUbicacionDt = BD.ExecuteQuery(ultimaUbicacionQuery, new Dictionary<string, object> { { "@IdReserva", idReserva } });

                if (ultimaUbicacionDt.Rows.Count > 0)
                {
                    distanciaAcumulada = Convert.ToDecimal(ultimaUbicacionDt.Rows[0]["Distancia_Acumulada_Metros"] ?? 0);
                    tiempoTranscurrido = Convert.ToInt32(ultimaUbicacionDt.Rows[0]["Tiempo_Transcurrido_Segundos"] ?? 0);
                    fechaInicioReal = Convert.ToDateTime(ultimaUbicacionDt.Rows[0]["Fecha_Hora"]);
                }
                else
                {
                    // Primera ubicación - marcar inicio real
                    fechaInicioReal = DateTime.Now;
                    string updateInicioQuery = @"
                        UPDATE ReservaProveedor 
                        SET Fecha_Hora_Inicio_Real = @FechaInicio
                        WHERE Id_Reserva = @IdReserva";
                    BD.ExecuteNonQuery(updateInicioQuery, new Dictionary<string, object>
                    {
                        { "@IdReserva", idReserva },
                        { "@FechaInicio", fechaInicioReal.Value }
                    });
                }

                // Calcular distancia desde última ubicación
                if (ultimaUbicacionDt.Rows.Count > 0 && ultimaUbicacionDt.Rows[0]["Latitud"] != DBNull.Value)
                {
                    decimal ultimaLat = Convert.ToDecimal(ultimaUbicacionDt.Rows[0]["Latitud"]);
                    decimal ultimaLng = Convert.ToDecimal(ultimaUbicacionDt.Rows[0]["Longitud"]);
                    decimal distanciaIncremental = CalcularDistanciaMetros(ultimaLat, ultimaLng, latitud, longitud);
                    distanciaAcumulada = Convert.ToDecimal(ultimaUbicacionDt.Rows[0]["Distancia_Acumulada_Metros"] ?? 0) + distanciaIncremental;
                }

                // Calcular tiempo transcurrido
                if (fechaInicioReal.HasValue)
                {
                    tiempoTranscurrido = (int)(DateTime.Now - fechaInicioReal.Value).TotalSeconds;
                }

                // Guardar ubicación
                string insertQuery = @"
                    INSERT INTO UbicacionServicio 
                    (Id_Reserva, Id_Proveedor, Latitud, Longitud, Fecha_Hora, Tipo, Distancia_Acumulada_Metros, Tiempo_Transcurrido_Segundos)
                    VALUES 
                    (@IdReserva, @IdProveedor, @Latitud, @Longitud, GETDATE(), 'Proveedor', @Distancia, @Tiempo)";

                BD.ExecuteNonQuery(insertQuery, new Dictionary<string, object>
                {
                    { "@IdReserva", idReserva },
                    { "@IdProveedor", idProveedor },
                    { "@Latitud", latitud },
                    { "@Longitud", longitud },
                    { "@Distancia", distanciaAcumulada },
                    { "@Tiempo", tiempoTranscurrido }
                });

                return Json(new 
                { 
                    success = true, 
                    distanciaAcumulada = distanciaAcumulada,
                    tiempoTranscurrido = tiempoTranscurrido
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Error en Paseo/GuardarUbicacion: " + ex.Message);
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ============================
        // GET: /Paseo/ObtenerRuta/{idReserva}
        // Obtener ruta completa del paseo
        // ============================
        [HttpGet]
        [Route("Paseo/ObtenerRuta/{idReserva}")]
        public IActionResult ObtenerRuta(int idReserva)
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("UserId");
                if (userId == null)
                {
                    return Json(new { success = false, message = "No autorizado" });
                }

                string query = @"
                    SELECT 
                        Latitud,
                        Longitud,
                        Fecha_Hora,
                        Distancia_Acumulada_Metros,
                        Tiempo_Transcurrido_Segundos
                    FROM UbicacionServicio
                    WHERE Id_Reserva = @IdReserva
                    ORDER BY Fecha_Hora ASC";

                DataTable dt = BD.ExecuteQuery(query, new Dictionary<string, object> { { "@IdReserva", idReserva } });

                var puntos = new List<object>();
                foreach (DataRow row in dt.Rows)
                {
                    puntos.Add(new
                    {
                        lat = Convert.ToDecimal(row["Latitud"]),
                        lng = Convert.ToDecimal(row["Longitud"]),
                        fechaHora = Convert.ToDateTime(row["Fecha_Hora"]).ToString("yyyy-MM-dd HH:mm:ss"),
                        distancia = Convert.ToDecimal(row["Distancia_Acumulada_Metros"] ?? 0),
                        tiempo = Convert.ToInt32(row["Tiempo_Transcurrido_Segundos"] ?? 0)
                    });
                }

                return Json(new { success = true, puntos = puntos });
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Error en Paseo/ObtenerRuta: " + ex.Message);
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ============================
        // POST: /Paseo/FinalizarPaseo
        // Finalizar paseo y generar resumen
        // ============================
        [HttpPost]
        [Route("Paseo/FinalizarPaseo")]
        public IActionResult FinalizarPaseo(int idReserva)
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("UserId");
                if (userId == null)
                {
                    return Json(new { success = false, message = "No autorizado" });
                }

                // Obtener última ubicación con estadísticas
                string ultimaQuery = @"
                    SELECT TOP 1 
                        Distancia_Acumulada_Metros,
                        Tiempo_Transcurrido_Segundos
                    FROM UbicacionServicio
                    WHERE Id_Reserva = @IdReserva
                    ORDER BY Fecha_Hora DESC";

                DataTable ultimaDt = BD.ExecuteQuery(ultimaQuery, new Dictionary<string, object> { { "@IdReserva", idReserva } });

                if (ultimaDt.Rows.Count == 0)
                {
                    return Json(new { success = false, message = "No hay datos de tracking" });
                }

                decimal distanciaTotal = Convert.ToDecimal(ultimaDt.Rows[0]["Distancia_Acumulada_Metros"] ?? 0);
                int tiempoTotal = Convert.ToInt32(ultimaDt.Rows[0]["Tiempo_Transcurrido_Segundos"] ?? 0);

                // Obtener ruta completa como JSON
                string rutaQuery = @"
                    SELECT Latitud, Longitud
                    FROM UbicacionServicio
                    WHERE Id_Reserva = @IdReserva
                    ORDER BY Fecha_Hora ASC";

                DataTable rutaDt = BD.ExecuteQuery(rutaQuery, new Dictionary<string, object> { { "@IdReserva", idReserva } });
                var ruta = new List<object>();
                foreach (DataRow row in rutaDt.Rows)
                {
                    ruta.Add(new
                    {
                        lat = Convert.ToDecimal(row["Latitud"]),
                        lng = Convert.ToDecimal(row["Longitud"])
                    });
                }

                string rutaJson = JsonSerializer.Serialize(ruta);

                // Actualizar reserva
                string updateQuery = @"
                    UPDATE ReservaProveedor 
                    SET Id_EstadoReserva = 4, -- Completada
                        Fecha_Hora_Fin_Real = GETDATE(),
                        Distancia_Total_Metros = @Distancia,
                        Tiempo_Total_Segundos = @Tiempo,
                        Ruta_GPS_JSON = @Ruta
                    WHERE Id_Reserva = @IdReserva";

                BD.ExecuteNonQuery(updateQuery, new Dictionary<string, object>
                {
                    { "@IdReserva", idReserva },
                    { "@Distancia", distanciaTotal },
                    { "@Tiempo", tiempoTotal },
                    { "@Ruta", rutaJson }
                });

                return Json(new { success = true, idReserva = idReserva });
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Error en Paseo/FinalizarPaseo: " + ex.Message);
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ============================
        // GET: /Paseo/Resumen/{idReserva}
        // Vista de resumen del paseo
        // ============================
        [HttpGet]
        [Route("Paseo/Resumen/{idReserva}")]
        public IActionResult Resumen(int idReserva)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            try
            {
                string query = @"
                    SELECT 
                        RP.*,
                        P.NombreCompleto AS ProveedorNombre,
                        P.Id_Proveedor,
                        M.Nombre AS MascotaNombre,
                        M.Foto AS MascotaFoto,
                        TS.Descripcion AS TipoServicio
                    FROM ReservaProveedor RP
                    INNER JOIN ProveedorServicio P ON RP.Id_Proveedor = P.Id_Proveedor
                    INNER JOIN Mascota M ON RP.Id_Mascota = M.Id_Mascota
                    INNER JOIN TipoServicio TS ON RP.Id_TipoServicio = TS.Id_TipoServicio
                    WHERE RP.Id_Reserva = @IdReserva
                      AND (RP.Id_User = @UserId OR P.Id_User = @UserId)";

                DataTable dt = BD.ExecuteQuery(query, new Dictionary<string, object>
                {
                    { "@IdReserva", idReserva },
                    { "@UserId", userId.Value }
                });

                if (dt.Rows.Count == 0)
                {
                    TempData["Error"] = "Reserva no encontrada.";
                    return RedirectToAction("Index", "Home");
                }

                ViewBag.Reserva = dt.Rows[0];
                ViewBag.IdReserva = idReserva;
                return View();
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Error en Paseo/Resumen: " + ex.Message);
                TempData["Error"] = "Error al cargar el resumen.";
                return RedirectToAction("Index", "Home");
            }
        }

        // Función auxiliar para calcular distancia en metros (Haversine)
        private decimal CalcularDistanciaMetros(decimal lat1, decimal lng1, decimal lat2, decimal lng2)
        {
            const double R = 6371000; // Radio de la Tierra en metros
            double dLat = (double)(lat2 - lat1) * Math.PI / 180.0;
            double dLng = (double)(lng2 - lng1) * Math.PI / 180.0;
            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                       Math.Cos((double)lat1 * Math.PI / 180.0) * Math.Cos((double)lat2 * Math.PI / 180.0) *
                       Math.Sin(dLng / 2) * Math.Sin(dLng / 2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return (decimal)(R * c);
        }
    }
}

