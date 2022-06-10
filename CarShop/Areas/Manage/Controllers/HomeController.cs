using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarShop.Areas.Manage.Controllers
{
    [Authorize]
    public class HomeController : ManageController
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
