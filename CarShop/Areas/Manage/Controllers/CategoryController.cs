using CarShop.Areas.Manage.Models;
using CarShop.Services.GenerateUrlSlug;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using ShopData;
using ShopData.Model;
using System.Text;
using System.Text.RegularExpressions;
using X.PagedList;

namespace CarShop.Areas.Manage.Controllers
{
    public class CategoryController : ManageController
    {
        private readonly ILogger<AutoMakerController> logger;
        private readonly Context context;
        private readonly UserManager<User> userManage;
        public CategoryController(ILogger<AutoMakerController> _logger, Context _context, UserManager<User> _userManage)
        {
            logger = _logger;
            context = _context;
            userManage = _userManage;
        }

        public IActionResult Index(int page = 1, int pageSize = 10)
        {
            List<CategoryModel> model = new List<CategoryModel>();
            context.Categories
                .Include(e => e.Author)
                .OrderByDescending(e=>e.DateCreated).ToList().ForEach(e => {
                var category = new CategoryModel
                {
                    Id = e.Id,
                    Name = e.Name,
                    Author = e.Author,
                    Status = e.Status
                };
                if(e.DateModified == null)
                {
                    category.DateModified = e.DateCreated.Value.ToString("dd/MM/yyyy");
                }
                else
                {
                    category.DateModified = e.DateModified.Value.ToString("dd/MM/yyyy");
                }
                model.Add(category);
            });
            return View(model.ToPagedList(page, pageSize));
        }

        [HttpPost]
        public async Task<IActionResult> SwitchStatus(string? Id)
        {
            if (Id == null)
            {
                return RedirectToAction("Index", "Home");
            }
            var category = context.Categories.Where(e=>e.Id == Id).FirstOrDefault();
            if (category == null)
            {
                return NotFound($"Không tìm thấy Thể loại với ID '{Id}'.");
            }
            category.Status = !category.Status;
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
        
        [HttpPost]
        public async Task<IActionResult> Delete(string? Id)
        {
            if (Id == null)
            {
                return RedirectToAction("Index", "Home");
            }
            var category = context.Categories.Where(e=>e.Id == Id).FirstOrDefault();
            if (category == null)
            {
                return NotFound($"Không tìm thấy Thể loại với ID '{Id}'.");
            }
            if (category.CoverImage != null)
            {
                System.IO.File.Delete(Path.Combine(@"wwwroot/", category.CoverImage));
            }
            context.Remove(category);
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

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CategoryModel model)
        {
            ModelState.Remove("Author");
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError(string.Empty, "Lỗi nhập liệu");
                return View(model);
            }
            else
            {
                var category = Activator.CreateInstance<Category>();
                category.Author = await userManage.GetUserAsync(User);
                category.AuthorId = category.Author.Id;
                category.Name = model.Name;
                category.Status = model.Status;
                category.Id = UrlSlug.GenerateSlug(model.Name, false);

                var listCateId = context.Categories.Where(e => e.Id.Contains(category.Id)).ToList();
                if (listCateId.Count > 0)
                {
                    category.Id += "-" + listCateId.Count;
                }
                if (model.CoverImage != null)
                {
                    var folder = string.Format(@"wwwroot/files/coverImage/");
                    if (!Directory.Exists(folder))
                    {
                        Directory.CreateDirectory(folder);
                    }
                    string fileName = Guid.NewGuid() + Path.GetExtension(model.CoverImage.FileName);
                    var filePath = Path.Combine(folder, fileName);

                    using (var stream = System.IO.File.Create(filePath))
                    {
                        await model.CoverImage.CopyToAsync(stream);
                    }
                    category.CoverImage = filePath.Substring(8);
                }
                context.Add(category);
                var res = context.SaveChanges();
                List<SystemMessage> SystemMessage = new List<SystemMessage>();

                if (res > 0)
                {
                    SystemMessage.Add(new SystemMessage() { Title = "SystemMessageSuccess", Message = "Thêm thành công" });
                }
                else
                {
                    SystemMessage.Add(new SystemMessage() { Title = "SystemMessageError", Message = "Thêm thất bại" });
                }
                TempData["SystemMessage"] = JsonConvert.SerializeObject(SystemMessage);
                return RedirectToAction("Index");

            }

        }
        public async Task<IActionResult> Update(string? Id)
        {
            if (Id == null)
            {
                return RedirectToAction("Index", "Home");
            }
            var category = context.Categories.Where(e => e.Id == Id)
                .Include(e => e.Author)
                .FirstOrDefault();
            if (category == null)
            {
                return NotFound($"Không tìm thấy Thể loại với ID '{Id}'.");
            }
            CategoryModel model = new CategoryModel()
            {
                Id = category.Id,
                Author = category.Author,
                AuthorId = category.AuthorId,
                Name = category.Name,
                Status = category.Status,
                Image = category.CoverImage,
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Update(CategoryModel model)
        {
            ModelState.Remove("Author");
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError(string.Empty, "Lỗi nhập liệu");
                return View(model);
            }
            else
            {
                var category = context.Categories.Where(e => e.Id == model.Id)
                    .Include(e => e.Author)
                    .FirstOrDefault();
                if (category == null)
                {
                    return NotFound($"Không tìm thấy thể loại với ID '{model.Id}'.");
                }
                category.Author = await userManage.GetUserAsync(User);
                category.AuthorId = category.Author.Id;
                category.Name = model.Name;
                category.Status = model.Status;
                if (model.CoverImage != null)
                {
                    if (category.CoverImage != null)
                    {
                        System.IO.File.Delete(Path.Combine(@"wwwroot/", category.CoverImage));
                    }
                    var folder = string.Format(@"wwwroot/files/coverImage/");
                    if (!Directory.Exists(folder))
                    {
                        Directory.CreateDirectory(folder);
                    }
                    string fileName = Guid.NewGuid() + Path.GetExtension(model.CoverImage.FileName);
                    var filePath = Path.Combine(folder, fileName);

                    using (var stream = System.IO.File.Create(filePath))
                    {
                        await model.CoverImage.CopyToAsync(stream);
                    }
                    category.CoverImage = filePath.Substring(8);
                }
                context.Update(category);
                var res = context.SaveChanges();
                List<SystemMessage> SystemMessage = new List<SystemMessage>();

                if (res > 0)
                {
                    SystemMessage.Add(new SystemMessage() { Title = "SystemMessageSuccess", Message = "Cập nhật thành công" });
                }
                else
                {
                    SystemMessage.Add(new SystemMessage() { Title = "SystemMessageError", Message = "Cập nhật thất bại" });
                }
                TempData["SystemMessage"] = JsonConvert.SerializeObject(SystemMessage);
                return RedirectToAction("Index");
            }
        }
    }
}
