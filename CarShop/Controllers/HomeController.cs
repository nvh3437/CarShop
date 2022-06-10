using CarShop.Models;
using CarShop.Services.Email;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopData;
using ShopData.Model;
using System.Diagnostics;

namespace CarShop.Controllers
{
    public class HomeController : Controller
    {
        private readonly Context context;
        public HomeController(Context _context)
        {
            context = _context;
        }


        public IActionResult Index()
        {
            var maker = context.AutoMakers.Where(e => e.Status == true).ToList();
            var car = context.Cars.Include(e=>e.CarAutoMaker).Where(e => e.Status == true).Take(12).ToList();
            var model = new HomeModel() { AutoMakers = maker, Cars = car };
            return View(model);
        }

        public IActionResult Tracking(string find)
        {
            var model = context.Bills.Where(e=>e.Email == find || e.Phone == find || e.Id == find).ToList();
            if (model.Count <= 0)
                model = null;
            return View(model);
        }

        public IActionResult Contact()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Contact(ContactModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var feedBack = Activator.CreateInstance<FeedBack>();
            feedBack.Email = model.Email;
            feedBack.Phone = model.Phone;
            feedBack.Name = model.Name;
            feedBack.Content = model.Content;
            feedBack.Status = false;
            context.Add(feedBack);
            var res = context.SaveChanges();
            if(res > 0)
            {
                ModelState.AddModelError(string.Empty, "Đã ghi nhận phản hồi từ bạn. Chúng tôi sẽ liên hệ sớm nhất có thể");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Có lỗi xảy ra");
            }
            return View();
        }
        
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}