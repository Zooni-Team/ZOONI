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

        // ============================
        // ✅ PASO 1: REGISTRO DE USUARIO
        // ============================

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

            // Verificar si ya existe un usuario con ese email
            bool existe = await _context.Usuario.AnyAsync(u => u.Email == model.Email);
            if (existe)
            {
                ViewBag.Error = "Ya existe un usuario con ese correo electrónico.";
                return View(model);
            }

            // Guardar usuario nuevo
            model.Fecha_Registro = DateTime.Now;
            model.Estado = true;

            _context.Usuario.Add(model);
            await _context.SaveChangesAsync();

            // Guardar sesión para los próximos pasos
            HttpContext.Session.SetInt32("UserId", model.Id);
            HttpContext.Session.SetString("UserEmail", model.Email);

            // Redirigir al paso 2 (mascota)
            return RedirectToAction(nameof(Registro2));
        }

        // ============================
        // ✅ PASO 2: REGISTRO DE MASCOTA
        // ============================

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
        public async Task<IActionResult> Registro2(Mascota mascota)
        {
            if (!ModelState.IsValid)
                return View(mascota);

            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction(nameof(Registro1));

            mascota.Id_User = userId.Value;

            _context.Mascotas.Add(mascota);
            await _context.SaveChangesAsync();

            // Paso siguiente: confirmación o configuración inicial
            return RedirectToAction(nameof(Registro3));
        }

        // ============================
        // ✅ RESTO DEL FLUJO DE REGISTRO
        // ============================

        [HttpGet]
        public IActionResult Registro3()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Registro4()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Registro5()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Registro6()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Registro7()
        {
            return View();
        }

        // ============================
        // ✅ MÉTODO AUXILIAR (opcional)
        // ============================
        // Permite limpiar sesión si se reinicia el registro
        [HttpGet]
        public IActionResult Reiniciar()
        {
            HttpContext.Session.Clear();
            return RedirectToAction(nameof(Registro1));
        }
    }
}
