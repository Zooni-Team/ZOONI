    using Microsoft.AspNetCore.Mvc;
using Zooni.Models;
using Microsoft.Extensions.Caching.Memory;
using System.Data;
using System.Collections.Generic;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.IO.Image;
using Zooni.Utils;
namespace Zooni.Controllers
{
    public class HomeController : BaseController
    {
        private readonly IMemoryCache _cache;
        
        public HomeController(IMemoryCache cache)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }
       private DataRow ObtenerMascotaActiva(int userId)
{
    // Asegurar que la tabla existe
    AsegurarTablaMascotaCompartida();

    int? mascotaId = HttpContext.Session.GetInt32("MascotaId");
    string query;
    Dictionary<string, object> param;

    if (mascotaId != null)
    {
        // Incluir mascotas propias y compartidas
        query = @"SELECT TOP 1 m.*, 
                    CASE WHEN m.PesoDisplay IS NULL OR m.PesoDisplay = '' THEN NULL ELSE m.PesoDisplay END AS PesoDisplay,
                    CASE WHEN m.Id_User = @UserId THEN 1 ELSE 0 END AS EsPropietario,
                    mc.Permiso_Edicion
                  FROM Mascota m
                  LEFT JOIN MascotaCompartida mc ON m.Id_Mascota = mc.Id_Mascota AND mc.Id_UsuarioCompartido = @UserId AND mc.Activo = 1
                  WHERE m.Id_Mascota = @Id 
                    AND (m.Id_User = @UserId OR mc.Id_UsuarioCompartido = @UserId)
                    AND (m.Archivada IS NULL OR m.Archivada = 0)";
        param = new Dictionary<string, object> { { "@Id", mascotaId.Value }, { "@UserId", userId } };
    }
    else
    {
        // Incluir mascotas propias y compartidas (solo activas, no archivadas)
        query = @"SELECT TOP 1 m.*, 
                    CASE WHEN m.PesoDisplay IS NULL OR m.PesoDisplay = '' THEN NULL ELSE m.PesoDisplay END AS PesoDisplay,
                    CASE WHEN m.Id_User = @UserId THEN 1 ELSE 0 END AS EsPropietario,
                    mc.Permiso_Edicion
                  FROM Mascota m
                  LEFT JOIN MascotaCompartida mc ON m.Id_Mascota = mc.Id_Mascota AND mc.Id_UsuarioCompartido = @UserId AND mc.Activo = 1
                  WHERE (m.Id_User = @UserId OR mc.Id_UsuarioCompartido = @UserId)
                    AND (m.Archivada IS NULL OR m.Archivada = 0)
                  ORDER BY m.Id_Mascota DESC";
        param = new Dictionary<string, object> { { "@UserId", userId } };
    }

    var dt = BD.ExecuteQuery(query, param);
    return dt.Rows.Count > 0 ? dt.Rows[0] : null;
}


        // 🔹 Método para setear los ViewBag básicos de mascota
        private void CargarViewBagMascota(DataRow mascota)
{
    if (mascota == null)
    {
        ViewBag.MascotaNombre = null;
        return;
    }

    ViewBag.MascotaNombre = mascota["Nombre"]?.ToString() ?? "Sin nombre";
    ViewBag.MascotaEspecie = mascota["Especie"]?.ToString() ?? "Desconocida";
    ViewBag.MascotaRaza = mascota["Raza"]?.ToString() ?? "";
    
    // 🎂 Calcular edad automáticamente desde Fecha_Nacimiento si existe
    DateTime? fechaNacimiento = null;
    if (mascota.Table.Columns.Contains("Fecha_Nacimiento") && mascota["Fecha_Nacimiento"] != DBNull.Value)
    {
        fechaNacimiento = Convert.ToDateTime(mascota["Fecha_Nacimiento"]);
        ViewBag.MascotaFechaNacimiento = fechaNacimiento.Value.ToString("yyyy-MM-dd");
        // Calcular edad automáticamente
        ViewBag.MascotaEdad = EdadHelper.CalcularEdadEnMeses(fechaNacimiento);
    }
    else
    {
        ViewBag.MascotaFechaNacimiento = null;
    ViewBag.MascotaEdad = mascota["Edad"] == DBNull.Value ? 0 : Convert.ToInt32(mascota["Edad"]);
    }
    
    // Manejo del peso usando PesoHelper - usar siempre el valor de la BD sin correcciones
    decimal pesoDecimal = 0;
    string? pesoDisplayBD = null;
    
    // Priorizar PesoDisplay de la BD si existe
    if (mascota.Table.Columns.Contains("PesoDisplay") && mascota["PesoDisplay"] != DBNull.Value)
    {
        pesoDisplayBD = mascota["PesoDisplay"].ToString();
    }
    
    // Obtener el peso decimal de la BD (usar el valor tal cual está guardado)
    if (mascota["Peso"] != DBNull.Value && decimal.TryParse(mascota["Peso"].ToString(), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out pesoDecimal))
    {
        // Usar el peso tal cual está en la BD, sin correcciones
        ViewBag.MascotaPeso = pesoDecimal;
        // Si hay PesoDisplay, usarlo; sino formatear el decimal
        ViewBag.MascotaPesoDisplay = pesoDisplayBD ?? PesoHelper.FormatearPeso(pesoDecimal);
    }
    else
    {
        ViewBag.MascotaPeso = 0.1M;
        ViewBag.MascotaPesoDisplay = pesoDisplayBD ?? "0,10 kg";
    }

    // 🟢 NUEVO: agregar la foto si existe
    ViewBag.MascotaFoto = mascota.Table.Columns.Contains("Foto") && mascota["Foto"] != DBNull.Value
        ? mascota["Foto"].ToString()
        : "";
    
    // 🎨 Avatar: usar el de sesión si existe, sino construir con la raza exacta de BD
    var especie = mascota["Especie"]?.ToString()?.ToLower() ?? "perro";
    var raza = mascota["Raza"]?.ToString() ?? "";
    
    var avatarSesion = HttpContext.Session.GetString("MascotaAvatar");
    if (!string.IsNullOrEmpty(avatarSesion))
    {
        ViewBag.MascotaAvatar = avatarSesion;
    }
    else
    {
        // Construir ruta: carpeta de la raza, pero archivo siempre _basico.png
        if (string.IsNullOrWhiteSpace(raza))
        {
            // Si no hay raza, usar "basico" como fallback
            ViewBag.MascotaAvatar = $"/img/mascotas/{especie}s/basico/{especie}_basico.png";
        }
        else
        {
            // Carpeta de la raza exacta de la BD, pero archivo siempre _basico.png
            ViewBag.MascotaAvatar = $"/img/mascotas/{especie}s/{raza}/{especie}_basico.png";
        }
    }
}


        [HttpGet]
        public IActionResult Index()
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("UserId");
                if (userId == null) return RedirectToAction("Login", "Auth");

                // Si es proveedor, redirigir al dashboard correspondiente
                string? esProveedor = HttpContext.Session.GetString("EsProveedor");
                if (esProveedor == "true")
                {
                    string tipoPrincipal = HttpContext.Session.GetString("ProveedorTipoPrincipal") ?? "";
                    if (tipoPrincipal == "Paseador")
                    {
                        return RedirectToAction("DashboardPaseador", "Proveedor");
                    }
                    else if (tipoPrincipal == "Cuidador")
                    {
                        return RedirectToAction("DashboardCuidador", "Proveedor");
                    }
                    else
                    {
                        return RedirectToAction("Dashboard", "Proveedor");
                    }
                }
var tema = HttpContext.Session.GetString("Tema") ?? "claro";
        ViewBag.Tema = tema;
                var param = new Dictionary<string, object> { { "@UserId", userId.Value } };
                var userDt = BD.ExecuteQuery("SELECT TOP 1 Nombre, Apellido FROM [User] WHERE Id_User = @UserId", param);

                if (userDt.Rows.Count == 0) return RedirectToAction("Login", "Auth");
                ViewBag.UserNombre = userDt.Rows[0]["Nombre"].ToString();

                // 🟢 Actualizar última actividad del usuario
                try
                {
                    AsegurarColumnasEstadoOnline();
                    string qUpdateActividad = @"
                        UPDATE [User] 
                        SET UltimaActividad = GETDATE(), EstadoOnline = 1
                        WHERE Id_User = @UserId";
                    BD.ExecuteNonQuery(qUpdateActividad, param);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("⚠️ Error al actualizar actividad: " + ex.Message);
                }

                var mascota = ObtenerMascotaActiva(userId.Value);
                CargarViewBagMascota(mascota);
                ViewBag.MascotaFoto = HttpContext.Session.GetString("MascotaFoto");

                // Cargar todas las mascotas activas para navegación (solo únicas por nombre y raza)
                // Si hay duplicados, tomar solo la más reciente (mayor Id_Mascota)
                string qTodasMascotas = @"
                    WITH MascotasUnicas AS (
                        SELECT 
                            m.Id_Mascota, 
                            m.Nombre, 
                            m.Especie, 
                            m.Raza, 
                            m.Foto,
                            ROW_NUMBER() OVER (PARTITION BY m.Nombre, m.Raza ORDER BY m.Id_Mascota DESC) AS rn
                        FROM Mascota m
                        LEFT JOIN MascotaCompartida mc ON m.Id_Mascota = mc.Id_Mascota AND mc.Id_UsuarioCompartido = @UserId AND mc.Activo = 1
                        WHERE (m.Id_User = @UserId OR mc.Id_UsuarioCompartido = @UserId)
                          AND (m.Archivada IS NULL OR m.Archivada = 0)
                    )
                    SELECT Id_Mascota, Nombre, Especie, Raza, Foto
                    FROM MascotasUnicas
                    WHERE rn = 1
                    ORDER BY Id_Mascota DESC";
                
                var dtTodasMascotas = BD.ExecuteQuery(qTodasMascotas, new Dictionary<string, object> { { "@UserId", userId.Value } });
                var listaMascotas = new List<Mascota>();
                foreach (System.Data.DataRow row in dtTodasMascotas.Rows)
                {
                    listaMascotas.Add(new Mascota
                    {
                        Id_Mascota = Convert.ToInt32(row["Id_Mascota"]),
                        Nombre = row["Nombre"]?.ToString() ?? "Sin nombre",
                        Especie = row["Especie"]?.ToString() ?? "Perro",
                        Raza = row["Raza"]?.ToString() ?? "",
                        Foto = row["Foto"]?.ToString() ?? ""
                    });
                }
                ViewBag.TodasMascotas = listaMascotas;
                
                // Índice de la mascota actual (buscar por nombre y raza si hay duplicados)
                int? mascotaIdActual = HttpContext.Session.GetInt32("MascotaId");
                int indiceActual = 0;
                if (mascotaIdActual != null)
                {
                    // Obtener nombre y raza de la mascota actual
                    string qMascotaActual = @"
                        SELECT TOP 1 Nombre, Raza
                        FROM Mascota
                        WHERE Id_Mascota = @Id";
                    var dtActual = BD.ExecuteQuery(qMascotaActual, new Dictionary<string, object> { { "@Id", mascotaIdActual.Value } });
                    
                    if (dtActual.Rows.Count > 0)
                    {
                        string nombreActual = dtActual.Rows[0]["Nombre"]?.ToString() ?? "";
                        string razaActual = dtActual.Rows[0]["Raza"]?.ToString() ?? "";
                        
                        // Buscar en la lista de mascotas únicas por nombre y raza
                        for (int i = 0; i < listaMascotas.Count; i++)
                        {
                            if (listaMascotas[i].Nombre == nombreActual && listaMascotas[i].Raza == razaActual)
                            {
                                indiceActual = i;
                                break;
                            }
                        }
                    }
                }
                ViewBag.IndiceMascotaActual = indiceActual;

                return View();
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Error Index: " + ex.Message);
                return RedirectToAction("Login", "Auth");
            }
        }
        [HttpGet]
        public IActionResult FichaMedica()
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("UserId");
                if (userId == null) return RedirectToAction("Login", "Auth");

                // Verificar que NO sea proveedor, si es proveedor, redirigir
                if (EsProveedor())
                {
                    return RedirigirProveedorSiEsNecesario();
                }

                var mascota = ObtenerMascotaActiva(userId.Value);
                if (mascota == null)
                {
                    TempData["Error"] = "No hay mascota activa.";
                    return RedirectToAction("Registro2", "Registro");
                }

                CargarViewBagMascota(mascota);
                if (mascota != null)
                    ViewBag.MascotaId = Convert.ToInt32(mascota["Id_Mascota"]); // Para el polling
                return View("FichaMedica");
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Error en FichaMedica: " + ex.Message);
                return RedirectToAction("Index");
            }
        }

        // ✅ FICHA OTROS
        [HttpGet]
        public IActionResult FichaOtros()
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("UserId");
                if (userId == null) return RedirectToAction("Login", "Auth");

                // Verificar que NO sea proveedor, si es proveedor, redirigir
                if (EsProveedor())
                {
                    return RedirigirProveedorSiEsNecesario();
                }

                var mascota = ObtenerMascotaActiva(userId.Value);
                if (mascota == null)
                {
                    TempData["Error"] = "No se encontró ninguna mascota.";
                    return RedirectToAction("Registro2", "Registro");
                }

                CargarViewBagMascota(mascota);
                return View("FichaOtros");
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Error en FichaOtros: " + ex.Message);
                TempData["Error"] = "Ocurrió un problema al cargar la ficha.";
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        public IActionResult Calendario()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Auth");

            // Verificar que NO sea proveedor, si es proveedor, redirigir
            if (EsProveedor())
            {
                return RedirigirProveedorSiEsNecesario();
            }

            var mascota = ObtenerMascotaActiva(userId.Value);
            CargarViewBagMascota(mascota);

            string query = @"
                SELECT E.Id_Evento, E.Id_User, E.Id_Mascota, E.Titulo, E.Descripcion, E.Fecha, E.Tipo
                FROM CalendarioEvento E
                INNER JOIN Calendario C ON E.Id_User = C.Id_User
                WHERE E.Id_User = @U
                ORDER BY E.Fecha ASC;";

            var tabla = BD.ExecuteQuery(query, new Dictionary<string, object> { { "@U", userId.Value } });
            List<CalendarioEvento> eventos = new();

            foreach (DataRow row in tabla.Rows)
            {
                eventos.Add(new CalendarioEvento
                {
                    Id_Evento = Convert.ToInt32(row["Id_Evento"]),
                    Id_User = Convert.ToInt32(row["Id_User"]),
                    Id_Mascota = row["Id_Mascota"] == DBNull.Value ? (int?)null : Convert.ToInt32(row["Id_Mascota"]),
                    Titulo = row["Titulo"].ToString(),
                    Descripcion = row["Descripcion"].ToString(),
                    Fecha = Convert.ToDateTime(row["Fecha"]),
                    Tipo = row["Tipo"].ToString()
                });
            }

            var calendario = new Calendario
            {
                Id_User = userId.Value,
                Nombre = "Calendario de cuidados",
                FechaCreacion = DateTime.Now,
                Activo = true,
                Eventos = eventos
            };

            return View("Calendario", calendario);
        }
        [HttpPost]
        public IActionResult CrearEvento(CalendarioEvento ev)
        {
            int? idUser = HttpContext.Session.GetInt32("UserId");
            if (idUser == null)
                return RedirectToAction("Login", "Auth");

            ev.Id_User = idUser.Value;

            if (ev.Fecha == default(DateTime) || ev.Fecha < new DateTime(1753, 1, 1))
            {
                TempData["ErrorCalendario"] = "Seleccioná una fecha válida para el evento 🕒";
                return RedirectToAction("Calendario");
            }

            string queryCal = "SELECT TOP 1 Id_Calendario FROM Calendario WHERE Id_User = @Id_User AND Activo = 1";
            var paramCal = new Dictionary<string, object> { { "@Id_User", idUser.Value } };
            object idCal = BD.ExecuteScalar(queryCal, paramCal);

            if (idCal == null || idCal == DBNull.Value)
            {
                string crearCal = @"INSERT INTO Calendario (Id_User, Nombre, Descripcion, FechaCreacion, Activo)
                            VALUES (@Id_User, 'Calendario de Cuidados', '', SYSDATETIME(), 1);
                            SELECT SCOPE_IDENTITY();";
                idCal = BD.ExecuteScalar(crearCal, paramCal);
            }

            string queryInsert = @"
        INSERT INTO CalendarioEvento (Id_Calendario, Id_User, Id_Mascota, Titulo, Descripcion, Fecha, Tipo)
        VALUES (@Id_Calendario, @Id_User, @Id_Mascota, @Titulo, @Descripcion, @Fecha, @Tipo);";

            var parametros = new Dictionary<string, object>
    {
        { "@Id_Calendario", Convert.ToInt32(idCal) },
        { "@Id_User", ev.Id_User },
        { "@Id_Mascota", ev.Id_Mascota ?? (object)DBNull.Value },
        { "@Titulo", ev.Titulo },
        { "@Descripcion", ev.Descripcion ?? "" },
        { "@Fecha", ev.Fecha },
        { "@Tipo", ev.Tipo }
    };

            BD.ExecuteNonQuery(queryInsert, parametros);

            TempData["ExitoCalendario"] = "Evento agregado con éxito 🎉";
            return RedirectToAction("Calendario");
        }

        [HttpPost]
        [Route("Home/EliminarEvento/{id}")]
        public IActionResult EliminarEvento(int id)
        {
            string query = "DELETE FROM CalendarioEvento WHERE Id_Evento = @Id";
            var param = new Dictionary<string, object> { { "@Id", id } };
            BD.ExecuteNonQuery(query, param);
            return Ok();
        }
        public IActionResult Error404(int? code = null)
        {
            Response.StatusCode = 404;
            ViewData["CodigoError"] = code ?? 404;
            return View("~/Views/Shared/Error404.cshtml");
        }

        [HttpGet]
        public IActionResult FichaVacunas()
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("UserId");
                if (userId == null) return RedirectToAction("Login", "Auth");

                // Verificar que NO sea proveedor, si es proveedor, redirigir
                if (EsProveedor())
                {
                    return RedirigirProveedorSiEsNecesario();
                }

                var mascota = ObtenerMascotaActiva(userId.Value);
                if (mascota == null)
                {
                    TempData["Error"] = "No se encontró ninguna mascota asociada.";
                    return RedirectToAction("Registro2", "Registro");
                }

                int idMascota = Convert.ToInt32(mascota["Id_Mascota"]);
                CargarViewBagMascota(mascota);
                ViewBag.IdMascota = idMascota;

                string queryVacunas = @"
                    SELECT Id_Vacuna, Nombre, Fecha_Aplicacion, Proxima_Dosis, Veterinario, Aplicada
                    FROM Vacuna WHERE Id_Mascota = @Id ORDER BY Proxima_Dosis ASC";
                var dtVac = BD.ExecuteQuery(queryVacunas, new Dictionary<string, object> { { "@Id", idMascota } });

                var vacunas = new List<Vacuna>();
                foreach (DataRow row in dtVac.Rows)
                {
                    vacunas.Add(new Vacuna
                    {
                        Id_Vacuna = Convert.ToInt32(row["Id_Vacuna"]),
                        Id_Mascota = idMascota,
                        Nombre = row["Nombre"].ToString(),
                        Fecha_Aplicacion = row["Fecha_Aplicacion"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(row["Fecha_Aplicacion"]),
                        Proxima_Dosis = row["Proxima_Dosis"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(row["Proxima_Dosis"]),
                        Veterinario = row["Veterinario"] == DBNull.Value ? "" : row["Veterinario"].ToString(),
                        Aplicada = row["Aplicada"] != DBNull.Value && Convert.ToBoolean(row["Aplicada"])
                    });
                }

                return View("FichaVacunas", vacunas);
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Error en FichaVacunas: " + ex.Message);
                TempData["Error"] = "Error al cargar la ficha de vacunas.";
                return RedirectToAction("Index");
            }
        }


        [HttpPost]
        public IActionResult MarcarVacuna(int idMascota, string nombre)
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("UserId");
                if (userId == null) return RedirectToAction("Login", "Auth");
                if (idMascota <= 0 || string.IsNullOrWhiteSpace(nombre))
                {
                    TempData["Error"] = "Datos inválidos para marcar vacuna.";
                    return RedirectToAction("FichaVacunas");
                }

                string update = @"
            UPDATE Vacuna
            SET Aplicada = 1, Fecha_Aplicacion = ISNULL(Fecha_Aplicacion, SYSDATETIME())
            WHERE Id_Mascota = @M
              AND LOWER(Nombre) = LOWER(@N);";

                int rows = BD.ExecuteNonQuery(update, new Dictionary<string, object> {
            { "@M", idMascota }, { "@N", nombre }
        });

                if (rows == 0)
                {
                    string insert = @"
                INSERT INTO Vacuna (Id_Mascota, Nombre, Fecha_Aplicacion, Proxima_Dosis, Veterinario, Aplicada)
                VALUES (@M, @N, SYSDATETIME(), NULL, NULL, 1);";
                    BD.ExecuteNonQuery(insert, new Dictionary<string, object> {
                { "@M", idMascota }, { "@N", nombre }
            });
                }

                TempData["Exito"] = "Vacuna marcada como aplicada ✅";
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Error en MarcarVacuna: " + ex.Message);
                TempData["Error"] = "No se pudo marcar la vacuna.";
            }
            return RedirectToAction("FichaVacunas");
        }


        [HttpPost]
        public IActionResult AgregarVacuna(Vacuna model)
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("UserId");
                if (userId == null) return RedirectToAction("Login", "Auth");

               if (model.Id_Mascota <= 0)
{
    string q = "SELECT TOP 1 Id_Mascota FROM Mascota WHERE Id_User = @U ORDER BY Id_Mascota DESC";
    object idMasc = BD.ExecuteScalar(q, new Dictionary<string, object> { { "@U", userId.Value } });
    if (idMasc == null || idMasc == DBNull.Value)
    {
        TempData["Error"] = "No se encontró mascota asociada.";
        return RedirectToAction("FichaVacunas");
    }
    model.Id_Mascota = Convert.ToInt32(idMasc);
}


                if (string.IsNullOrWhiteSpace(model.Nombre))
                {
                    TempData["Error"] = "Ingresá el nombre de la vacuna.";
                    return RedirectToAction("FichaVacunas");
                }

                string query = @"
            INSERT INTO Vacuna (Id_Mascota, Nombre, Fecha_Aplicacion, Proxima_Dosis, Veterinario, Aplicada)
            VALUES (@Id_Mascota, @Nombre, @Fecha_Aplicacion, @Proxima_Dosis, @Veterinario, @Aplicada);";

                BD.ExecuteNonQuery(query, new Dictionary<string, object>
        {
            { "@Id_Mascota", model.Id_Mascota },
            { "@Nombre", model.Nombre.Trim() },
            { "@Fecha_Aplicacion", model.Fecha_Aplicacion == DateTime.MinValue ? (object)DBNull.Value : model.Fecha_Aplicacion },
            { "@Proxima_Dosis", model.Proxima_Dosis == null ? (object)DBNull.Value : model.Proxima_Dosis },
            { "@Veterinario", string.IsNullOrWhiteSpace(model.Veterinario) ? (object)DBNull.Value : model.Veterinario.Trim() },
            { "@Aplicada", model.Aplicada }
        });

                TempData["Exito"] = "Vacuna agregada 💉";
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Error al agregar vacuna: " + ex.Message);
                TempData["Error"] = "No se pudo registrar la vacuna.";
            }
            return RedirectToAction("FichaVacunas");
        }


        [HttpPost]
        public IActionResult EliminarVacuna(int id)
        {
            try
            {
                string query = "DELETE FROM Vacuna WHERE Id_Vacuna = @Id";
                BD.ExecuteNonQuery(query, new Dictionary<string, object> { { "@Id", id } });
                TempData["Exito"] = "Vacuna eliminada correctamente 🩹";
                return RedirectToAction("FichaVacunas");
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Error al eliminar vacuna: " + ex.Message);
                TempData["Error"] = "Error al eliminar la vacuna.";
                return RedirectToAction("FichaVacunas");
            }
        }

        [HttpPost]
        public IActionResult EditarVacuna(Vacuna model)
        {
            try
            {
                string query = @"
            UPDATE Vacuna
            SET Nombre = @Nombre,
                Fecha_Aplicacion = @Fecha_Aplicacion,
                Proxima_Dosis = @Proxima_Dosis,
                Veterinario = @Veterinario
            WHERE Id_Vacuna = @Id_Vacuna";

                var parametros = new Dictionary<string, object>
        {
            { "@Id_Vacuna", model.Id_Vacuna },
            { "@Nombre", model.Nombre.Trim() },
            { "@Fecha_Aplicacion", model.Fecha_Aplicacion },
            { "@Proxima_Dosis", model.Proxima_Dosis ?? (object)DBNull.Value },
            { "@Veterinario", string.IsNullOrWhiteSpace(model.Veterinario) ? (object)DBNull.Value : model.Veterinario.Trim() }
        };

                BD.ExecuteNonQuery(query, parametros);
                TempData["Exito"] = "Vacuna actualizada correctamente 🩺";
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Error al editar vacuna: " + ex.Message);
                TempData["Error"] = "No se pudo actualizar la vacuna.";
            }

            return RedirectToAction("FichaVacunas");
        }
        [HttpGet]
        public IActionResult Configuracion()
        {

            try
            {
                var userId = HttpContext.Session.GetInt32("UserId");
                if (userId == null)
                    return RedirectToAction("Login", "Auth");

                // Verificar que NO sea proveedor, si es proveedor, redirigir
                if (EsProveedor())
                {
                    return RedirigirProveedorSiEsNecesario();
                }

                var tema = HttpContext.Session.GetString("Tema") ?? "claro";
        ViewBag.Tema = tema;
                string qUser = @"
                    SELECT U.Nombre, U.Apellido, U.Telefono, M.Correo
                    FROM [User] U
                    INNER JOIN Mail M ON U.Id_Mail = M.Id_Mail
                    WHERE U.Id_User = @Id";

                var dtUser = BD.ExecuteQuery(qUser, new Dictionary<string, object> { { "@Id", userId.Value } });

                if (dtUser.Rows.Count > 0)
                {
                    var u = dtUser.Rows[0];
                    ViewBag.Mail = u["Correo"].ToString();
                    ViewBag.Telefono = u["Telefono"]?.ToString() ?? "";
                }

                string qMascotas = @"
    WITH MascotasUnicas AS (
        SELECT 
            Id_Mascota, 
            Nombre, 
            Especie, 
            Raza, 
            Edad, 
            Peso, 
            Sexo, 
            Fecha_Nacimiento,
            ROW_NUMBER() OVER (
                PARTITION BY Nombre, Raza 
                ORDER BY Id_Mascota DESC
            ) AS rn
        FROM Mascota
        WHERE Id_User = @Id
    )
    SELECT 
        Id_Mascota, Nombre, Especie, Raza, Edad, Peso, Sexo, Fecha_Nacimiento
    FROM MascotasUnicas
    WHERE rn = 1
    ORDER BY Nombre ASC;";


                var dtMascotas = BD.ExecuteQuery(qMascotas, new Dictionary<string, object> { { "@Id", userId.Value } });

                var mascotas = new List<Mascota>();
                foreach (System.Data.DataRow row in dtMascotas.Rows)
                {
                    mascotas.Add(new Mascota
                    {
                        Id_Mascota = Convert.ToInt32(row["Id_Mascota"]),
                        Nombre = row["Nombre"].ToString(),
                        Especie = row["Especie"].ToString(),
                        Raza = row["Raza"].ToString(),
                        Edad = row["Edad"] == DBNull.Value ? 0 : Convert.ToInt32(row["Edad"]),
                        Peso = row["Peso"] == DBNull.Value ? 0 : Convert.ToDecimal(row["Peso"]),
                        Sexo = row["Sexo"].ToString(),
Fecha_Nacimiento = row["Fecha_Nacimiento"] == DBNull.Value 
    ? DateTime.MinValue 
    : Convert.ToDateTime(row["Fecha_Nacimiento"])
                    });
                }

                ViewBag.Mascotas = mascotas;

                // 🦴 Mascota activa (última seleccionada)
                int? mascotaActivaId = HttpContext.Session.GetInt32("MascotaId");
                if (mascotaActivaId != null)
                {
                    var activa = mascotas.Find(m => m.Id_Mascota == mascotaActivaId);
                    ViewBag.MascotaActiva = activa ?? new Mascota();
                }
                else
                {
                    ViewBag.MascotaActiva = mascotas.Count > 0 ? mascotas[0] : new Mascota();
                }
// 🔄 Si se acaba de agregar una nueva mascota, recargar desde la BD
if (TempData["Exito"] != null && TempData["Exito"].ToString().Contains("Mascota agregada"))
{
    // Recuperar nuevamente todas las mascotas del usuario
    var dtMascotasRefrescado = BD.ExecuteQuery(qMascotas, new Dictionary<string, object> { { "@Id", userId.Value } });
    var mascotasRefrescadas = new List<Mascota>();

    foreach (System.Data.DataRow row in dtMascotasRefrescado.Rows)
    {
        mascotasRefrescadas.Add(new Mascota
        {
            Id_Mascota = Convert.ToInt32(row["Id_Mascota"]),
            Nombre = row["Nombre"].ToString(),
            Especie = row["Especie"].ToString(),
            Raza = row["Raza"].ToString(),
            Edad = row["Edad"] == DBNull.Value ? 0 : Convert.ToInt32(row["Edad"]),
            Peso = row["Peso"] == DBNull.Value ? 0 : Convert.ToDecimal(row["Peso"]),
            Sexo = row["Sexo"].ToString(),
            Fecha_Nacimiento = row["Fecha_Nacimiento"] == DBNull.Value
                ? DateTime.MinValue
                : Convert.ToDateTime(row["Fecha_Nacimiento"])
        });
    }

    ViewBag.Mascotas = mascotasRefrescadas;
}

                return View();
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Error en Configuracion: " + ex.Message);
                TempData["Error"] = "Error al cargar configuración.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public IActionResult CambiarMascota(int MascotaId)
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("UserId");
                if (userId == null)
                    return RedirectToAction("Login", "Auth");

                // Verificar que la mascota pertenece al usuario o está compartida y obtener su nombre y raza
                string q = @"
                    SELECT TOP 1 m.Nombre, m.Especie, m.Raza 
                    FROM Mascota m
                    LEFT JOIN MascotaCompartida mc ON m.Id_Mascota = mc.Id_Mascota AND mc.Id_UsuarioCompartido = @User AND mc.Activo = 1
                    WHERE m.Id_Mascota = @Id 
                      AND (m.Id_User = @User OR mc.Id_UsuarioCompartido = @User)
                      AND (m.Archivada IS NULL OR m.Archivada = 0)";
                var dt = BD.ExecuteQuery(q, new Dictionary<string, object> { { "@Id", MascotaId }, { "@User", userId.Value } });

                if (dt.Rows.Count == 0)
                {
                    TempData["Error"] = "Mascota no encontrada.";
                    return RedirectToAction("Index");
                }

                var m = dt.Rows[0];
                string nombre = m["Nombre"]?.ToString() ?? "";
                string raza = m["Raza"]?.ToString() ?? "";
                
                // Buscar la mascota más reciente con ese nombre y raza (por si hay duplicados)
                string qMascotaUnica = @"
                    SELECT TOP 1 m.Id_Mascota, m.Nombre, m.Especie, m.Raza
                    FROM Mascota m
                    LEFT JOIN MascotaCompartida mc ON m.Id_Mascota = mc.Id_Mascota AND mc.Id_UsuarioCompartido = @User AND mc.Activo = 1
                    WHERE (m.Id_User = @User OR mc.Id_UsuarioCompartido = @User)
                      AND (m.Archivada IS NULL OR m.Archivada = 0)
                      AND m.Nombre = @Nombre
                      AND (m.Raza = @Raza OR (m.Raza IS NULL AND @Raza IS NULL))
                    ORDER BY m.Id_Mascota DESC";
                
                var dtUnica = BD.ExecuteQuery(qMascotaUnica, new Dictionary<string, object> 
                { 
                    { "@User", userId.Value },
                    { "@Nombre", nombre },
                    { "@Raza", raza ?? (object)DBNull.Value }
                });

                if (dtUnica.Rows.Count == 0)
                {
                    TempData["Error"] = "Mascota no encontrada.";
                    return RedirectToAction("Index");
                }

                var mascotaUnica = dtUnica.Rows[0];
                int idMascotaFinal = Convert.ToInt32(mascotaUnica["Id_Mascota"]);
                
                HttpContext.Session.SetInt32("MascotaId", idMascotaFinal);
                HttpContext.Session.SetString("MascotaNombre", mascotaUnica["Nombre"].ToString());
                HttpContext.Session.SetString("MascotaEspecie", mascotaUnica["Especie"].ToString());
                HttpContext.Session.SetString("MascotaRaza", mascotaUnica["Raza"]?.ToString() ?? "");
                
                // Limpiar avatar de sesión al cambiar de mascota (cada mascota tiene su propio avatar)
                HttpContext.Session.Remove("MascotaAvatar");

                // Redirigir al Index
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Error en CambiarMascota: " + ex.Message);
                TempData["Error"] = "Error al cambiar mascota.";
                return RedirectToAction("Index");
            }
        }

        

        [HttpPost]
        public IActionResult ActualizarContacto(string Correo, string Telefono)
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("UserId");
                if (userId == null)
                    return RedirectToAction("Login", "Auth");

                BD.ExecuteNonQuery(@"
                    UPDATE M
                    SET M.Correo=@Correo
                    FROM Mail M
                    INNER JOIN [User] U ON M.Id_Mail = U.Id_Mail
                    WHERE U.Id_User=@Id",
                    new Dictionary<string, object> { { "@Correo", Correo }, { "@Id", userId.Value } });

                BD.ExecuteNonQuery("UPDATE [User] SET Telefono=@Tel WHERE Id_User=@Id",
                    new Dictionary<string, object> { { "@Tel", Telefono }, { "@Id", userId.Value } });

                TempData["Exito"] = "Datos de contacto actualizados ✉️";
                return RedirectToAction("Configuracion");
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Error en ActualizarContacto: " + ex.Message);
                TempData["Error"] = "No se pudieron actualizar los datos.";
                return RedirectToAction("Configuracion");
            }
        }
        [HttpPost]
        public IActionResult EliminarUsuario()
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("UserId");
                if (userId == null)
                    return RedirectToAction("Login", "Auth");

                string q = "DELETE FROM Mascota WHERE Id_User=@Id; DELETE FROM [User] WHERE Id_User=@Id;";
                BD.ExecuteNonQuery(q, new Dictionary<string, object> { { "@Id", userId.Value } });

                HttpContext.Session.Clear();
                TempData["Exito"] = "Tu cuenta y mascotas fueron eliminadas 💀";
                return RedirectToAction("Login", "Auth");
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Error en EliminarUsuario: " + ex.Message);
                TempData["Error"] = "No se pudo eliminar la cuenta.";
                return RedirectToAction("Configuracion");
            }
        }
        [HttpGet]
