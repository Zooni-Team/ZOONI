using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Zooni.Data;
using Zooni.Models;

namespace Zooni.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ========================
        // ✅ REGISTRO DE USUARIO
        // ========================
        [HttpGet]
        public IActionResult Register()
        {
            return View(new User());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(User model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Verificar si ya existe el usuario
            var existingUser = await _context.Usuario
                .FirstOrDefaultAsync(u => u.Email == model.Email);

            if (existingUser != null)
            {
                ViewBag.Error = "Ya existe un usuario con ese correo electrónico.";
                return View(model);
            }

            // Guardar usuario
            model.Fecha_Registro = DateTime.Now;
            model.Estado = true;

            _context.Usuario.Add(model);
            await _context.SaveChangesAsync();

            // Crear sesión
            HttpContext.Session.SetString("UserEmail", model.Email);
            HttpContext.Session.SetInt32("UserId", model.Id);

            // Redirige al paso 2 del flujo de registro (mascota)
            return RedirectToAction("Registro2", "Registro");
        }

        // ========================
        // ✅ LOGIN DE USUARIO
        // ========================
        [HttpGet]
        public IActionResult Login()
        {
            return View(new User());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Por favor ingresá tus credenciales.";
                return View();
            }

            var user = await _context.Usuario
                .FirstOrDefaultAsync(u => u.Email == email && u.Password == password);

            if (user == null)
            {
                ViewBag.Error = "Correo o contraseña incorrectos.";
                return View();
            }

            // Crear sesión
            HttpContext.Session.SetString("UserEmail", user.Email);
            HttpContext.Session.SetInt32("UserId", user.Id);

            // Si el usuario tiene mascota → ir al perfil, sino → crear mascota
            var tieneMascota = await _context.Mascotas.AnyAsync(m => m.Id_User == user.Id);
            if (tieneMascota)
                return RedirectToAction("Perfil");
            else
                return RedirectToAction("Registro2", "Registro");
        }

        // ========================
        // ✅ LOGOUT
        // ========================
        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        // ========================
        // ✅ PERFIL DE USUARIO
        // ========================
        [HttpGet]
        public async Task<IActionResult> Perfil()
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
                return RedirectToAction("Login");

            var usuario = await _context.Usuario
                .Include(u => u.Perfil)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (usuario == null)
                return RedirectToAction("Login");

            return View(usuario);
        }

        // ========================
        // ✅ ACTUALIZAR PERFIL
        // ========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ActualizarPerfil(Perfil perfil)
        {
            if (!ModelState.IsValid)
                return View("Perfil", perfil);

            _context.Perfiles.Update(perfil);
            await _context.SaveChangesAsync();

            TempData["Mensaje"] = "Perfil actualizado correctamente.";
            return RedirectToAction("Perfil");
        }
    }
}
