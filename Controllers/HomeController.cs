    using Microsoft.AspNetCore.Mvc;
using Zooni.Models;
using System.Data;
using System.Collections.Generic;
using System;
using System.IO;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.IO.Image;
namespace Zooni.Controllers
{
    public class HomeController : Controller
    {
       private DataRow ObtenerMascotaActiva(int userId)
{
    int? mascotaId = HttpContext.Session.GetInt32("MascotaId");
    string query;
    Dictionary<string, object> param;

    if (mascotaId != null)
    {
        query = "SELECT TOP 1 * FROM Mascota WHERE Id_Mascota = @Id";
        param = new Dictionary<string, object> { { "@Id", mascotaId.Value } };
    }
    else
    {
        query = @"SELECT TOP 1 * FROM Mascota WHERE Id_User = @UserId ORDER BY Id_Mascota DESC";
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

    ViewBag.MascotaNombre = mascota["Nombre"].ToString();
    ViewBag.MascotaEspecie = mascota["Especie"].ToString();
    ViewBag.MascotaRaza = mascota["Raza"].ToString();
    ViewBag.MascotaEdad = mascota["Edad"] == DBNull.Value ? 0 : Convert.ToInt32(mascota["Edad"]);
    ViewBag.MascotaPeso = mascota["Peso"] == DBNull.Value ? 0 : Convert.ToDecimal(mascota["Peso"]);

    // 🟢 NUEVO: agregar la foto si existe
    ViewBag.MascotaFoto = mascota.Table.Columns.Contains("Foto") && mascota["Foto"] != DBNull.Value
        ? mascota["Foto"].ToString()
        : "";
}


        [HttpGet]
        public IActionResult Index()
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("UserId");
                if (userId == null) return RedirectToAction("Login", "Auth");

                var param = new Dictionary<string, object> { { "@UserId", userId.Value } };
                var userDt = BD.ExecuteQuery("SELECT TOP 1 Nombre, Apellido FROM [User] WHERE Id_User = @UserId", param);

                if (userDt.Rows.Count == 0) return RedirectToAction("Login", "Auth");
                ViewBag.UserNombre = userDt.Rows[0]["Nombre"].ToString();

                var mascota = ObtenerMascotaActiva(userId.Value);
                CargarViewBagMascota(mascota);
                ViewBag.MascotaFoto = HttpContext.Session.GetString("MascotaFoto");


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

                var mascota = ObtenerMascotaActiva(userId.Value);
                if (mascota == null)
                {
                    TempData["Error"] = "No hay mascota activa.";
                    return RedirectToAction("Registro2", "Registro");
                }

                CargarViewBagMascota(mascota);
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

                string q = "SELECT Nombre, Especie, Raza FROM Mascota WHERE Id_Mascota = @Id AND Id_User = @User";
                var dt = BD.ExecuteQuery(q, new Dictionary<string, object> { { "@Id", MascotaId }, { "@User", userId.Value } });

                if (dt.Rows.Count == 0)
                {
                    TempData["Error"] = "Mascota no encontrada.";
                    return RedirectToAction("Configuracion");
                }

                var m = dt.Rows[0];
                HttpContext.Session.SetInt32("MascotaId", MascotaId);
                HttpContext.Session.SetString("MascotaNombre", m["Nombre"].ToString());
                HttpContext.Session.SetString("MascotaEspecie", m["Especie"].ToString());
                HttpContext.Session.SetString("MascotaRaza", m["Raza"].ToString());

                TempData["Exito"] = $"Mascota activa: {m["Nombre"]} 🐾";
                return RedirectToAction("Configuracion");
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Error en CambiarMascota: " + ex.Message);
                TempData["Error"] = "Error al cambiar mascota.";
                return RedirectToAction("Configuracion");
            }
        }