public IActionResult FichaTratamientos()
{
    try
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
            return RedirectToAction("Login", "Auth");
 var tema = HttpContext.Session.GetString("Tema") ?? "claro";
        ViewBag.Tema = tema;
        // 🔹 Obtener la mascota activa o la última registrada
        var mascota = ObtenerMascotaActiva(userId.Value);
        if (mascota == null)
        {
            TempData["Error"] = "No se encontró ninguna mascota asociada.";
            return RedirectToAction("Registro2", "Registro");
        }

        int idMascota = Convert.ToInt32(mascota["Id_Mascota"]);
        CargarViewBagMascota(mascota);
        ViewBag.IdMascota = idMascota;

        // 🔹 Obtener los tratamientos asociados a esa mascota
        string qTrat = @"
            SELECT Id_Tratamiento, Nombre, Fecha_Inicio, Proximo_Control, Veterinario
            FROM Tratamiento
            WHERE Id_Mascota = @Id
            ORDER BY Fecha_Inicio DESC";

        var pTrat = new Dictionary<string, object> { { "@Id", idMascota } };
        var dtTrat = BD.ExecuteQuery(qTrat, pTrat);

        var tratamientos = new List<Tratamiento>();
        foreach (DataRow row in dtTrat.Rows)
        {
            tratamientos.Add(new Tratamiento
            {
                Id_Tratamiento = Convert.ToInt32(row["Id_Tratamiento"]),
                Id_Mascota = idMascota,
                Nombre = row["Nombre"].ToString(),
                Fecha_Inicio = row["Fecha_Inicio"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(row["Fecha_Inicio"]),
                Proximo_Control = row["Proximo_Control"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(row["Proximo_Control"]),
                Veterinario = row["Veterinario"] == DBNull.Value ? "" : row["Veterinario"].ToString()
            });
        }

        return View("FichaTratamientos", tratamientos);
    }
    catch (Exception ex)
    {
        Console.WriteLine("❌ Error en FichaTratamientos: " + ex.Message);
        TempData["Error"] = "No se pudo cargar la ficha de tratamientos.";
        return RedirectToAction("Index");
    }
}


[HttpPost]
public IActionResult AgregarTratamiento(Tratamiento t)
{
    if (t == null || t.Id_Mascota <= 0) return RedirectToAction("FichaTratamientos");
    string q = @"INSERT INTO Tratamiento (Id_Mascota, Nombre, Fecha_Inicio, Proximo_Control, Veterinario)
                 VALUES (@Id_Mascota,@Nombre,@Fecha_Inicio,@Proximo_Control,@Veterinario)";
    var p = new Dictionary<string, object> {
        {"@Id_Mascota", t.Id_Mascota},
        {"@Nombre", t.Nombre},
        {"@Fecha_Inicio", t.Fecha_Inicio},
        {"@Proximo_Control", t.Proximo_Control},
        {"@Veterinario", t.Veterinario}
    };
    BD.ExecuteNonQuery(q, p);
    return RedirectToAction("FichaTratamientos");
}

[HttpPost]
public IActionResult EliminarTratamiento(int id)
{
    string q = "DELETE FROM Tratamiento WHERE Id_Tratamiento=@id";
    var p = new Dictionary<string, object> { { "@id", id } };
    BD.ExecuteNonQuery(q, p);
    return RedirectToAction("FichaTratamientos");
}
[HttpGet]
public IActionResult DescargarPDF()
{
    var userId = HttpContext.Session.GetInt32("UserId");
    if (userId == null)
        return RedirectToAction("Login", "Auth");
 var tema = HttpContext.Session.GetString("Tema") ?? "claro";
        ViewBag.Tema = tema;
    var mascota = ObtenerMascotaActiva(userId.Value);
    if (mascota == null)
        return Content("No se encontró ninguna mascota para generar el PDF");

    int idMascota = Convert.ToInt32(mascota["Id_Mascota"]);
    string nombre = mascota["Nombre"].ToString();
    string especie = mascota["Especie"].ToString();
    string raza = mascota["Raza"].ToString();

    // ✅ Peso: usar PesoDisplay si está disponible, sino formatear el decimal (sin correcciones)
    string? pesoDisplayBD = null;
    if (mascota.Table.Columns.Contains("PesoDisplay") && mascota["PesoDisplay"] != DBNull.Value)
    {
        pesoDisplayBD = mascota["PesoDisplay"].ToString();
    }
    
    decimal pesoDecimal = 0;
    string peso;
    if (mascota["Peso"] != DBNull.Value && decimal.TryParse(mascota["Peso"].ToString(), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out pesoDecimal))
    {
        // Usar PesoDisplay si existe, sino formatear el decimal (usar valor tal cual de la BD)
        peso = !string.IsNullOrEmpty(pesoDisplayBD) 
            ? pesoDisplayBD 
            : PesoHelper.FormatearPeso(pesoDecimal);
    }
    else
    {
        peso = pesoDisplayBD ?? PesoHelper.FormatearPeso(0.1M);
    }

    // 🎂 Edad formateada
    int edadMeses = Convert.ToInt32(mascota["Edad"] ?? 0);
    int años = edadMeses / 12;
    int meses = edadMeses % 12;
    string edad =
        $"{(años > 0 ? $"{años} año{(años > 1 ? "s" : "")}" : "")}" +
        $"{(meses > 0 ? $" y {meses} mes{(meses > 1 ? "es" : "")}" : "")}";

    string foto = mascota.Table.Columns.Contains("Foto") && mascota["Foto"] != DBNull.Value
        ? mascota["Foto"].ToString()
        : "/img/mascotas/default.png";

    // 🔹 Crear PDF en memoria
    using var memory = new MemoryStream();
    var writer = new PdfWriter(memory);
    var pdf = new PdfDocument(writer);
    var doc = new Document(pdf);

    // 🔹 Colores y fuentes
    var marron = new iText.Kernel.Colors.DeviceRgb(60, 42, 27);
    var verde = new iText.Kernel.Colors.DeviceRgb(57, 183, 124);
    var grisSuave = new iText.Kernel.Colors.DeviceRgb(245, 245, 245);
    var fuenteNegrita = iText.Kernel.Font.PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA_BOLD);
    var fuenteRegular = iText.Kernel.Font.PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA);

    // 🔹 Encabezado visual
    doc.Add(new Paragraph("Ficha Médica Veterinaria 🦮")
        .SetFont(fuenteNegrita)
        .SetFontSize(20)
        .SetFontColor(verde)
        .SetTextAlignment(TextAlignment.CENTER)
        .SetMarginBottom(5));

    doc.Add(new Paragraph("Emitido por Zooni – Tu mascota, tu mundo")
        .SetFont(fuenteRegular)
        .SetFontSize(11)
        .SetFontColor(marron)
        .SetTextAlignment(TextAlignment.CENTER)
        .SetMarginBottom(20));

    doc.Add(new Paragraph($"📅 Fecha de emisión: {DateTime.Now:dd/MM/yyyy}")
        .SetFont(fuenteRegular)
        .SetFontSize(9)
        .SetTextAlignment(TextAlignment.RIGHT)
        .SetFontColor(marron));

    // 🔹 Foto si existe
    string rutaCompleta = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", foto.TrimStart('/'));
    if (System.IO.File.Exists(rutaCompleta))
    {
        var img = new Image(ImageDataFactory.Create(rutaCompleta))
            .ScaleAbsolute(110, 110)
            .SetHorizontalAlignment(HorizontalAlignment.CENTER)
            .SetMarginTop(5)
            .SetMarginBottom(10);
        doc.Add(img);
    }

    // 🔹 Datos generales
    var tablaDatos = new Table(UnitValue.CreatePercentArray(new float[] { 1, 2 }))
        .UseAllAvailableWidth()
        .SetBackgroundColor(grisSuave)
        .SetMarginBottom(20);

    void Celda(string titulo, string valor)
    {
        tablaDatos.AddCell(new Cell().Add(new Paragraph(titulo)
            .SetFont(fuenteNegrita)
            .SetFontSize(11)
            .SetFontColor(marron)
            .SetTextAlignment(TextAlignment.RIGHT)
            .SetPadding(6)));
        tablaDatos.AddCell(new Cell().Add(new Paragraph(valor))
            .SetFont(fuenteRegular)
            .SetFontSize(11)
            .SetPadding(6));
    }

    Celda("Nombre:", nombre);
    Celda("Especie:", especie);
    Celda("Raza:", raza);
    Celda("Peso:", peso);
    Celda("Edad:", edad);

    doc.Add(tablaDatos);

    // 🔹 Vacunas
    string qVacunas = @"SELECT TOP 10 Nombre, Fecha_Aplicacion, Proxima_Dosis 
                        FROM Vacuna WHERE Id_Mascota = @Id ORDER BY Fecha_Aplicacion DESC";
    var vacunas = BD.ExecuteQuery(qVacunas, new Dictionary<string, object> { { "@Id", idMascota } });

    doc.Add(new Paragraph("💉 Vacunas registradas")
        .SetFont(fuenteNegrita)
        .SetFontSize(14)
        .SetFontColor(verde)
        .SetMarginBottom(6));

    if (vacunas.Rows.Count == 0)
    {
        doc.Add(new Paragraph("No hay vacunas registradas en el historial.").SetFont(fuenteRegular).SetFontSize(11));
    }
    else
    {
        var tablaVac = new Table(UnitValue.CreatePercentArray(new float[] { 2, 1, 1 }))
            .UseAllAvailableWidth()
            .SetMarginBottom(15);

        tablaVac.AddHeaderCell("Nombre").SetFont(fuenteNegrita);
        tablaVac.AddHeaderCell("Aplicada").SetFont(fuenteNegrita);
        tablaVac.AddHeaderCell("Próxima dosis").SetFont(fuenteNegrita);

        foreach (DataRow v in vacunas.Rows)
        {
            tablaVac.AddCell(v["Nombre"].ToString());
            tablaVac.AddCell(v["Fecha_Aplicacion"] == DBNull.Value ? "—" : Convert.ToDateTime(v["Fecha_Aplicacion"]).ToString("dd/MM/yyyy"));
            tablaVac.AddCell(v["Proxima_Dosis"] == DBNull.Value ? "—" : Convert.ToDateTime(v["Proxima_Dosis"]).ToString("dd/MM/yyyy"));
        }

        doc.Add(tablaVac);
    }

    // 🔹 Tratamientos
    string qTrat = @"SELECT TOP 10 Nombre, Fecha_Inicio, Proximo_Control, Veterinario
                     FROM Tratamiento WHERE Id_Mascota = @Id ORDER BY Fecha_Inicio DESC";
    var tratamientos = BD.ExecuteQuery(qTrat, new Dictionary<string, object> { { "@Id", idMascota } });

    doc.Add(new Paragraph("💊 Tratamientos activos")
        .SetFont(fuenteNegrita)
        .SetFontSize(14)
        .SetFontColor(verde)
        .SetMarginBottom(6));

    if (tratamientos.Rows.Count == 0)
    {
        doc.Add(new Paragraph("No hay tratamientos activos registrados.")
            .SetFont(fuenteRegular).SetFontSize(11));
    }
    else
    {
        var tablaTrat = new Table(UnitValue.CreatePercentArray(new float[] { 2, 1, 1, 1 }))
            .UseAllAvailableWidth();

        tablaTrat.AddHeaderCell("Nombre").SetFont(fuenteNegrita);
        tablaTrat.AddHeaderCell("Inicio").SetFont(fuenteNegrita);
        tablaTrat.AddHeaderCell("Próximo control").SetFont(fuenteNegrita);
        tablaTrat.AddHeaderCell("Veterinario").SetFont(fuenteNegrita);

        foreach (DataRow t in tratamientos.Rows)
        {
            tablaTrat.AddCell(t["Nombre"].ToString());
            tablaTrat.AddCell(t["Fecha_Inicio"] == DBNull.Value ? "—" : Convert.ToDateTime(t["Fecha_Inicio"]).ToString("dd/MM/yyyy"));
            tablaTrat.AddCell(t["Proximo_Control"] == DBNull.Value ? "—" : Convert.ToDateTime(t["Proximo_Control"]).ToString("dd/MM/yyyy"));
            tablaTrat.AddCell(t["Veterinario"] == DBNull.Value ? "—" : t["Veterinario"].ToString());
        }

        doc.Add(tablaTrat);
    }

    // 🐾 Cierre
    doc.Add(new Paragraph("\n\nZooni – Tu mascota, tu mundo 💚")
        .SetTextAlignment(TextAlignment.CENTER)
        .SetFont(fuenteRegular)
        .SetFontColor(marron)
        .SetFontSize(10));

    doc.Add(new Paragraph("Documento generado automáticamente – No reemplaza una consulta veterinaria.")
        .SetTextAlignment(TextAlignment.CENTER)
        .SetFont(fuenteRegular)
        .SetFontSize(9)
        .SetFontColor(new iText.Kernel.Colors.DeviceRgb(120, 120, 120))
        .SetMarginTop(5));

    doc.Close();

    return File(memory.ToArray(), "application/pdf", $"{nombre}_FichaMedica_Zooni.pdf");
}
[HttpGet]
public IActionResult Perfil(int? id = null)
{
    var userId = HttpContext.Session.GetInt32("UserId");
    if (userId == null) return RedirectToAction("Login", "Auth");
    
    // ✅ Crear tablas si no existen
    try
    {
        CrearTablasPerfilSocial();
    }
    catch (Exception ex)
    {
        Console.WriteLine("⚠️ Error al crear tablas (puede ser normal si ya existen): " + ex.Message);
    }
    
    // Verificar que NO sea proveedor, si es proveedor, redirigir
    if (EsProveedor())
    {
        return RedirigirProveedorSiEsNecesario();
    }

    var tema = HttpContext.Session.GetString("Tema") ?? "claro";
    ViewBag.Tema = tema;
    
    // Si se especifica un id, ver ese perfil, sino ver el propio
    int perfilId = id ?? userId.Value;
    bool esMiPerfil = perfilId == userId.Value;
    ViewBag.EsMiPerfil = esMiPerfil;
    ViewBag.PerfilId = perfilId;
    // 🔍 Intentar obtener perfil
    string qPerfil = @"
        SELECT TOP 1 P.Id_Perfil, U.Nombre, U.Apellido, U.Pais, P.Descripcion, P.FotoPerfil
        FROM Perfil P 
        INNER JOIN [User] U ON P.Id_Usuario = U.Id_User
        WHERE U.Id_User = @Id";

    var dtPerfil = BD.ExecuteQuery(qPerfil, new Dictionary<string, object> { { "@Id", perfilId } });

    // 🧩 Si no hay perfil, crear uno por defecto
    if (dtPerfil.Rows.Count == 0)
    {
        string qInsert = @"
            INSERT INTO Perfil (Id_Usuario, FotoPerfil, Descripcion, AniosVigencia)
            VALUES (@U, '/img/perfil/default.png', 'Amante de los animales ❤️', 1)";
        BD.ExecuteNonQuery(qInsert, new Dictionary<string, object> { { "@U", perfilId } });

        // volver a consultar
        dtPerfil = BD.ExecuteQuery(qPerfil, new Dictionary<string, object> { { "@Id", perfilId } });
    }

    var p = dtPerfil.Rows[0];
    
    // ✅ Limpieza y validación exhaustiva de datos
    string LimpiarTexto(string texto)
    {
        if (string.IsNullOrWhiteSpace(texto)) return "";
        // Remover caracteres de control y caracteres problemáticos
        return System.Text.RegularExpressions.Regex.Replace(
            texto.Trim(), 
            @"[\x00-\x1F\x7F-\x9F]", 
            ""
        ).Replace("??", "").Replace("?", "");
    }
    
    var nombre = LimpiarTexto(p["Nombre"]?.ToString() ?? "");
    var apellido = LimpiarTexto(p["Apellido"]?.ToString() ?? "");
    
    // Validar que los nombres no sean solo espacios o caracteres raros
    if (string.IsNullOrWhiteSpace(nombre) || nombre.Length < 2 || nombre.All(c => !char.IsLetter(c)))
        nombre = "";
    if (string.IsNullOrWhiteSpace(apellido) || apellido.Length < 2 || apellido.All(c => !char.IsLetter(c)))
        apellido = "";
    
    ViewBag.Nombre = string.IsNullOrWhiteSpace(nombre) && string.IsNullOrWhiteSpace(apellido) 
        ? "Usuario" 
        : $"{nombre} {apellido}".Trim();
    ViewBag.Pais = LimpiarTexto(p["Pais"]?.ToString() ?? "Argentina");
    if (string.IsNullOrWhiteSpace(ViewBag.Pais.ToString())) ViewBag.Pais = "Argentina";
    
    var descripcionRaw = p["Descripcion"]?.ToString() ?? "";
    var descripcionLimpia = LimpiarTexto(descripcionRaw);
    ViewBag.Descripcion = string.IsNullOrWhiteSpace(descripcionLimpia) 
        ? "Amante de los animales" 
        : descripcionLimpia;
    
    var fotoPerfilRaw = p["FotoPerfil"]?.ToString()?.Trim() ?? "";
    ViewBag.FotoPerfil = string.IsNullOrWhiteSpace(fotoPerfilRaw) || !fotoPerfilRaw.StartsWith("/")
        ? "/img/perfil/default.png"
        : fotoPerfilRaw;
    
    ViewBag.NombreUsuario = nombre;
    ViewBag.ApellidoUsuario = apellido;

    // 🔍 Verificar si es amigo
    string qEsAmigo = @"
        SELECT COUNT(*) as EsAmigo
        FROM CirculoConfianza
        WHERE (Id_User = @UserId AND Id_Amigo = @PerfilId)
           OR (Id_User = @PerfilId AND Id_Amigo = @UserId)";
    var dtEsAmigo = BD.ExecuteQuery(qEsAmigo, new Dictionary<string, object> 
    { 
        { "@UserId", userId.Value },
        { "@PerfilId", perfilId }
    });
    bool esAmigo = dtEsAmigo.Rows.Count > 0 && Convert.ToInt32(dtEsAmigo.Rows[0]["EsAmigo"]) > 0;
    ViewBag.EsAmigo = esAmigo;
    
    // 🔍 Verificar si hay solicitud pendiente
    string qSolicitud = @"
        SELECT COUNT(*) as TieneSolicitud
        FROM Invitacion
        WHERE ((Id_Emisor = @UserId AND Id_Receptor = @PerfilId)
            OR (Id_Emisor = @PerfilId AND Id_Receptor = @UserId))
        AND Rol = 'Amigo' AND Estado = 'Pendiente'";
    var dtSolicitud = BD.ExecuteQuery(qSolicitud, new Dictionary<string, object>
    {
        { "@UserId", userId.Value },
        { "@PerfilId", perfilId }
    });
    bool tieneSolicitud = dtSolicitud.Rows.Count > 0 && Convert.ToInt32(dtSolicitud.Rows[0]["TieneSolicitud"]) > 0;
    ViewBag.TieneSolicitud = tieneSolicitud;
    
    // 📊 Contar publicaciones, seguidores, seguidos
    string qStats = @"
        SELECT 
            (SELECT COUNT(*) FROM Publicacion WHERE Id_User = @Id AND Eliminada = 0) as Publicaciones,
            (SELECT COUNT(*) FROM CirculoConfianza WHERE Id_Amigo = @Id) as Seguidores,
            (SELECT COUNT(*) FROM CirculoConfianza WHERE Id_User = @Id) as Siguiendo";
    var dtStats = BD.ExecuteQuery(qStats, new Dictionary<string, object> { { "@Id", perfilId } });
    if (dtStats.Rows.Count > 0)
    {
        ViewBag.CantidadPublicaciones = Convert.ToInt32(dtStats.Rows[0]["Publicaciones"]);
        ViewBag.CantidadSeguidores = Convert.ToInt32(dtStats.Rows[0]["Seguidores"]);
        ViewBag.CantidadSiguiendo = Convert.ToInt32(dtStats.Rows[0]["Siguiendo"]);
    }
    else
    {
        ViewBag.CantidadPublicaciones = 0;
        ViewBag.CantidadSeguidores = 0;
        ViewBag.CantidadSiguiendo = 0;
    }
    
    // 🐾 Mascotas (filtrar duplicados: misma raza y nombre exacto, excluir archivadas)
    string qMascotas = @"
        SELECT Id_Mascota, Nombre, Especie, Raza, Foto,
               ROW_NUMBER() OVER (PARTITION BY Nombre, Raza ORDER BY Id_Mascota DESC) as rn
        FROM Mascota 
        WHERE Id_User = @Id 
          AND (Archivada IS NULL OR Archivada = 0)";
    var dtMascotas = BD.ExecuteQuery(qMascotas, new Dictionary<string, object> { { "@Id", perfilId } });

    var mascotas = new List<Mascota>();
    var mascotasVistas = new HashSet<string>(); // Para evitar duplicados
    
    // Función para limpiar nombres de mascotas
    string LimpiarNombreMascota(string nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre)) return "Sin nombre";
        var limpio = System.Text.RegularExpressions.Regex.Replace(
            nombre.Trim(), 
            @"[\x00-\x1F\x7F-\x9F]", 
            ""
        ).Replace("??", "").Replace("?", "");
        
        // Validar que tenga al menos una letra
        if (string.IsNullOrWhiteSpace(limpio) || limpio.All(c => !char.IsLetterOrDigit(c)))
            return "Sin nombre";
        
        return limpio;
    }
    
    foreach (System.Data.DataRow m in dtMascotas.Rows)
    {
        string nombreMascotaRaw = m["Nombre"]?.ToString() ?? "";
        string nombreMascota = LimpiarNombreMascota(nombreMascotaRaw);
        string razaMascota = m["Raza"]?.ToString()?.Trim() ?? "Sin raza";
        string clave = $"{nombreMascota}|{razaMascota}"; // Clave única para nombre+raza
        
        // Solo agregar si no hemos visto esta combinación antes
        if (!mascotasVistas.Contains(clave))
        {
            mascotasVistas.Add(clave);
            
            var fotoMascota = m["Foto"] == DBNull.Value || m["Foto"] == null 
                ? "/img/mascotas/default.png" 
                : m["Foto"].ToString()?.Trim() ?? "/img/mascotas/default.png";
            
            if (string.IsNullOrWhiteSpace(fotoMascota) || !fotoMascota.StartsWith("/"))
                fotoMascota = "/img/mascotas/default.png";
            
            mascotas.Add(new Mascota
            {
                Id_Mascota = Convert.ToInt32(m["Id_Mascota"]),
                Nombre = nombreMascota,
                Especie = m["Especie"]?.ToString()?.Trim() ?? "Perro",
                Raza = razaMascota,
                Foto = fotoMascota
            });
        }
    }

    ViewBag.Mascotas = mascotas;
    
    // 📸 Obtener publicaciones (ancladas primero, luego por fecha)
    string qPublicaciones = @"
        SELECT P.*, 
               U.Nombre + ' ' + U.Apellido as NombreUsuario,
               PR.FotoPerfil as FotoPerfilUsuario,
               M.Nombre as NombreMascota,
               (SELECT COUNT(*) FROM LikePublicacion WHERE Id_Publicacion = P.Id_Publicacion) as CantidadLikes,
               (SELECT COUNT(*) FROM ComentarioPublicacion WHERE Id_Publicacion = P.Id_Publicacion AND Eliminado = 0) as CantidadComentarios,
               (SELECT COUNT(*) FROM CompartirPublicacion WHERE Id_Publicacion = P.Id_Publicacion) as CantidadCompartidos,
               CASE WHEN EXISTS (SELECT 1 FROM LikePublicacion WHERE Id_Publicacion = P.Id_Publicacion AND Id_User = @UserId) THEN 1 ELSE 0 END as MeGusta,
               CASE WHEN EXISTS (SELECT 1 FROM CompartirPublicacion WHERE Id_Publicacion = P.Id_Publicacion AND Id_User = @UserId) THEN 1 ELSE 0 END as Compartida
        FROM Publicacion P
        INNER JOIN [User] U ON P.Id_User = U.Id_User
        LEFT JOIN Perfil PR ON PR.Id_Usuario = U.Id_User
        LEFT JOIN Mascota M ON P.Id_Mascota = M.Id_Mascota
        WHERE P.Id_User = @PerfilId AND P.Eliminada = 0
        ORDER BY P.Anclada DESC, P.Fecha DESC";
    var dtPublicaciones = BD.ExecuteQuery(qPublicaciones, new Dictionary<string, object>
    {
        { "@PerfilId", perfilId },
        { "@UserId", userId.Value }
    });
    
    var publicaciones = new List<Publicacion>();
    foreach (System.Data.DataRow row in dtPublicaciones.Rows)
    {
        publicaciones.Add(new Publicacion
        {
            Id_Publicacion = Convert.ToInt32(row["Id_Publicacion"]),
            Id_User = Convert.ToInt32(row["Id_User"]),
            Id_Mascota = row["Id_Mascota"] == DBNull.Value ? null : Convert.ToInt32(row["Id_Mascota"]),
            ImagenUrl = row["ImagenUrl"]?.ToString(),
            Descripcion = row["Descripcion"]?.ToString(),
            Fecha = Convert.ToDateTime(row["Fecha"]),
            Anclada = Convert.ToBoolean(row["Anclada"]),
            NombreUsuario = row["NombreUsuario"]?.ToString(),
            FotoPerfilUsuario = row["FotoPerfilUsuario"]?.ToString() ?? "/img/perfil/default.png",
            NombreMascota = row["NombreMascota"]?.ToString(),
            CantidadLikes = Convert.ToInt32(row["CantidadLikes"]),
            CantidadComentarios = Convert.ToInt32(row["CantidadComentarios"]),
            CantidadCompartidos = Convert.ToInt32(row["CantidadCompartidos"]),
            MeGusta = Convert.ToInt32(row["MeGusta"]) == 1,
            Compartida = Convert.ToInt32(row["Compartida"]) == 1
        });
    }
    ViewBag.Publicaciones = publicaciones;
    
    // 📱 Obtener historias activas (no expiradas)
    string qHistorias = @"
        SELECT H.*,
               U.Nombre + ' ' + U.Apellido as NombreUsuario,
               PR.FotoPerfil as FotoPerfilUsuario,
               M.Nombre as NombreMascota
        FROM Historia H
        INNER JOIN [User] U ON H.Id_User = U.Id_User
        LEFT JOIN Perfil PR ON PR.Id_Usuario = U.Id_User
        LEFT JOIN Mascota M ON H.Id_Mascota = M.Id_Mascota
        WHERE H.Id_User = @PerfilId 
        AND H.Eliminada = 0 
        AND H.Expiracion > GETDATE()
        ORDER BY H.Fecha DESC";
    var dtHistorias = BD.ExecuteQuery(qHistorias, new Dictionary<string, object> { { "@PerfilId", perfilId } });
    
    var historias = new List<Historia>();
    foreach (System.Data.DataRow row in dtHistorias.Rows)
    {
        historias.Add(new Historia
        {
            Id_Historia = Convert.ToInt32(row["Id_Historia"]),
            Id_User = Convert.ToInt32(row["Id_User"]),
            Id_Mascota = row["Id_Mascota"] == DBNull.Value ? null : Convert.ToInt32(row["Id_Mascota"]),
            ImagenUrl = row["ImagenUrl"]?.ToString() ?? "",
            Texto = row["Texto"]?.ToString(),
            Fecha = Convert.ToDateTime(row["Fecha"]),
            Expiracion = Convert.ToDateTime(row["Expiracion"]),
            NombreUsuario = row["NombreUsuario"]?.ToString(),
            FotoPerfilUsuario = row["FotoPerfilUsuario"]?.ToString() ?? "/img/perfil/default.png",
            NombreMascota = row["NombreMascota"]?.ToString()
        });
    }
    ViewBag.Historias = historias;
    
    // ⭐ Obtener historias destacadas
    string qDestacadas = @"
        SELECT HD.*, H.ImagenUrl
        FROM HistoriaDestacada HD
        INNER JOIN Historia H ON HD.Id_Historia = H.Id_Historia
        WHERE HD.Id_User = @PerfilId
        ORDER BY HD.Fecha DESC";
    var dtDestacadas = BD.ExecuteQuery(qDestacadas, new Dictionary<string, object> { { "@PerfilId", perfilId } });
    
    var destacadas = new List<HistoriaDestacada>();
    foreach (System.Data.DataRow row in dtDestacadas.Rows)
    {
        destacadas.Add(new HistoriaDestacada
        {
            Id_Destacada = Convert.ToInt32(row["Id_Destacada"]),
            Id_User = Convert.ToInt32(row["Id_User"]),
            Id_Historia = Convert.ToInt32(row["Id_Historia"]),
            Titulo = row["Titulo"]?.ToString(),
            Fecha = Convert.ToDateTime(row["Fecha"]),
            ImagenUrl = row["ImagenUrl"]?.ToString()
        });
    }
    ViewBag.HistoriasDestacadas = destacadas;
    
    // 👥 Obtener amigos (para mostrar en el perfil)
    string qAmigos = @"
        SELECT TOP 6 U.Id_User, U.Nombre + ' ' + U.Apellido as NombreCompleto, PR.FotoPerfil
        FROM CirculoConfianza CC
        INNER JOIN [User] U ON CC.Id_Amigo = U.Id_User
        LEFT JOIN Perfil PR ON PR.Id_Usuario = U.Id_User
        WHERE CC.Id_User = @PerfilId
        ORDER BY CC.UltimaConexion DESC";
    var dtAmigos = BD.ExecuteQuery(qAmigos, new Dictionary<string, object> { { "@PerfilId", perfilId } });
    
    var amigos = new List<object>();
    foreach (System.Data.DataRow row in dtAmigos.Rows)
    {
        amigos.Add(new
        {
            Id = Convert.ToInt32(row["Id_User"]),
            Nombre = row["NombreCompleto"]?.ToString() ?? "Usuario",
            FotoPerfil = row["FotoPerfil"]?.ToString() ?? "/img/perfil/default.png"
        });
    }
    ViewBag.Amigos = amigos;
    
    return View("Perfil");
}

