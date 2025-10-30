using Microsoft.AspNetCore.Mvc;
using Zooni.Models;
using System.Data;
using System.Collections.Generic;
using System;

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
    }
}