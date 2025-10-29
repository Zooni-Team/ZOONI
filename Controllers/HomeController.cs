using Microsoft.AspNetCore.Mvc;
using Zooni.Models;
using System.Data;
using System.Collections.Generic;
using System;

namespace Zooni.Controllers
{
    public class HomeController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("UserId");
                if (userId == null)
                    return RedirectToAction("Login", "Auth");

                string queryUser = "SELECT TOP 1 Id_User, Nombre, Apellido FROM [User] WHERE Id_User = @UserId";
                var param = new Dictionary<string, object> { { "@UserId", userId.Value } };
                DataTable userDt = BD.ExecuteQuery(queryUser, param);

                if (userDt.Rows.Count == 0)
                    return RedirectToAction("Login", "Auth");

                var user = userDt.Rows[0];
                ViewBag.UserNombre = user["Nombre"].ToString();

                string queryMascota = @"
                    SELECT TOP 1 Nombre, Especie, Raza 
                    FROM Mascota WHERE Id_User = @UserId 
                    ORDER BY Id_Mascota DESC";

                DataTable dtMascota = BD.ExecuteQuery(queryMascota, param);

                if (dtMascota.Rows.Count > 0)
                {
                    var mascota = dtMascota.Rows[0];
                    ViewBag.MascotaNombre = mascota["Nombre"].ToString();
                    ViewBag.MascotaEspecie = mascota["Especie"].ToString();
                    ViewBag.MascotaRaza = mascota["Raza"].ToString();
                }
                else
                {
                    ViewBag.MascotaNombre = null;
                }

