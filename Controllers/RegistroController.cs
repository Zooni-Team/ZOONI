using Microsoft.AspNetCore.Mvc;
using Zooni.Models;
using System;
using System.Collections.Generic;

namespace Zooni.Controllers
{
    public class RegistroController : Controller
    {
        // =============================
        // PASO 1 - REGISTRO USUARIO
        // =============================
        [HttpGet]
        public IActionResult Registro1()
        {
            return View();
        }

        [HttpPost]
        public IActionResult CrearUsuarioRapido(string correo, string contrasena)
        {
            try
            {
                // 1️⃣ Crear registro en Mail
                string queryMail = @"
                    INSERT INTO Mail (Correo, Contrasena, Fecha_Creacion)
                    VALUES (@Correo, @Contrasena, SYSDATETIME());
                    SELECT SCOPE_IDENTITY();";

                var mailParams = new Dictionary<string, object>
                {
                    { "@Correo", correo ?? $"temp_{Guid.NewGuid()}@zooni.app" },
                    { "@Contrasena", contrasena ?? "temp123" }
                };

                int idMail = Convert.ToInt32(BD.ExecuteScalar(queryMail, mailParams));

                // 2️⃣ Crear registro en User vinculado
                string queryUser = @"
                    INSERT INTO [User] (Id_Mail, Nombre, Apellido, Fecha_Registro, Id_Ubicacion, Id_TipoUsuario)
                    VALUES (@Id_Mail, 'Nuevo', 'Usuario', SYSDATETIME(), 1, 1);
                    SELECT SCOPE_IDENTITY();";

                var userParams = new Dictionary<string, object>
                {
                    { "@Id_Mail", idMail }
                };

                int idUser = Convert.ToInt32(BD.ExecuteScalar(queryUser, userParams));

                // Guardar sesión
                HttpContext.Session.SetInt32("UserId", idUser);

                return Json(new { success = true, idUser });
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Error al crear usuario rápido: " + ex.Message);
                return Json(new { success = false, message = "Error al conectar con la base de datos." });
            }
        }

        // =============================
        // PASO 2 - REGISTRO MASCOTA
        // =============================
        [HttpGet]
        public IActionResult Registro2()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                TempData["Error"] = "Primero registrá un usuario.";
                return RedirectToAction("Registro1");
            }

            return View(new Mascota());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Registro2(Mascota model)
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("UserId");
                if (userId == null)
                {
                    TempData["Error"] = "No hay usuario en sesión.";
                    return RedirectToAction("Registro1");
                }

                if (string.IsNullOrWhiteSpace(model.Especie))
                {
                    ViewBag.Error = "Seleccioná una especie antes de continuar.";
                    return View(model);
                }

                // ✅ Inserción sin la columna Estado
                string query = @"
                    INSERT INTO Mascota (Id_User, Nombre, Especie, Edad, Raza, Sexo, Peso, Color, Chip, Foto, Esterilizado, Fecha_Nacimiento)
                    VALUES (@Id_User, @Nombre, @Especie, @Edad, @Raza, @Sexo, @Peso, @Color, @Chip, @Foto, @Esterilizado, SYSDATETIME());
                    SELECT SCOPE_IDENTITY();";

                var parametros = new Dictionary<string, object>
                {
                    { "@Id_User", userId.Value },
                    { "@Nombre", model.Nombre ?? "MiMascota" },
                    { "@Especie", model.Especie },
                    { "@Edad", model.Edad },
                    { "@Raza", model.Raza ?? "" },
                    { "@Sexo", model.Sexo ?? "" },
                    { "@Peso", model.Peso },
                    { "@Color", model.Color ?? "" },
                    { "@Chip", model.Chip ?? "" },
                    { "@Foto", model.Foto ?? "" },
                    { "@Esterilizado", model.Esterilizado }
                };

                var idMascotaObj = BD.ExecuteScalar(query, parametros);

                if (idMascotaObj == null)
                {
                    ViewBag.Error = "Error al registrar la mascota.";
                    return View(model);
                }

                int idMascota = Convert.ToInt32(idMascotaObj);

                // Guardar en sesión
                HttpContext.Session.SetInt32("MascotaId", idMascota);
                HttpContext.Session.SetString("MascotaNombre", model.Nombre ?? "MiMascota");
                HttpContext.Session.SetString("MascotaEspecie", model.Especie ?? "Desconocida");

                return RedirectToAction("Registro3");
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Error en Registro2 POST: " + ex.Message);
                ViewBag.Error = "Ocurrió un error al registrar la mascota.";
                return View(model);
            }
        }

        // =============================
        // PASO 3 - CONFIRMACIÓN
        // =============================
        [HttpGet]
public IActionResult Registro3()
{
    var userId = HttpContext.Session.GetInt32("UserId");
    var mascotaNombre = HttpContext.Session.GetString("MascotaNombre");
    var mascotaEspecie = HttpContext.Session.GetString("MascotaEspecie");

    if (userId == null || string.IsNullOrEmpty(mascotaNombre) || string.IsNullOrEmpty(mascotaEspecie))
    {
        TempData["Error"] = "Faltan datos del registro anterior.";
        return RedirectToAction("Registro1");
    }

    ViewBag.MascotaNombre = mascotaNombre;
    ViewBag.MascotaEspecie = mascotaEspecie;

    return View(new Mascota());
}