[HttpPost]
[ValidateAntiForgeryToken]
public IActionResult ActualizarPerfil(string Nombre, string Apellido, string Pais, string Descripcion, IFormFile FotoPerfil)
{
    var userId = HttpContext.Session.GetInt32("UserId");
    if (userId == null)
        return RedirectToAction("Login", "Auth");

    try
    {
        // ✅ Limpiar y validar datos antes de guardar
        string LimpiarTexto(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto)) return "";
            return System.Text.RegularExpressions.Regex.Replace(
                texto.Trim(), 
                @"[\x00-\x1F\x7F-\x9F]", 
                ""
            ).Replace("??", "").Replace("?", "");
        }
        
        var nombreLimpio = LimpiarTexto(Nombre ?? "");
        var apellidoLimpio = LimpiarTexto(Apellido ?? "");
        var paisLimpio = LimpiarTexto(Pais ?? "Argentina");
        var descripcionLimpia = LimpiarTexto(Descripcion ?? "");
        
        // Validar nombres (deben tener al menos 2 caracteres y al menos una letra)
        if (string.IsNullOrWhiteSpace(nombreLimpio) || nombreLimpio.Length < 2 || nombreLimpio.All(c => !char.IsLetter(c)))
            nombreLimpio = "";
        if (string.IsNullOrWhiteSpace(apellidoLimpio) || apellidoLimpio.Length < 2 || apellidoLimpio.All(c => !char.IsLetter(c)))
            apellidoLimpio = "";
        
        if (string.IsNullOrWhiteSpace(paisLimpio))
            paisLimpio = "Argentina";
        
        if (string.IsNullOrWhiteSpace(descripcionLimpia))
            descripcionLimpia = "Amante de los animales";
        
        // Actualizar datos del usuario
        BD.ExecuteNonQuery(@"
            UPDATE [User]
            SET Nombre = @Nombre, Apellido = @Apellido, Pais = @Pais
            WHERE Id_User = @Id", new Dictionary<string, object>
        {
            { "@Nombre", nombreLimpio },
            { "@Apellido", apellidoLimpio },
            { "@Pais", paisLimpio },
            { "@Id", userId.Value }
        });

        // Manejar foto de perfil si se subió
        string fotoPerfilPath = null;
        if (FotoPerfil != null && FotoPerfil.Length > 0)
        {
            var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "perfiles");
            if (!Directory.Exists(uploadsPath))
                Directory.CreateDirectory(uploadsPath);

            var fileName = $"perfil_{userId}_{Guid.NewGuid()}{Path.GetExtension(FotoPerfil.FileName)}";
            var filePath = Path.Combine(uploadsPath, fileName);
            
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                FotoPerfil.CopyTo(stream);
            }
            
            fotoPerfilPath = $"/uploads/perfiles/{fileName}";
        }

        // Actualizar perfil
        string queryUpdate = @"
            UPDATE Perfil
            SET Descripcion = @Descripcion" + (fotoPerfilPath != null ? ", FotoPerfil = @FotoPerfil" : "") + @"
            WHERE Id_Usuario = @Id";

        var parametros = new Dictionary<string, object>
        {
            { "@Descripcion", descripcionLimpia },
            { "@Id", userId.Value }
        };

        if (fotoPerfilPath != null)
        {
            parametros.Add("@FotoPerfil", fotoPerfilPath);
        }

        BD.ExecuteNonQuery(queryUpdate, parametros);

        TempData["Exito"] = "Perfil actualizado correctamente";
    }
    catch (Exception ex)
    {
            Console.WriteLine("Error ActualizarPerfil: " + ex.Message);
        TempData["Error"] = "Error al actualizar el perfil. Intenta nuevamente.";
    }

    return RedirectToAction("Perfil");
}

[HttpPost]
public IActionResult CambiarTema(string modo)
{
    if (modo != "claro" && modo != "oscuro") modo = "claro";
    HttpContext.Session.SetString("Tema", modo);
    return RedirectToAction("ConfigTema");
}

// 1) GET ConfigMascotas: listado completo
[HttpGet]
public IActionResult ConfigMascotas()
{
    var userId = HttpContext.Session.GetInt32("UserId");
    if (userId == null)
        return RedirectToAction("Login", "Auth");

    // Verificar que NO sea proveedor, si es proveedor, redirigir
    if (EsProveedor())
    {
        return RedirigirProveedorSiEsNecesario();
    }

    // Limpiar TempData de errores que no son relevantes para esta vista
    if (TempData["Error"] != null && TempData["Error"].ToString().Contains("reseña"))
    {
        TempData.Remove("Error");
    }

    ViewBag.Tema = HttpContext.Session.GetString("Tema") ?? "claro";

    // ✅ Evita duplicados por nombre y raza (solo muestra la más nueva)
    string qActivas = @"
        WITH MascotasUnicas AS (
            SELECT 
                Id_Mascota, Nombre, Especie, Raza, Foto, 
                ROW_NUMBER() OVER (PARTITION BY Nombre, Raza ORDER BY Id_Mascota DESC) AS rn
            FROM Mascota
            WHERE Id_User = @U AND (Archivada IS NULL OR Archivada = 0)
        )
        SELECT Id_Mascota, Nombre, Especie, Raza, Foto 
        FROM MascotasUnicas WHERE rn = 1
        ORDER BY Nombre ASC;";

    var dtAct = BD.ExecuteQuery(qActivas, new Dictionary<string, object> { { "@U", userId.Value } });
    var listaAct = new List<Mascota>();
    foreach (System.Data.DataRow r in dtAct.Rows)
    {
        string fotoRaw = r["Foto"]?.ToString()?.Trim() ?? "";
        string especie = (r["Especie"]?.ToString() ?? "perro").ToLower();
        string raza = r["Raza"]?.ToString()?.Trim() ?? "";
        string fotoFinal = "";
        
        // Si hay foto en BD, usarla (asegurando que empiece con /)
        if (!string.IsNullOrWhiteSpace(fotoRaw))
        {
            fotoFinal = fotoRaw.StartsWith("/") ? fotoRaw : "/" + fotoRaw;
        }
        else
        {
            // Si no hay foto, construir ruta basada en especie/raza como en otras vistas
            if (string.IsNullOrWhiteSpace(raza))
            {
                fotoFinal = $"/img/mascotas/{especie}s/basico/{especie}_basico.png";
            }
            else
            {
                fotoFinal = $"/img/mascotas/{especie}s/{raza}/{especie}_basico.png";
            }
        }
        
        listaAct.Add(new Mascota {
            Id_Mascota = Convert.ToInt32(r["Id_Mascota"]),
            Nombre = r["Nombre"]?.ToString() ?? "Sin nombre",
            Especie = r["Especie"]?.ToString() ?? "Perro",
            Raza = raza,
            Foto = fotoFinal
        });
    }

    // 🗃 Archivadas también sin duplicados
    string qArch = @"
        WITH MascotasArchivadas AS (
            SELECT 
                Id_Mascota, Nombre, Especie, Raza, Foto, 
                ROW_NUMBER() OVER (PARTITION BY Nombre, Raza ORDER BY Id_Mascota DESC) AS rn
            FROM Mascota
            WHERE Id_User = @U AND Archivada = 1
        )
        SELECT Id_Mascota, Nombre, Especie, Raza, Foto 
        FROM MascotasArchivadas WHERE rn = 1
        ORDER BY Nombre ASC;";

    var dtArch = BD.ExecuteQuery(qArch, new Dictionary<string, object> { { "@U", userId.Value } });
    var listaArch = new List<Mascota>();
    foreach (System.Data.DataRow r in dtArch.Rows)
    {
        string fotoRaw = r["Foto"]?.ToString()?.Trim() ?? "";
        string especie = (r["Especie"]?.ToString() ?? "perro").ToLower();
        string raza = r["Raza"]?.ToString()?.Trim() ?? "";
        string fotoFinal = "";
        
        // Si hay foto en BD, usarla (asegurando que empiece con /)
        if (!string.IsNullOrWhiteSpace(fotoRaw))
        {
            fotoFinal = fotoRaw.StartsWith("/") ? fotoRaw : "/" + fotoRaw;
        }
        else
        {
            // Si no hay foto, construir ruta basada en especie/raza como en otras vistas
            if (string.IsNullOrWhiteSpace(raza))
            {
                fotoFinal = $"/img/mascotas/{especie}s/basico/{especie}_basico.png";
            }
            else
            {
                fotoFinal = $"/img/mascotas/{especie}s/{raza}/{especie}_basico.png";
            }
        }
        
        listaArch.Add(new Mascota {
            Id_Mascota = Convert.ToInt32(r["Id_Mascota"]),
            Nombre = r["Nombre"]?.ToString() ?? "Sin nombre",
            Especie = r["Especie"]?.ToString() ?? "Perro",
            Raza = raza,
            Foto = fotoFinal
        });
    }

    ViewBag.MascotasActivas = listaAct;
    ViewBag.MascotasArchivadas = listaArch;

    return View();
}[HttpGet]
[Route("Home/ConfigUsuario")]
public IActionResult ConfigUsuario()
{
    var userId = HttpContext.Session.GetInt32("UserId");
    if (userId == null)
        return RedirectToAction("Login", "Auth");

    ViewBag.Tema = HttpContext.Session.GetString("Tema") ?? "claro";

    try
    {
        string query = @"
            SELECT U.Nombre, U.Apellido, M.Correo, U.Telefono
            FROM [User] U
            INNER JOIN Mail M ON U.Id_Mail = M.Id_Mail
            WHERE U.Id_User = @Id";

        var dt = BD.ExecuteQuery(query, new Dictionary<string, object> { { "@Id", userId.Value } });

        if (dt.Rows.Count == 0)
        {
            ViewBag.Nombre = "";
            ViewBag.Apellido = "";
            ViewBag.Mail = "";
            ViewBag.Telefono = "";
        }
        else
        {
            var u = dt.Rows[0];
            ViewBag.Nombre = u["Nombre"].ToString();
            ViewBag.Apellido = u["Apellido"].ToString();
            ViewBag.Mail = u["Correo"].ToString();
            ViewBag.Telefono = u["Telefono"]?.ToString() ?? "";
        }

        return View("~/Views/Home/ConfigUsuario.cshtml");
    }
    catch (Exception ex)
    {
        Console.WriteLine("❌ Error ConfigUsuario: " + ex.Message);
        TempData["Error"] = "Error al cargar los datos del usuario.";
        return RedirectToAction("Configuracion");
    }
}

[HttpPost]
[ValidateAntiForgeryToken]
public IActionResult ActualizarDatosUsuario(string Nombre, string Apellido, string Correo, string Telefono)
{
    var userId = HttpContext.Session.GetInt32("UserId");
    if (userId == null)
        return RedirectToAction("Login", "Auth");

    try
    {
        // Verificar que el correo no esté en uso por otro usuario
        string qVerificarCorreo = @"
            SELECT COUNT(*) 
            FROM Mail M 
            INNER JOIN [User] U ON M.Id_Mail = U.Id_Mail
            WHERE M.Correo = @Correo AND U.Id_User <> @Id";
        
        int correoEnUso = Convert.ToInt32(BD.ExecuteScalar(qVerificarCorreo, new Dictionary<string, object>
        {
            { "@Correo", Correo?.Trim() ?? "" },
            { "@Id", userId.Value }
        }));

        if (correoEnUso > 0)
        {
            TempData["Error"] = "Este correo electrónico ya está en uso por otro usuario";
            return RedirectToAction("ConfigUsuario");
        }

        // Actualizar datos del usuario
        BD.ExecuteNonQuery(@"
            UPDATE [User]
            SET Nombre = @N, Apellido = @A, Telefono = @T
            WHERE Id_User = @Id", new Dictionary<string, object>
        {
            { "@N", Nombre?.Trim() ?? "" },
            { "@A", Apellido?.Trim() ?? "" },
            { "@T", Telefono?.Trim() ?? "" },
            { "@Id", userId.Value }
        });

        // Actualizar correo
        BD.ExecuteNonQuery(@"
            UPDATE M
            SET M.Correo = @Correo
            FROM Mail M
            INNER JOIN [User] U ON M.Id_Mail = U.Id_Mail
            WHERE U.Id_User = @Id_User", new Dictionary<string, object>
        {
            { "@Correo", Correo?.Trim() ?? "" },
            { "@Id_User", userId.Value }
        });

        TempData["Exito"] = "Datos personales actualizados correctamente ✅";
    }
    catch (Exception ex)
    {
        Console.WriteLine("❌ Error ActualizarDatosUsuario: " + ex.Message);
        TempData["Error"] = "Error al actualizar los datos.";
    }

    return RedirectToAction("ConfigUsuario");
}

[HttpPost]
[ValidateAntiForgeryToken]
public IActionResult CambiarContrasena(string ContrasenaActual, string NuevaContrasena, string ConfirmarContrasena)
{
    var userId = HttpContext.Session.GetInt32("UserId");
    if (userId == null)
        return RedirectToAction("Login", "Auth");

    try
    {
        // Validaciones
        if (string.IsNullOrWhiteSpace(NuevaContrasena) || NuevaContrasena.Length < 6)
        {
            TempData["Error"] = "La nueva contraseña debe tener al menos 6 caracteres";
            return RedirectToAction("ConfigUsuario");
        }

        if (NuevaContrasena != ConfirmarContrasena)
        {
            TempData["Error"] = "Las contraseñas no coinciden";
            return RedirectToAction("ConfigUsuario");
        }

        // Verificar contraseña actual
        string qCheck = @"
            SELECT M.Contrasena
            FROM Mail M 
            INNER JOIN [User] U ON M.Id_Mail = U.Id_Mail
            WHERE U.Id_User = @Id";
        var contrasenaBD = BD.ExecuteScalar(qCheck, new Dictionary<string, object> { { "@Id", userId.Value } })?.ToString();

        if (string.IsNullOrEmpty(ContrasenaActual) || contrasenaBD != ContrasenaActual)
        {
            TempData["Error"] = "La contraseña actual no es correcta ❌";
            return RedirectToAction("ConfigUsuario");
        }

        // Actualizar contraseña
        BD.ExecuteNonQuery(@"
            UPDATE M
            SET M.Contrasena = @NuevaContrasena
            FROM Mail M
            INNER JOIN [User] U ON M.Id_Mail = U.Id_Mail
            WHERE U.Id_User = @Id_User", new Dictionary<string, object>
        {
            { "@NuevaContrasena", NuevaContrasena },
            { "@Id_User", userId.Value }
        });

        TempData["Exito"] = "Contraseña cambiada correctamente ✅";
    }
    catch (Exception ex)
    {
        Console.WriteLine("❌ Error CambiarContrasena: " + ex.Message);
        TempData["Error"] = "Error al cambiar la contraseña.";
    }

    return RedirectToAction("ConfigUsuario");
}

[HttpPost]
[ValidateAntiForgeryToken]
public IActionResult EliminarCuenta(string Contrasena)
{
    var userId = HttpContext.Session.GetInt32("UserId");
    if (userId == null)
        return RedirectToAction("Login", "Auth");

    try
    {
        // Verificar contraseña
        string qCheck = @"
            SELECT M.Contrasena
            FROM Mail M 
            INNER JOIN [User] U ON M.Id_Mail = U.Id_Mail
            WHERE U.Id_User = @Id";
        var contrasenaBD = BD.ExecuteScalar(qCheck, new Dictionary<string, object> { { "@Id", userId.Value } })?.ToString();

        if (string.IsNullOrEmpty(Contrasena) || contrasenaBD != Contrasena)
        {
            TempData["Error"] = "La contraseña no es correcta ❌";
            return RedirectToAction("ConfigUsuario");
        }

        // Desactivar cuenta (soft delete)
        BD.ExecuteNonQuery(@"
            UPDATE [User]
            SET Estado = 0
            WHERE Id_User = @Id", new Dictionary<string, object>
        {
            { "@Id", userId.Value }
        });

        // Limpiar sesión
        HttpContext.Session.Clear();

        TempData["Exito"] = "Tu cuenta ha sido eliminada. Gracias por usar Zooni.";
        return RedirectToAction("Login", "Auth");
    }
    catch (Exception ex)
    {
        Console.WriteLine("❌ Error EliminarCuenta: " + ex.Message);
        TempData["Error"] = "Error al eliminar la cuenta.";
        return RedirectToAction("ConfigUsuario");
    }
}
[HttpGet]
public IActionResult ConfigAyuda()
{
    var userId = HttpContext.Session.GetInt32("UserId");
    if (userId == null)
        return RedirectToAction("Login", "Auth");

    ViewBag.Tema = HttpContext.Session.GetString("Tema") ?? "claro";
    return View();
}[HttpPost]
public IActionResult EnviarSoporte(string nombre, string correo, string contrasena, string mensaje)
{
    try
    {
        if (string.IsNullOrWhiteSpace(nombre) || string.IsNullOrWhiteSpace(correo) ||
            string.IsNullOrWhiteSpace(contrasena) || string.IsNullOrWhiteSpace(mensaje))
        {
            TempData["Error"] = "Completá todos los campos antes de enviar 🐾";
            return RedirectToAction("ConfigAyuda");
        }

        // 🔍 Validar si la combinación correo + contraseña es correcta
        string q = @"
            SELECT COUNT(*) 
            FROM Mail M
            INNER JOIN [User] U ON M.Id_Mail = U.Id_Mail
            WHERE M.Correo = @Correo AND M.Contrasena = @Contrasena";

        var param = new Dictionary<string, object>
        {
            { "@Correo", correo },
            { "@Contrasena", contrasena }
        };

        int valido = Convert.ToInt32(BD.ExecuteScalar(q, param));

        if (valido == 0)
        {
            TempData["Error"] = "La contraseña no coincide con el correo ingresado 🐾";
            return RedirectToAction("ConfigAyuda");
        }

        // ✅ Si es válido, se envía el mensaje (simulado por consola)
        Console.WriteLine($"📩 Soporte recibido de {nombre} ({correo}): {mensaje}");

        TempData["Exito"] = "Tu mensaje fue enviado correctamente 🩵 ¡Gracias por comunicarte con Zooni!";
        return RedirectToAction("ConfigAyuda");
    }
    catch (Exception ex)
    {
        Console.WriteLine("❌ Error en EnviarSoporte: " + ex.Message);
        TempData["Error"] = "Hubo un problema al enviar el mensaje. Intentá de nuevo más tarde.";
        return RedirectToAction("ConfigAyuda");
    }
}






// 3) GET EditarMascota: cargar vista de edición
[HttpGet]
public IActionResult EditarMascota(int id)
{
    var userId = HttpContext.Session.GetInt32("UserId");
    if (userId == null)
        return RedirectToAction("Login", "Auth");

    string q = "SELECT * FROM Mascota WHERE Id_Mascota = @Id AND Id_User = @U";
    var dt = BD.ExecuteQuery(q, new Dictionary<string, object> { { "@Id", id }, { "@U", userId.Value } });
    if (dt.Rows.Count == 0)
        return RedirectToAction("ConfigMascotas");

    var r = dt.Rows[0];
    
    // 🎂 Cargar Fecha_Nacimiento
    DateTime? fechaNac = null;
    if (r["Fecha_Nacimiento"] != DBNull.Value)
    {
        fechaNac = Convert.ToDateTime(r["Fecha_Nacimiento"]);
    }
    
    // Calcular edad automáticamente desde Fecha_Nacimiento si existe
    int edadCalculada = fechaNac.HasValue ? EdadHelper.CalcularEdadEnMeses(fechaNac) : 
                        (r["Edad"] == DBNull.Value ? 0 : Convert.ToInt32(r["Edad"]));
    
    // Manejo mejorado del peso usando PesoHelper
    decimal pesoDecimal = 0;
    string? pesoDisplayBD = null;
    
    if (dt.Columns.Contains("PesoDisplay") && r["PesoDisplay"] != DBNull.Value)
    {
        pesoDisplayBD = r["PesoDisplay"].ToString();
    }
    
    if (r["Peso"] != DBNull.Value && decimal.TryParse(r["Peso"].ToString(), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out pesoDecimal))
    {
        // ✅ Corrección global: dividir por 10 si no hay PesoDisplay y el peso parece incorrecto (>= 10)
        if (string.IsNullOrEmpty(pesoDisplayBD) && pesoDecimal >= 10)
        {
            decimal pesoCorregido = pesoDecimal / 10;
            if (pesoCorregido <= 200 && pesoCorregido >= 0.1M)
            {
                pesoDecimal = pesoCorregido;
                pesoDisplayBD = PesoHelper.FormatearPeso(pesoDecimal);
            }
        }
        // Si el peso es muy alto incluso después de dividir, aplicar corrección adicional
        else if (string.IsNullOrEmpty(pesoDisplayBD) && pesoDecimal > 200)
        {
            decimal pesoCorregido = pesoDecimal / 10;
            if (pesoCorregido > 200)
            {
                pesoCorregido = pesoDecimal / 100;
            }
            if (pesoCorregido <= 200 && pesoCorregido >= 0.1M)
            {
                pesoDecimal = pesoCorregido;
                pesoDisplayBD = PesoHelper.FormatearPeso(pesoDecimal);
            }
        }
    }
    
    var m = new Mascota {
        Id_Mascota = id,
        Nombre     = r["Nombre"].ToString(),
        Especie    = r["Especie"].ToString(),
        Raza       = r["Raza"].ToString(),
        Edad       = edadCalculada,
        Peso       = pesoDecimal > 0 ? pesoDecimal : (r["Peso"] == DBNull.Value ? 0.1M : Convert.ToDecimal(r["Peso"])),
        Sexo       = r["Sexo"].ToString(),
        Foto       = r["Foto"]?.ToString(),
        Fecha_Nacimiento = fechaNac
    };
    
    // Cargar PesoDisplay (ya corregido si fue necesario)
    ViewBag.PesoDisplay = pesoDisplayBD ?? PesoHelper.FormatearPeso(m.Peso);
    
    ViewBag.Tema = HttpContext.Session.GetString("Tema") ?? "claro";
    ViewBag.Especie = m.Especie; // Pasar especie para el select de razas
    return View(m);
}

