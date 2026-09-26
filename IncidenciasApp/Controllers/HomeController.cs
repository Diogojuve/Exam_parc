using Microsoft.AspNetCore.Mvc;

namespace IncidenciasApp.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Incidencias", new { area = "Operaciones" });
            }
            return RedirectToAction("Login", "Account");
        }
    }
}
