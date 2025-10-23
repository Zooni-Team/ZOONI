using Microsoft.AspNetCore.Mvc;
using Zooni.Models;
using System;
using System.Collections.Generic;
using System.Data;

namespace Zooni.Controllers
{
    public class AccountController : Controller
    {
        private int EnsureTipoUsuario()
        {
            var result = BD.ExecuteScalar(
                "SELECT ISNULL((SELECT TOP 1 Id_TipoUsuario FROM TipoUsuario WHERE Descripcion = 'Usuario'), 0)"
            );

            if (Convert.ToInt32(result) > 0)
                return Convert.ToInt32(result);

            return Convert.ToInt32(BD.ExecuteScalar(
                "INSERT INTO TipoUsuario (Descripcion) VALUES ('Usuario'); SELECT SCOPE_IDENTITY();"
            ));
        }

        private int EnsureUbicacionDefault()
        {
            var result = BD.ExecuteScalar(
                "SELECT ISNULL((SELECT TOP 1 Id_Ubicacion FROM Ubicacion WHERE Tipo = 'Default'), 0)"
            );

            if (Convert.ToInt32(result) > 0)
                return Convert.ToInt32(result);

            return Convert.ToInt32(BD.ExecuteScalar(@"
                INSERT INTO Ubicacion (Latitud, Longitud, Direccion, Tipo)
                VALUES (0, 0, 'Sin especificar', 'Default');
                SELECT SCOPE_IDENTITY();"
            ));
        }

        // ===============================================
        // ✅ Registro de usuario
        // ===============================================
        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        public IActionResult Register(string correo, string contrasena)
        {
            if (string.IsNullOrWhiteSpace(correo) || string.IsNullOrWhiteSpace(contrasena))
            {
                ViewBag.Error = "Por favor completá correo y contraseña.";
                return View();
            }

            try
            {
                int existe = Convert.ToInt32(BD.ExecuteScalar(
                    "SELECT COUNT(*) FROM Mail WHERE Correo = @Correo",
                    new() { { "@Correo", correo } }
                ));

                if (existe > 0)
                {
                    ViewBag.Error = "Ese correo ya está registrado.";
                    return View();
                }

                // Crear Mail
                int idMail = Convert.ToInt32(BD.ExecuteScalar(@"
                    INSERT INTO Mail (Correo, Contrasena, Fecha_Creacion)
                    VALUES (@Correo, @Contrasena, SYSDATETIME());
                    SELECT SCOPE_IDENTITY();",
                    new() { { "@Correo", correo }, { "@Contrasena", contrasena } }
                ));

                // Crear User
                int idTipoUsuario = EnsureTipoUsuario();
                int idUbicacion = EnsureUbicacionDefault();

                int idUser = Convert.ToInt32(BD.ExecuteScalar(@"
                    INSERT INTO [User] (Id_Mail, Nombre, Apellido, Fecha_Registro, Estado, Id_TipoUsuario, Id_Ubicacion)
                    VALUES (@Id_Mail, 'Nuevo', 'Usuario', SYSDATETIME(), 1, @Id_TipoUsuario, @Id_Ubicacion);
                    SELECT SCOPE_IDENTITY();",
                    new() { { "@Id_Mail", idMail }, { "@Id_TipoUsuario", idTipoUsuario }, { "@Id_Ubicacion", idUbicacion } }
                ));

                HttpContext.Session.SetInt32("UserId", idUser);
                HttpContext.Session.SetString("UserEmail", correo);

                return RedirectToAction("Registro2", "Registro");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error en Register: {ex.Message}");
                ViewBag.Error = "Error al crear usuario.";
                return View();
            }
        }

        // ===============================================
        // ✅ Login
        // ===============================================
        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public IActionResult Login(string correo, string contrasena)
        {
            try
            {
                var dt = BD.ExecuteQuery(@"
                    SELECT TOP 1 U.Id_User, M.Correo
                    FROM Mail M
                    JOIN [User] U ON M.Id_Mail = U.Id_Mail
                    WHERE M.Correo = @Correo AND M.Contrasena = @Contrasena AND U.Estado = 1;",
                    new() { { "@Correo", correo }, { "@Contrasena", contrasena } }
                );

                if (dt.Rows.Count == 0)
                {
                    ViewBag.Error = "Usuario o contraseña incorrectos.";
                    return View();
                }

                int userId = Convert.ToInt32(dt.Rows[0]["Id_User"]);
                HttpContext.Session.SetInt32("UserId", userId);
                HttpContext.Session.SetString("UserEmail", correo);

                return RedirectToAction("Registro2", "Registro");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error en Login: {ex.Message}");
                ViewBag.Error = "No se pudo conectar con la base de datos.";
                return View();
            }
        }

        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
