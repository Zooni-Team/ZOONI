using Microsoft.AspNetCore.Mvc;
using Zooni.Models;
using System;
using System.Collections.Generic;
using System.Data;

namespace Zooni.Controllers
{
    public class AuthController : Controller
    {
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
                HttpContext.Session.SetInt32("UserId", Convert.ToInt32(user["Id_User"]));
                HttpContext.Session.SetString("UserNombre", user["Nombre"].ToString() ?? "");
                HttpContext.Session.SetString("UserApellido", user["Apellido"].ToString() ?? "");
                HttpContext.Session.SetString("UserMail", user["Correo"].ToString() ?? "");

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
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