[HttpPost]
[ValidateAntiForgeryToken]
public IActionResult Registro3(Mascota model)
{
    try
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        var mascotaId = HttpContext.Session.GetInt32("MascotaId");

        if (userId == null || mascotaId == null)
        {
            TempData["Error"] = "No hay usuario o mascota activa.";
            return RedirectToAction("Registro1");
        }

        if (string.IsNullOrEmpty(model.Raza) || string.IsNullOrEmpty(model.Color) || string.IsNullOrEmpty(model.Sexo))
        {
            TempData["Error"] = "Completá todos los datos antes de continuar.";
            return RedirectToAction("Registro3");
        }

        // ✅ Actualizamos los datos de la mascota
        string query = @"
            UPDATE Mascota
            SET Sexo = @Sexo,
                Raza = @Raza,
                Color = @Color,
                Edad = @Edad
            WHERE Id_Mascota = @Id_Mascota";

        var parametros = new Dictionary<string, object>
        {
            { "@Sexo", model.Sexo ?? "" },
            { "@Raza", model.Raza ?? "" },
            { "@Color", model.Color ?? "" },
            { "@Edad", model.Edad },
            { "@Id_Mascota", mascotaId.Value }
        };

        BD.ExecuteNonQuery(query, parametros);

        // ✅ Guardamos los datos en sesión
        HttpContext.Session.SetString("MascotaSexo", model.Sexo ?? "");
        HttpContext.Session.SetString("MascotaRaza", model.Raza ?? "");
        HttpContext.Session.SetString("MascotaColor", model.Color ?? "");
        HttpContext.Session.SetInt32("MascotaEdad", model.Edad);

        // 🔥 Vamos al paso 4
        return RedirectToAction("Registro4", "Registro");
    }
    catch (Exception ex)
    {
        Console.WriteLine("❌ Error en Registro3 POST: " + ex.Message);
        TempData["Error"] = "Error al guardar los datos de la mascota.";
        return RedirectToAction("Registro3");
    }
}

[HttpGet]
public IActionResult Registro4()
{
    var mascotaNombre = HttpContext.Session.GetString("MascotaNombre");
    var mascotaEspecie = HttpContext.Session.GetString("MascotaEspecie");
    var mascotaRaza = HttpContext.Session.GetString("MascotaRaza");
    var mascotaColor = HttpContext.Session.GetString("MascotaColor");
    var mascotaSexo = HttpContext.Session.GetString("MascotaSexo");
    var mascotaEdad = HttpContext.Session.GetInt32("MascotaEdad");

    if (string.IsNullOrEmpty(mascotaNombre) || string.IsNullOrEmpty(mascotaEspecie))
    {
        TempData["Error"] = "Faltan datos de la mascota.";
        return RedirectToAction("Registro3");
    }

    // 🔹 Enviamos todo a la vista para mostrar resumen
    ViewBag.MascotaNombre = mascotaNombre;
    ViewBag.MascotaEspecie = mascotaEspecie;
    ViewBag.MascotaRaza = mascotaRaza;
    ViewBag.MascotaColor = mascotaColor;
    ViewBag.MascotaSexo = mascotaSexo;
    ViewBag.MascotaEdad = mascotaEdad;

    return View();
}

[HttpPost]
[ValidateAntiForgeryToken]
public IActionResult Registro4(string nombre, string apellido, string email, string contrasena)
{
    try
    {
        var userId = HttpContext.Session.GetInt32("UserId");

        if (userId == null)
        {
            TempData["Error"] = "No se encontró el usuario.";
            return RedirectToAction("Registro1");
        }

        // ✅ Actualizamos los datos del usuario
        string query = @"
            UPDATE [User]
            SET Nombre = @Nombre,
                Apellido = @Apellido,
                Email = @Email,
                Fecha_Registro = SYSDATETIME()
            WHERE Id_User = @Id_User";

        var parametros = new Dictionary<string, object>
        {
            { "@Nombre", nombre },
            { "@Apellido", apellido },
            { "@Email", email },
            { "@Id_User", userId.Value }
        };

        BD.ExecuteNonQuery(query, parametros);

        // ✅ Actualizamos contraseña del mail asociado
        string mailQuery = @"
            UPDATE Mail
            SET Contrasena = @Contrasena
            WHERE Id_Mail = (SELECT Id_Mail FROM [User] WHERE Id_User = @Id_User)";

        BD.ExecuteNonQuery(mailQuery, new Dictionary<string, object>
        {
            { "@Contrasena", contrasena },
            { "@Id_User", userId.Value }
        });

        // 🔥 Registro completo → paso final
        return RedirectToAction("RegistroFinal");
    }
    catch (Exception ex)
    {
        Console.WriteLine("❌ Error en Registro4 POST: " + ex.Message);
        TempData["Error"] = "Error al completar el registro.";
        return RedirectToAction("Registro4");
    }
}

}}
