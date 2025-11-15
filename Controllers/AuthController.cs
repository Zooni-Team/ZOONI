using Microsoft.AspNetCore.Mvc;
using Zooni.Models;
using System;
using System.Collections.Generic;
using System.Data;

namespace Zooni.Controllers
{
    public class AuthController : Controller
    {
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
        // ============================
        // ✅ GET: /Auth/Login
        // ============================
        [HttpGet]
        [Route("Auth/Login")]
        public IActionResult Login()
        {
            return View();
        }

        // ============================
        // ✅ POST: /Auth/Login
        // ============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Auth/Login")]
        public IActionResult Login(string correo, string contrasena)
        {
            try
            {
                if (string.IsNullOrEmpty(correo) || string.IsNullOrEmpty(contrasena))
                {
                    ViewBag.Error = "Completá todos los campos.";
                    return View();
                }

                // 🔍 Buscar usuario y validar credenciales
                string query = @"
                    SELECT TOP 1 
                        U.Id_User, U.Nombre, U.Apellido, 
                        M.Correo, M.Contrasena,
                        U.Pais, U.Provincia, U.Ciudad, U.Telefono
                    FROM [User] U
                    INNER JOIN Mail M ON U.Id_Mail = M.Id_Mail
                    WHERE M.Correo = @Correo AND M.Contrasena = @Contrasena AND U.Estado = 1";

                var parametros = new Dictionary<string, object>
                {
                    { "@Correo", correo },
                    { "@Contrasena", contrasena }
                };

                DataTable dt = BD.ExecuteQuery(query, parametros);

                if (dt.Rows.Count == 0)
                {
                    ViewBag.Error = "Usuario o contraseña incorrectos.";
                    return View();
                }

                // ✅ Usuario válido → guardar sesión
                var user = dt.Rows[0];
                int userId = Convert.ToInt32(user["Id_User"]);
                HttpContext.Session.SetInt32("UserId", userId);
                HttpContext.Session.SetString("UserNombre", user["Nombre"].ToString() ?? "");
                HttpContext.Session.SetString("UserApellido", user["Apellido"].ToString() ?? "");
                HttpContext.Session.SetString("UserMail", user["Correo"].ToString() ?? "");

                // 🟢 Marcar usuario como online
                try
                {
                    AsegurarColumnasEstadoOnline();
                    string qUpdateOnline = @"
                        UPDATE [User] 
                        SET EstadoOnline = 1, UltimaActividad = GETDATE()
                        WHERE Id_User = @UserId";
                    BD.ExecuteNonQuery(qUpdateOnline, new Dictionary<string, object> { { "@UserId", userId } });
                }
                catch (Exception ex)
                {
                    Console.WriteLine("⚠️ Error al actualizar estado online: " + ex.Message);
                }

return RedirectToAction("Index", "Home");            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Error en Auth/Login POST: " + ex.Message);
                ViewBag.Error = "Error al iniciar sesión. Intentalo de nuevo.";
                return View();
            }
        }

        // ============================
        // ✅ GET: /Auth/Logout
        // ============================
        [HttpGet]
        [Route("Auth/Logout")]
        public IActionResult Logout()
        {
            // 🔴 Marcar usuario como offline antes de limpiar la sesión
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId != null)
            {
                try
                {
                    AsegurarColumnasEstadoOnline();
                    string qUpdateOffline = @"
                        UPDATE [User] 
                        SET EstadoOnline = 0, UltimaActividad = GETDATE()
                        WHERE Id_User = @UserId";
                    BD.ExecuteNonQuery(qUpdateOffline, new Dictionary<string, object> { { "@UserId", userId.Value } });
                }
                catch (Exception ex)
                {
                    Console.WriteLine("⚠️ Error al actualizar estado offline: " + ex.Message);
                }
            }

            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
