using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Zooni.Data;
using Zooni.Models;

namespace Zooni.Controllers
{
    public class RegistroController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RegistroController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ==========================
        // ✅ PASO 1: REGISTRO USUARIO
        // ==========================

        [HttpGet]
        public IActionResult Registro1()
        {
            return View(new User());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registro1(User model)
        {
            if (!ModelState.IsValid)
                return View(model);

            bool existe = await _context.Usuario.AnyAsync(u => u.Email == model.Email);
            if (existe)
            {
                ViewBag.Error = "Ya existe un usuario con ese correo electrónico.";
                return View(model);
            }

            model.Fecha_Registro = DateTime.Now;
            model.Estado = true;

            _context.Usuario.Add(model);
            await _context.SaveChangesAsync();

            HttpContext.Session.SetInt32("UserId", model.Id);
            HttpContext.Session.SetString("UserEmail", model.Email);

            return RedirectToAction(nameof(Registro2));
        }

        // =========================================================
        // 🟡 CREAR USUARIO RÁPIDO (para el botón "Crear cuenta")
        // =========================================================
        [HttpPost]
        public async Task<IActionResult> CrearUsuarioRapido()
        {
            var user = new User
            {
                Nombre = "Nuevo",
                Apellido = "Usuario",
                Email = $"temp_{Guid.NewGuid()}@zooni.app",
                Password = "temp",
                Fecha_Registro = DateTime.Now,
                Estado = true
            };

            _context.Usuario.Add(user);
            await _context.SaveChangesAsync();

            HttpContext.Session.SetInt32("UserId", user.Id);
            HttpContext.Session.SetString("UserEmail", user.Email);

            return Json(new { success = true, userId = user.Id });
        }

        // ==========================
        // ✅ PASO 2: REGISTRO MASCOTA
        // ==========================

        [HttpGet]
        public IActionResult Registro2()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction(nameof(Registro1));

            return View(new Mascota());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registro2(Mascota model)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction(nameof(Registro1));

            if (string.IsNullOrEmpty(model.Especie))
            {
                ViewBag.Error = "Por favor seleccioná la especie de mascota.";
                return View(model);
            }

            model.Id_User = userId.Value;
            model.Estado = true;

            _context.Mascotas.Add(model);
            await _context.SaveChangesAsync();

            HttpContext.Session.SetInt32("MascotaId", model.Id_Mascota);
            HttpContext.Session.SetString("MascotaEspecie", model.Especie);
            HttpContext.Session.SetString("MascotaNombre", model.Nombre);

            return RedirectToAction(nameof(Registro3));
        }

        // ==========================
        // ✅ PASO 3 Y SIGUIENTES
        // ==========================
        [HttpGet]
        public IActionResult Registro3()
        {
            ViewBag.MascotaEspecie = HttpContext.Session.GetString("MascotaEspecie");
            ViewBag.MascotaNombre = HttpContext.Session.GetString("MascotaNombre");
            return View();
        }

        [HttpGet]
        public IActionResult Registro4() => View();

        [HttpGet]
        public IActionResult Registro5() => View();

        [HttpGet]
        public IActionResult Registro6() => View();

        [HttpGet]
        public IActionResult Registro7() => View();

        // ==========================
        // 🔄 REINICIAR FLUJO
        // ==========================
        [HttpGet]
        public IActionResult Reiniciar()
        {
            HttpContext.Session.Clear();
            return RedirectToAction(nameof(Registro1));
        }
    }
}
