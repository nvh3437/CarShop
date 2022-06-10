using Microsoft.AspNetCore.Mvc;

namespace CarShop.Controllers
{
    public class DeniedController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
