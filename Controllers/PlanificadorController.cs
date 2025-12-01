using Microsoft.AspNetCore.Mvc;
using Zooni.Models;
using Zooni.Utils;
using Zooni.Controllers;
using System;
using System.Collections.Generic;
using System.Data;

namespace Zooni.Controllers
{
    public class PlanificadorController : BaseController
    {
        // Asegurar que existan las tablas necesarias
        private void AsegurarTablasReservas()
        {
            try
            {
                // Crear tabla ReservaProveedor si no existe
                string crearTabla = @"
                    IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ReservaProveedor]') AND type in (N'U'))
                    BEGIN
                        CREATE TABLE [dbo].[ReservaProveedor](
                            [Id_Reserva] [int] IDENTITY(1,1) NOT NULL,
                            [Id_User] [int] NOT NULL,
                            [Id_Proveedor] [int] NOT NULL,
                            [Id_Mascota] [int] NOT NULL,
                            [Id_TipoServicio] [int] NOT NULL,
                            [Fecha_Inicio] [datetime2](7) NOT NULL,
                            [Fecha_Fin] [datetime2](7) NULL,
                            [Hora_Inicio] [time](0) NOT NULL,
                            [Hora_Fin] [time](0) NULL,
                            [Duracion_Horas] [decimal](5,2) NULL,
                            [Precio_Total] [decimal](12, 2) NOT NULL,
                            [Id_EstadoReserva] [int] NOT NULL DEFAULT 1,
                            [Notas] [nvarchar](1000) NULL,
                            [Direccion_Servicio] [nvarchar](500) NULL,
                            [Latitud_Servicio] [decimal](10, 8) NULL,
                            [Longitud_Servicio] [decimal](11, 8) NULL,
                            [Compartir_Ubicacion] [bit] NOT NULL DEFAULT 0,
                            [Fecha_Creacion] [datetime2](7) NOT NULL DEFAULT GETDATE(),
                            CONSTRAINT [PK_ReservaProveedor] PRIMARY KEY CLUSTERED ([Id_Reserva] ASC)
                        )
                    END";
                BD.ExecuteNonQuery(crearTabla, new Dictionary<string, object>());
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error creando tabla ReservaProveedor: " + ex.Message);
            }
        }

        // ============================
        // GET: Planificador de Servicios
        // ============================
        [HttpGet]
        public IActionResult Index()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Auth");

            // Redirigir proveedores a su dashboard
            var redirect = RedirigirProveedorSiEsNecesario();
            if (redirect != null) return redirect;

            // Asegurar que existan las tablas
            AsegurarTablasReservas();

