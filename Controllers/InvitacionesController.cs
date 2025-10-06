using Microsoft.AspNetCore.Mvc;

namespace Zooni.Controllers
{
    public class InvitacionesController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Crear(int mascotaId, int receptorId)
        {
            return View();
        }

        [HttpPost]
        public IActionResult CrearConfirmado()
        {
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Aceptar(int id)
        {
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Rechazar(int id)
        {
            return RedirectToAction("Index");
        }
    }
}
