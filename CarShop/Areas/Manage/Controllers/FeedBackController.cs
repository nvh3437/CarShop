using CarShop.Areas.Manage.Models;
using CarShop.Services.GenerateUrlSlug;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using ShopData;
using ShopData.Enum;
using ShopData.Model;
using System.Text;
using System.Text.RegularExpressions;
using X.PagedList;

namespace CarShop.Areas.Manage.Controllers
{
    public class FeedBackController : ManageController
    {
        private readonly Context context;
        private readonly UserManager<User> userManage;
        public FeedBackController(Context _context, UserManager<User> _userManage)
        {
            context = _context;
            userManage = _userManage;
        }

        public IActionResult Index(int page = 1, int pageSize = 10)
        {
            List<FeedBack> model = context.FeedBacks.OrderByDescending(e=>e.DateCreated).ToList();
            return View(model.ToPagedList(page, pageSize));
        }

        [HttpPost]
        public async Task<IActionResult> SwitchStatus(int? Id)
        {
            if (Id == null)
            {
                return RedirectToAction("Index", "Home");
            }
            var feed = context.FeedBacks.Where(e => e.Id == Id).FirstOrDefault();
            if (feed == null)
            {
                return NotFound($"Không tìm thấy Phản hồi với ID '{Id}'.");
            }
            feed.Status = !feed.Status;
            context.Update(feed);
            var res = await context.SaveChangesAsync();
            var SystemMessage = new List<SystemMessage>();
            if (res > 0)
            {
                SystemMessage.Add(new SystemMessage() { Title = "SystemMessageSuccess", Message = "Chuyển trạng thái thành công" });
            }
            else
            {
                SystemMessage.Add(new SystemMessage() { Title = "SystemMessageError", Message = "Chuyển trạng thái thất bại" });
            }
            TempData["SystemMessage"] = JsonConvert.SerializeObject(SystemMessage);
            return RedirectToAction("Index");
        }
        
        public async Task<IActionResult> Detail(int? Id)
        {
            if (Id == null)
            {
                return RedirectToAction("Index", "Home");
            }
            var feed = context.FeedBacks.Where(e => e.Id == Id).FirstOrDefault();
            if (feed == null)
            {
                return NotFound($"Không tìm thấy Phản hồi với ID '{Id}'.");
            }
            return View(feed);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int? Id)
        {
            if (Id == null)
            {
                return RedirectToAction("Index", "Home");
            }
            var feed = context.FeedBacks.Where(e => e.Id == Id).FirstOrDefault();
            if (feed == null)
            {
                return NotFound($"Không tìm thấy Phản hồi với ID '{Id}'.");
            }
            context.Remove(feed);
            var res = await context.SaveChangesAsync();
            var SystemMessage = new List<SystemMessage>();
            if (res > 0)
            {
                SystemMessage.Add(new SystemMessage() { Title = "SystemMessageSuccess", Message = "Xóa thành công" });
            }
            else
            {
                SystemMessage.Add(new SystemMessage() { Title = "SystemMessageError", Message = "Xóa thất bại" });
            }
            TempData["SystemMessage"] = JsonConvert.SerializeObject(SystemMessage);
            return RedirectToAction("Index");
        }

    }
}
