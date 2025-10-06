using Microsoft.AspNetCore.Mvc;

namespace Zooni.Controllers
{
    public class PerfilController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Details(int id)
        {
            return View();
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            return View();
        }

        [HttpPost]
        public IActionResult Edit()
        {
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult UpdateFoto()
        {
            return RedirectToAction("Details");
        }
    }
}