        [HttpPost]
        public IActionResult EditarMascota(Mascota model)
        {
            try
            {
                var mascotaId = HttpContext.Session.GetInt32("MascotaId");
                if (mascotaId == null)
                {
                    TempData["Error"] = "No hay mascota activa.";
                    return RedirectToAction("Configuracion");
                }

                string q = @"
                    UPDATE Mascota
                    SET Nombre=@Nombre, Raza=@Raza, Edad=@Edad, Peso=@Peso, Sexo=@Sexo
                    WHERE Id_Mascota=@Id";

                var p = new Dictionary<string, object>
                {
                    { "@Nombre", model.Nombre },
                    { "@Raza", model.Raza },
                    { "@Edad", model.Edad },
                    { "@Peso", model.Peso },
                    { "@Sexo", model.Sexo },
                    { "@Id", mascotaId.Value }
                };

                BD.ExecuteNonQuery(q, p);
                TempData["Exito"] = "Datos de mascota actualizados ✅";
                return RedirectToAction("Configuracion");
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Error en EditarMascota: " + ex.Message);
                TempData["Error"] = "No se pudieron guardar los cambios.";
                return RedirectToAction("Configuracion");
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

    var mascota = ObtenerMascotaActiva(userId.Value);
    if (mascota == null)
        return Content("No se encontró ninguna mascota para generar el PDF");

    int idMascota = Convert.ToInt32(mascota["Id_Mascota"]);
    string nombre = mascota["Nombre"].ToString();
    string especie = mascota["Especie"].ToString();
    string raza = mascota["Raza"].ToString();

    // ✅ Peso corregido (misma lógica que en las views)
    string pesoStr = mascota["Peso"] == DBNull.Value ? "0" : mascota["Peso"].ToString();
    decimal pesoNum = 0;
    decimal.TryParse(pesoStr.Replace(',', '.'), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out pesoNum);

    if (pesoNum >= 100)
        pesoNum /= 10;

    string peso = $"{pesoNum:0.##}".Replace('.', ',') + " kg";

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
public IActionResult Perfil()
{
    var userId = HttpContext.Session.GetInt32("UserId");
    if (userId == null) return RedirectToAction("Login", "Auth");

    string qPerfil = @"SELECT TOP 1 P.Id_Perfil, U.Nombre, U.Apellido, U.Pais, P.Descripcion, P.FotoPerfil
                       FROM Perfil P 
                       INNER JOIN [User] U ON P.Id_Usuario = U.Id_User
                       WHERE U.Id_User = @Id";

    var dtPerfil = BD.ExecuteQuery(qPerfil, new Dictionary<string, object> { { "@Id", userId.Value } });
    if (dtPerfil.Rows.Count == 0)
    {
        TempData["Error"] = "No se encontró perfil.";
        return RedirectToAction("Index");
    }

    var p = dtPerfil.Rows[0];
    ViewBag.Nombre = $"{p["Nombre"]} {p["Apellido"]}";
    ViewBag.Pais = p["Pais"]?.ToString() ?? "Argentina";
    ViewBag.Descripcion = p["Descripcion"]?.ToString() ?? "Amante de los animales ❤️";
    ViewBag.FotoPerfil = p["FotoPerfil"]?.ToString() ?? "/img/perfil/default.png";

    // Mascotas del usuario
    string qMascotas = @"SELECT Id_Mascota, Nombre, Especie, Raza, Foto 
                         FROM Mascota WHERE Id_User = @Id";
    var dtMascotas = BD.ExecuteQuery(qMascotas, new Dictionary<string, object> { { "@Id", userId.Value } });

    var mascotas = new List<Mascota>();
    foreach (System.Data.DataRow m in dtMascotas.Rows)
    {
        mascotas.Add(new Mascota
        {
            Id_Mascota = Convert.ToInt32(m["Id_Mascota"]),
            Nombre = m["Nombre"].ToString(),
            Especie = m["Especie"].ToString(),
            Raza = m["Raza"].ToString(),
            Foto = m["Foto"] == DBNull.Value ? "/img/mascotas/default.png" : m["Foto"].ToString()
        });
    }

    ViewBag.Mascotas = mascotas;
    return View("Perfil");
}
[HttpGet]
public IActionResult Juegos()
{
    return View();
}

    }
}