using Microsoft.AspNetCore.Mvc;

namespace Zooni.Controllers
{
    public class CuidadoController : Controller
    {
        public IActionResult Index(int mascotaId)
        {
            return View();
        }

        [HttpGet]
        public IActionResult Create(int mascotaId)
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create()
        {
            return RedirectToAction("Index");
        }

        public IActionResult Details(int id)
        {
            return View();
        }
    }
}