// 4) POST (o PUT) para cambiar color o editar datos (simplificado)
[HttpPost]
public IActionResult GuardarMascotaEditada(Mascota model, string PesoDisplay, DateTime? Fecha_Nacimiento)
{
    var userId = HttpContext.Session.GetInt32("UserId");
    if (userId == null)
        return RedirectToAction("Login", "Auth");

    // 🎂 Calcular edad automáticamente desde Fecha_Nacimiento si existe
    int edadFinal = model.Edad;
    if (Fecha_Nacimiento.HasValue && Fecha_Nacimiento.Value != DateTime.MinValue)
    {
        edadFinal = EdadHelper.CalcularEdadEnMeses(Fecha_Nacimiento);
    }

    // Normalizar peso
    var (pesoNormalizado, pesoDisplayFinal) = PesoHelper.NormalizarPeso(PesoDisplay ?? model.Peso.ToString());
    if (model.Peso > 0)
    {
        pesoNormalizado = model.Peso;
    }

    string q = @"
        UPDATE Mascota
        SET Nombre = @Nombre,
            Raza   = @Raza,
            Edad   = @Edad,
            Peso   = @Peso,
            Sexo   = @Sexo,
            Foto   = @Foto,
            TagColor = @TagColor,
            Fecha_Nacimiento = @FechaNac,
            PesoDisplay = @PesoDisplay
        WHERE Id_Mascota = @Id AND Id_User = @U";
    BD.ExecuteNonQuery(q, new Dictionary<string,object> {
        { "@Nombre", model.Nombre },
        { "@Raza", model.Raza },
        { "@Edad", edadFinal },
        { "@Peso", pesoNormalizado },
        { "@Sexo", model.Sexo },
        { "@Foto", model.Foto ?? "" },
        { "@TagColor", model.TagColor ?? "" },
        { "@FechaNac", Fecha_Nacimiento.HasValue && Fecha_Nacimiento.Value != DateTime.MinValue ? (object)Fecha_Nacimiento.Value : DBNull.Value },
        { "@PesoDisplay", pesoDisplayFinal ?? "" },
        { "@Id", model.Id_Mascota },
        { "@U", userId.Value }
    });

    TempData["Exito"] = "Datos de la mascota guardados ✅";
        return RedirectToAction("ConfigMascotas");
    }

    // ============================
    // POST: ArchivarMascota
    // ============================
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ArchivarMascota(int id)
    {
        try
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Auth");

            // Verificar que la mascota pertenece al usuario
            string checkQuery = "SELECT Id_Mascota FROM Mascota WHERE Id_Mascota = @Id AND Id_User = @UserId";
            var checkResult = BD.ExecuteQuery(checkQuery, new Dictionary<string, object> 
            { 
                { "@Id", id }, 
                { "@UserId", userId.Value } 
            });

            if (checkResult.Rows.Count == 0)
            {
                TempData["Error"] = "Mascota no encontrada o no tenés permiso para archivarla.";
                return RedirectToAction("ConfigMascotas");
            }

            // Archivar la mascota (marcar Archivada = 1)
            string updateQuery = "UPDATE Mascota SET Archivada = 1 WHERE Id_Mascota = @Id AND Id_User = @UserId";
            BD.ExecuteNonQuery(updateQuery, new Dictionary<string, object> 
            { 
                { "@Id", id }, 
                { "@UserId", userId.Value } 
            });

            // Si la mascota archivada era la activa, limpiar la sesión
            int? mascotaActivaId = HttpContext.Session.GetInt32("MascotaId");
            if (mascotaActivaId == id)
            {
                HttpContext.Session.Remove("MascotaId");
            }

            TempData["Exito"] = "Mascota archivada correctamente ✅";
            return RedirectToAction("ConfigMascotas");
        }
        catch (Exception ex)
        {
            Console.WriteLine("❌ Error en ArchivarMascota: " + ex.Message);
            TempData["Error"] = "Error al archivar la mascota.";
            return RedirectToAction("ConfigMascotas");
        }
    }

    // ============================
    // POST: DesarchivarMascota
    // ============================
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult DesarchivarMascota(int id)
    {
        try
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Auth");

            // Verificar que la mascota pertenece al usuario y está archivada
            string checkQuery = @"
                SELECT Id_Mascota, Archivada 
                FROM Mascota 
                WHERE Id_Mascota = @Id AND Id_User = @UserId";
            var checkResult = BD.ExecuteQuery(checkQuery, new Dictionary<string, object> 
            { 
                { "@Id", id }, 
                { "@UserId", userId.Value } 
            });

            if (checkResult.Rows.Count == 0)
            {
                TempData["Error"] = "Mascota no encontrada o no tenés permiso para recuperarla.";
                return RedirectToAction("ConfigMascotas");
            }

            // Verificar que realmente está archivada
            var row = checkResult.Rows[0];
            bool estaArchivada = row["Archivada"] != DBNull.Value && Convert.ToBoolean(row["Archivada"]);
            
            if (!estaArchivada)
            {
                TempData["Info"] = "Esta mascota ya está activa.";
                return RedirectToAction("ConfigMascotas");
            }

            // Desarchivar la mascota (marcar Archivada = 0 o NULL)
            string updateQuery = @"
                UPDATE Mascota 
                SET Archivada = 0 
                WHERE Id_Mascota = @Id AND Id_User = @UserId";
            
            int rowsAffected = BD.ExecuteNonQuery(updateQuery, new Dictionary<string, object> 
            { 
                { "@Id", id }, 
                { "@UserId", userId.Value } 
            });

            if (rowsAffected > 0)
            {
                TempData["Exito"] = "Mascota recuperada correctamente ✅";
            }
            else
            {
                TempData["Error"] = "No se pudo recuperar la mascota. Intentá nuevamente.";
            }

            return RedirectToAction("ConfigMascotas");
        }
        catch (Exception ex)
        {
            Console.WriteLine("❌ Error en DesarchivarMascota: " + ex.Message);
            Console.WriteLine("Stack trace: " + ex.StackTrace);
            TempData["Error"] = "Error al recuperar la mascota: " + ex.Message;
            return RedirectToAction("ConfigMascotas");
        }
    }

    // 🎂 POST: Actualizar peso desde FichaMedica
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ActualizarPesoMascota(string peso)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
            return Json(new { success = false, message = "No autorizado" });

        try
        {
            var mascota = ObtenerMascotaActiva(userId.Value);
            if (mascota == null)
                return Json(new { success = false, message = "Mascota no encontrada" });

            int idMascota = Convert.ToInt32(mascota["Id_Mascota"]);
            bool esPropietario = mascota.Table.Columns.Contains("EsPropietario") && 
                                 Convert.ToInt32(mascota["EsPropietario"]) == 1;
            bool permisoEdicion = !mascota.Table.Columns.Contains("Permiso_Edicion") || 
                                 mascota["Permiso_Edicion"] == DBNull.Value ||
                                 Convert.ToBoolean(mascota["Permiso_Edicion"]);
            
            // Verificar permisos: solo propietario o usuario compartido con permiso de edición
            if (!esPropietario && !permisoEdicion)
                return Json(new { success = false, message = "No tenés permisos para editar esta mascota" });
            
            // Normalizar peso
            var (pesoNormalizado, pesoDisplay) = PesoHelper.NormalizarPeso(peso);

            // Si es propietario, actualizar directamente. Si no, también puede actualizar si tiene permiso
            string q = @"
                UPDATE Mascota
                SET Peso = @Peso, PesoDisplay = @PesoDisplay
                WHERE Id_Mascota = @Id";
            BD.ExecuteNonQuery(q, new Dictionary<string, object> {
                { "@Peso", pesoNormalizado },
                { "@PesoDisplay", pesoDisplay ?? "" },
                { "@Id", idMascota }
            });

            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            Console.WriteLine("❌ Error en ActualizarPesoMascota: " + ex.Message);
            return Json(new { success = false, message = "Error al actualizar el peso" });
        }
    }

    // 🎂 POST: Actualizar fecha de nacimiento desde FichaMedica
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ActualizarFechaNacimientoMascota(string fechaNacimiento)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
            return Json(new { success = false, message = "No autorizado" });

        try
        {
            var mascota = ObtenerMascotaActiva(userId.Value);
            if (mascota == null)
                return Json(new { success = false, message = "Mascota no encontrada" });

            int idMascota = Convert.ToInt32(mascota["Id_Mascota"]);
            bool esPropietario = mascota.Table.Columns.Contains("EsPropietario") && 
                                 Convert.ToInt32(mascota["EsPropietario"]) == 1;
            bool permisoEdicion = !mascota.Table.Columns.Contains("Permiso_Edicion") || 
                                 mascota["Permiso_Edicion"] == DBNull.Value ||
                                 Convert.ToBoolean(mascota["Permiso_Edicion"]);
            
            // Verificar permisos: solo propietario o usuario compartido con permiso de edición
            if (!esPropietario && !permisoEdicion)
                return Json(new { success = false, message = "No tenés permisos para editar esta mascota" });
            
            // Parsear fecha de nacimiento
            DateTime? fechaNac = null;
            if (!string.IsNullOrEmpty(fechaNacimiento) && DateTime.TryParse(fechaNacimiento, out DateTime fecha))
            {
                fechaNac = fecha;
            }

            // Calcular edad automáticamente desde fecha de nacimiento
            int edadCalculada = fechaNac.HasValue ? EdadHelper.CalcularEdadEnMeses(fechaNac) : 
                                (mascota["Edad"] == DBNull.Value ? 0 : Convert.ToInt32(mascota["Edad"]));

            string q = @"
                UPDATE Mascota
                SET Fecha_Nacimiento = @FechaNac, Edad = @Edad
                WHERE Id_Mascota = @Id";
            BD.ExecuteNonQuery(q, new Dictionary<string, object> {
                { "@FechaNac", fechaNac.HasValue ? (object)fechaNac.Value : DBNull.Value },
                { "@Edad", edadCalculada },
                { "@Id", idMascota }
            });

            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            Console.WriteLine("❌ Error en ActualizarFechaNacimientoMascota: " + ex.Message);
            return Json(new { success = false, message = "Error al actualizar la fecha de nacimiento" });
        }
    }

    [HttpGet]
    public IActionResult ServicioVeterinarios()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
            return RedirectToAction("Login", "Auth");

        // Verificar que NO sea proveedor, si es proveedor, redirigir
        if (EsProveedor())
        {
            return RedirigirProveedorSiEsNecesario();
        }

        var tema = HttpContext.Session.GetString("Tema") ?? "claro";
        ViewBag.Tema = tema;

        // Obtener veterinarios de emergencia o disponibles
        try
        {
            string query = @"
                SELECT TOP 10 
                    V.Id_Vet, U.Nombre, U.Apellido, V.Especialidad, 
                    V.Clinica, V.Horario_Atencion, V.Valoracion_Promedio,
                    M.Correo, U.Telefono
                FROM Veterinario V
                INNER JOIN [User] U ON V.Id_User = U.Id_User
                INNER JOIN Mail M ON U.Id_Mail = M.Id_Mail
                WHERE V.Valoracion_Promedio >= 4.0
                ORDER BY V.Valoracion_Promedio DESC";

            var dtVets = BD.ExecuteQuery(query, new Dictionary<string, object>());
            
            // Si no hay veterinarios en BD, cargar desde JSON
            if (dtVets == null || dtVets.Rows.Count == 0)
            {
                var jsonPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "data", "veterinarios.json");
                if (System.IO.File.Exists(jsonPath))
                {
                    var jsonContent = System.IO.File.ReadAllText(jsonPath);
                    var veterinariosData = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(jsonContent);
                    
                    // Crear DataTable desde JSON
                    dtVets = new System.Data.DataTable();
                    dtVets.Columns.Add("Nombre", typeof(string));
                    dtVets.Columns.Add("Apellido", typeof(string));
                    dtVets.Columns.Add("Especialidad", typeof(string));
                    dtVets.Columns.Add("Clinica", typeof(string));
                    dtVets.Columns.Add("Horario_Atencion", typeof(string));
                    dtVets.Columns.Add("Valoracion_Promedio", typeof(decimal));
                    dtVets.Columns.Add("Correo", typeof(string));
                    dtVets.Columns.Add("Telefono", typeof(string));
                    dtVets.Columns.Add("Direccion", typeof(string));
                    dtVets.Columns.Add("Lat", typeof(double));
                    dtVets.Columns.Add("Lng", typeof(double));
                    dtVets.Columns.Add("GoogleMaps", typeof(string));
                    
                    if (veterinariosData.TryGetProperty("veterinarios", out var vets))
                    {
                        foreach (var vet in vets.EnumerateArray())
                        {
                            var row = dtVets.NewRow();
                            row["Nombre"] = vet.TryGetProperty("nombre", out var nom) ? nom.GetString() : "";
                            row["Apellido"] = vet.TryGetProperty("apellido", out var ape) ? ape.GetString() : "";
                            row["Especialidad"] = vet.TryGetProperty("especialidad", out var esp) ? esp.GetString() : "";
                            row["Clinica"] = vet.TryGetProperty("clinica", out var cli) ? cli.GetString() : "";
                            row["Horario_Atencion"] = vet.TryGetProperty("horario", out var hor) ? hor.GetString() : "";
                            row["Valoracion_Promedio"] = vet.TryGetProperty("valoracion", out var val) ? val.GetDecimal() : 0;
                            row["Correo"] = vet.TryGetProperty("correo", out var cor) ? cor.GetString() : "";
                            row["Telefono"] = vet.TryGetProperty("telefono", out var tel) ? tel.GetString() : "";
                            row["Direccion"] = vet.TryGetProperty("direccion", out var dir) ? dir.GetString() : "";
                            row["Lat"] = vet.TryGetProperty("lat", out var lat) ? lat.GetDouble() : 0.0;
                            row["Lng"] = vet.TryGetProperty("lng", out var lng) ? lng.GetDouble() : 0.0;
                            row["GoogleMaps"] = vet.TryGetProperty("googleMaps", out var maps) ? maps.GetString() : "";
                            dtVets.Rows.Add(row);
                        }
                    }
                }
            }
            
            ViewBag.Veterinarios = dtVets;
        }
        catch (Exception ex)
        {
            Console.WriteLine("❌ Error al cargar veterinarios: " + ex.Message);
            ViewBag.Veterinarios = null;
        }

        return View();
    }
[HttpGet]
public IActionResult ConfigTema()
{
    var userId = HttpContext.Session.GetInt32("UserId");
    if (userId == null)
        return RedirectToAction("Login", "Auth");

    var tema = HttpContext.Session.GetString("Tema") ?? "claro";
    ViewBag.Tema = tema;
    return View();
}

[HttpGet]
public IActionResult Closet()
{
    var userId = HttpContext.Session.GetInt32("UserId");
    if (userId == null)
        return RedirectToAction("Login", "Auth");

    // Verificar que NO sea proveedor, si es proveedor, redirigir
    if (EsProveedor())
    {
        return RedirigirProveedorSiEsNecesario();
    }

    var tema = HttpContext.Session.GetString("Tema") ?? "claro";
    ViewBag.Tema = tema;

    var mascota = ObtenerMascotaActiva(userId.Value);
    if (mascota == null)
        return RedirectToAction("Index");

    CargarViewBagMascota(mascota);
    
    var especie = mascota["Especie"]?.ToString()?.ToLower() ?? "perro";
    var raza = mascota["Raza"]?.ToString() ?? "basico"; // ✅ Usar raza exacta de la BD, sin normalizar
    
    // Obtener todos los avatares disponibles para esta especie/raza (usando raza exacta)
    var avataresDisponibles = new List<string>();
    var avatarPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img", "mascotas", $"{especie}s", raza);
    
    if (Directory.Exists(avatarPath))
    {
        var archivos = Directory.GetFiles(avatarPath, "*.png");
        foreach (var archivo in archivos)
        {
            var nombreArchivo = Path.GetFileName(archivo);
            var rutaRelativa = $"/img/mascotas/{especie}s/{raza}/{nombreArchivo}";
            avataresDisponibles.Add(rutaRelativa);
        }
    }
    
    // Si no hay avatares específicos, usar el básico
    if (avataresDisponibles.Count == 0)
    {
        avataresDisponibles.Add($"/img/mascotas/{especie}s/{raza}/{especie}_basico.png");
    }
    
    ViewBag.AvataresDisponibles = avataresDisponibles;
    // ✅ Usar MascotaAvatar del ViewBag si está disponible (ya calculado en CargarViewBagMascota)
    ViewBag.AvatarActual = HttpContext.Session.GetString("MascotaAvatar") ?? ViewBag.MascotaAvatar ?? $"/img/mascotas/{especie}s/{raza}/{especie}_basico.png";
    
    return View();
}