                return View();
            }
            catch (Exception)
            {
                return RedirectToAction("Login", "Auth");
            }
        }
        [HttpGet]
        public IActionResult FichaMedica()
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("UserId");
                if (userId == null)
                    return RedirectToAction("Login", "Auth");

                // 🔹 Traemos datos de la mascota actual
                string queryMascota = @"
            SELECT TOP 1 Nombre, Especie, Raza, Edad, Peso
            FROM Mascota
            WHERE Id_User = @UserId
            ORDER BY Id_Mascota DESC";

                var param = new Dictionary<string, object> { { "@UserId", userId.Value } };
                var dt = BD.ExecuteQuery(queryMascota, param);

                if (dt.Rows.Count > 0)
                {
                    var m = dt.Rows[0];
                    ViewBag.MascotaNombre = m["Nombre"].ToString();
                    ViewBag.MascotaEspecie = m["Especie"].ToString();
                    ViewBag.MascotaRaza = m["Raza"].ToString();
                    ViewBag.MascotaEdad = m["Edad"].ToString();
                    ViewBag.MascotaPeso = m["Peso"].ToString();
                }

                return View("FichaMedica");
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Error en FichaMedica: " + ex.Message);
                return RedirectToAction("Index");
            }
        }
        [HttpGet]
        public IActionResult FichaOtros()
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("UserId");
                if (userId == null)
                {
                    TempData["Error"] = "Iniciá sesión para ver esta sección.";
                    return RedirectToAction("Login", "Auth");
                }

                var param = new Dictionary<string, object> { { "@UserId", userId.Value } };
                string queryMascota = @"
            SELECT TOP 1 
                Nombre, Especie, Raza, Peso, Edad
            FROM Mascota
            WHERE Id_User = @UserId
            ORDER BY Id_Mascota DESC";

                DataTable dt = BD.ExecuteQuery(queryMascota, param);

                if (dt.Rows.Count == 0)
                {
                    TempData["Error"] = "No se encontró ninguna mascota asociada.";
                    return RedirectToAction("Registro2", "Registro");
                }

                var mascota = dt.Rows[0];

                // 🧠 Convertimos edad a meses y formateamos peso
                int edadMeses = mascota["Edad"] != DBNull.Value ? Convert.ToInt32(mascota["Edad"]) : 0;
                double peso = 0;
                double.TryParse(mascota["Peso"]?.ToString(), out peso);

                // ✅ Cargar datos en ViewBag
                ViewBag.MascotaNombre = mascota["Nombre"].ToString();
                ViewBag.MascotaEspecie = mascota["Especie"].ToString();
                ViewBag.MascotaRaza = mascota["Raza"].ToString();
                ViewBag.MascotaPeso = peso;
                ViewBag.MascotaEdad = edadMeses;

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
            int? idUser = HttpContext.Session.GetInt32("UserId");
            if (idUser == null)
                return RedirectToAction("Login", "Auth");

            string query = @"
        SELECT E.Id_Evento, E.Id_User, E.Id_Mascota, E.Titulo, E.Descripcion, E.Fecha, E.Tipo
        FROM CalendarioEvento E
        INNER JOIN Calendario C ON E.Id_User = C.Id_User
        WHERE E.Id_User = @Id_User
        ORDER BY E.Fecha ASC;";

            var parametros = new Dictionary<string, object> { { "@Id_User", idUser.Value } };
            var tabla = BD.ExecuteQuery(query, parametros);

            List<CalendarioEvento> eventos = new List<CalendarioEvento>();
            foreach (System.Data.DataRow row in tabla.Rows)
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
                Id_User = idUser.Value,
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

            // 🔹 Validar fecha
            if (ev.Fecha == default(DateTime) || ev.Fecha < new DateTime(1753, 1, 1))
            {
                TempData["ErrorCalendario"] = "Seleccioná una fecha válida para el evento 🕒";
                return RedirectToAction("Calendario");
            }

            // 🔹 Buscar calendario activo
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

            // 🔹 Insert evento
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
// =============================
// 💉 FICHA DE VACUNAS
// =============================
// =============================
// 💉 FICHA DE VACUNAS
// =============================
// =============================
// 💉 FICHA DE VACUNAS
// =============================
[HttpGet]
public IActionResult FichaVacunas()
{
    try
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null) return RedirectToAction("Login", "Auth");

        string queryMascota = @"
            SELECT TOP 1 Id_Mascota, Nombre, Especie, Raza, Edad, Peso
            FROM Mascota
            WHERE Id_User = @UserId
            ORDER BY Id_Mascota DESC";

        var p = new Dictionary<string, object> { { "@UserId", userId.Value } };
        var dtMascota = BD.ExecuteQuery(queryMascota, p);
        if (dtMascota.Rows.Count == 0) {
            TempData["Error"] = "No se encontró ninguna mascota asociada.";
            return RedirectToAction("Registro2", "Registro");
        }

        var m = dtMascota.Rows[0];
        int idMascota = Convert.ToInt32(m["Id_Mascota"]);
        ViewBag.IdMascota     = idMascota; // ✅ lo usamos en la view
        ViewBag.MascotaNombre = m["Nombre"].ToString();
        ViewBag.MascotaEspecie= m["Especie"].ToString();
        ViewBag.MascotaRaza   = m["Raza"].ToString();
        ViewBag.MascotaPeso   = m["Peso"].ToString();
        ViewBag.MascotaEdad   = m["Edad"].ToString();

        string queryVacunas = @"
            SELECT Id_Vacuna, Nombre, Fecha_Aplicacion, Proxima_Dosis, Veterinario, Aplicada
            FROM Vacuna
            WHERE Id_Mascota = @Id_Mascota
            ORDER BY Proxima_Dosis ASC";

        var dtVac = BD.ExecuteQuery(queryVacunas, new Dictionary<string, object> { { "@Id_Mascota", idMascota } });

        var vacunas = new List<Vacuna>();
        foreach (System.Data.DataRow row in dtVac.Rows)
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

// =============================
// ✅ MARCAR/UPSERT VACUNA DEFAULT
// =============================
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

        // UPDATE por nombre (case-insensitive) para esa mascota
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
            // INSERT si no existe
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

// =============================
// ➕ AÑADIR VACUNA MANUAL
// =============================
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

// =============================
// ❌ ELIMINAR VACUNA
// =============================
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

    }
}