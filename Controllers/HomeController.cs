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

    ViewBag.MascotaNombre = mascota["Nombre"]?.ToString() ?? "Sin nombre";
    ViewBag.MascotaEspecie = mascota["Especie"]?.ToString() ?? "Desconocida";
    ViewBag.MascotaRaza = mascota["Raza"]?.ToString() ?? "";
    ViewBag.MascotaEdad = mascota["Edad"] == DBNull.Value ? 0 : Convert.ToInt32(mascota["Edad"]);
    
    // Manejo mejorado del peso usando PesoHelper
    decimal pesoDecimal = 0;
    string? pesoDisplayBD = null;
    
    // Priorizar PesoDisplay de la BD si existe
    if (mascota.Table.Columns.Contains("PesoDisplay") && mascota["PesoDisplay"] != DBNull.Value)
    {
        pesoDisplayBD = mascota["PesoDisplay"].ToString();
    }
    
    // Obtener el peso decimal para cálculos
    if (mascota["Peso"] != DBNull.Value && decimal.TryParse(mascota["Peso"].ToString(), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out pesoDecimal))
    {
        // ✅ Corrección global: dividir por 10 si no hay PesoDisplay y el peso parece incorrecto (>= 10)
        // Esto corrige casos como 400 -> 40.0, 300 -> 30.0, etc.
        if (string.IsNullOrEmpty(pesoDisplayBD) && pesoDecimal >= 10)
        {
            // Dividir por 10 para corregir el error de parseo
            decimal pesoCorregido = pesoDecimal / 10;
            // Validar que el peso corregido sea razonable (menor a 200kg para cualquier mascota)
            if (pesoCorregido <= 200)
            {
                pesoDecimal = pesoCorregido;
                pesoDisplayBD = PesoHelper.FormatearPeso(pesoDecimal);
            }
        }
        
        ViewBag.MascotaPeso = pesoDecimal;
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
var tema = HttpContext.Session.GetString("Tema") ?? "claro";
        ViewBag.Tema = tema;
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
                
                // Limpiar avatar de sesión al cambiar de mascota (cada mascota tiene su propio avatar)
                HttpContext.Session.Remove("MascotaAvatar");

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

    // ✅ Peso: usar PesoDisplay si está disponible, sino formatear el decimal
    string? pesoDisplayBD = null;
    if (mascota.Table.Columns.Contains("PesoDisplay") && mascota["PesoDisplay"] != DBNull.Value)
    {
        pesoDisplayBD = mascota["PesoDisplay"].ToString();
    }
    
    decimal pesoDecimal = 0;
    string peso;
    if (mascota["Peso"] != DBNull.Value && decimal.TryParse(mascota["Peso"].ToString(), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out pesoDecimal))
    {
        // ✅ Corrección global: dividir por 10 si no hay PesoDisplay y el peso parece incorrecto (>= 10)
        if (string.IsNullOrEmpty(pesoDisplayBD) && pesoDecimal >= 10)
        {
            decimal pesoCorregido = pesoDecimal / 10;
            if (pesoCorregido <= 200)
            {
                pesoDecimal = pesoCorregido;
                pesoDisplayBD = PesoHelper.FormatearPeso(pesoDecimal);
            }
        }
        
        // Usar PesoDisplay si existe, sino formatear el decimal
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
public IActionResult Perfil()
{
    var userId = HttpContext.Session.GetInt32("UserId");
    if (userId == null) return RedirectToAction("Login", "Auth");
  var tema = HttpContext.Session.GetString("Tema") ?? "claro";
        ViewBag.Tema = tema;
    // 🔍 Intentar obtener perfil
    string qPerfil = @"
        SELECT TOP 1 P.Id_Perfil, U.Nombre, U.Apellido, U.Pais, P.Descripcion, P.FotoPerfil
        FROM Perfil P 
        INNER JOIN [User] U ON P.Id_Usuario = U.Id_User
        WHERE U.Id_User = @Id";

    var dtPerfil = BD.ExecuteQuery(qPerfil, new Dictionary<string, object> { { "@Id", userId.Value } });

    // 🧩 Si no hay perfil, crear uno por defecto
    if (dtPerfil.Rows.Count == 0)
    {
        string qInsert = @"
            INSERT INTO Perfil (Id_Usuario, FotoPerfil, Descripcion, AniosVigencia)
            VALUES (@U, '/img/perfil/default.png', 'Amante de los animales ❤️', 1)";
        BD.ExecuteNonQuery(qInsert, new Dictionary<string, object> { { "@U", userId.Value } });

        // volver a consultar
        dtPerfil = BD.ExecuteQuery(qPerfil, new Dictionary<string, object> { { "@Id", userId.Value } });
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
        ? "Amante de los animales ❤️" 
        : descripcionLimpia;
    
    var fotoPerfilRaw = p["FotoPerfil"]?.ToString()?.Trim() ?? "";
    ViewBag.FotoPerfil = string.IsNullOrWhiteSpace(fotoPerfilRaw) || !fotoPerfilRaw.StartsWith("/")
        ? "/img/perfil/default.png"
        : fotoPerfilRaw;
    
    ViewBag.NombreUsuario = nombre;
    ViewBag.ApellidoUsuario = apellido;

    // 🐾 Mascotas (filtrar duplicados: misma raza y nombre exacto)
    string qMascotas = @"
        SELECT Id_Mascota, Nombre, Especie, Raza, Foto,
               ROW_NUMBER() OVER (PARTITION BY Nombre, Raza ORDER BY Id_Mascota DESC) as rn
        FROM Mascota 
        WHERE Id_User = @Id";
    var dtMascotas = BD.ExecuteQuery(qMascotas, new Dictionary<string, object> { { "@Id", userId.Value } });

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
            descripcionLimpia = "Amante de los animales ❤️";
        
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

        TempData["Exito"] = "Perfil actualizado correctamente ✅";
    }
    catch (Exception ex)
    {
        Console.WriteLine("❌ Error ActualizarPerfil: " + ex.Message);
        TempData["Error"] = "Error al actualizar el perfil. Intenta nuevamente.";
    }

    return RedirectToAction("Perfil");
}

[HttpPost]
public IActionResult CambiarTema(string modo)
{
    if (modo != "claro" && modo != "oscuro" && modo != "duelo") modo = "claro";
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
        string fotoFinal = "/img/mascotas/default.png";
        
        if (!string.IsNullOrWhiteSpace(fotoRaw))
        {
            // Asegurar que la ruta empiece con /
            fotoFinal = fotoRaw.StartsWith("/") ? fotoRaw : "/" + fotoRaw;
        }
        
        listaAct.Add(new Mascota {
            Id_Mascota = Convert.ToInt32(r["Id_Mascota"]),
            Nombre = r["Nombre"]?.ToString() ?? "Sin nombre",
            Especie = r["Especie"]?.ToString() ?? "Perro",
            Raza = r["Raza"]?.ToString() ?? "Sin raza",
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
        string fotoFinal = "/img/mascotas/default.png";
        
        if (!string.IsNullOrWhiteSpace(fotoRaw))
        {
            // Asegurar que la ruta empiece con /
            fotoFinal = fotoRaw.StartsWith("/") ? fotoRaw : "/" + fotoRaw;
        }
        
        listaArch.Add(new Mascota {
            Id_Mascota = Convert.ToInt32(r["Id_Mascota"]),
            Nombre = r["Nombre"]?.ToString() ?? "Sin nombre",
            Especie = r["Especie"]?.ToString() ?? "Perro",
            Raza = r["Raza"]?.ToString() ?? "Sin raza",
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
    var m = new Mascota {
        Id_Mascota = id,
        Nombre     = r["Nombre"].ToString(),
        Especie    = r["Especie"].ToString(),
        Raza       = r["Raza"].ToString(),
        Edad       = r["Edad"] == DBNull.Value ? 0 : Convert.ToInt32(r["Edad"]),
        Peso       = r["Peso"] == DBNull.Value ? 0 : Convert.ToDecimal(r["Peso"]),
        Sexo       = r["Sexo"].ToString(),
        Foto       = r["Foto"]?.ToString()
    };
    ViewBag.Tema = HttpContext.Session.GetString("Tema") ?? "claro";
    return View(m);
}

// 4) POST (o PUT) para cambiar color o editar datos (simplificado)
[HttpPost]
public IActionResult GuardarMascotaEditada(Mascota model)
{
    var userId = HttpContext.Session.GetInt32("UserId");
    if (userId == null)
        return RedirectToAction("Login", "Auth");

    string q = @"
        UPDATE Mascota
        SET Nombre = @Nombre,
            Raza   = @Raza,
            Edad   = @Edad,
            Peso   = @Peso,
            Sexo   = @Sexo,
            Foto   = @Foto,
            TagColor = @TagColor
        WHERE Id_Mascota = @Id AND Id_User = @U";
    BD.ExecuteNonQuery(q, new Dictionary<string,object> {
        { "@Nombre", model.Nombre },
        { "@Raza", model.Raza },
        { "@Edad", model.Edad },
        { "@Peso", model.Peso },
        { "@Sexo", model.Sexo },
        { "@Foto", model.Foto },
        { "@TagColor", model.TagColor },
        { "@Id", model.Id_Mascota },
        { "@U", userId.Value }
    });

    TempData["Exito"] = "Datos de la mascota guardados ✅";
        return RedirectToAction("ConfigMascotas");
    }

    [HttpGet]
    public IActionResult ServicioVeterinarios()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
            return RedirectToAction("Login", "Auth");

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

    var tema = HttpContext.Session.GetString("Tema") ?? "claro";
    ViewBag.Tema = tema;
    return View();
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

    [HttpGet]
    public IActionResult ObtenerAmigos()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
            return Json(new { success = false, message = "No autenticado" });

        try
        {
            // Asegurar que la tabla existe
            AsegurarTablaConfiguracionUsuario();

            // Obtener amigos del círculo de confianza (solo los que tienen solicitud aceptada)
            // Verificar que existe una invitación aceptada o están en CirculoConfianza
            // Solo mostrar ubicación según el estado de MostrableUbicacion (nadie/amigos/todos)
            // Por defecto es 'amigos' si no está configurado
            string qAmigos = @"
                SELECT DISTINCT
                    U.Id_User,
                    U.Nombre + ' ' + U.Apellido as NombreCompleto,
                    P.FotoPerfil,
                    UB.Latitud as Lat,
                    UB.Longitud as Lng,
                    (SELECT TOP 1 Nombre FROM Mascota WHERE Id_User = U.Id_User ORDER BY Id_Mascota DESC) as MascotaNombre,
                    (SELECT TOP 1 Foto FROM Mascota WHERE Id_User = U.Id_User ORDER BY Id_Mascota DESC) as MascotaFoto,
                    (SELECT TOP 1 Especie FROM Mascota WHERE Id_User = U.Id_User ORDER BY Id_Mascota DESC) as MascotaEspecie,
                    (SELECT TOP 1 Raza FROM Mascota WHERE Id_User = U.Id_User ORDER BY Id_Mascota DESC) as MascotaRaza,
                    ISNULL(CU.MostrableUbicacion, 'amigos') as MostrableUbicacion,
                    CU.Apodo as Apodo
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
                LEFT JOIN Ubicacion UB ON UB.Id_Ubicacion = U.Id_Ubicacion
                LEFT JOIN ConfiguracionUsuario CU ON CU.Id_User = U.Id_User
                WHERE U.Estado = 1
                AND (ISNULL(CU.MostrableUbicacion, 'amigos') IN ('amigos', 'todos'))
                AND UB.Latitud IS NOT NULL
                AND UB.Longitud IS NOT NULL";

        var dtAmigos = BD.ExecuteQuery(qAmigos, new Dictionary<string, object> { { "@UserId", userId.Value } });

        // Obtener apodos
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
        catch { } // Si la tabla no existe aún, continuar sin apodos

        var amigos = new List<object>();
        foreach (System.Data.DataRow row in dtAmigos.Rows)
        {
            var especie = row["MascotaEspecie"]?.ToString()?.ToLower() ?? "perro";
            var raza = row["MascotaRaza"]?.ToString() ?? "";
            var mascotaFoto = row["MascotaFoto"]?.ToString();
            int amigoId = Convert.ToInt32(row["Id_User"]);
            
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
                : (row["NombreCompleto"]?.ToString() ?? "Usuario");
            
            amigos.Add(new
            {
                id = amigoId,
                nombre = row["NombreCompleto"]?.ToString() ?? "Usuario",
                nombreMostrar = nombreMostrar,
                apodo = apodos.ContainsKey(amigoId) ? apodos[amigoId] : "",
                fotoPerfil = row["FotoPerfil"]?.ToString() ?? "/img/perfil/default.png",
                lat = row["Lat"] != DBNull.Value ? Convert.ToDouble(row["Lat"]) : (double?)null,
                lng = row["Lng"] != DBNull.Value ? Convert.ToDouble(row["Lng"]) : (double?)null,
                mascotaNombre = row["MascotaNombre"]?.ToString() ?? "Sin mascota",
                mascotaAvatar = avatarMascota
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
            usuarios.Add(new
            {
                id = Convert.ToInt32(row["Id_User"]),
                nombre = row["NombreCompleto"]?.ToString() ?? "Usuario",
                email = row["Email"]?.ToString() ?? "",
                fotoPerfil = row["FotoPerfil"]?.ToString() ?? "/img/perfil/default.png",
                mascotaNombre = row["MascotaNombre"]?.ToString() ?? "Sin mascota"
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
        string qEliminar = @"
            DELETE FROM CirculoConfianza 
            WHERE Id_User = @UserId AND Id_Amigo = @AmigoId";

        BD.ExecuteNonQuery(qEliminar, new Dictionary<string, object>
        {
            { "@UserId", userId.Value },
            { "@AmigoId", request.AmigoId }
        });

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
}
}