[HttpPost]
[ValidateAntiForgeryToken]
public IActionResult CambiarAvatar(string avatarRuta)
{
    var userId = HttpContext.Session.GetInt32("UserId");
    if (userId == null)
        return RedirectToAction("Login", "Auth");

    var mascota = ObtenerMascotaActiva(userId.Value);
    if (mascota == null)
        return RedirectToAction("Index");

    var mascotaId = Convert.ToInt32(mascota["Id_Mascota"]);
    
    // Guardar el avatar en la sesión
    HttpContext.Session.SetString("MascotaAvatar", avatarRuta);
    
    // Actualizar la mascota activa en sesión
    HttpContext.Session.SetInt32("MascotaId", mascotaId);
    
    TempData["Exito"] = "Avatar cambiado exitosamente ✅";
    return RedirectToAction("Closet");
}

    [HttpGet]
    public IActionResult Comunidad()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
            return RedirectToAction("Login", "Auth");

        // Comunidad es accesible tanto para dueños como proveedores
        // No necesitamos verificación adicional aquí

        // Asegurar que la tabla de carteles existe
        AsegurarTablaCartelesMascota();

        var tema = HttpContext.Session.GetString("Tema") ?? "claro";
        ViewBag.Tema = tema;
        return View();
    }

    // ============================
    // Método para asegurar tabla de carteles
    // ============================
    private void AsegurarTablaCartelesMascota()
    {
        try
        {
            string query = @"
                IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'CartelMascota')
                BEGIN
                    CREATE TABLE CartelMascota (
                        Id_Cartel INT IDENTITY(1,1) PRIMARY KEY,
                        Id_User INT NOT NULL,
                        Id_Mascota INT NULL,
                        Tipo VARCHAR(20) NOT NULL CHECK (Tipo IN ('Perdida', 'Encontrada')),
                        Latitud DECIMAL(11, 8) NOT NULL,
                        Longitud DECIMAL(12, 8) NOT NULL,
                        Descripcion NVARCHAR(500) NULL,
                        TelefonoContacto NVARCHAR(50) NOT NULL,
                        FotoCartel NVARCHAR(500) NULL,
                        RazaMascota NVARCHAR(100) NULL,
                        FechaCreacion DATETIME2 NOT NULL DEFAULT GETDATE(),
                        Activo BIT NOT NULL DEFAULT 1,
                        FOREIGN KEY (Id_User) REFERENCES [User](Id_User),
                        FOREIGN KEY (Id_Mascota) REFERENCES Mascota(Id_Mascota)
                    )
                END
                ELSE
                BEGIN
                    -- Agregar columna FotoCartel si no existe
                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('CartelMascota') AND name = 'FotoCartel')
                    BEGIN
                        ALTER TABLE CartelMascota ADD FotoCartel NVARCHAR(500) NULL;
                    END
                    -- Agregar columna RazaMascota si no existe
                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('CartelMascota') AND name = 'RazaMascota')
                    BEGIN
                        ALTER TABLE CartelMascota ADD RazaMascota NVARCHAR(100) NULL;
                    END
                END";
            BD.ExecuteNonQuery(query, new Dictionary<string, object>());
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error al crear tabla CartelMascota: " + ex.Message);
        }
    }

    // ============================
    // GET: Obtener carteles de mascotas
    // ============================
    [HttpGet]
    public IActionResult ObtenerCarteles()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
            return Json(new { success = false, message = "No autenticado" });

        try
        {
            AsegurarTablaCartelesMascota();
            
            string query = @"
                SELECT 
                    C.Id_Cartel,
                    C.Id_User,
                    C.Id_Mascota,
                    C.Tipo,
                    C.Latitud,
                    C.Longitud,
                    C.Descripcion,
                    C.TelefonoContacto,
                    C.FechaCreacion,
                    C.FotoCartel,
                    C.RazaMascota,
                    U.Nombre + ' ' + U.Apellido AS NombreUsuario,
                    M.Nombre AS NombreMascota,
                    M.Foto AS FotoMascota,
                    M.Especie AS EspecieMascota,
                    M.Raza AS RazaMascotaBD
                FROM CartelMascota C
                INNER JOIN [User] U ON C.Id_User = U.Id_User
                LEFT JOIN Mascota M ON C.Id_Mascota = M.Id_Mascota
                WHERE C.Activo = 1
                ORDER BY C.FechaCreacion DESC";
            
            var dt = BD.ExecuteQuery(query, new Dictionary<string, object>());
            var carteles = new List<object>();
            
            foreach (DataRow row in dt.Rows)
            {
                // Usar RazaMascota del cartel si existe, sino de la mascota
                string raza = row["RazaMascota"]?.ToString();
                if (string.IsNullOrEmpty(raza) && row.Table.Columns.Contains("RazaMascotaBD"))
                {
                    raza = row["RazaMascotaBD"]?.ToString();
                }
                
                // Usar FotoCartel si existe, sino FotoMascota
                string foto = row["FotoCartel"]?.ToString();
                if (string.IsNullOrEmpty(foto))
                {
                    foto = row["FotoMascota"]?.ToString() ?? "/img/mascotas/default.png";
                }
                
                carteles.Add(new
                {
                    id = Convert.ToInt32(row["Id_Cartel"]),
                    idUser = Convert.ToInt32(row["Id_User"]),
                    idMascota = row["Id_Mascota"] == DBNull.Value ? (int?)null : Convert.ToInt32(row["Id_Mascota"]),
                    tipo = row["Tipo"].ToString(),
                    lat = Convert.ToDecimal(row["Latitud"]),
                    lng = Convert.ToDecimal(row["Longitud"]),
                    descripcion = row["Descripcion"]?.ToString() ?? "",
                    telefono = row["TelefonoContacto"].ToString(),
                    fechaCreacion = Convert.ToDateTime(row["FechaCreacion"]).ToString("dd/MM/yyyy HH:mm"),
                    nombreUsuario = row["NombreUsuario"].ToString(),
                    nombreMascota = row["NombreMascota"]?.ToString(),
                    fotoMascota = row["FotoMascota"]?.ToString() ?? "/img/mascotas/default.png",
                    fotoCartel = foto,
                    especieMascota = row["EspecieMascota"]?.ToString(),
                    razaMascota = raza,
                    esMio = Convert.ToInt32(row["Id_User"]) == userId.Value
                });
            }
            
            return Json(new { success = true, carteles = carteles });
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error ObtenerCarteles: " + ex.Message);
            return Json(new { success = false, message = ex.Message });
        }
    }

    // ============================
    // Método para corregir tipo de dato de TelefonoContacto
    // ============================
    private void CorregirTipoTelefonoContacto()
    {
        try
        {
            // Verificar si el campo es numérico y corregirlo
            string checkQuery = @"
                SELECT t.name as TipoDato
                FROM sys.columns c
                INNER JOIN sys.types t ON c.user_type_id = t.user_type_id
                WHERE c.object_id = OBJECT_ID('CartelMascota') 
                AND c.name = 'TelefonoContacto'";
            
            var tipoResult = BD.ExecuteScalar(checkQuery, new Dictionary<string, object>());
            
            if (tipoResult != null)
            {
                string tipoDato = tipoResult.ToString().ToLower();
                if (tipoDato.Contains("numeric") || tipoDato.Contains("int") || tipoDato.Contains("decimal") || 
                    tipoDato.Contains("float") || tipoDato.Contains("real") || tipoDato.Contains("bigint") ||
                    tipoDato.Contains("smallint") || tipoDato.Contains("tinyint"))
                {
                    // Verificar estado de las columnas
                    string checkCols = @"
                        SELECT 
                            CASE WHEN EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('CartelMascota') AND name = 'TelefonoContacto') THEN 1 ELSE 0 END as ExisteOriginal,
                            CASE WHEN EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('CartelMascota') AND name = 'TelefonoContacto_Temp') THEN 1 ELSE 0 END as ExisteTemp";
                    
                    var dtCols = BD.ExecuteQuery(checkCols, new Dictionary<string, object>());
                    bool existeOriginal = Convert.ToInt32(dtCols.Rows[0]["ExisteOriginal"]) == 1;
                    bool existeTemp = Convert.ToInt32(dtCols.Rows[0]["ExisteTemp"]) == 1;
                    
                    if (existeOriginal && !existeTemp)
                    {
                        // Caso normal: columna original existe, temporal no
                        // Paso 1: Crear columna temporal
                        string step1 = "ALTER TABLE CartelMascota ADD TelefonoContacto_Temp NVARCHAR(50) NULL";
                        BD.ExecuteNonQuery(step1, new Dictionary<string, object>());
                        
                        // Paso 2: Copiar y convertir datos
                        string step2 = "UPDATE CartelMascota SET TelefonoContacto_Temp = CAST(TelefonoContacto AS NVARCHAR(50))";
                        BD.ExecuteNonQuery(step2, new Dictionary<string, object>());
                        
                        // Paso 3: Eliminar columna antigua
                        string step3 = "ALTER TABLE CartelMascota DROP COLUMN TelefonoContacto";
                        BD.ExecuteNonQuery(step3, new Dictionary<string, object>());
                        
                        // Paso 4: Renombrar columna temporal
                        string step4 = "EXEC sp_rename 'CartelMascota.TelefonoContacto_Temp', 'TelefonoContacto', 'COLUMN'";
                        BD.ExecuteNonQuery(step4, new Dictionary<string, object>());
                    }
                    else if (!existeOriginal && existeTemp)
                    {
                        // Caso: migración incompleta, solo completar renombrado
                        string step4 = "EXEC sp_rename 'CartelMascota.TelefonoContacto_Temp', 'TelefonoContacto', 'COLUMN'";
                        BD.ExecuteNonQuery(step4, new Dictionary<string, object>());
                    }
                    else if (!existeOriginal && !existeTemp)
                    {
                        // Caso: algo salió mal, crear columna directamente
                        string step1 = "ALTER TABLE CartelMascota ADD TelefonoContacto NVARCHAR(50) NOT NULL DEFAULT '0000000000'";
                        BD.ExecuteNonQuery(step1, new Dictionary<string, object>());
                        return; // Ya está corregido
                    }
                    
                    // Paso 5: Establecer como NOT NULL (si la columna ya existe como texto)
                    if (existeOriginal || existeTemp)
                    {
                        try
                        {
                            string step5 = "ALTER TABLE CartelMascota ALTER COLUMN TelefonoContacto NVARCHAR(50) NOT NULL";
                            BD.ExecuteNonQuery(step5, new Dictionary<string, object>());
                        }
                        catch
                        {
                            // Si falla, puede ser que ya sea NOT NULL o tenga datos NULL
                            // Intentar hacerlo NULL primero y luego NOT NULL
                            try
                            {
                                BD.ExecuteNonQuery("ALTER TABLE CartelMascota ALTER COLUMN TelefonoContacto NVARCHAR(50) NULL", new Dictionary<string, object>());
                                BD.ExecuteNonQuery("UPDATE CartelMascota SET TelefonoContacto = '0000000000' WHERE TelefonoContacto IS NULL", new Dictionary<string, object>());
                                BD.ExecuteNonQuery("ALTER TABLE CartelMascota ALTER COLUMN TelefonoContacto NVARCHAR(50) NOT NULL", new Dictionary<string, object>());
                            }
                            catch { }
                        }
                    }
                    
                    Console.WriteLine("✅ Tipo de dato de TelefonoContacto corregido a NVARCHAR(50)");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ Error al corregir tipo de TelefonoContacto: {ex.Message}");
            // No lanzar excepción, solo registrar el error
        }
    }

    // ============================
    // Método para corregir precisión de coordenadas
    // ============================
    private void CorregirPrecisionCoordenadas()
    {
        try
        {
            // Verificar precisión actual de Latitud
            string checkLat = @"
                SELECT c.precision, c.scale
                FROM sys.columns c
                WHERE c.object_id = OBJECT_ID('CartelMascota') 
                AND c.name = 'Latitud'";
            
            var latInfo = BD.ExecuteQuery(checkLat, new Dictionary<string, object>());
            if (latInfo.Rows.Count > 0)
            {
                int precision = Convert.ToInt32(latInfo.Rows[0]["precision"]);
                int scale = Convert.ToInt32(latInfo.Rows[0]["scale"]);
                
                Console.WriteLine($"📍 Latitud actual: DECIMAL({precision},{scale})");
                
                // Si la precisión es menor a 11, corregirla
                if (precision < 11 || scale < 8)
                {
                    Console.WriteLine($"⚠️ Latitud tiene precisión {precision},{scale}, corrigiendo a 11,8...");
                    try
                    {
                        BD.ExecuteNonQuery("ALTER TABLE CartelMascota ALTER COLUMN Latitud DECIMAL(11, 8) NOT NULL", new Dictionary<string, object>());
                        Console.WriteLine("✅ Precisión de Latitud corregida a DECIMAL(11,8).");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"❌ Error al corregir Latitud: {ex.Message}");
                        // Intentar hacerlo NULL primero
                        try
                        {
                            BD.ExecuteNonQuery("ALTER TABLE CartelMascota ALTER COLUMN Latitud DECIMAL(11, 8) NULL", new Dictionary<string, object>());
                            BD.ExecuteNonQuery("ALTER TABLE CartelMascota ALTER COLUMN Latitud DECIMAL(11, 8) NOT NULL", new Dictionary<string, object>());
                            Console.WriteLine("✅ Precisión de Latitud corregida (con paso intermedio).");
                        }
                        catch (Exception ex2)
                        {
                            Console.WriteLine($"❌ Error crítico al corregir Latitud: {ex2.Message}");
                        }
                    }
                }
                else
                {
                    Console.WriteLine($"✅ Latitud ya tiene precisión correcta: DECIMAL({precision},{scale})");
                }
            }
            
            // Verificar precisión actual de Longitud
            string checkLng = @"
                SELECT c.precision, c.scale
                FROM sys.columns c
                WHERE c.object_id = OBJECT_ID('CartelMascota') 
                AND c.name = 'Longitud'";
            
            var lngInfo = BD.ExecuteQuery(checkLng, new Dictionary<string, object>());
            if (lngInfo.Rows.Count > 0)
            {
                int precision = Convert.ToInt32(lngInfo.Rows[0]["precision"]);
                int scale = Convert.ToInt32(lngInfo.Rows[0]["scale"]);
                
                Console.WriteLine($"📍 Longitud actual: DECIMAL({precision},{scale})");
                
                // Si la precisión es menor a 12, corregirla
                if (precision < 12 || scale < 8)
                {
                    Console.WriteLine($"⚠️ Longitud tiene precisión {precision},{scale}, corrigiendo a 12,8...");
                    try
                    {
                        BD.ExecuteNonQuery("ALTER TABLE CartelMascota ALTER COLUMN Longitud DECIMAL(12, 8) NOT NULL", new Dictionary<string, object>());
                        Console.WriteLine("✅ Precisión de Longitud corregida a DECIMAL(12,8).");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"❌ Error al corregir Longitud: {ex.Message}");
                        // Intentar hacerlo NULL primero
                        try
                        {
                            BD.ExecuteNonQuery("ALTER TABLE CartelMascota ALTER COLUMN Longitud DECIMAL(12, 8) NULL", new Dictionary<string, object>());
                            BD.ExecuteNonQuery("ALTER TABLE CartelMascota ALTER COLUMN Longitud DECIMAL(12, 8) NOT NULL", new Dictionary<string, object>());
                            Console.WriteLine("✅ Precisión de Longitud corregida (con paso intermedio).");
                        }
                        catch (Exception ex2)
                        {
                            Console.WriteLine($"❌ Error crítico al corregir Longitud: {ex2.Message}");
                        }
                    }
                }
                else
                {
                    Console.WriteLine($"✅ Longitud ya tiene precisión correcta: DECIMAL({precision},{scale})");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ Error al verificar precisión de coordenadas: {ex.Message}");
        }
    }

    // ============================
    // POST: Crear cartel de mascota
    // ============================
    [HttpPost]
    public async Task<IActionResult> CrearCartel()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
            return Json(new { success = false, message = "No autenticado" });

        try
        {
            AsegurarTablaCartelesMascota();
            
            // Forzar corrección del tipo de dato antes de continuar
            Console.WriteLine("🔧 Verificando y corrigiendo tipo de dato de TelefonoContacto...");
            CorregirTipoTelefonoContacto();
            
            // Corregir precisión de Latitud y Longitud si es necesario
            Console.WriteLine("🔧 Verificando precisión de Latitud y Longitud...");
            CorregirPrecisionCoordenadas();
            
            // Verificar que la corrección funcionó
            string verifyQuery = @"
                SELECT t.name as TipoDato
                FROM sys.columns c
                INNER JOIN sys.types t ON c.user_type_id = t.user_type_id
                WHERE c.object_id = OBJECT_ID('CartelMascota') 
                AND c.name = 'TelefonoContacto'";
            var verifyResult = BD.ExecuteScalar(verifyQuery, new Dictionary<string, object>());
            if (verifyResult != null)
            {
                string tipoDato = verifyResult.ToString().ToLower();
                if (tipoDato.Contains("numeric") || tipoDato.Contains("int") || tipoDato.Contains("decimal"))
                {
                    Console.WriteLine($"⚠️ ADVERTENCIA: TelefonoContacto sigue siendo {tipoDato}. Intentando corrección forzada...");
                    // Intentar corrección forzada paso a paso
                    try
                    {
                        // Paso 1: Eliminar columna temporal si existe
                        try
                        {
                            BD.ExecuteNonQuery("ALTER TABLE CartelMascota DROP COLUMN TelefonoContacto_Temp", new Dictionary<string, object>());
                        }
                        catch { }
                        
                        // Paso 2: Crear columna temporal
                        BD.ExecuteNonQuery("ALTER TABLE CartelMascota ADD TelefonoContacto_Temp NVARCHAR(50) NULL", new Dictionary<string, object>());
                        
                        // Paso 3: Copiar datos usando SQL dinámico
                        string copySql = "UPDATE CartelMascota SET TelefonoContacto_Temp = CONVERT(NVARCHAR(50), TelefonoContacto)";
                        BD.ExecuteNonQuery(copySql, new Dictionary<string, object>());
                        
                        // Paso 4: Eliminar columna antigua
                        BD.ExecuteNonQuery("ALTER TABLE CartelMascota DROP COLUMN TelefonoContacto", new Dictionary<string, object>());
                        
                        // Paso 5: Renombrar
                        BD.ExecuteNonQuery("EXEC sp_rename 'CartelMascota.TelefonoContacto_Temp', 'TelefonoContacto', 'COLUMN'", new Dictionary<string, object>());
                        
                        // Paso 6: Actualizar NULLs
                        BD.ExecuteNonQuery("UPDATE CartelMascota SET TelefonoContacto = '0000000000' WHERE TelefonoContacto IS NULL", new Dictionary<string, object>());
                        
                        // Paso 7: NOT NULL
                        BD.ExecuteNonQuery("ALTER TABLE CartelMascota ALTER COLUMN TelefonoContacto NVARCHAR(50) NOT NULL", new Dictionary<string, object>());
                        
                        Console.WriteLine("✅ Corrección forzada completada.");
                    }
                    catch (Exception fixEx)
                    {
                        Console.WriteLine($"❌ Error en corrección forzada: {fixEx.Message}");
                        return Json(new { success = false, message = $"Error de configuración de base de datos. Por favor ejecuta el script SQL/FixCartelMascotaTelefono.sql manualmente en SQL Server Management Studio. Error: {fixEx.Message}" });
                    }
                }
                else
                {
                    Console.WriteLine($"✅ TelefonoContacto es de tipo {tipoDato} (correcto)");
                }
            }
            
            string tipo = "";
            decimal latitud = 0;
            decimal longitud = 0;
            string descripcion = "";
            string telefono = "";
            int? idMascota = null;
            string razaMascota = null;
            string fotoCartel = null;
            
            // Verificar si es FormData (con foto) o JSON
            if (Request.HasFormContentType)
            {
                tipo = Request.Form["tipo"].ToString();
                string latStr = Request.Form["latitud"].ToString();
                string lngStr = Request.Form["longitud"].ToString();
                
                Console.WriteLine($"📍 Valores recibidos - Latitud: {latStr}, Longitud: {lngStr}");
                
                // Usar InvariantCulture para parsear correctamente los decimales con punto
                latitud = decimal.Parse(latStr, CultureInfo.InvariantCulture);
                longitud = decimal.Parse(lngStr, CultureInfo.InvariantCulture);
                
                Console.WriteLine($"📍 Valores parseados - Latitud: {latitud}, Longitud: {longitud}");
                
                descripcion = Request.Form["descripcion"].ToString();
                telefono = Request.Form["telefono"].ToString();
                var idMascotaStr = Request.Form["idMascota"].ToString();
                if (!string.IsNullOrEmpty(idMascotaStr))
                {
                    idMascota = int.Parse(idMascotaStr);
                }
                
                // Procesar foto si existe
                if (Request.Form.Files.Count > 0 && Request.Form.Files["foto"] != null && Request.Form.Files["foto"].Length > 0)
                {
                    var file = Request.Form.Files["foto"];
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "carteles");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }
                    
                    var fileExtension = Path.GetExtension(file.FileName);
                    var uniqueFileName = $"{userId}_{DateTime.Now.Ticks}{fileExtension}";
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }
                    
                    fotoCartel = $"/uploads/carteles/{uniqueFileName}";
                }
            }
            else
            {
                // Es JSON
                var request = await System.Text.Json.JsonSerializer.DeserializeAsync<System.Text.Json.JsonElement>(Request.Body);
                tipo = request.TryGetProperty("tipo", out var tipoProp) ? tipoProp.GetString() ?? "Perdida" : "Perdida";
                
                if (request.TryGetProperty("latitud", out var latProp))
                {
                    // JSON ya parsea correctamente los decimales
                    latitud = latProp.GetDecimal();
                    Console.WriteLine($"📍 Latitud desde JSON: {latitud}");
                }
                else
                {
                    latitud = 0;
                }
                
                if (request.TryGetProperty("longitud", out var lngProp))
                {
                    // JSON ya parsea correctamente los decimales
                    longitud = lngProp.GetDecimal();
                    Console.WriteLine($"📍 Longitud desde JSON: {longitud}");
                }
                else
                {
                    longitud = 0;
                }
                
                descripcion = request.TryGetProperty("descripcion", out var descProp) ? descProp.GetString() ?? "" : "";
                telefono = request.TryGetProperty("telefono", out var telProp) ? telProp.GetString() ?? "" : "";
                idMascota = request.TryGetProperty("idMascota", out var mascotaProp) && mascotaProp.ValueKind != System.Text.Json.JsonValueKind.Null 
                    ? mascotaProp.GetInt32() : (int?)null;
            }
            
            if (string.IsNullOrWhiteSpace(telefono))
            {
                return Json(new { success = false, message = "El teléfono de contacto es requerido" });
            }
            
            if (tipo != "Perdida" && tipo != "Encontrada")
            {
                return Json(new { success = false, message = "Tipo inválido" });
            }
            
            // Obtener raza de la mascota si existe
            if (idMascota.HasValue)
            {
                string razaQuery = "SELECT Raza FROM Mascota WHERE Id_Mascota = @IdMascota AND Id_User = @IdUser";
                var razaResult = BD.ExecuteScalar(razaQuery, new Dictionary<string, object>
                {
                    { "@IdMascota", idMascota.Value },
                    { "@IdUser", userId.Value }
                });
                if (razaResult != null && razaResult != DBNull.Value)
                {
                    razaMascota = razaResult.ToString();
                }
            }
            
            // Forzar corrección de precisión ANTES de insertar
            Console.WriteLine("🔧 Forzando corrección de precisión de coordenadas antes de insertar...");
            try
            {
                BD.ExecuteNonQuery("ALTER TABLE CartelMascota ALTER COLUMN Latitud DECIMAL(11, 8) NOT NULL", new Dictionary<string, object>());
                Console.WriteLine("✅ Latitud corregida a DECIMAL(11,8)");
            }
            catch
            {
                try
                {
                    BD.ExecuteNonQuery("ALTER TABLE CartelMascota ALTER COLUMN Latitud DECIMAL(11, 8) NULL", new Dictionary<string, object>());
                    BD.ExecuteNonQuery("ALTER TABLE CartelMascota ALTER COLUMN Latitud DECIMAL(11, 8) NOT NULL", new Dictionary<string, object>());
                    Console.WriteLine("✅ Latitud corregida a DECIMAL(11,8) (con paso intermedio)");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ No se pudo corregir Latitud: {ex.Message}");
                }
            }
            
            try
            {
                BD.ExecuteNonQuery("ALTER TABLE CartelMascota ALTER COLUMN Longitud DECIMAL(12, 8) NOT NULL", new Dictionary<string, object>());
                Console.WriteLine("✅ Longitud corregida a DECIMAL(12,8)");
            }
            catch
            {
                try
                {
                    BD.ExecuteNonQuery("ALTER TABLE CartelMascota ALTER COLUMN Longitud DECIMAL(12, 8) NULL", new Dictionary<string, object>());
                    BD.ExecuteNonQuery("ALTER TABLE CartelMascota ALTER COLUMN Longitud DECIMAL(12, 8) NOT NULL", new Dictionary<string, object>());
                    Console.WriteLine("✅ Longitud corregida a DECIMAL(12,8) (con paso intermedio)");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ No se pudo corregir Longitud: {ex.Message}");
                }
            }
            
            // Redondear coordenadas a 8 decimales para asegurar que no excedan la precisión
            latitud = Math.Round(latitud, 8);
            longitud = Math.Round(longitud, 8);
            
            Console.WriteLine($"📍 Valores finales a insertar - Latitud: {latitud}, Longitud: {longitud}");
            
            string query = @"
                INSERT INTO CartelMascota (Id_User, Id_Mascota, Tipo, Latitud, Longitud, Descripcion, TelefonoContacto, FotoCartel, RazaMascota)
                VALUES (@IdUser, @IdMascota, @Tipo, @Latitud, @Longitud, @Descripcion, @Telefono, @FotoCartel, @RazaMascota)";
            
            var param = new Dictionary<string, object>
            {
                { "@IdUser", userId.Value },
                { "@IdMascota", idMascota.HasValue ? (object)idMascota.Value : DBNull.Value },
                { "@Tipo", tipo },
                { "@Latitud", latitud },
                { "@Longitud", longitud },
                { "@Descripcion", descripcion },
                { "@Telefono", telefono },
                { "@FotoCartel", string.IsNullOrEmpty(fotoCartel) ? DBNull.Value : (object)fotoCartel },
                { "@RazaMascota", string.IsNullOrEmpty(razaMascota) ? DBNull.Value : (object)razaMascota }
            };
            
            BD.ExecuteNonQuery(query, param);
            
            // Crear notificaciones para usuarios cercanos
            NotificarCartelCercano(latitud, longitud, tipo, userId.Value);
            
            return Json(new { success = true, message = "Cartel creado exitosamente" });
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error CrearCartel: " + ex.Message);
            return Json(new { success = false, message = ex.Message });
        }
    }

    // ============================
    // POST: Eliminar cartel
    // ============================
    [HttpPost]
    public IActionResult EliminarCartel([FromBody] dynamic request)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
            return Json(new { success = false, message = "No autenticado" });

        try
        {
            int idCartel = Convert.ToInt32(request.idCartel);
            
            // Verificar que el cartel pertenece al usuario
            string verificarQuery = "SELECT Id_User FROM CartelMascota WHERE Id_Cartel = @IdCartel AND Activo = 1";
            object? idUserResult = BD.ExecuteScalar(verificarQuery, new Dictionary<string, object> { { "@IdCartel", idCartel } });
            
            if (idUserResult == null || Convert.ToInt32(idUserResult) != userId.Value)
            {
                return Json(new { success = false, message = "No tenés permiso para eliminar este cartel" });
            }
            
            string query = "UPDATE CartelMascota SET Activo = 0 WHERE Id_Cartel = @IdCartel";
            BD.ExecuteNonQuery(query, new Dictionary<string, object> { { "@IdCartel", idCartel } });
            
            return Json(new { success = true, message = "Cartel eliminado exitosamente" });
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error EliminarCartel: " + ex.Message);
            return Json(new { success = false, message = ex.Message });
        }
    }

    // ============================
    // GET: Obtener mascotas del usuario para carteles
    // ============================
    [HttpGet]
    public IActionResult ObtenerMascotas()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
            return Json(new { success = false, message = "No autenticado" });

        try
        {
            string query = @"
                SELECT Id_Mascota, Nombre, Especie, Raza
                FROM Mascota
                WHERE Id_User = @UserId AND (Archivada IS NULL OR Archivada = 0)
                ORDER BY Nombre ASC";
            
            var dt = BD.ExecuteQuery(query, new Dictionary<string, object> { { "@UserId", userId.Value } });
            var mascotas = new List<object>();
            
            foreach (DataRow row in dt.Rows)
            {
                mascotas.Add(new
                {
                    id = Convert.ToInt32(row["Id_Mascota"]),
                    nombre = row["Nombre"].ToString(),
                    especie = row["Especie"].ToString(),
                    raza = row["Raza"]?.ToString() ?? ""
                });
            }
            
            return Json(new { success = true, mascotas = mascotas });
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error ObtenerMascotas: " + ex.Message);
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpPost]
public IActionResult GuardarUbicacion([FromBody] UbicacionRequest request)
{
    var userId = HttpContext.Session.GetInt32("UserId");
    if (userId == null)
        return Json(new { success = false, message = "No autenticado" });

    try
    {
        // Obtener Id_Ubicacion del usuario
        string qUser = "SELECT Id_Ubicacion FROM [User] WHERE Id_User = @Id";
        var dtUser = BD.ExecuteQuery(qUser, new Dictionary<string, object> { { "@Id", userId.Value } });
        
        int ubicacionId = 0;
        if (dtUser.Rows.Count > 0 && dtUser.Rows[0]["Id_Ubicacion"] != DBNull.Value)
        {
            ubicacionId = Convert.ToInt32(dtUser.Rows[0]["Id_Ubicacion"]);
        }

        if (ubicacionId > 0)
        {
            // Actualizar ubicación existente
            string qUpdate = @"
                UPDATE Ubicacion 
                SET Latitud = @Lat, Longitud = @Lng, Tipo = 'Usuario'
                WHERE Id_Ubicacion = @Id";
            BD.ExecuteNonQuery(qUpdate, new Dictionary<string, object>
            {
                { "@Lat", request.Lat },
                { "@Lng", request.Lng },
                { "@Id", ubicacionId }
            });
        }
        else
        {
            // Crear nueva ubicación
            string qInsert = @"
                INSERT INTO Ubicacion (Latitud, Longitud, Tipo)
                VALUES (@Lat, @Lng, 'Usuario');
                SELECT CAST(SCOPE_IDENTITY() AS INT);";
            ubicacionId = Convert.ToInt32(BD.ExecuteScalar(qInsert, new Dictionary<string, object>
            {
                { "@Lat", request.Lat },
                { "@Lng", request.Lng }
            }));

            // Actualizar usuario con la nueva ubicación
            string qUpdateUser = "UPDATE [User] SET Id_Ubicacion = @UbicacionId WHERE Id_User = @UserId";
            BD.ExecuteNonQuery(qUpdateUser, new Dictionary<string, object>
            {
                { "@UbicacionId", ubicacionId },
                { "@UserId", userId.Value }
            });
        }

        return Json(new { success = true });
    }
    catch (Exception ex)
    {
        Console.WriteLine("Error GuardarUbicacion: " + ex.Message);
        return Json(new { success = false, message = ex.Message });
    }
}

    private void AsegurarTablaConfiguracionUsuario()
    {
        try
        {
            // Verificar si la tabla existe
            string qCheck = @"
                IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ConfiguracionUsuario')
                BEGIN
                    CREATE TABLE ConfiguracionUsuario (
                        Id_Config INT IDENTITY(1,1) PRIMARY KEY,
                        Id_User INT NOT NULL,
                        MostrableUbicacion NVARCHAR(20) DEFAULT 'amigos',
                        Apodo NVARCHAR(100) NULL,
                        FOREIGN KEY (Id_User) REFERENCES [User](Id_User)
                    )
                END";

            BD.ExecuteNonQuery(qCheck, new Dictionary<string, object>());
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error al crear tabla ConfiguracionUsuario: " + ex.Message);
        }
    }

    private void AsegurarTablaApodoAmigo()
    {
        try
        {
            // Verificar si la tabla existe y crearla si no
            string qCheck = @"
                IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ApodoAmigo')
                BEGIN
                    CREATE TABLE ApodoAmigo (
                        Id_Apodo INT IDENTITY(1,1) PRIMARY KEY,
                        Id_User INT NOT NULL,
                        Id_Amigo INT NOT NULL,
                        Apodo NVARCHAR(100) NULL,
                        FOREIGN KEY (Id_User) REFERENCES [User](Id_User),
                        FOREIGN KEY (Id_Amigo) REFERENCES [User](Id_User),
                        UNIQUE (Id_User, Id_Amigo)
                    )
                END";

            BD.ExecuteNonQuery(qCheck, new Dictionary<string, object>());
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error al crear tabla ApodoAmigo: " + ex.Message);
        }
    }

    private void AsegurarColumnasEstadoOnline()
    {
        try
        {
            // Verificar y crear columna EstadoOnline
            string qEstadoOnline = @"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE Name = N'EstadoOnline' AND Object_ID = Object_ID(N'[User]'))
                BEGIN
                    ALTER TABLE [User] ADD EstadoOnline BIT NOT NULL DEFAULT 0;
                END";

            BD.ExecuteNonQuery(qEstadoOnline, new Dictionary<string, object>());

            // Verificar y crear columna UltimaActividad
            string qUltimaActividad = @"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE Name = N'UltimaActividad' AND Object_ID = Object_ID(N'[User]'))
                BEGIN
                    ALTER TABLE [User] ADD UltimaActividad DATETIME2 NULL;
                END";

            BD.ExecuteNonQuery(qUltimaActividad, new Dictionary<string, object>());
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error al crear columnas EstadoOnline/UltimaActividad: " + ex.Message);
        }
    }

    private void AsegurarTablaMascotaCompartida()
    {
        try
        {
            // Verificar si la tabla existe y crearla si no
            string qCheck = @"
                IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'MascotaCompartida')
                BEGIN
                    CREATE TABLE MascotaCompartida (
                        Id_MascotaCompartida INT IDENTITY(1,1) PRIMARY KEY,
                        Id_Mascota INT NOT NULL,
                        Id_Propietario INT NOT NULL,
                        Id_UsuarioCompartido INT NOT NULL,
                        Permiso_Edicion BIT NOT NULL DEFAULT 0,
                        Fecha_Compartido DATETIME2 NOT NULL DEFAULT GETDATE(),
                        Activo BIT NOT NULL DEFAULT 1,
                        FOREIGN KEY (Id_Mascota) REFERENCES Mascota(Id_Mascota),
                        FOREIGN KEY (Id_Propietario) REFERENCES [User](Id_User),
                        FOREIGN KEY (Id_UsuarioCompartido) REFERENCES [User](Id_User),
                        UNIQUE (Id_Mascota, Id_UsuarioCompartido)
                    )
                END";

            BD.ExecuteNonQuery(qCheck, new Dictionary<string, object>());
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error al crear tabla MascotaCompartida: " + ex.Message);
        }
    }

    [HttpGet]
    public IActionResult ObtenerAmigos()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
            return Json(new { success = false, message = "No autenticado" });

        try
        {
            // Asegurar que las tablas y columnas existen
            AsegurarTablaConfiguracionUsuario();
            AsegurarTablaApodoAmigo();
            AsegurarColumnasEstadoOnline();

            // Verificar si la tabla ProveedorServicio existe
            string checkTable = "SELECT COUNT(*) FROM sys.tables WHERE name = 'ProveedorServicio'";
            object? tableExists = BD.ExecuteScalar(checkTable);
            bool tieneProveedorServicio = tableExists != null && Convert.ToInt32(tableExists) > 0;

            // Verificar si las columnas de ubicación existen en ProveedorServicio
            bool tieneColumnasUbicacion = false;
            if (tieneProveedorServicio)
            {
                string checkLatitud = "SELECT COUNT(*) FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[ProveedorServicio]') AND name = 'Latitud'";
                string checkLongitud = "SELECT COUNT(*) FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[ProveedorServicio]') AND name = 'Longitud'";
                object? latExists = BD.ExecuteScalar(checkLatitud);
                object? lngExists = BD.ExecuteScalar(checkLongitud);
                tieneColumnasUbicacion = latExists != null && Convert.ToInt32(latExists) > 0 && 
                                         lngExists != null && Convert.ToInt32(lngExists) > 0;
            }

            // Obtener amigos del círculo de confianza (solo los que tienen solicitud aceptada)
            // Verificar que existe una invitación aceptada o están en CirculoConfianza
            // Solo mostrar ubicación según el estado de MostrableUbicacion (nadie/amigos/todos)
            // Por defecto es 'amigos' si no está configurado
            string qAmigos = $@"
                SELECT DISTINCT
                    U.Id_User,
                    U.Nombre + ' ' + U.Apellido as NombreCompleto,
                    {(tieneProveedorServicio ? "ISNULL(PS.NombreCompleto, U.Nombre + ' ' + U.Apellido)" : "U.Nombre + ' ' + U.Apellido")} as NombreMostrar,
                    {(tieneProveedorServicio ? "ISNULL(PS.FotoPerfil, P.FotoPerfil)" : "P.FotoPerfil")} as FotoPerfil,
                    {(tieneColumnasUbicacion ? "ISNULL(PS.Latitud, UB.Latitud)" : "UB.Latitud")} as Lat,
                    {(tieneColumnasUbicacion ? "ISNULL(PS.Longitud, UB.Longitud)" : "UB.Longitud")} as Lng,
                    (SELECT TOP 1 Nombre FROM Mascota WHERE Id_User = U.Id_User ORDER BY Id_Mascota DESC) as MascotaNombre,
                    (SELECT TOP 1 Foto FROM Mascota WHERE Id_User = U.Id_User ORDER BY Id_Mascota DESC) as MascotaFoto,
                    (SELECT TOP 1 Especie FROM Mascota WHERE Id_User = U.Id_User ORDER BY Id_Mascota DESC) as MascotaEspecie,
                    (SELECT TOP 1 Raza FROM Mascota WHERE Id_User = U.Id_User ORDER BY Id_Mascota DESC) as MascotaRaza,
                    ISNULL(CU.MostrableUbicacion, 'amigos') as MostrableUbicacion,
                    CU.Apodo as Apodo,
                    CASE 
                        WHEN U.EstadoOnline = 1 AND U.UltimaActividad >= DATEADD(MINUTE, -5, GETDATE()) THEN 1
                        ELSE 0
                    END as EstaOnline,
                    U.UltimaActividad,
                    {(tieneProveedorServicio ? "CASE WHEN PS.Id_Proveedor IS NOT NULL THEN 1 ELSE 0 END" : "0")} as EsProveedor
                FROM (
                    SELECT Id_Amigo as Id_User FROM CirculoConfianza WHERE Id_User = @UserId
                    UNION
                    SELECT Id_Receptor as Id_User FROM Invitacion 
                    WHERE Id_Emisor = @UserId AND Rol = 'Amigo' AND Estado = 'Aceptada'
                    UNION
                    SELECT Id_Emisor as Id_User FROM Invitacion 
                    WHERE Id_Receptor = @UserId AND Rol = 'Amigo' AND Estado = 'Aceptada'
                ) AS AmigosIds
                INNER JOIN [User] U ON AmigosIds.Id_User = U.Id_User
                LEFT JOIN Perfil P ON P.Id_Usuario = U.Id_User
                {(tieneProveedorServicio ? "LEFT JOIN ProveedorServicio PS ON PS.Id_User = U.Id_User" : "")}
                LEFT JOIN Ubicacion UB ON UB.Id_Ubicacion = U.Id_Ubicacion
                LEFT JOIN ConfiguracionUsuario CU ON CU.Id_User = U.Id_User
                WHERE U.Estado = 1
                AND (ISNULL(CU.MostrableUbicacion, 'amigos') IN ('amigos', 'todos'){(tieneProveedorServicio ? " OR PS.Id_Proveedor IS NOT NULL" : "")})";

        var dtAmigos = BD.ExecuteQuery(qAmigos, new Dictionary<string, object> { { "@UserId", userId.Value } });

        // Obtener apodos (la tabla ya está asegurada arriba)
        var apodos = new Dictionary<int, string>();
        try
        {
            string qApodos = @"
                SELECT Id_Amigo, Apodo FROM ApodoAmigo WHERE Id_User = @UserId";
            
            var dtApodos = BD.ExecuteQuery(qApodos, new Dictionary<string, object> { { "@UserId", userId.Value } });
            foreach (System.Data.DataRow rowApodo in dtApodos.Rows)
            {
                int amigoId = Convert.ToInt32(rowApodo["Id_Amigo"]);
                string apodo = rowApodo["Apodo"]?.ToString() ?? "";
                if (!string.IsNullOrEmpty(apodo))
                    apodos[amigoId] = apodo;
            }
        }
        catch (Exception ex)
        { 
            // Si hay error, continuar sin apodos
            Console.WriteLine("⚠️ Error al obtener apodos: " + ex.Message);
        }

        var amigos = new List<object>();
        foreach (System.Data.DataRow row in dtAmigos.Rows)
        {
            var especie = row["MascotaEspecie"]?.ToString()?.ToLower() ?? "perro";
            var raza = row["MascotaRaza"]?.ToString() ?? "";
            var mascotaFoto = row["MascotaFoto"]?.ToString();
            int amigoId = Convert.ToInt32(row["Id_User"]);
            
            // 🟢 Determinar si está online
            bool estaOnline = false;
            if (row.Table.Columns.Contains("EstaOnline"))
            {
                estaOnline = Convert.ToInt32(row["EstaOnline"]) == 1;
            }
            else if (row.Table.Columns.Contains("UltimaActividad") && row["UltimaActividad"] != DBNull.Value)
            {
                // Calcular si está online basado en última actividad (últimos 5 minutos)
                DateTime ultimaActividad = Convert.ToDateTime(row["UltimaActividad"]);
                estaOnline = (DateTime.Now - ultimaActividad).TotalMinutes <= 5;
            }
            
            string avatarMascota;
            if (!string.IsNullOrEmpty(mascotaFoto) && mascotaFoto != "null")
            {
                avatarMascota = mascotaFoto.StartsWith("/") ? mascotaFoto : "/" + mascotaFoto;
            }
            else
            {
                if (string.IsNullOrWhiteSpace(raza))
                {
                    avatarMascota = $"/img/mascotas/{especie}s/basico/{especie}_basico.png";
                }
                else
                {
                    // Usar la raza exacta de la BD, pero verificar si existe
                    // Si no existe, usar el básico de la especie
                    string rutaRaza = $"/img/mascotas/{especie}s/{raza}/{especie}_basico.png";
                    string rutaFisica = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img", "mascotas", $"{especie}s", raza, $"{especie}_basico.png");
                    
                    if (System.IO.File.Exists(rutaFisica))
                    {
                        avatarMascota = rutaRaza;
                    }
                    else
                    {
                        // Fallback: usar el básico de la especie
                        avatarMascota = $"/img/mascotas/{especie}s/basico/{especie}_basico.png";
                    }
                }
            }
            
            string nombreMostrar = apodos.ContainsKey(amigoId) && !string.IsNullOrEmpty(apodos[amigoId])
                ? apodos[amigoId]
                : (row["NombreMostrar"]?.ToString() ?? row["NombreCompleto"]?.ToString() ?? "Usuario");
            
            bool esProveedor = row.Table.Columns.Contains("EsProveedor") && Convert.ToInt32(row["EsProveedor"]) == 1;
            
            amigos.Add(new
            {
                id = amigoId,
                nombre = row["NombreCompleto"]?.ToString() ?? "Usuario",
                nombreMostrar = nombreMostrar,
                apodo = apodos.ContainsKey(amigoId) ? apodos[amigoId] : "",
                fotoPerfil = row["FotoPerfil"]?.ToString() ?? "/img/perfil/default.png",
                lat = row["Lat"] != DBNull.Value ? Convert.ToDouble(row["Lat"]) : (double?)null,
                lng = row["Lng"] != DBNull.Value ? Convert.ToDouble(row["Lng"]) : (double?)null,
                mascotaNombre = row["MascotaNombre"]?.ToString() ?? (esProveedor ? "Proveedor" : "Sin mascota"),
                mascotaAvatar = avatarMascota,
                estaOnline = estaOnline,
                esProveedor = esProveedor
            });
        }

        return Json(new { success = true, amigos = amigos });
    }
    catch (Exception ex)
    {
        Console.WriteLine("Error ObtenerAmigos: " + ex.Message);
        return Json(new { success = false, message = ex.Message });
    }
}

