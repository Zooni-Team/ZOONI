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
    }
}