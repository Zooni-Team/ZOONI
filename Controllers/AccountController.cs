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

        // GET: /Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        public async Task<IActionResult> Register(User model)
        {
            if (ModelState.IsValid)
            {
                var existingUser = await _context.Usuario
                    .FirstOrDefaultAsync(u => u.Email == model.Email);

                if (existingUser != null)
                {
                    ViewBag.Error = "Ya existe un usuario con ese correo.";
                    return View(model);
                }

                _context.Usuario.Add(model);
                await _context.SaveChangesAsync();

                HttpContext.Session.SetString("UserEmail", model.Email);
                HttpContext.Session.SetInt32("UserId", model.Id);

                return RedirectToAction("Index", "Home");
            }

            return View(model);
        }

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            var user = await _context.Usuario
                .FirstOrDefaultAsync(u => u.Email == email && u.Password == password);

            if (user != null)
            {
                HttpContext.Session.SetString("UserEmail", user.Email);
                HttpContext.Session.SetInt32("UserId", user.Id);
                return RedirectToAction("Index", "Home");
            }

            ViewBag.Error = "Correo o contraseña incorrectos.";
            return View();
        }

        // GET: /Account/Logout
        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        // GET: /Account/Perfil
        [HttpGet]
        public async Task<IActionResult> Perfil()
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return RedirectToAction("Login");
            }

            var usuario = await _context.Usuario
                .Include(u => u.Perfil)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (usuario == null)
            {
                return RedirectToAction("Login");
            }

            return View(usuario);
        }

        // POST: /Account/ActualizarPerfil
        [HttpPost]
        public async Task<IActionResult> ActualizarPerfil(Perfil perfil)
        {
            if (ModelState.IsValid)
            {
                _context.Perfiles.Update(perfil);
                await _context.SaveChangesAsync();
                return RedirectToAction("Perfil");
            }

            return View("Perfil", perfil);
        }
    }
}