            try
            {
                // Obtener mascotas del usuario (solo una por combinación de nombre y raza)
                string mascotasQuery = @"
                    WITH MascotasUnicas AS (
                        SELECT 
                            Id_Mascota, 
                            Nombre, 
                            Especie, 
                            Raza,
                            ROW_NUMBER() OVER (PARTITION BY Nombre, Raza ORDER BY Id_Mascota DESC) AS rn
                        FROM Mascota
                        WHERE Id_User = @UserId AND (Archivada IS NULL OR Archivada = 0)
                    )
                    SELECT Id_Mascota, Nombre, Especie, Raza
                    FROM MascotasUnicas
                    WHERE rn = 1
                    ORDER BY Nombre ASC";
                DataTable mascotasDt = BD.ExecuteQuery(mascotasQuery, new Dictionary<string, object> { { "@UserId", userId.Value } });
                ViewBag.Mascotas = mascotasDt;

                // Obtener reservas activas y próximas (manejo seguro si la tabla está vacía)
                try
                {
                    string reservasQuery = @"
                        SELECT 
                            RP.Id_Reserva,
                            RP.Fecha_Inicio,
                            RP.Hora_Inicio,
                            RP.Duracion_Horas,
                            RP.Precio_Total,
                            RP.Id_EstadoReserva,
                            ER.Descripcion AS Estado,
                            M.Nombre AS MascotaNombre,
                            M.Especie AS MascotaEspecie,
                            PS.NombreCompleto AS ProveedorNombre,
                            PS.Id_Proveedor,
                            TS.Descripcion AS TipoServicio,
                            RP.Notas
                        FROM ReservaProveedor RP
                        INNER JOIN Mascota M ON RP.Id_Mascota = M.Id_Mascota
                        INNER JOIN ProveedorServicio PS ON RP.Id_Proveedor = PS.Id_Proveedor
                        INNER JOIN TipoServicio TS ON RP.Id_TipoServicio = TS.Id_TipoServicio
                        LEFT JOIN EstadoReserva ER ON RP.Id_EstadoReserva = ER.Id_EstadoReserva
                        WHERE RP.Id_User = @UserId
                          AND RP.Fecha_Inicio >= CAST(GETDATE() AS DATE)
                        ORDER BY RP.Fecha_Inicio ASC, RP.Hora_Inicio ASC";
                    
                    DataTable reservasDt = BD.ExecuteQuery(reservasQuery, new Dictionary<string, object> { { "@UserId", userId.Value } });
                    ViewBag.Reservas = reservasDt;
                }
                catch
                {
                    ViewBag.Reservas = new DataTable();
                }

                // Obtener proveedores favoritos (manejo seguro)
                try
                {
                    string favoritosQuery = @"
                        SELECT TOP 5
                            PS.Id_Proveedor,
                            PS.NombreCompleto,
                            PS.Precio_Hora,
                            COUNT(RP.Id_Reserva) AS CantidadReservas,
                            AVG(CAST(R.Calificacion AS FLOAT)) AS CalificacionPromedio
                        FROM ProveedorServicio PS
                        INNER JOIN ReservaProveedor RP ON PS.Id_Proveedor = RP.Id_Proveedor
                        LEFT JOIN Resena R ON PS.Id_Proveedor = R.Id_Proveedor
                        WHERE RP.Id_User = @UserId
                        GROUP BY PS.Id_Proveedor, PS.NombreCompleto, PS.Precio_Hora
                        ORDER BY CantidadReservas DESC";
                    
                    DataTable favoritosDt = BD.ExecuteQuery(favoritosQuery, new Dictionary<string, object> { { "@UserId", userId.Value } });
                    ViewBag.Favoritos = favoritosDt;
                }
                catch
                {
                    ViewBag.Favoritos = new DataTable();
                }

                ViewBag.Tema = HttpContext.Session.GetString("Tema") ?? "claro";
                return View("~/Views/Home/Planificador/Index.cshtml");
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Error en Planificador/Index: " + ex.Message);
                TempData["Error"] = "Error al cargar el planificador.";
                return RedirectToAction("Index", "Home");
            }
        }

        // ============================
        // GET: Obtener disponibilidad de proveedor
        // ============================
        [HttpGet]
        public IActionResult ObtenerDisponibilidad(int idProveedor, DateTime fecha)
        {
            try
            {
                // Obtener reservas del proveedor para esa fecha
                string query = @"
                    SELECT Hora_Inicio, Duracion_Horas
                    FROM ReservaProveedor
                    WHERE Id_Proveedor = @IdProveedor
                      AND CAST(Fecha_Inicio AS DATE) = @Fecha
                      AND Id_EstadoReserva IN (1, 2, 3) -- Pendiente, Confirmada, EnCurso
                    ORDER BY Hora_Inicio ASC";
                
                DataTable dt = BD.ExecuteQuery(query, new Dictionary<string, object>
                {
                    { "@IdProveedor", idProveedor },
                    { "@Fecha", fecha.Date }
                });

                var horariosOcupados = new List<object>();
                foreach (DataRow row in dt.Rows)
                {
                    TimeSpan horaInicio = (TimeSpan)row["Hora_Inicio"];
                    decimal duracion = Convert.ToDecimal(row["Duracion_Horas"]);
                    TimeSpan horaFin = horaInicio.Add(TimeSpan.FromHours((double)duracion));
                    
                    horariosOcupados.Add(new
                    {
                        inicio = horaInicio.ToString(@"hh\:mm"),
                        fin = horaFin.ToString(@"hh\:mm")
                    });
                }

                return Json(new { success = true, horariosOcupados = horariosOcupados });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error ObtenerDisponibilidad: " + ex.Message);
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ============================
        // POST: Crear reserva desde planificador
        // ============================
        [HttpPost]
        public IActionResult CrearReserva([FromBody] dynamic request)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return Json(new { success = false, message = "No autenticado" });

            try
            {
                int idProveedor = Convert.ToInt32(request.idProveedor);
                int idMascota = Convert.ToInt32(request.idMascota);
                int idTipoServicio = Convert.ToInt32(request.idTipoServicio);
                DateTime fechaInicio = Convert.ToDateTime(request.fechaInicio);
                TimeSpan horaInicio = TimeSpan.Parse(request.horaInicio.ToString());
                decimal duracionHoras = Convert.ToDecimal(request.duracionHoras);
                string? notas = request.notas?.ToString();
                string? direccionServicio = request.direccionServicio?.ToString();
                decimal? latitudServicio = request.latitudServicio != null ? Convert.ToDecimal(request.latitudServicio) : null;
                decimal? longitudServicio = request.longitudServicio != null ? Convert.ToDecimal(request.longitudServicio) : null;
                bool compartirUbicacion = request.compartirUbicacion != null ? Convert.ToBoolean(request.compartirUbicacion) : false;

                // Obtener precio del proveedor
                string precioQuery = "SELECT Precio_Hora FROM ProveedorServicio WHERE Id_Proveedor = @IdProveedor";
                DataTable precioDt = BD.ExecuteQuery(precioQuery, new Dictionary<string, object> { { "@IdProveedor", idProveedor } });
                decimal precioHora = 0;
                if (precioDt.Rows.Count > 0 && precioDt.Rows[0]["Precio_Hora"] != DBNull.Value)
                {
                    precioHora = Convert.ToDecimal(precioDt.Rows[0]["Precio_Hora"]);
                }

                decimal precioTotal = precioHora * duracionHoras;

                // Verificar disponibilidad
                string checkDisponibilidad = @"
                    SELECT COUNT(*) 
                    FROM ReservaProveedor
                    WHERE Id_Proveedor = @IdProveedor
                      AND CAST(Fecha_Inicio AS DATE) = @Fecha
                      AND Id_EstadoReserva IN (1, 2, 3)
                      AND (
                          (@HoraInicio >= Hora_Inicio AND @HoraInicio < DATEADD(HOUR, Duracion_Horas, Hora_Inicio))
                          OR (DATEADD(HOUR, @Duracion, @HoraInicio) > Hora_Inicio AND DATEADD(HOUR, @Duracion, @HoraInicio) <= DATEADD(HOUR, Duracion_Horas, Hora_Inicio))
                          OR (@HoraInicio <= Hora_Inicio AND DATEADD(HOUR, @Duracion, @HoraInicio) >= DATEADD(HOUR, Duracion_Horas, Hora_Inicio))
                      )";
                
                int conflictos = Convert.ToInt32(BD.ExecuteScalar(checkDisponibilidad, new Dictionary<string, object>
                {
                    { "@IdProveedor", idProveedor },
                    { "@Fecha", fechaInicio.Date },
                    { "@HoraInicio", horaInicio },
                    { "@Duracion", duracionHoras }
                }));

                if (conflictos > 0)
                {
                    return Json(new { success = false, message = "El proveedor no está disponible en ese horario" });
                }

                // Crear reserva
                string insertQuery = @"
                    INSERT INTO ReservaProveedor 
                    (Id_User, Id_Proveedor, Id_Mascota, Id_TipoServicio, Fecha_Inicio, Hora_Inicio, Duracion_Horas, 
                     Precio_Total, Id_EstadoReserva, Notas, Direccion_Servicio, Latitud_Servicio, Longitud_Servicio, 
                     Compartir_Ubicacion, Fecha_Creacion)
                    VALUES 
                    (@IdUser, @IdProveedor, @IdMascota, @IdTipoServicio, @FechaInicio, @HoraInicio, @DuracionHoras, 
                     @PrecioTotal, 1, @Notas, @DireccionServicio, @LatitudServicio, @LongitudServicio, 
                     @CompartirUbicacion, GETDATE());
                    SELECT CAST(SCOPE_IDENTITY() AS INT);";
                
                int idReserva = Convert.ToInt32(BD.ExecuteScalar(insertQuery, new Dictionary<string, object>
                {
                    { "@IdUser", userId.Value },
                    { "@IdProveedor", idProveedor },
                    { "@IdMascota", idMascota },
                    { "@IdTipoServicio", idTipoServicio },
                    { "@FechaInicio", fechaInicio },
                    { "@HoraInicio", horaInicio },
                    { "@DuracionHoras", duracionHoras },
                    { "@PrecioTotal", precioTotal },
                    { "@Notas", notas ?? "" },
                    { "@DireccionServicio", direccionServicio ?? (object)DBNull.Value },
                    { "@LatitudServicio", latitudServicio ?? (object)DBNull.Value },
                    { "@LongitudServicio", longitudServicio ?? (object)DBNull.Value },
                    { "@CompartirUbicacion", compartirUbicacion }
                }));

                // Obtener información del proveedor para la notificación
                string qProveedor = @"
                    SELECT PS.Id_User, TS.Descripcion as TipoServicio
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

                return Json(new { success = true, idReserva = idReserva, message = "Reserva creada exitosamente" });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error CrearReserva: " + ex.Message);
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ============================
        // POST: Cancelar reserva
        // ============================
        [HttpPost]
        public IActionResult CancelarReserva([FromBody] dynamic request)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return Json(new { success = false, message = "No autenticado" });

            try
            {
                int idReserva = Convert.ToInt32(request.idReserva);

                // Verificar que la reserva pertenece al usuario
                string verificarQuery = "SELECT Id_User FROM ReservaProveedor WHERE Id_Reserva = @IdReserva";
                object? idUserResult = BD.ExecuteScalar(verificarQuery, new Dictionary<string, object> { { "@IdReserva", idReserva } });
                
                if (idUserResult == null || Convert.ToInt32(idUserResult) != userId.Value)
                {
                    return Json(new { success = false, message = "No autorizado" });
                }

                // Actualizar estado a Cancelada (5)
                string updateQuery = "UPDATE ReservaProveedor SET Id_EstadoReserva = 5 WHERE Id_Reserva = @IdReserva";
                BD.ExecuteNonQuery(updateQuery, new Dictionary<string, object> { { "@IdReserva", idReserva } });

                return Json(new { success = true, message = "Reserva cancelada exitosamente" });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error CancelarReserva: " + ex.Message);
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}

