using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace CarShop.Areas.Manage.Controllers
{
    [Area("Manage")]
    [Authorize]
    public class ManageController : Controller
    {
        
    }
}
