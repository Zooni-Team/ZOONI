using Microsoft.AspNetCore.Mvc;

namespace Zooni.Controllers
{
    public class EventoController : Controller
    {
        public IActionResult Index(int mascotaId)
        {
            return View();
        }

        [HttpGet]
        public IActionResult Crear()
        {
            return View();
        }

        [HttpPost]
        public IActionResult CrearConfirmado()
        {
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            return RedirectToAction("Index");
        }
    }
}

