using CarShop.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopData;

namespace CarShop.Controllers
{
    public class AutoMakerController : Controller
    {
        private readonly Context context;
        public AutoMakerController(Context _context)
        {
            context = _context;
        }
        public IActionResult Index(string? id)
        {
            if (id == null)
            {
                return RedirectToAction("Index", "Home");
            }
            var maker = context.AutoMakers.Where(e => e.SeoAlias == id && e.Status == true).FirstOrDefault();
            if (maker == null)
            {
                return NotFound($"Không tìm thấy Xe với ID '{id}'.");
            }
            var categories = context.Categories.Where(e=>e.Status == true).ToList();
            var CarId = context.CarAutoMakers.Where(e => e.AutoMakerId == maker.Id).Select(e => e.CarId).ToList();
            var model = new AutoMakerModel() { 
                AutoMaker = maker, 
                Categories = categories,
                Cars = context.Cars.Where(e=>CarId.Contains(e.Id) && e.Status==true).Include(e=>e.CarCategory).Take(12).ToList() };
            return View(model);
        }
    }
}