[HttpPost]
public IActionResult AgregarAmigo([FromBody] AgregarAmigoRequest request)
{
    var userId = HttpContext.Session.GetInt32("UserId");
    if (userId == null)
        return Json(new { success = false, message = "No autenticado" });

    try
    {
        // Buscar usuario por email
        string qBuscar = @"
            SELECT U.Id_User
            FROM [User] U
            INNER JOIN Mail M ON U.Id_Mail = M.Id_Mail
            WHERE M.Correo = @Email AND U.Estado = 1";

        var dtUsuario = BD.ExecuteQuery(qBuscar, new Dictionary<string, object> { { "@Email", request.Email } });

        if (dtUsuario.Rows.Count == 0)
            return Json(new { success = false, message = "No se encontró un usuario con ese email" });

        int amigoId = Convert.ToInt32(dtUsuario.Rows[0]["Id_User"]);

        if (amigoId == userId.Value)
            return Json(new { success = false, message = "No podés agregarte a vos mismo" });

        // Verificar si ya es amigo (en CirculoConfianza)
        string qVerificar = @"
            SELECT COUNT(*) FROM CirculoConfianza 
            WHERE Id_User = @UserId AND Id_Amigo = @AmigoId";

        int existe = Convert.ToInt32(BD.ExecuteScalar(qVerificar, new Dictionary<string, object>
        {
            { "@UserId", userId.Value },
            { "@AmigoId", amigoId }
        }));

        if (existe > 0)
            return Json(new { success = false, message = "Este usuario ya está en tu círculo de confianza" });

        // Verificar si ya hay una solicitud pendiente
        string qVerificarSolicitud = @"
            SELECT COUNT(*) FROM Invitacion 
            WHERE ((Id_Emisor = @UserId AND Id_Receptor = @AmigoId) OR (Id_Emisor = @AmigoId AND Id_Receptor = @UserId))
            AND Rol = 'Amigo' AND Estado = 'Pendiente'";

        int existeSolicitud = Convert.ToInt32(BD.ExecuteScalar(qVerificarSolicitud, new Dictionary<string, object>
        {
            { "@UserId", userId.Value },
            { "@AmigoId", amigoId }
        }));

        if (existeSolicitud > 0)
            return Json(new { success = false, message = "Ya existe una solicitud pendiente con este usuario" });

        // Obtener la primera mascota del usuario emisor (requerido por la tabla Invitacion)
        string qMascota = @"
            SELECT TOP 1 Id_Mascota FROM Mascota WHERE Id_User = @UserId ORDER BY Id_Mascota";

        var dtMascota = BD.ExecuteQuery(qMascota, new Dictionary<string, object> { { "@UserId", userId.Value } });
        
        if (dtMascota.Rows.Count == 0)
            return Json(new { success = false, message = "Necesitás tener al menos una mascota para agregar amigos" });

        int mascotaId = Convert.ToInt32(dtMascota.Rows[0]["Id_Mascota"]);

        // Crear solicitud de amistad en lugar de agregar directamente
        string qInsert = @"
            INSERT INTO Invitacion (Id_Mascota, Id_Emisor, Id_Receptor, Rol, Estado, Fecha)
            VALUES (@MascotaId, @UserId, @AmigoId, 'Amigo', 'Pendiente', GETDATE())";

        BD.ExecuteNonQuery(qInsert, new Dictionary<string, object>
        {
            { "@MascotaId", mascotaId },
            { "@UserId", userId.Value },
            { "@AmigoId", amigoId }
        });

        // Obtener nombre del emisor para la notificación
        string qNombre = "SELECT Nombre + ' ' + Apellido as NombreCompleto FROM [User] WHERE Id_User = @UserId";
        DataTable dtNombre = BD.ExecuteQuery(qNombre, new Dictionary<string, object> { { "@UserId", userId.Value } });
        string nombreEmisor = dtNombre.Rows.Count > 0 ? dtNombre.Rows[0]["NombreCompleto"].ToString() : "Un usuario";

        // Crear notificación para el receptor
        NotificacionController.CrearNotificacion(
            amigoId,
            "SolicitudAmistad",
            "Nueva solicitud de amistad",
            $"{nombreEmisor} te envió una solicitud de amistad",
            amigoId,
            "/Home/Comunidad"
        );

        return Json(new { success = true, message = "Solicitud de amistad enviada" });
    }
    catch (Exception ex)
    {
        Console.WriteLine("Error AgregarAmigo: " + ex.Message);
        return Json(new { success = false, message = "Error al agregar amigo: " + ex.Message });
    }
}

    public class UbicacionRequest
    {
        public double Lat { get; set; }
        public double Lng { get; set; }
    }

    public class AgregarAmigoRequest
    {
        public string Email { get; set; }
    }

[HttpGet]
public IActionResult BuscarUsuarios(string query)
{
    var userId = HttpContext.Session.GetInt32("UserId");
    if (userId == null)
        return Json(new { success = false, message = "No autenticado" });

    try
    {
        if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
            return Json(new { success = true, usuarios = new List<object>() });

        string qBuscar = @"
            SELECT DISTINCT TOP 20
                U.Id_User,
                U.Nombre + ' ' + U.Apellido as NombreCompleto,
                M.Correo as Email,
                P.FotoPerfil,
                (SELECT TOP 1 Nombre FROM Mascota WHERE Id_User = U.Id_User ORDER BY Id_Mascota DESC) as MascotaNombre
            FROM [User] U
            INNER JOIN Mail M ON U.Id_Mail = M.Id_Mail
            LEFT JOIN Perfil P ON P.Id_Usuario = U.Id_User
            WHERE U.Estado = 1
            AND U.Id_User <> @UserId
            AND (
                U.Nombre LIKE @Query OR
                U.Apellido LIKE @Query OR
                M.Correo LIKE @Query OR
                U.Nombre + ' ' + U.Apellido LIKE @Query
            )
            AND NOT EXISTS (
                SELECT 1 FROM CirculoConfianza CC 
                WHERE CC.Id_User = @UserId AND CC.Id_Amigo = U.Id_User
            )";

        var dt = BD.ExecuteQuery(qBuscar, new Dictionary<string, object>
        {
            { "@UserId", userId.Value },
            { "@Query", "%" + query.Trim() + "%" }
        });

        var usuarios = new List<object>();
        foreach (System.Data.DataRow row in dt.Rows)
        {
            bool estaOnline = row.Table.Columns.Contains("EstaOnline") && 
                             Convert.ToInt32(row["EstaOnline"]) == 1;
            
            usuarios.Add(new
            {
                id = Convert.ToInt32(row["Id_User"]),
                nombre = row["NombreCompleto"]?.ToString() ?? "Usuario",
                email = row["Email"]?.ToString() ?? "",
                fotoPerfil = row["FotoPerfil"]?.ToString() ?? "/img/perfil/default.png",
                mascotaNombre = row["MascotaNombre"]?.ToString() ?? "Sin mascota",
                estaOnline = estaOnline
            });
        }

        return Json(new { success = true, usuarios = usuarios });
    }
    catch (Exception ex)
    {
        Console.WriteLine("Error BuscarUsuarios: " + ex.Message);
        return Json(new { success = false, message = ex.Message });
    }
}

    [HttpGet]
    public IActionResult ObtenerUsuariosSugeridos()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
            return Json(new { success = false, message = "No autenticado" });

        try
        {
            // Verificar si la tabla ProveedorServicio existe
            string checkTable = "SELECT COUNT(*) FROM sys.tables WHERE name = 'ProveedorServicio'";
            object? tableExists = BD.ExecuteScalar(checkTable);
            bool tieneProveedorServicio = tableExists != null && Convert.ToInt32(tableExists) > 0;

            // Obtener usuarios sugeridos (dueños de mascotas y proveedores)
            string query = $@"
                SELECT TOP 10
                    U.Id_User as id,
                    U.Nombre + ' ' + U.Apellido as nombre,
                    M.Correo as email,
                    {(tieneProveedorServicio ? "ISNULL(PR.FotoPerfil, PS.FotoPerfil)" : "PR.FotoPerfil")} as fotoPerfil,
                    ISNULL((SELECT TOP 1 Nombre FROM Mascota WHERE Id_User = U.Id_User), 'Sin mascota') as mascotaNombre,
                    {(tieneProveedorServicio ? "CASE WHEN PS.Id_Proveedor IS NOT NULL THEN 1 ELSE 0 END" : "0")} as esProveedor,
                    {(tieneProveedorServicio ? "PS.NombreCompleto" : "NULL")} as nombreProveedor
                FROM [User] U
                INNER JOIN Mail M ON U.Id_Mail = M.Id_Mail
                LEFT JOIN Perfil PR ON PR.Id_Usuario = U.Id_User
                {(tieneProveedorServicio ? "LEFT JOIN ProveedorServicio PS ON PS.Id_User = U.Id_User" : "")}
                WHERE U.Id_User != @UserId
                  AND U.Id_User NOT IN (
                      SELECT Id_Amigo FROM CirculoConfianza WHERE Id_User = @UserId
                      UNION
                      SELECT Id_Receptor FROM Invitacion WHERE Id_Emisor = @UserId AND Estado IN ('Pendiente', 'Aceptada')
                      UNION
                      SELECT Id_Emisor FROM Invitacion WHERE Id_Receptor = @UserId AND Estado IN ('Pendiente', 'Aceptada')
                  )
                ORDER BY NEWID()";
            
            var dt = BD.ExecuteQuery(query, new Dictionary<string, object> { { "@UserId", userId.Value } });
            
            var sugeridos = new List<object>();
            foreach (System.Data.DataRow row in dt.Rows)
            {
                bool esProveedor = tieneProveedorServicio && Convert.ToInt32(row["esProveedor"]) == 1;
                string nombreMostrar = esProveedor && row["nombreProveedor"] != DBNull.Value && row["nombreProveedor"] != null
                    ? row["nombreProveedor"].ToString()
                    : row["nombre"].ToString();
                
                sugeridos.Add(new
                {
                    id = Convert.ToInt32(row["id"]),
                    nombre = nombreMostrar,
                    email = row["email"].ToString(),
                    fotoPerfil = row["fotoPerfil"]?.ToString() ?? "/img/perfil/default.png",
                    mascotaNombre = row["mascotaNombre"]?.ToString() ?? "Sin mascota",
                    esProveedor = esProveedor
                });
            }
            
            return Json(new { success = true, sugeridos = sugeridos.Take(10) });
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error ObtenerUsuariosSugeridos: " + ex.Message);
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpGet]
    public IActionResult ObtenerUsuariosSugeridos_OLD()
{
    var userId = HttpContext.Session.GetInt32("UserId");
    if (userId == null)
        return Json(new { success = false, message = "No autenticado" });

    try
    {
        // Obtener usuarios que no son amigos aún (sugeridos)
        // Excluir usuarios que ya son amigos o tienen solicitudes pendientes
        string qSugeridos = @"
            SELECT TOP 10
                U.Id_User,
                U.Nombre + ' ' + U.Apellido as NombreCompleto,
                M.Correo as Email,
                P.FotoPerfil,
                (SELECT TOP 1 Nombre FROM Mascota WHERE Id_User = U.Id_User ORDER BY Id_Mascota DESC) as MascotaNombre
            FROM [User] U
            INNER JOIN Mail M ON U.Id_Mail = M.Id_Mail
            LEFT JOIN Perfil P ON P.Id_Usuario = U.Id_User
            WHERE U.Estado = 1
            AND U.Id_User <> @UserId
            AND NOT EXISTS (
                SELECT 1 FROM CirculoConfianza CC 
                WHERE CC.Id_User = @UserId AND CC.Id_Amigo = U.Id_User
            )
            AND NOT EXISTS (
                SELECT 1 FROM Invitacion I
                WHERE ((I.Id_Emisor = @UserId AND I.Id_Receptor = U.Id_User) OR (I.Id_Emisor = U.Id_User AND I.Id_Receptor = @UserId))
                AND I.Rol = 'Amigo' AND I.Estado IN ('Pendiente', 'Aceptada')
            )
            ORDER BY NEWID()";

        var dt = BD.ExecuteQuery(qSugeridos, new Dictionary<string, object> { { "@UserId", userId.Value } });

        var sugeridos = new List<object>();
        foreach (System.Data.DataRow row in dt.Rows)
        {
            sugeridos.Add(new
            {
                id = Convert.ToInt32(row["Id_User"]),
                nombre = row["NombreCompleto"]?.ToString() ?? "Usuario",
                email = row["Email"]?.ToString() ?? "",
                fotoPerfil = row["FotoPerfil"]?.ToString() ?? "/img/perfil/default.png",
                mascotaNombre = row["MascotaNombre"]?.ToString() ?? "Sin mascota"
            });
        }

        return Json(new { success = true, sugeridos = sugeridos });
    }
    catch (Exception ex)
    {
        Console.WriteLine("Error ObtenerUsuariosSugeridos: " + ex.Message);
        return Json(new { success = false, message = ex.Message });
    }
}

