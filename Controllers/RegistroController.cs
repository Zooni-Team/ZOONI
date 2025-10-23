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
            ViewBag.MascotaNombre = HttpContext.Session.GetString("MascotaNombre");
            ViewBag.MascotaEspecie = HttpContext.Session.GetString("MascotaEspecie");

            if (ViewBag.MascotaNombre == null)
                return RedirectToAction("Registro1");

            return View();
        }
        
    }
}
