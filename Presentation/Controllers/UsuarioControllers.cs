using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers
{
    public class UsuarioControllers : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