[HttpPost]
public IActionResult EliminarAmigo([FromBody] EliminarAmigoRequest request)
{
    var userId = HttpContext.Session.GetInt32("UserId");
    if (userId == null)
        return Json(new { success = false, message = "No autenticado" });

    try
    {
        // Eliminar relación bidireccional de CirculoConfianza
        string qEliminar = @"
            DELETE FROM CirculoConfianza 
            WHERE (Id_User = @UserId AND Id_Amigo = @AmigoId)
               OR (Id_User = @AmigoId AND Id_Amigo = @UserId)";

        BD.ExecuteNonQuery(qEliminar, new Dictionary<string, object>
        {
            { "@UserId", userId.Value },
            { "@AmigoId", request.AmigoId }
        });

        // También eliminar apodos relacionados
        try
        {
            string qEliminarApodos = @"
                DELETE FROM ApodoAmigo 
                WHERE (Id_User = @UserId AND Id_Amigo = @AmigoId)
                   OR (Id_User = @AmigoId AND Id_Amigo = @UserId)";
            
            BD.ExecuteNonQuery(qEliminarApodos, new Dictionary<string, object>
            {
                { "@UserId", userId.Value },
                { "@AmigoId", request.AmigoId }
            });
        }
        catch (Exception ex)
        {
            // Si la tabla no existe, continuar sin error
            Console.WriteLine("⚠️ Error al eliminar apodos (puede ser normal si la tabla no existe): " + ex.Message);
        }

        return Json(new { success = true, message = "Amigo eliminado correctamente" });
    }
    catch (Exception ex)
    {
        Console.WriteLine("Error EliminarAmigo: " + ex.Message);
        return Json(new { success = false, message = "Error al eliminar amigo" });
    }
}

    [HttpPost]
    public IActionResult AceptarSolicitud([FromBody] AceptarSolicitudRequest request)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
            return Json(new { success = false, message = "No autenticado" });

        try
        {
            // Actualizar el estado de la solicitud a "Aceptada"
            string qUpdate = @"
                UPDATE Invitacion 
                SET Estado = 'Aceptada'
                WHERE Id_Invitacion = @InvitacionId 
                AND Id_Receptor = @UserId 
                AND Estado = 'Pendiente'";

            int filas = BD.ExecuteNonQuery(qUpdate, new Dictionary<string, object>
            {
                { "@InvitacionId", request.InvitacionId },
                { "@UserId", userId.Value }
            });

            if (filas == 0)
                return Json(new { success = false, message = "No se encontró la solicitud o ya fue procesada" });

            // Obtener el Id_Emisor de la solicitud
            string qObtenerEmisor = @"
                SELECT Id_Emisor FROM Invitacion WHERE Id_Invitacion = @InvitacionId";

            var dtEmisor = BD.ExecuteQuery(qObtenerEmisor, new Dictionary<string, object> { { "@InvitacionId", request.InvitacionId } });
            
            if (dtEmisor.Rows.Count > 0)
            {
                int emisorId = Convert.ToInt32(dtEmisor.Rows[0]["Id_Emisor"]);
                
                // Agregar a CirculoConfianza (bidireccional)
                string qInsert1 = @"
                    IF NOT EXISTS (SELECT 1 FROM CirculoConfianza WHERE Id_User = @UserId AND Id_Amigo = @EmisorId)
                    INSERT INTO CirculoConfianza (Id_User, Id_Amigo, Rol, Latitud, Longitud, UltimaConexion)
                    VALUES (@UserId, @EmisorId, 'Amigo', 0, 0, GETDATE())";

                BD.ExecuteNonQuery(qInsert1, new Dictionary<string, object>
                {
                    { "@UserId", userId.Value },
                    { "@EmisorId", emisorId }
                });

                string qInsert2 = @"
                    IF NOT EXISTS (SELECT 1 FROM CirculoConfianza WHERE Id_User = @EmisorId AND Id_Amigo = @UserId)
                    INSERT INTO CirculoConfianza (Id_User, Id_Amigo, Rol, Latitud, Longitud, UltimaConexion)
                    VALUES (@EmisorId, @UserId, 'Amigo', 0, 0, GETDATE())";

                BD.ExecuteNonQuery(qInsert2, new Dictionary<string, object>
                {
                    { "@EmisorId", emisorId },
                    { "@UserId", userId.Value }
                });
            }

            return Json(new { success = true, message = "Solicitud aceptada" });
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error AceptarSolicitud: " + ex.Message);
            return Json(new { success = false, message = "Error al aceptar solicitud: " + ex.Message });
        }
    }

    [HttpPost]
    public IActionResult RechazarSolicitud([FromBody] RechazarSolicitudRequest request)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
            return Json(new { success = false, message = "No autenticado" });

        try
        {
            string qUpdate = @"
                UPDATE Invitacion 
                SET Estado = 'Rechazada'
                WHERE Id_Invitacion = @InvitacionId 
                AND Id_Receptor = @UserId 
                AND Estado = 'Pendiente'";

            int filas = BD.ExecuteNonQuery(qUpdate, new Dictionary<string, object>
            {
                { "@InvitacionId", request.InvitacionId },
                { "@UserId", userId.Value }
            });

            if (filas == 0)
                return Json(new { success = false, message = "No se encontró la solicitud o ya fue procesada" });

            return Json(new { success = true, message = "Solicitud rechazada" });
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error RechazarSolicitud: " + ex.Message);
            return Json(new { success = false, message = "Error al rechazar solicitud: " + ex.Message });
        }
    }

    [HttpGet]
    public IActionResult ObtenerSolicitudes()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
            return Json(new { success = false, message = "No autenticado" });

        try
        {
            string qSolicitudes = @"
                SELECT 
                    I.Id_Invitacion,
                    I.Id_Emisor,
                    U.Nombre + ' ' + U.Apellido as NombreCompleto,
                    P.FotoPerfil,
                    I.Fecha
                FROM Invitacion I
                INNER JOIN [User] U ON I.Id_Emisor = U.Id_User
                LEFT JOIN Perfil P ON P.Id_Usuario = U.Id_User
                WHERE I.Id_Receptor = @UserId 
                AND I.Rol = 'Amigo' 
                AND I.Estado = 'Pendiente'
                ORDER BY I.Fecha DESC";

            var dtSolicitudes = BD.ExecuteQuery(qSolicitudes, new Dictionary<string, object> { { "@UserId", userId.Value } });

            var solicitudes = new List<object>();
            foreach (System.Data.DataRow row in dtSolicitudes.Rows)
            {
                solicitudes.Add(new
                {
                    idInvitacion = Convert.ToInt32(row["Id_Invitacion"]),
                    idEmisor = Convert.ToInt32(row["Id_Emisor"]),
                    nombre = row["NombreCompleto"]?.ToString() ?? "Usuario",
                    fotoPerfil = row["FotoPerfil"]?.ToString() ?? "/img/perfil/default.png",
                    fecha = Convert.ToDateTime(row["Fecha"]).ToString("dd/MM/yyyy HH:mm")
                });
            }

            return Json(new { success = true, solicitudes = solicitudes });
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error ObtenerSolicitudes: " + ex.Message);
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpPost]
    public IActionResult ActualizarUbicacionMostrable([FromBody] UbicacionMostrableRequest request)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
            return Json(new { success = false, message = "No autenticado" });

        try
        {
            // Asegurar que la tabla existe
            AsegurarTablaConfiguracionUsuario();

            // Validar el valor (debe ser: 'nadie', 'amigos', o 'todos')
            string estado = request.Estado?.ToLower() ?? "amigos";
            if (estado != "nadie" && estado != "amigos" && estado != "todos")
                estado = "amigos";

            string qUpsert = @"
                IF EXISTS (SELECT 1 FROM ConfiguracionUsuario WHERE Id_User = @UserId)
                    UPDATE ConfiguracionUsuario SET MostrableUbicacion = @Estado WHERE Id_User = @UserId
                ELSE
                    INSERT INTO ConfiguracionUsuario (Id_User, MostrableUbicacion) VALUES (@UserId, @Estado)";

            BD.ExecuteNonQuery(qUpsert, new Dictionary<string, object>
            {
                { "@UserId", userId.Value },
                { "@Estado", estado }
            });

            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error ActualizarUbicacionMostrable: " + ex.Message);
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpGet]
    public IActionResult ObtenerEstadoUbicacionMostrable()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
            return Json(new { success = false, message = "No autenticado" });

        try
        {
            // Asegurar que la tabla existe
            AsegurarTablaConfiguracionUsuario();

            string qObtener = @"
                SELECT MostrableUbicacion FROM ConfiguracionUsuario WHERE Id_User = @UserId";

            var dt = BD.ExecuteQuery(qObtener, new Dictionary<string, object> { { "@UserId", userId.Value } });

            string estado = "amigos"; // Por defecto, solo para amigos
            if (dt.Rows.Count > 0 && dt.Rows[0]["MostrableUbicacion"] != DBNull.Value)
            {
                estado = dt.Rows[0]["MostrableUbicacion"]?.ToString() ?? "amigos";
            }

            return Json(new { success = true, estado = estado });
        }
        catch (Exception ex)
        {
            return Json(new { success = true, estado = "amigos" });
        }
    }

    public class EliminarAmigoRequest
    {
        public int AmigoId { get; set; }
    }

    public class AceptarSolicitudRequest
    {
        public int InvitacionId { get; set; }
    }

    public class RechazarSolicitudRequest
    {
        public int InvitacionId { get; set; }
    }

    [HttpPost]
    public IActionResult ActualizarApodoAmigo([FromBody] ActualizarApodoRequest request)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
            return Json(new { success = false, message = "No autenticado" });

        try
        {
            // Crear tabla ApodoAmigo si no existe
            string qCreateApodoTable = @"
                IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ApodoAmigo')
                BEGIN
                    CREATE TABLE ApodoAmigo (
                        Id_Apodo INT IDENTITY(1,1) PRIMARY KEY,
                        Id_User INT NOT NULL,
                        Id_Amigo INT NOT NULL,
                        Apodo NVARCHAR(100) NULL,
                        FOREIGN KEY (Id_User) REFERENCES [User](Id_User),
                        FOREIGN KEY (Id_Amigo) REFERENCES [User](Id_User),
                        UNIQUE (Id_User, Id_Amigo)
                    )
                END";

            BD.ExecuteNonQuery(qCreateApodoTable, new Dictionary<string, object>());

            // Verificar que el amigo existe en el círculo de confianza o invitación aceptada
            string qVerificar = @"
                SELECT COUNT(*) FROM (
                    SELECT Id_Amigo as Id_User FROM CirculoConfianza WHERE Id_User = @UserId AND Id_Amigo = @AmigoId
                    UNION
                    SELECT Id_Receptor as Id_User FROM Invitacion 
                    WHERE Id_Emisor = @UserId AND Id_Receptor = @AmigoId AND Rol = 'Amigo' AND Estado = 'Aceptada'
                    UNION
                    SELECT Id_Emisor as Id_User FROM Invitacion 
                    WHERE Id_Receptor = @UserId AND Id_Emisor = @AmigoId AND Rol = 'Amigo' AND Estado = 'Aceptada'
                ) AS Amigos";

            int existe = Convert.ToInt32(BD.ExecuteScalar(qVerificar, new Dictionary<string, object>
            {
                { "@UserId", userId.Value },
                { "@AmigoId", request.AmigoId }
            }));

            if (existe == 0)
                return Json(new { success = false, message = "Este usuario no es tu amigo" });

            string qUpsertApodo = @"
                IF EXISTS (SELECT 1 FROM ApodoAmigo WHERE Id_User = @UserId AND Id_Amigo = @AmigoId)
                    UPDATE ApodoAmigo SET Apodo = @Apodo WHERE Id_User = @UserId AND Id_Amigo = @AmigoId
                ELSE
                    INSERT INTO ApodoAmigo (Id_User, Id_Amigo, Apodo) VALUES (@UserId, @AmigoId, @Apodo)";

            BD.ExecuteNonQuery(qUpsertApodo, new Dictionary<string, object>
            {
                { "@UserId", userId.Value },
                { "@AmigoId", request.AmigoId },
                { "@Apodo", request.Apodo ?? "" }
            });

            return Json(new { success = true, message = "Apodo actualizado" });
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error ActualizarApodoAmigo: " + ex.Message);
            return Json(new { success = false, message = ex.Message });
        }
    }

    public class UbicacionMostrableRequest
    {
        public string Estado { get; set; } // "nadie", "amigos", "todos"
    }

    public class ActualizarApodoRequest
    {
        public int AmigoId { get; set; }
        public string Apodo { get; set; }
    }

    // 🐾 Request classes para compartir mascotas
    public class EnviarSolicitudCompartirRequest
    {
        public int MascotaId { get; set; }
        public string Email { get; set; }
        public string Mensaje { get; set; }
    }

    public class ProcesarSolicitudRequest
    {
        public int IdSolicitud { get; set; }
    }

    // 💬 Mensajería entre amigos
    [HttpGet]
    public IActionResult Mensajes(int? amigoId)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
            return RedirectToAction("Login", "Auth");

        var tema = HttpContext.Session.GetString("Tema") ?? "claro";
        ViewBag.Tema = tema;
        ViewBag.AmigoId = amigoId;

        if (amigoId.HasValue)
        {
            // Obtener información del amigo
            string qAmigo = @"
                SELECT U.Id_User, U.Nombre + ' ' + U.Apellido as NombreCompleto, P.FotoPerfil
                FROM [User] U
                LEFT JOIN Perfil P ON P.Id_Usuario = U.Id_User
                WHERE U.Id_User = @AmigoId";

            var dtAmigo = BD.ExecuteQuery(qAmigo, new Dictionary<string, object> { { "@AmigoId", amigoId.Value } });
            if (dtAmigo.Rows.Count > 0)
            {
                ViewBag.AmigoNombre = dtAmigo.Rows[0]["NombreCompleto"]?.ToString() ?? "Amigo";
                ViewBag.AmigoFoto = dtAmigo.Rows[0]["FotoPerfil"]?.ToString() ?? "/img/perfil/default.png";
            }
        }

        return View();
    }

    [HttpPost]
    public IActionResult ObtenerOCrearChat([FromBody] int amigoId)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
            return Json(new { success = false, message = "No autenticado" });

        try
        {
            // Verificar que el usuario existe y es amigo o proveedor
            // Permitir chat con amigos o con proveedores (sin necesidad de ser amigos)
            string qVerificar = @"
                SELECT COUNT(*) FROM (
                    SELECT Id_Amigo as Id_User FROM CirculoConfianza WHERE Id_User = @UserId AND Id_Amigo = @AmigoId
                    UNION
                    SELECT Id_Receptor as Id_User FROM Invitacion 
                    WHERE Id_Emisor = @UserId AND Id_Receptor = @AmigoId AND Rol = 'Amigo' AND Estado = 'Aceptada'
                    UNION
                    SELECT Id_Emisor as Id_User FROM Invitacion 
                    WHERE Id_Receptor = @UserId AND Id_Emisor = @AmigoId AND Rol = 'Amigo' AND Estado = 'Aceptada'
                    UNION
                    -- Permitir chat con proveedores
                    SELECT PS.Id_User FROM ProveedorServicio PS WHERE PS.Id_User = @AmigoId AND PS.Estado = 1
                ) AS Usuarios";

            int existe = Convert.ToInt32(BD.ExecuteScalar(qVerificar, new Dictionary<string, object>
            {
                { "@UserId", userId.Value },
                { "@AmigoId", amigoId }
            }));

            if (existe == 0)
                return Json(new { success = false, message = "No podés chatear con este usuario" });

            // Buscar chat existente entre estos dos usuarios
            string qChatExistente = @"
                SELECT TOP 1 c.Id_Chat
                FROM Chat c
                INNER JOIN ParticipanteChat pc1 ON c.Id_Chat = pc1.Id_Chat AND pc1.Id_User = @UserId
                INNER JOIN ParticipanteChat pc2 ON c.Id_Chat = pc2.Id_Chat AND pc2.Id_User = @AmigoId
                WHERE c.EsGrupo = 0";

            var dtChat = BD.ExecuteQuery(qChatExistente, new Dictionary<string, object>
            {
                { "@UserId", userId.Value },
                { "@AmigoId", amigoId }
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
                    VALUES (@ChatId, @AmigoId, 0, GETDATE())",
                    new Dictionary<string, object> { { "@ChatId", chatId }, { "@AmigoId", amigoId } });
            }

            return Json(new { success = true, chatId = chatId });
        }
        catch (Exception ex)
        {
            Console.WriteLine("❌ Error en ObtenerOCrearChat: " + ex.Message);
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpGet]
    public IActionResult ObtenerMensajes(int chatId)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
            return Json(new { success = false, message = "No autenticado" });

        try
        {
            // Verificar que el usuario es participante del chat
            string qVerificar = @"
                SELECT COUNT(*) FROM ParticipanteChat 
                WHERE Id_Chat = @ChatId AND Id_User = @UserId";
            int esParticipante = Convert.ToInt32(BD.ExecuteScalar(qVerificar, new Dictionary<string, object>
            {
                { "@ChatId", chatId },
                { "@UserId", userId.Value }
            }));

            if (esParticipante == 0)
                return Json(new { success = false, message = "No tenés acceso a este chat" });

            // Obtener mensajes
            string qMensajes = @"
                SELECT m.Id_Mensaje, m.Id_User, m.Contenido, m.Fecha, m.Leido,
                       U.Nombre + ' ' + U.Apellido as NombreUsuario,
                       P.FotoPerfil
                FROM Mensaje m
                INNER JOIN [User] U ON m.Id_User = U.Id_User
                LEFT JOIN Perfil P ON P.Id_Usuario = U.Id_User
                WHERE m.Id_Chat = @ChatId
                ORDER BY m.Fecha ASC";

            var dtMensajes = BD.ExecuteQuery(qMensajes, new Dictionary<string, object> { { "@ChatId", chatId } });

            var mensajes = new List<object>();
            foreach (DataRow row in dtMensajes.Rows)
            {
                mensajes.Add(new
                {
                    id = Convert.ToInt32(row["Id_Mensaje"]),
                    userId = Convert.ToInt32(row["Id_User"]),
                    esMio = Convert.ToInt32(row["Id_User"]) == userId.Value,
                    contenido = row["Contenido"]?.ToString() ?? "",
                    fecha = Convert.ToDateTime(row["Fecha"]).ToString("yyyy-MM-ddTHH:mm:ss"),
                    leido = Convert.ToBoolean(row["Leido"]),
                    nombreUsuario = row["NombreUsuario"]?.ToString() ?? "",
                    fotoPerfil = row["FotoPerfil"]?.ToString() ?? "/img/perfil/default.png"
                });
            }

            // Marcar mensajes como leídos
            BD.ExecuteNonQuery(@"
                UPDATE Mensaje SET Leido = 1 
                WHERE Id_Chat = @ChatId AND Id_User != @UserId AND Leido = 0",
                new Dictionary<string, object> { { "@ChatId", chatId }, { "@UserId", userId.Value } });

            return Json(new { success = true, mensajes = mensajes });
        }
        catch (Exception ex)
        {
            Console.WriteLine("❌ Error en ObtenerMensajes: " + ex.Message);
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpPost]
    public IActionResult EnviarMensaje([FromBody] EnviarMensajeRequest request)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
            return Json(new { success = false, message = "No autenticado" });

        try
        {
            // Verificar que el usuario es participante del chat
            string qVerificar = @"
                SELECT COUNT(*) FROM ParticipanteChat 
                WHERE Id_Chat = @ChatId AND Id_User = @UserId";
            int esParticipante = Convert.ToInt32(BD.ExecuteScalar(qVerificar, new Dictionary<string, object>
            {
                { "@ChatId", request.ChatId },
                { "@UserId", userId.Value }
            }));

            if (esParticipante == 0)
                return Json(new { success = false, message = "No tenés acceso a este chat" });

            // Insertar mensaje
            string qInsertar = @"
                INSERT INTO Mensaje (Id_Chat, Id_User, Contenido, Fecha, Leido)
                VALUES (@ChatId, @UserId, @Contenido, GETDATE(), 0);
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            int mensajeId = Convert.ToInt32(BD.ExecuteScalar(qInsertar, new Dictionary<string, object>
            {
                { "@ChatId", request.ChatId },
                { "@UserId", userId.Value },
                { "@Contenido", request.Contenido ?? "" }
            }));

            // Obtener el otro participante del chat para notificar
            string qOtroParticipante = @"
                SELECT TOP 1 pc.Id_User, U.Nombre + ' ' + U.Apellido as NombreCompleto
                FROM ParticipanteChat pc
                INNER JOIN [User] U ON pc.Id_User = U.Id_User
                WHERE pc.Id_Chat = @ChatId AND pc.Id_User != @UserId";
            
            DataTable dtOtro = BD.ExecuteQuery(qOtroParticipante, new Dictionary<string, object>
            {
                { "@ChatId", request.ChatId },
                { "@UserId", userId.Value }
            });

            if (dtOtro.Rows.Count > 0)
            {
                int idOtroUsuario = Convert.ToInt32(dtOtro.Rows[0]["Id_User"]);
                string nombreEmisor = dtOtro.Rows[0]["NombreCompleto"]?.ToString() ?? "Un usuario";
                string contenidoCorto = request.Contenido?.Length > 50 
                    ? request.Contenido.Substring(0, 50) + "..." 
                    : request.Contenido ?? "";

                NotificacionController.CrearNotificacion(
                    idOtroUsuario,
                    "Mensaje",
                    "Nuevo mensaje",
                    $"{nombreEmisor}: {contenidoCorto}",
                    request.ChatId,
                    $"/Home/Mensajes?chatId={request.ChatId}"
                );
            }

            return Json(new { success = true, mensajeId = mensajeId });
        }
        catch (Exception ex)
        {
            Console.WriteLine("❌ Error en EnviarMensaje: " + ex.Message);
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpGet]
    public IActionResult ObtenerChats()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
            return Json(new { success = false, message = "No autenticado" });

        try
        {
            // Obtener chats del usuario con último mensaje
            string qChats = @"
                SELECT DISTINCT
                    c.Id_Chat,
                    c.Nombre,
                    c.EsGrupo,
                    c.FechaCreacion,
                    (SELECT TOP 1 Contenido FROM Mensaje WHERE Id_Chat = c.Id_Chat ORDER BY Fecha DESC) as UltimoMensaje,
                    (SELECT TOP 1 Fecha FROM Mensaje WHERE Id_Chat = c.Id_Chat ORDER BY Fecha DESC) as FechaUltimoMensaje,
                    (SELECT COUNT(*) FROM Mensaje WHERE Id_Chat = c.Id_Chat AND Id_User != @UserId AND Leido = 0) as MensajesNoLeidos
                FROM Chat c
                INNER JOIN ParticipanteChat pc ON c.Id_Chat = pc.Id_Chat
                WHERE pc.Id_User = @UserId
                ORDER BY FechaUltimoMensaje DESC";

            var dtChats = BD.ExecuteQuery(qChats, new Dictionary<string, object> { { "@UserId", userId.Value } });

            var chats = new List<object>();
            foreach (DataRow row in dtChats.Rows)
            {
                int chatId = Convert.ToInt32(row["Id_Chat"]);
                
                // Obtener el otro participante si es chat individual
                string otroParticipante = "";
                string fotoOtroParticipante = "/img/perfil/default.png";
                if (!Convert.ToBoolean(row["EsGrupo"]))
                {
                    string qOtro = @"
                        SELECT U.Id_User, U.Nombre + ' ' + U.Apellido as NombreCompleto, P.FotoPerfil
                        FROM ParticipanteChat pc
                        INNER JOIN [User] U ON pc.Id_User = U.Id_User
                        LEFT JOIN Perfil P ON P.Id_Usuario = U.Id_User
                        WHERE pc.Id_Chat = @ChatId AND pc.Id_User != @UserId";

                    var dtOtro = BD.ExecuteQuery(qOtro, new Dictionary<string, object>
                    {
                        { "@ChatId", chatId },
                        { "@UserId", userId.Value }
                    });

                    if (dtOtro.Rows.Count > 0)
                    {
                        otroParticipante = dtOtro.Rows[0]["NombreCompleto"]?.ToString() ?? "";
                        fotoOtroParticipante = dtOtro.Rows[0]["FotoPerfil"]?.ToString() ?? "/img/perfil/default.png";
                    }
                }

                chats.Add(new
                {
                    id = chatId,
                    nombre = row["Nombre"]?.ToString() ?? otroParticipante,
                    esGrupo = Convert.ToBoolean(row["EsGrupo"]),
                    ultimoMensaje = row["UltimoMensaje"]?.ToString() ?? "",
                    fechaUltimoMensaje = row["FechaUltimoMensaje"] != DBNull.Value 
                        ? Convert.ToDateTime(row["FechaUltimoMensaje"]).ToString("yyyy-MM-ddTHH:mm:ss")
                        : "",
                    mensajesNoLeidos = Convert.ToInt32(row["MensajesNoLeidos"]),
                    foto = fotoOtroParticipante
                });
            }

            return Json(new { success = true, chats = chats });
        }
        catch (Exception ex)
        {
            Console.WriteLine("❌ Error en ObtenerChats: " + ex.Message);
            return Json(new { success = false, message = ex.Message });
        }
    }

    public class EnviarMensajeRequest
    {
        public int ChatId { get; set; }
        public string Contenido { get; set; }
    }

    // 🟢 Heartbeat para mantener el estado online
    [HttpPost]
    public IActionResult Heartbeat()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
            return Json(new { success = false, message = "No autenticado" });

        try
        {
            AsegurarColumnasEstadoOnline();
            string qUpdate = @"
                UPDATE [User] 
                SET UltimaActividad = GETDATE(), EstadoOnline = 1
                WHERE Id_User = @UserId";
            BD.ExecuteNonQuery(qUpdate, new Dictionary<string, object> { { "@UserId", userId.Value } });

            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            Console.WriteLine("❌ Error en Heartbeat: " + ex.Message);
            return Json(new { success = false, message = ex.Message });
        }
    }

    // 🟢 Obtener estado online de usuarios
    [HttpGet]
    public IActionResult ObtenerEstadoOnline([FromQuery] string userIds)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
            return Json(new { success = false, message = "No autenticado" });

        try
        {
            AsegurarColumnasEstadoOnline();
            
            if (string.IsNullOrWhiteSpace(userIds))
                return Json(new { success = true, estados = new Dictionary<int, bool>() });

            // Parsear IDs desde string separado por comas
            var ids = userIds.Split(',').Select(id => int.Parse(id.Trim())).ToArray();
            
            if (ids.Length == 0)
                return Json(new { success = true, estados = new Dictionary<int, bool>() });

            // Crear lista de IDs para la query
            string idsString = string.Join(",", ids);
            
            string q = $@"
                SELECT 
                    Id_User,
                    CASE 
                        WHEN EstadoOnline = 1 AND UltimaActividad >= DATEADD(MINUTE, -5, GETDATE()) THEN 1
                        ELSE 0
                    END as EstaOnline
                FROM [User]
                WHERE Id_User IN ({idsString})";

            var dt = BD.ExecuteQuery(q, new Dictionary<string, object>());
            var estados = new Dictionary<int, bool>();

            foreach (System.Data.DataRow row in dt.Rows)
            {
                int id = Convert.ToInt32(row["Id_User"]);
                bool online = Convert.ToInt32(row["EstaOnline"]) == 1;
                estados[id] = online;
            }

            return Json(new { success = true, estados = estados });
        }
        catch (Exception ex)
        {
            Console.WriteLine("❌ Error en ObtenerEstadoOnline: " + ex.Message);
            return Json(new { success = false, message = ex.Message });
        }
    }

    // ========== CURIOSIDADES POR RAZA ==========
    
    [HttpGet]
    public IActionResult ObtenerCuriosidades(string especie, string raza)
    {
        try
        {
            // Verificar si la tabla existe antes de consultarla
            string checkTableQuery = @"
                SELECT COUNT(*) as TableExists
                FROM INFORMATION_SCHEMA.TABLES
                WHERE TABLE_NAME = 'CuriosidadRaza'";
            
            DataTable checkTable = BD.ExecuteQuery(checkTableQuery, new Dictionary<string, object>());
            
            if (checkTable.Rows.Count == 0 || Convert.ToInt32(checkTable.Rows[0]["TableExists"]) == 0)
            {
                // La tabla no existe, devolver lista vacía
                return Json(new { success = true, curiosidades = new List<object>() });
            }
            
            string query = @"
                SELECT Curiosidad, Categoria
                FROM CuriosidadRaza
                WHERE Especie = @Especie AND Raza = @Raza
                ORDER BY NEWID()";
            
            DataTable dt = BD.ExecuteQuery(query, new Dictionary<string, object>
            {
                { "@Especie", especie },
                { "@Raza", raza }
            });
            
            var curiosidades = new List<object>();
            foreach (DataRow row in dt.Rows)
            {
                curiosidades.Add(new
                {
                    curiosidad = row["Curiosidad"].ToString(),
                    categoria = row["Categoria"]?.ToString() ?? "General"
                });
            }
            
            return Json(new { success = true, curiosidades = curiosidades });
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error ObtenerCuriosidades: " + ex.Message);
            // En caso de error, devolver lista vacía en lugar de fallar
            return Json(new { success = true, curiosidades = new List<object>() });
        }
    }

    // ========== PUBLICACIONES ==========
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult CrearPublicacion(IFormFile imagen, string descripcion, int? idMascota)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
            return Json(new { success = false, message = "No autenticado" });

        if (imagen == null || imagen.Length == 0)
        {
            return Json(new { success = false, message = "Debes seleccionar una imagen" });
        }

        try
        {
            string imagenUrl = "";
            if (imagen != null && imagen.Length > 0)
            {
                var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "publicaciones");
                if (!Directory.Exists(uploadsPath))
                    Directory.CreateDirectory(uploadsPath);
                
                var fileName = $"pub_{Guid.NewGuid()}{Path.GetExtension(imagen.FileName)}";
                var filePath = Path.Combine(uploadsPath, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    imagen.CopyTo(stream);
                }
                imagenUrl = $"/uploads/publicaciones/{fileName}";
            }

            string q = @"
                INSERT INTO Publicacion (Id_User, Id_Mascota, ImagenUrl, Descripcion, Fecha)
                VALUES (@UserId, @IdMascota, @ImagenUrl, @Descripcion, GETDATE());
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            int publicacionId = Convert.ToInt32(BD.ExecuteScalar(q, new Dictionary<string, object>
            {
                { "@UserId", userId.Value },
                { "@IdMascota", idMascota ?? (object)DBNull.Value },
                { "@ImagenUrl", imagenUrl },
                { "@Descripcion", descripcion ?? "" }
            }));

            return Json(new { success = true, publicacionId = publicacionId });
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error en CrearPublicacion: " + ex.Message);
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpPost]
    public IActionResult LikePublicacion([FromBody] LikePublicacionRequest request)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
            return Json(new { success = false, message = "No autenticado" });

        try
        {
            // Verificar si ya le dio like
            string qVerificar = @"
                SELECT COUNT(*) FROM LikePublicacion 
                WHERE Id_Publicacion = @PublicacionId AND Id_User = @UserId";
            int existe = Convert.ToInt32(BD.ExecuteScalar(qVerificar, new Dictionary<string, object>
            {
                { "@PublicacionId", request.PublicacionId },
                { "@UserId", userId.Value }
            }));

            if (existe > 0)
            {
                // Quitar like
                string qEliminar = @"
                    DELETE FROM LikePublicacion 
                    WHERE Id_Publicacion = @PublicacionId AND Id_User = @UserId";
                BD.ExecuteNonQuery(qEliminar, new Dictionary<string, object>
                {
                    { "@PublicacionId", request.PublicacionId },
                    { "@UserId", userId.Value }
                });
            }
            else
            {
                // Agregar like
                string qInsertar = @"
                    INSERT INTO LikePublicacion (Id_Publicacion, Id_User, Fecha)
                    VALUES (@PublicacionId, @UserId, GETDATE())";
                BD.ExecuteNonQuery(qInsertar, new Dictionary<string, object>
                {
                    { "@PublicacionId", request.PublicacionId },
                    { "@UserId", userId.Value }
                });
            }

            // Obtener cantidad de likes actualizada
            string qCount = @"
                SELECT COUNT(*) FROM LikePublicacion 
                WHERE Id_Publicacion = @PublicacionId";
            int cantidadLikes = Convert.ToInt32(BD.ExecuteScalar(qCount, new Dictionary<string, object>
            {
                { "@PublicacionId", request.PublicacionId }
            }));

            return Json(new { success = true, cantidadLikes = cantidadLikes, meGusta = existe == 0 });
        }
        catch (Exception ex)
        {
            Console.WriteLine("❌ Error en LikePublicacion: " + ex.Message);
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpPost]
    public IActionResult ComentarPublicacion([FromBody] ComentarRequest request)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
            return Json(new { success = false, message = "No autenticado" });

        try
        {
            string q = @"
                INSERT INTO ComentarioPublicacion (Id_Publicacion, Id_User, Contenido, Fecha)
                VALUES (@PublicacionId, @UserId, @Contenido, GETDATE());
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            int comentarioId = Convert.ToInt32(BD.ExecuteScalar(q, new Dictionary<string, object>
            {
                { "@PublicacionId", request.PublicacionId },
                { "@UserId", userId.Value },
                { "@Contenido", request.Contenido ?? "" }
            }));

            // Obtener comentario con datos del usuario
            string qComentario = @"
                SELECT C.*, U.Nombre + ' ' + U.Apellido as NombreUsuario, PR.FotoPerfil
                FROM ComentarioPublicacion C
                INNER JOIN [User] U ON C.Id_User = U.Id_User
                LEFT JOIN Perfil PR ON PR.Id_Usuario = U.Id_User
                WHERE C.Id_Comentario = @ComentarioId";
            var dt = BD.ExecuteQuery(qComentario, new Dictionary<string, object> { { "@ComentarioId", comentarioId } });
            
            if (dt.Rows.Count > 0)
            {
                var row = dt.Rows[0];
                return Json(new
                {
                    success = true,
                    comentario = new
                    {
                        id = comentarioId,
                        contenido = row["Contenido"]?.ToString(),
                        fecha = Convert.ToDateTime(row["Fecha"]).ToString("yyyy-MM-ddTHH:mm:ss"),
                        nombreUsuario = row["NombreUsuario"]?.ToString(),
                        fotoPerfil = row["FotoPerfil"]?.ToString() ?? "/img/perfil/default.png"
                    }
                });
            }

            return Json(new { success = true, comentarioId = comentarioId });
        }
        catch (Exception ex)
        {
            Console.WriteLine("❌ Error en ComentarPublicacion: " + ex.Message);
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpGet]
    public IActionResult ObtenerComentarios(int publicacionId)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
            return Json(new { success = false, message = "No autenticado" });

        try
        {
            string q = @"
                SELECT C.*, U.Nombre + ' ' + U.Apellido as NombreUsuario, PR.FotoPerfil
                FROM ComentarioPublicacion C
                INNER JOIN [User] U ON C.Id_User = U.Id_User
                LEFT JOIN Perfil PR ON PR.Id_Usuario = U.Id_User
                WHERE C.Id_Publicacion = @PublicacionId AND C.Eliminado = 0
                ORDER BY C.Fecha ASC";
            
            var dt = BD.ExecuteQuery(q, new Dictionary<string, object> { { "@PublicacionId", publicacionId } });
            var comentarios = new List<object>();
            
            foreach (System.Data.DataRow row in dt.Rows)
            {
                comentarios.Add(new
                {
                    id = Convert.ToInt32(row["Id_Comentario"]),
                    contenido = row["Contenido"]?.ToString(),
                    fecha = Convert.ToDateTime(row["Fecha"]).ToString("yyyy-MM-ddTHH:mm:ss"),
                    nombreUsuario = row["NombreUsuario"]?.ToString(),
                    fotoPerfil = row["FotoPerfil"]?.ToString() ?? "/img/perfil/default.png"
                });
            }

            return Json(new { success = true, comentarios = comentarios });
        }
        catch (Exception ex)
        {
            Console.WriteLine("❌ Error en ObtenerComentarios: " + ex.Message);
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpPost]
    public IActionResult CompartirPublicacion([FromBody] CompartirRequest request)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
            return Json(new { success = false, message = "No autenticado" });

        try
        {
            // Verificar si ya compartió
            string qVerificar = @"
                SELECT COUNT(*) FROM CompartirPublicacion 
                WHERE Id_Publicacion = @PublicacionId AND Id_User = @UserId";
            int existe = Convert.ToInt32(BD.ExecuteScalar(qVerificar, new Dictionary<string, object>
            {
                { "@PublicacionId", request.PublicacionId },
                { "@UserId", userId.Value }
            }));

            if (existe == 0)
            {
                string q = @"
                    INSERT INTO CompartirPublicacion (Id_Publicacion, Id_User, Fecha)
                    VALUES (@PublicacionId, @UserId, GETDATE())";
                BD.ExecuteNonQuery(q, new Dictionary<string, object>
                {
                    { "@PublicacionId", request.PublicacionId },
                    { "@UserId", userId.Value }
                });
            }

            // Obtener cantidad de compartidos
            string qCount = @"
                SELECT COUNT(*) FROM CompartirPublicacion 
                WHERE Id_Publicacion = @PublicacionId";
            int cantidad = Convert.ToInt32(BD.ExecuteScalar(qCount, new Dictionary<string, object>
            {
                { "@PublicacionId", request.PublicacionId }
            }));

            return Json(new { success = true, cantidadCompartidos = cantidad });
        }
        catch (Exception ex)
        {
            Console.WriteLine("❌ Error en CompartirPublicacion: " + ex.Message);
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpPost]
    public IActionResult AnclarPublicacion([FromBody] AnclarRequest request)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
            return Json(new { success = false, message = "No autenticado" });

        try
        {
            // Verificar que la publicación es del usuario
            string qVerificar = @"
                SELECT Id_User FROM Publicacion 
                WHERE Id_Publicacion = @PublicacionId";
            var dt = BD.ExecuteQuery(qVerificar, new Dictionary<string, object> { { "@PublicacionId", request.PublicacionId } });
            
            if (dt.Rows.Count == 0 || Convert.ToInt32(dt.Rows[0]["Id_User"]) != userId.Value)
                return Json(new { success = false, message = "No tenés permisos para anclar esta publicación" });

            // Si se ancla, desanclar las demás
            if (request.Anclar)
            {
                string qDesanclar = @"
                    UPDATE Publicacion 
                    SET Anclada = 0, FechaAnclada = NULL
                    WHERE Id_User = @UserId AND Id_Publicacion != @PublicacionId";
                BD.ExecuteNonQuery(qDesanclar, new Dictionary<string, object>
                {
                    { "@UserId", userId.Value },
                    { "@PublicacionId", request.PublicacionId }
                });
            }

            string q = @"
                UPDATE Publicacion 
                SET Anclada = @Anclar, FechaAnclada = @FechaAnclada
                WHERE Id_Publicacion = @PublicacionId";
            BD.ExecuteNonQuery(q, new Dictionary<string, object>
            {
                { "@Anclar", request.Anclar },
                { "@FechaAnclada", request.Anclar ? DateTime.Now : (object)DBNull.Value },
                { "@PublicacionId", request.PublicacionId }
            });

            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            Console.WriteLine("❌ Error en AnclarPublicacion: " + ex.Message);
            return Json(new { success = false, message = ex.Message });
        }
    }

    // ========== HISTORIAS ==========

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult CrearHistoria(IFormFile imagen, string texto, int? idMascota)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
            return Json(new { success = false, message = "No autenticado" });

        try
        {
            if (imagen == null || imagen.Length == 0)
                return Json(new { success = false, message = "Debés subir una imagen" });

            var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "historias");
            if (!Directory.Exists(uploadsPath))
                Directory.CreateDirectory(uploadsPath);
            
            var fileName = $"hist_{Guid.NewGuid()}{Path.GetExtension(imagen.FileName)}";
            var filePath = Path.Combine(uploadsPath, fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                imagen.CopyTo(stream);
            }
            string imagenUrl = $"/uploads/historias/{fileName}";

            string q = @"
                INSERT INTO Historia (Id_User, Id_Mascota, ImagenUrl, Texto, Fecha, Expiracion)
                VALUES (@UserId, @IdMascota, @ImagenUrl, @Texto, GETDATE(), DATEADD(HOUR, 24, GETDATE()));
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            int historiaId = Convert.ToInt32(BD.ExecuteScalar(q, new Dictionary<string, object>
            {
                { "@UserId", userId.Value },
                { "@IdMascota", idMascota ?? (object)DBNull.Value },
                { "@ImagenUrl", imagenUrl },
                { "@Texto", texto ?? "" }
            }));

            return Json(new { success = true, historiaId = historiaId });
        }
        catch (Exception ex)
        {
            Console.WriteLine("❌ Error en CrearHistoria: " + ex.Message);
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpPost]
    public IActionResult AgregarHistoriaDestacada([FromBody] DestacadaRequest request)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
            return Json(new { success = false, message = "No autenticado" });

        try
        {
            // Verificar que la historia es del usuario
            string qVerificar = @"
                SELECT Id_User FROM Historia 
                WHERE Id_Historia = @HistoriaId";
            var dt = BD.ExecuteQuery(qVerificar, new Dictionary<string, object> { { "@HistoriaId", request.HistoriaId } });
            
            if (dt.Rows.Count == 0 || Convert.ToInt32(dt.Rows[0]["Id_User"]) != userId.Value)
                return Json(new { success = false, message = "No tenés permisos para destacar esta historia" });

            string q = @"
                INSERT INTO HistoriaDestacada (Id_User, Id_Historia, Titulo, Fecha)
                VALUES (@UserId, @HistoriaId, @Titulo, GETDATE())";
            BD.ExecuteNonQuery(q, new Dictionary<string, object>
            {
                { "@UserId", userId.Value },
                { "@HistoriaId", request.HistoriaId },
                { "@Titulo", request.Titulo ?? "" }
            });

            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            Console.WriteLine("❌ Error en AgregarHistoriaDestacada: " + ex.Message);
            return Json(new { success = false, message = ex.Message });
        }
    }

    // ========== MENCIONES Y REPOST ==========

    [HttpPost]
    public IActionResult RepostearMencion([FromBody] RepostRequest request)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
            return Json(new { success = false, message = "No autenticado" });

        try
        {
            // Verificar que existe la mención
            string qVerificar = @"
                SELECT Id_Publicacion, Id_Historia, Id_User_Mencionado
                FROM Mencion 
                WHERE Id_Mencion = @MencionId AND Id_User_Mencionado = @UserId";
            var dt = BD.ExecuteQuery(qVerificar, new Dictionary<string, object>
            {
                { "@MencionId", request.MencionId },
                { "@UserId", userId.Value }
            });

            if (dt.Rows.Count == 0)
                return Json(new { success = false, message = "Mención no encontrada" });

            var row = dt.Rows[0];
            int? publicacionId = row["Id_Publicacion"] == DBNull.Value ? null : Convert.ToInt32(row["Id_Publicacion"]);
            int? historiaId = row["Id_Historia"] == DBNull.Value ? null : Convert.ToInt32(row["Id_Historia"]);

            if (publicacionId.HasValue)
            {
                // Repostear publicación: crear una nueva publicación compartiendo la original
                string qPublicacion = @"
                    SELECT ImagenUrl, Descripcion FROM Publicacion 
                    WHERE Id_Publicacion = @PublicacionId";
                var dtPub = BD.ExecuteQuery(qPublicacion, new Dictionary<string, object> { { "@PublicacionId", publicacionId.Value } });
                
                if (dtPub.Rows.Count > 0)
                {
                    var pubRow = dtPub.Rows[0];
                    string nuevaDescripcion = $"📢 Reposteado: {pubRow["Descripcion"]?.ToString()}";
                    
                    string qRepost = @"
                        INSERT INTO Publicacion (Id_User, ImagenUrl, Descripcion, Fecha)
                        VALUES (@UserId, @ImagenUrl, @Descripcion, GETDATE());
                        SELECT CAST(SCOPE_IDENTITY() AS INT);";
                    int nuevaPubId = Convert.ToInt32(BD.ExecuteScalar(qRepost, new Dictionary<string, object>
                    {
                        { "@UserId", userId.Value },
                        { "@ImagenUrl", pubRow["ImagenUrl"]?.ToString() ?? "" },
                        { "@Descripcion", nuevaDescripcion }
                    }));

                    // Marcar mención como reposteada
                    string qUpdate = @"
                        UPDATE Mencion 
                        SET Reposteada = 1 
                        WHERE Id_Mencion = @MencionId";
                    BD.ExecuteNonQuery(qUpdate, new Dictionary<string, object> { { "@MencionId", request.MencionId } });

                    return Json(new { success = true, publicacionId = nuevaPubId });
                }
            }

            return Json(new { success = false, message = "No se pudo repostear" });
        }
        catch (Exception ex)
        {
            Console.WriteLine("❌ Error en RepostearMencion: " + ex.Message);
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpGet]
    public IActionResult ObtenerMenciones()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
            return Json(new { success = false, message = "No autenticado" });

        try
        {
            string q = @"
                SELECT M.*, 
                       U.Nombre + ' ' + U.Apellido as NombreUsuarioMenciona,
                       PR.FotoPerfil as FotoPerfilUsuarioMenciona,
                       P.ImagenUrl as ImagenPublicacion,
                       P.Descripcion as DescripcionPublicacion,
                       H.ImagenUrl as ImagenHistoria
                FROM Mencion M
                INNER JOIN [User] U ON M.Id_User_Menciona = U.Id_User
                LEFT JOIN Perfil PR ON PR.Id_Usuario = U.Id_User
                LEFT JOIN Publicacion P ON M.Id_Publicacion = P.Id_Publicacion
                LEFT JOIN Historia H ON M.Id_Historia = H.Id_Historia
                WHERE M.Id_User_Mencionado = @UserId
                ORDER BY M.Fecha DESC";
            
            var dt = BD.ExecuteQuery(q, new Dictionary<string, object> { { "@UserId", userId.Value } });
            var menciones = new List<object>();
            
            foreach (System.Data.DataRow row in dt.Rows)
            {
                menciones.Add(new
                {
                    id = Convert.ToInt32(row["Id_Mencion"]),
                    tipo = row["Id_Publicacion"] != DBNull.Value ? "publicacion" : "historia",
                    nombreUsuario = row["NombreUsuarioMenciona"]?.ToString(),
                    fotoPerfil = row["FotoPerfilUsuarioMenciona"]?.ToString() ?? "/img/perfil/default.png",
                    imagenPublicacion = row["ImagenPublicacion"]?.ToString(),
                    imagenHistoria = row["ImagenHistoria"]?.ToString(),
                    descripcion = row["DescripcionPublicacion"]?.ToString(),
                    fecha = Convert.ToDateTime(row["Fecha"]).ToString("yyyy-MM-ddTHH:mm:ss"),
                    vista = Convert.ToBoolean(row["Vista"]),
                    reposteada = Convert.ToBoolean(row["Reposteada"])
                });
            }

            return Json(new { success = true, menciones = menciones });
        }
        catch (Exception ex)
        {
            Console.WriteLine("❌ Error en ObtenerMenciones: " + ex.Message);
            return Json(new { success = false, message = ex.Message });
        }
    }

    // Clases de request
    public class LikePublicacionRequest
    {
        public int PublicacionId { get; set; }
    }

    public class ComentarRequest
    {
        public int PublicacionId { get; set; }
        public string? Contenido { get; set; }
    }

    public class CompartirRequest
    {
        public int PublicacionId { get; set; }
    }

    public class AnclarRequest
    {
        public int PublicacionId { get; set; }
        public bool Anclar { get; set; }
    }

    public class DestacadaRequest
    {
        public int HistoriaId { get; set; }
        public string? Titulo { get; set; }
    }

    public class RepostRequest
    {
        public int MencionId { get; set; }
    }

    // ========== CREAR TABLAS ==========
    private void CrearTablasPerfilSocial()
    {
        try
        {
            // Tabla Publicacion
            string qPublicacion = @"
                IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Publicacion')
                BEGIN
                    CREATE TABLE [dbo].[Publicacion](
                        [Id_Publicacion] [int] IDENTITY(1,1) NOT NULL,
                        [Id_User] [int] NOT NULL,
                        [Id_Mascota] [int] NULL,
                        [ImagenUrl] [nvarchar](500) NULL,
                        [Descripcion] [nvarchar](2000) NULL,
                        [Fecha] [datetime2](7) NOT NULL DEFAULT GETDATE(),
                        [Anclada] [bit] NOT NULL DEFAULT 0,
                        [FechaAnclada] [datetime2](7) NULL,
                        [Eliminada] [bit] NOT NULL DEFAULT 0,
                        CONSTRAINT [PK_Publicacion] PRIMARY KEY CLUSTERED ([Id_Publicacion] ASC),
                        CONSTRAINT [FK_Publicacion_User] FOREIGN KEY ([Id_User]) REFERENCES [dbo].[User]([Id_User]),
                        CONSTRAINT [FK_Publicacion_Mascota] FOREIGN KEY ([Id_Mascota]) REFERENCES [dbo].[Mascota]([Id_Mascota])
                    ) ON [PRIMARY]
                END";
            BD.ExecuteNonQuery(qPublicacion);

            // Tabla LikePublicacion
            string qLike = @"
                IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'LikePublicacion')
                BEGIN
                    CREATE TABLE [dbo].[LikePublicacion](
                        [Id_Like] [int] IDENTITY(1,1) NOT NULL,
                        [Id_Publicacion] [int] NOT NULL,
                        [Id_User] [int] NOT NULL,
                        [Fecha] [datetime2](7) NOT NULL DEFAULT GETDATE(),
                        CONSTRAINT [PK_LikePublicacion] PRIMARY KEY CLUSTERED ([Id_Like] ASC),
                        CONSTRAINT [FK_LikePublicacion_Publicacion] FOREIGN KEY ([Id_Publicacion]) REFERENCES [dbo].[Publicacion]([Id_Publicacion]) ON DELETE CASCADE,
                        CONSTRAINT [FK_LikePublicacion_User] FOREIGN KEY ([Id_User]) REFERENCES [dbo].[User]([Id_User]),
                        CONSTRAINT [UQ_LikePublicacion_User_Publicacion] UNIQUE ([Id_User], [Id_Publicacion])
                    ) ON [PRIMARY]
                END";
            BD.ExecuteNonQuery(qLike);

            // Tabla ComentarioPublicacion
            string qComentario = @"
                IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ComentarioPublicacion')
                BEGIN
                    CREATE TABLE [dbo].[ComentarioPublicacion](
                        [Id_Comentario] [int] IDENTITY(1,1) NOT NULL,
                        [Id_Publicacion] [int] NOT NULL,
                        [Id_User] [int] NOT NULL,
                        [Contenido] [nvarchar](1000) NOT NULL,
                        [Fecha] [datetime2](7) NOT NULL DEFAULT GETDATE(),
                        [Eliminado] [bit] NOT NULL DEFAULT 0,
                        CONSTRAINT [PK_ComentarioPublicacion] PRIMARY KEY CLUSTERED ([Id_Comentario] ASC),
                        CONSTRAINT [FK_ComentarioPublicacion_Publicacion] FOREIGN KEY ([Id_Publicacion]) REFERENCES [dbo].[Publicacion]([Id_Publicacion]) ON DELETE CASCADE,
                        CONSTRAINT [FK_ComentarioPublicacion_User] FOREIGN KEY ([Id_User]) REFERENCES [dbo].[User]([Id_User])
                    ) ON [PRIMARY]
                END";
            BD.ExecuteNonQuery(qComentario);

            // Tabla CompartirPublicacion
            string qCompartir = @"
                IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'CompartirPublicacion')
                BEGIN
                    CREATE TABLE [dbo].[CompartirPublicacion](
                        [Id_Compartir] [int] IDENTITY(1,1) NOT NULL,
                        [Id_Publicacion] [int] NOT NULL,
                        [Id_User] [int] NOT NULL,
                        [Fecha] [datetime2](7) NOT NULL DEFAULT GETDATE(),
                        CONSTRAINT [PK_CompartirPublicacion] PRIMARY KEY CLUSTERED ([Id_Compartir] ASC),
                        CONSTRAINT [FK_CompartirPublicacion_Publicacion] FOREIGN KEY ([Id_Publicacion]) REFERENCES [dbo].[Publicacion]([Id_Publicacion]) ON DELETE CASCADE,
                        CONSTRAINT [FK_CompartirPublicacion_User] FOREIGN KEY ([Id_User]) REFERENCES [dbo].[User]([Id_User]),
                        CONSTRAINT [UQ_CompartirPublicacion_User_Publicacion] UNIQUE ([Id_User], [Id_Publicacion])
                    ) ON [PRIMARY]
                END";
            BD.ExecuteNonQuery(qCompartir);

            // Tabla Historia
            string qHistoria = @"
                IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Historia')
                BEGIN
                    CREATE TABLE [dbo].[Historia](
                        [Id_Historia] [int] IDENTITY(1,1) NOT NULL,
                        [Id_User] [int] NOT NULL,
                        [Id_Mascota] [int] NULL,
                        [ImagenUrl] [nvarchar](500) NOT NULL,
                        [Texto] [nvarchar](500) NULL,
                        [Fecha] [datetime2](7) NOT NULL DEFAULT GETDATE(),
                        [Expiracion] [datetime2](7) NOT NULL,
                        [Eliminada] [bit] NOT NULL DEFAULT 0,
                        CONSTRAINT [PK_Historia] PRIMARY KEY CLUSTERED ([Id_Historia] ASC),
                        CONSTRAINT [FK_Historia_User] FOREIGN KEY ([Id_User]) REFERENCES [dbo].[User]([Id_User]),
                        CONSTRAINT [FK_Historia_Mascota] FOREIGN KEY ([Id_Mascota]) REFERENCES [dbo].[Mascota]([Id_Mascota])
                    ) ON [PRIMARY]
                END";
            BD.ExecuteNonQuery(qHistoria);

            // Tabla HistoriaDestacada
            string qDestacada = @"
                IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'HistoriaDestacada')
                BEGIN
                    CREATE TABLE [dbo].[HistoriaDestacada](
                        [Id_Destacada] [int] IDENTITY(1,1) NOT NULL,
                        [Id_User] [int] NOT NULL,
                        [Id_Historia] [int] NOT NULL,
                        [Titulo] [nvarchar](100) NULL,
                        [Fecha] [datetime2](7) NOT NULL DEFAULT GETDATE(),
                        CONSTRAINT [PK_HistoriaDestacada] PRIMARY KEY CLUSTERED ([Id_Destacada] ASC),
                        CONSTRAINT [FK_HistoriaDestacada_User] FOREIGN KEY ([Id_User]) REFERENCES [dbo].[User]([Id_User]),
                        CONSTRAINT [FK_HistoriaDestacada_Historia] FOREIGN KEY ([Id_Historia]) REFERENCES [dbo].[Historia]([Id_Historia]) ON DELETE CASCADE
                    ) ON [PRIMARY]
                END";
            BD.ExecuteNonQuery(qDestacada);

            // Tabla Mencion
            string qMencion = @"
                IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Mencion')
                BEGIN
                    CREATE TABLE [dbo].[Mencion](
                        [Id_Mencion] [int] IDENTITY(1,1) NOT NULL,
                        [Id_User_Mencionado] [int] NOT NULL,
                        [Id_Publicacion] [int] NULL,
                        [Id_Historia] [int] NULL,
                        [Id_User_Menciona] [int] NOT NULL,
                        [Fecha] [datetime2](7) NOT NULL DEFAULT GETDATE(),
                        [Vista] [bit] NOT NULL DEFAULT 0,
                        [Reposteada] [bit] NOT NULL DEFAULT 0,
                        CONSTRAINT [PK_Mencion] PRIMARY KEY CLUSTERED ([Id_Mencion] ASC),
                        CONSTRAINT [FK_Mencion_User_Mencionado] FOREIGN KEY ([Id_User_Mencionado]) REFERENCES [dbo].[User]([Id_User]),
                        CONSTRAINT [FK_Mencion_User_Menciona] FOREIGN KEY ([Id_User_Menciona]) REFERENCES [dbo].[User]([Id_User]),
                        CONSTRAINT [FK_Mencion_Publicacion] FOREIGN KEY ([Id_Publicacion]) REFERENCES [dbo].[Publicacion]([Id_Publicacion]) ON DELETE CASCADE,
                        CONSTRAINT [FK_Mencion_Historia] FOREIGN KEY ([Id_Historia]) REFERENCES [dbo].[Historia]([Id_Historia]) ON DELETE CASCADE,
                        CONSTRAINT [CK_Mencion_Tipo] CHECK (([Id_Publicacion] IS NOT NULL AND [Id_Historia] IS NULL) OR ([Id_Publicacion] IS NULL AND [Id_Historia] IS NOT NULL))
                    ) ON [PRIMARY]
                END";
            BD.ExecuteNonQuery(qMencion);

            // Índices
            string qIdx1 = @"
                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Publicacion_User_Fecha')
                BEGIN
                    CREATE INDEX [IX_Publicacion_User_Fecha] ON [dbo].[Publicacion]([Id_User], [Fecha] DESC)
                END";
            BD.ExecuteNonQuery(qIdx1);

            string qIdx2 = @"
                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Historia_User_Expiracion')
                BEGIN
                    CREATE INDEX [IX_Historia_User_Expiracion] ON [dbo].[Historia]([Id_User], [Expiracion] DESC)
                END";
            BD.ExecuteNonQuery(qIdx2);

            string qIdx3 = @"
                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Mencion_User_Mencionado')
                BEGIN
                    CREATE INDEX [IX_Mencion_User_Mencionado] ON [dbo].[Mencion]([Id_User_Mencionado], [Vista])
                END";
            BD.ExecuteNonQuery(qIdx3);

            Console.WriteLine("✅ Tablas de perfil social creadas correctamente");
        }
        catch (Exception ex)
        {
            Console.WriteLine("❌ Error al crear tablas de perfil social: " + ex.Message);
            throw;
        }
    }

    // ============================
    // Método para notificar carteles cercanos
    // ============================
    private void NotificarCartelCercano(decimal latitud, decimal longitud, string tipo, int idUsuarioCreador)
    {
        try
        {
            // Buscar usuarios cercanos (dentro de 5 km) usando la tabla Ubicacion
            string query = @"
                SELECT DISTINCT U.Id_User, U.Nombre + ' ' + U.Apellido as NombreCompleto
                FROM [User] U
                INNER JOIN Ubicacion UB ON U.Id_Ubicacion = UB.Id_Ubicacion
                WHERE U.Id_User != @IdUsuarioCreador
                  AND UB.Latitud IS NOT NULL 
                  AND UB.Longitud IS NOT NULL
                  AND (
                      6371 * acos(
                          cos(radians(CAST(@Latitud AS FLOAT))) * 
                          cos(radians(CAST(UB.Latitud AS FLOAT))) * 
                          cos(radians(CAST(UB.Longitud AS FLOAT)) - radians(CAST(@Longitud AS FLOAT))) + 
                          sin(radians(CAST(@Latitud AS FLOAT))) * 
                          sin(radians(CAST(UB.Latitud AS FLOAT)))
                      )
                  ) <= 5";

            DataTable dt = BD.ExecuteQuery(query, new Dictionary<string, object>
            {
                { "@IdUsuarioCreador", idUsuarioCreador },
                { "@Latitud", latitud },
                { "@Longitud", longitud }
            });

            string tipoMensaje = tipo == "Perdida" ? "perdida" : "encontrada";
            string emoji = tipo == "Perdida" ? "🔴" : "🟢";

            foreach (DataRow row in dt.Rows)
            {
                int idUser = Convert.ToInt32(row["Id_User"]);
                NotificacionController.CrearNotificacion(
                    idUser,
                    "CartelCercano",
                    $"{emoji} Mascota {tipoMensaje} cerca de ti",
                    $"Hay una mascota {tipoMensaje} cerca de tu ubicación. Revisá el mapa de la comunidad.",
                    null,
                    "/Home/Comunidad"
                );
            }
            
            Console.WriteLine($"✅ Notificaciones enviadas a {dt.Rows.Count} usuarios cercanos");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error NotificarCartelCercano: " + ex.Message);
        }
    }

        // ============================
        // GET: /Home/BuscarProveedores
        // ============================
        [HttpGet]
        [Route("Home/BuscarProveedores")]
        public IActionResult BuscarProveedores(string? ciudad, string? provincia, string? pais, string? tipoServicio, string? especie, 
            decimal? latitud, decimal? longitud, decimal? radioKm, decimal? precioMax, decimal? calificacionMin)
        {
            try
            {
                // Asegurar que las tablas existan
                string checkTable = "SELECT COUNT(*) FROM sys.tables WHERE name = 'ProveedorServicio'";
                object? tableExists = BD.ExecuteScalar(checkTable);
                if (tableExists == null || Convert.ToInt32(tableExists) == 0)
                {
                    Console.WriteLine("⚠️ La tabla ProveedorServicio no existe. Se debe ejecutar el script SQL.");
                }
                
                // Verificar si las columnas de ubicación existen
                string checkLatitud = "SELECT COUNT(*) FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[ProveedorServicio]') AND name = 'Latitud'";
                string checkLongitud = "SELECT COUNT(*) FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[ProveedorServicio]') AND name = 'Longitud'";
                string checkRadio = "SELECT COUNT(*) FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[ProveedorServicio]') AND name = 'Radio_Atencion_Km'";
                string checkTipo = "SELECT COUNT(*) FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[ProveedorServicio]') AND name = 'Tipo_Ubicacion'";
                
                object? latExists = BD.ExecuteScalar(checkLatitud);
                object? lngExists = BD.ExecuteScalar(checkLongitud);
                object? radioExists = BD.ExecuteScalar(checkRadio);
                object? tipoExists = BD.ExecuteScalar(checkTipo);
                
                bool tieneLatitud = latExists != null && Convert.ToInt32(latExists) > 0;
                bool tieneLongitud = lngExists != null && Convert.ToInt32(lngExists) > 0;
                bool tieneRadio = radioExists != null && Convert.ToInt32(radioExists) > 0;
                bool tieneTipo = tipoExists != null && Convert.ToInt32(tipoExists) > 0;
                bool tieneColumnasUbicacion = tieneLatitud && tieneLongitud;
                
                // Cargar tipos de servicio
                string tiposQuery = "SELECT Id_TipoServicio, Descripcion FROM TipoServicio ORDER BY Descripcion";
                DataTable tiposDt = BD.ExecuteQuery(tiposQuery);
                ViewBag.TiposServicio = tiposDt;

                // Construir query de búsqueda
                string query = $@"
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
                        P.Experiencia_Anios
                        {(tieneLatitud ? ", P.Latitud" : "")}
                        {(tieneLongitud ? ", P.Longitud" : "")}
                        {(tieneRadio ? ", P.Radio_Atencion_Km" : "")}
                        {(tieneTipo ? ", P.Tipo_Ubicacion" : "")}";

                // Si hay coordenadas, calcular distancia
                if (latitud.HasValue && longitud.HasValue && tieneColumnasUbicacion)
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
                if (latitud.HasValue && longitud.HasValue && tieneColumnasUbicacion)
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
                if (latitud.HasValue && longitud.HasValue && tieneColumnasUbicacion)
                {
                    parametros.Add("@Latitud", latitud.Value);
                    parametros.Add("@Longitud", longitud.Value);
                    
                    decimal radioBusqueda = radioKm ?? 10.0M;
                    string tipoUbicacionCheck = tieneTipo ? "P.Tipo_Ubicacion" : "'Cobertura'";
                    string radioAtencionCheck = tieneRadio ? "P.Radio_Atencion_Km" : "5.00";
                    
                    query += $@"
                        AND (
                            (6371 * acos(
                                cos(radians(@Latitud)) * 
                                cos(radians(P.Latitud)) * 
                                cos(radians(P.Longitud) - radians(@Longitud)) + 
                                sin(radians(@Latitud)) * 
                                sin(radians(P.Latitud))
                            )) <= 
                            CASE 
                                WHEN {tipoUbicacionCheck} = 'Precisa' THEN @RadioBusqueda
                                WHEN {tipoUbicacionCheck} = 'Cobertura' THEN ({radioAtencionCheck} + @RadioBusqueda)
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

                // Filtro por precio máximo
                if (precioMax.HasValue && precioMax.Value > 0)
                {
                    query += " AND (P.Precio_Hora IS NULL OR P.Precio_Hora <= @PrecioMax)";
                    parametros.Add("@PrecioMax", precioMax.Value);
                }

                // Filtro por calificación mínima
                if (calificacionMin.HasValue && calificacionMin.Value > 0)
                {
                    query += " AND (P.Calificacion_Promedio IS NULL OR P.Calificacion_Promedio >= @CalificacionMin)";
                    parametros.Add("@CalificacionMin", calificacionMin.Value);
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
                ViewBag.PrecioMax = precioMax;
                ViewBag.CalificacionMin = calificacionMin;

                return View();
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Error en Home/BuscarProveedores: " + ex.Message);
                ViewBag.Error = "Error al buscar proveedores.";
                return View();
            }
        }

        // ============================
        // GET: /Home/PerfilProveedor/{id}
        // ============================
        [HttpGet]
        [Route("Home/PerfilProveedor/{id}")]
        public IActionResult PerfilProveedor(int id)
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
                    return RedirectToAction("BuscarProveedores", "Home");
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
                Console.WriteLine("❌ Error en Home/PerfilProveedor: " + ex.Message);
                TempData["Error"] = "Error al cargar el perfil.";
                return RedirectToAction("BuscarProveedores", "Home");
            }
        }

        // ============================
        // POST: /Home/CrearResena
        // ============================
        [HttpPost]
        [Route("Home/CrearResena")]
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
                Console.WriteLine("❌ Error en Home/CrearResena: " + ex.Message);
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ============================
        // GET: /Home/ContratarProveedor/{id}
        // ============================
        [HttpGet]
        [Route("Home/ContratarProveedor/{id}")]
        public IActionResult ContratarProveedor(int id)
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
                    return RedirectToAction("BuscarProveedores", "Home");
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

                return View();
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Error en Home/ContratarProveedor: " + ex.Message);
                TempData["Error"] = "Error al cargar la página de contratación.";
                return RedirectToAction("BuscarProveedores", "Home");
            }
        }

        // ============================
        // POST: /Home/CrearReserva
        // ============================
        [HttpPost]
        [Route("Home/CrearReserva")]
        public IActionResult CrearReserva([FromBody] System.Text.Json.JsonElement request)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return Json(new { success = false, message = "No autenticado" });

            try
            {
                // Verificar y crear tabla ReservaProveedor si no existe
                string checkTableReserva = "SELECT COUNT(*) FROM sys.tables WHERE name = 'ReservaProveedor'";
                object? tableReservaExists = BD.ExecuteScalar(checkTableReserva);
                if (tableReservaExists == null || Convert.ToInt32(tableReservaExists) == 0)
                {
                    string createTableReserva = @"
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
                        )";
                    BD.ExecuteNonQuery(createTableReserva);
                }

                int idProveedor = request.TryGetProperty("idProveedor", out var provProp) ? provProp.GetInt32() : 0;
                int idMascota = request.TryGetProperty("idMascota", out var mascProp) ? mascProp.GetInt32() : 0;
                int idTipoServicio = request.TryGetProperty("idTipoServicio", out var tipoProp) ? tipoProp.GetInt32() : 0;
                DateTime fechaInicio = request.TryGetProperty("fechaInicio", out var fechaProp) ? DateTime.Parse(fechaProp.GetString() ?? "") : DateTime.Now;
                TimeSpan horaInicio = request.TryGetProperty("horaInicio", out var horaProp) ? TimeSpan.Parse(horaProp.GetString() ?? "") : TimeSpan.Zero;
                decimal duracionHoras = request.TryGetProperty("duracionHoras", out var durProp) ? durProp.GetDecimal() : 1.0M;
                string? notas = request.TryGetProperty("notas", out var notasProp) ? notasProp.GetString() : null;
                string? direccionServicio = request.TryGetProperty("direccionServicio", out var dirProp) ? dirProp.GetString() : null;
                decimal? latitudServicio = request.TryGetProperty("latitudServicio", out var latProp) && latProp.ValueKind != System.Text.Json.JsonValueKind.Null ? latProp.GetDecimal() : null;
                decimal? longitudServicio = request.TryGetProperty("longitudServicio", out var lngProp) && lngProp.ValueKind != System.Text.Json.JsonValueKind.Null ? lngProp.GetDecimal() : null;
                bool compartirUbicacion = request.TryGetProperty("compartirUbicacion", out var compProp) ? compProp.GetBoolean() : false;

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
                Console.WriteLine("❌ Error en Home/CrearReserva: " + ex.Message);
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ============================
        // POST: /Home/CancelarReserva
        // ============================
        [HttpPost]
        [Route("Home/CancelarReserva")]
        public IActionResult CancelarReserva([FromBody] System.Text.Json.JsonElement request)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return Json(new { success = false, message = "No autenticado" });

            try
            {
                int idReserva = request.TryGetProperty("idReserva", out var idProp) ? idProp.GetInt32() : 0;

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

        // ============================
        // POST: /Home/ObtenerOCrearChatProveedor
        // ============================
        [HttpPost]
        [Route("Home/ObtenerOCrearChatProveedor")]
        public IActionResult ObtenerOCrearChatProveedor([FromBody] int idProveedorUser)
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
                Console.WriteLine("❌ Error en ObtenerOCrearChatProveedor: " + ex.Message);
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ============================
        // GET: Obtener todos los proveedores (para planificador)
        // ============================
        [HttpGet]
        [Route("Home/ObtenerProveedores")]
        public IActionResult ObtenerProveedores()
        {
            try
            {
                string checkTable = "SELECT COUNT(*) FROM sys.tables WHERE name = 'ProveedorServicio'";
                object tableExists = BD.ExecuteScalar(checkTable);
                if (tableExists == null || Convert.ToInt32(tableExists) == 0)
                {
                    Console.WriteLine("⚠️ La tabla ProveedorServicio no existe.");
                }
                
                string query = @"
                    SELECT 
                        P.Id_Proveedor,
                        P.NombreCompleto,
                        P.Precio_Hora,
                        P.Estado,
                        P.FotoPerfil,
                        P.Calificacion_Promedio
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
                        precioHora = row["Precio_Hora"] != DBNull.Value ? Convert.ToDecimal(row["Precio_Hora"]) : (decimal?)null,
                        fotoPerfil = row["FotoPerfil"]?.ToString() ?? null,
                        calificacion = row["Calificacion_Promedio"] != DBNull.Value ? Convert.ToDouble(row["Calificacion_Promedio"]) : (double?)null
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
        [Route("Home/ObtenerTiposServicio/{idProveedor}")]
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

        // ============================
        // GET: Obtener Id_User de un proveedor
        // ============================
        [HttpGet]
        [Route("Home/ObtenerUserIdProveedor/{idProveedor}")]
        public IActionResult ObtenerUserIdProveedor(int idProveedor)
        {
            try
            {
                string query = "SELECT Id_User FROM ProveedorServicio WHERE Id_Proveedor = @IdProveedor";
                DataTable dt = BD.ExecuteQuery(query, new Dictionary<string, object> { { "@IdProveedor", idProveedor } });
                
                if (dt.Rows.Count > 0)
                {
                    int userId = Convert.ToInt32(dt.Rows[0]["Id_User"]);
                    return Json(new { success = true, userId = userId });
                }
                
                return Json(new { success = false, message = "Proveedor no encontrado" });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error ObtenerUserIdProveedor: " + ex.Message);
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ============================
        // GET: Obtener proveedores para Comunidad (con ubicación y detalles)
        // ============================
        [HttpGet]
        [Route("Home/ObtenerProveedoresParaComunidad")]
        public IActionResult ObtenerProveedoresParaComunidad()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return Json(new { success = false, message = "No autenticado" });

            try
            {
                // Verificar si las columnas de ubicación existen en ProveedorServicio
                string checkLatitud = "SELECT COUNT(*) FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[ProveedorServicio]') AND name = 'Latitud'";
                string checkLongitud = "SELECT COUNT(*) FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[ProveedorServicio]') AND name = 'Longitud'";
                string checkRadio = "SELECT COUNT(*) FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[ProveedorServicio]') AND name = 'Radio_Atencion_Km'";
                string checkTipo = "SELECT COUNT(*) FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[ProveedorServicio]') AND name = 'Tipo_Ubicacion'";
                
                object? latExists = BD.ExecuteScalar(checkLatitud);
                object? lngExists = BD.ExecuteScalar(checkLongitud);
                object? radioExists = BD.ExecuteScalar(checkRadio);
                object? tipoExists = BD.ExecuteScalar(checkTipo);
                
                bool tieneLatitud = latExists != null && Convert.ToInt32(latExists) > 0;
                bool tieneLongitud = lngExists != null && Convert.ToInt32(lngExists) > 0;
                bool tieneRadio = radioExists != null && Convert.ToInt32(radioExists) > 0;
                bool tieneTipo = tipoExists != null && Convert.ToInt32(tipoExists) > 0;
                bool tieneColumnasUbicacion = tieneLatitud && tieneLongitud;

                // Obtener ubicación del usuario si existe
                string qUbicacion = @"
                    SELECT Latitud, Longitud FROM Ubicacion WHERE Id_Ubicacion = 
                    (SELECT Id_Ubicacion FROM [User] WHERE Id_User = @UserId)";
                var dtUbicacion = BD.ExecuteQuery(qUbicacion, new Dictionary<string, object> { { "@UserId", userId.Value } });
                
                decimal? latUsuario = null;
                decimal? lngUsuario = null;
                if (dtUbicacion.Rows.Count > 0 && dtUbicacion.Rows[0]["Latitud"] != DBNull.Value)
                {
                    latUsuario = Convert.ToDecimal(dtUbicacion.Rows[0]["Latitud"]);
                    lngUsuario = Convert.ToDecimal(dtUbicacion.Rows[0]["Longitud"]);
                }

                string query = $@"
                    SELECT DISTINCT
                        P.Id_Proveedor,
                        P.Id_User,
                        P.NombreCompleto,
                        P.Descripcion,
                        P.FotoPerfil,
                        P.Ciudad,
                        P.Provincia,
                        P.Precio_Hora,
                        P.Calificacion_Promedio,
                        P.Cantidad_Resenas,
                        P.Experiencia_Anios
                        {(tieneLatitud ? ", P.Latitud" : "")}
                        {(tieneLongitud ? ", P.Longitud" : "")}
                        {(tieneRadio ? ", P.Radio_Atencion_Km" : "")}
                        {(tieneTipo ? ", P.Tipo_Ubicacion" : "")}";

                if (latUsuario.HasValue && lngUsuario.HasValue && tieneColumnasUbicacion)
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
                
                // Solo filtrar por ubicación si el usuario tiene ubicación y queremos mostrar solo los cercanos
                // Si no tiene ubicación, mostrar todos los proveedores
                if (tieneColumnasUbicacion && latUsuario.HasValue && lngUsuario.HasValue)
                {
                    query += " AND P.Latitud IS NOT NULL AND P.Longitud IS NOT NULL";
                }

                var parametros = new Dictionary<string, object>();
                if (latUsuario.HasValue && lngUsuario.HasValue && tieneColumnasUbicacion)
                {
                    parametros.Add("@Latitud", latUsuario.Value);
                    parametros.Add("@Longitud", lngUsuario.Value);
                    query += " ORDER BY Distancia_Km ASC, P.Calificacion_Promedio DESC";
                }
                else
                {
                    query += " ORDER BY P.Calificacion_Promedio DESC, P.Cantidad_Resenas DESC";
                }

                DataTable dt = BD.ExecuteQuery(query, parametros);
                var proveedores = new List<object>();

                foreach (DataRow row in dt.Rows)
                {
                    // Obtener tipos de servicio
                    string qTipos = @"
                        SELECT TS.Descripcion
                        FROM ProveedorServicio_TipoServicio PST
                        INNER JOIN TipoServicio TS ON PST.Id_TipoServicio = TS.Id_TipoServicio
                        WHERE PST.Id_Proveedor = @IdProveedor";
                    var dtTipos = BD.ExecuteQuery(qTipos, new Dictionary<string, object> { { "@IdProveedor", Convert.ToInt32(row["Id_Proveedor"]) } });
                    var tiposServicio = new List<string>();
                    foreach (DataRow tipoRow in dtTipos.Rows)
                    {
                        tiposServicio.Add(tipoRow["Descripcion"].ToString());
                    }

                    // Desencriptar campos encriptados
                    string nombreCompletoDecrypted = row["NombreCompleto"] != DBNull.Value 
                        ? EncryptionHelper.Decrypt(row["NombreCompleto"].ToString() ?? "") 
                        : "Proveedor";
                    string descripcionDecrypted = row["Descripcion"] != DBNull.Value 
                        ? EncryptionHelper.Decrypt(row["Descripcion"].ToString() ?? "") 
                        : "";
                    string ciudadDecrypted = row["Ciudad"] != DBNull.Value 
                        ? EncryptionHelper.Decrypt(row["Ciudad"].ToString() ?? "") 
                        : "";
                    string provinciaDecrypted = row["Provincia"] != DBNull.Value 
                        ? EncryptionHelper.Decrypt(row["Provincia"].ToString() ?? "") 
                        : "";

                    proveedores.Add(new
                    {
                        id = Convert.ToInt32(row["Id_Proveedor"]),
                        idUser = Convert.ToInt32(row["Id_User"]),
                        nombreCompleto = nombreCompletoDecrypted,
                        descripcion = descripcionDecrypted,
                        fotoPerfil = row["FotoPerfil"]?.ToString() ?? "/img/perfil/default.png",
                        ciudad = ciudadDecrypted,
                        provincia = provinciaDecrypted,
                        precioHora = row["Precio_Hora"] != DBNull.Value ? Convert.ToDecimal(row["Precio_Hora"]) : (decimal?)null,
                        calificacion = row["Calificacion_Promedio"] != DBNull.Value ? Convert.ToDecimal(row["Calificacion_Promedio"]) : (decimal?)null,
                        cantidadResenas = row["Cantidad_Resenas"] != DBNull.Value ? Convert.ToInt32(row["Cantidad_Resenas"]) : 0,
                        experienciaAnios = row["Experiencia_Anios"] != DBNull.Value ? Convert.ToInt32(row["Experiencia_Anios"]) : 0,
                        lat = tieneLatitud && row.Table.Columns.Contains("Latitud") && row["Latitud"] != DBNull.Value ? Convert.ToDouble(row["Latitud"]) : (double?)null,
                        lng = tieneLongitud && row.Table.Columns.Contains("Longitud") && row["Longitud"] != DBNull.Value ? Convert.ToDouble(row["Longitud"]) : (double?)null,
                        radioAtencionKm = tieneRadio && row.Table.Columns.Contains("Radio_Atencion_Km") && row["Radio_Atencion_Km"] != DBNull.Value ? Convert.ToDouble(row["Radio_Atencion_Km"]) : 5.0,
                        tipoUbicacion = tieneTipo && row.Table.Columns.Contains("Tipo_Ubicacion") ? row["Tipo_Ubicacion"]?.ToString() ?? "Cobertura" : "Cobertura",
                        distanciaKm = row.Table.Columns.Contains("Distancia_Km") && row["Distancia_Km"] != DBNull.Value ? Convert.ToDouble(row["Distancia_Km"]) : (double?)null,
                        tiposServicio = tiposServicio
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
    }
}