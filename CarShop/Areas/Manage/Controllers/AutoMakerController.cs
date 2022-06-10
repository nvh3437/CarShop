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
    public class AutoMakerController : ManageController
    {
        private readonly ILogger<AutoMakerController> logger;
        private readonly Context context;
        private readonly UserManager<User> userManage;
        public AutoMakerController(ILogger<AutoMakerController> _logger, Context _context, UserManager<User> _userManage)
        {
            logger = _logger;
            context = _context;
            userManage = _userManage;
        }

        public IActionResult Index(int page = 1, int pageSize = 10)
        {
            List<Models.AutoMaker> model = new List<Models.AutoMaker>();
            context.AutoMakers.Include(e => e.Author).OrderByDescending(e=>e.DateCreated).ToList().ForEach(e => {
                var maker = new Models.AutoMaker
                {
                    Author = e.Author,
                    //Content = e.Content,
                    Id = e.Id,
                    Name = e.Name,
                    SeoAlias = e.SeoAlias,
                    Status = e.Status
                };
                if(e.DateModified == null)
                {
                    maker.DateModified = e.DateCreated.Value.ToString("dd/MM/yyyy");
                }
                else
                {
                    maker.DateModified = e.DateModified.Value.ToString("dd/MM/yyyy");
                }
                model.Add(maker);
            });
            return View(model.ToPagedList(page, pageSize));
        }

        [HttpPost]
        public async Task<IActionResult> SwitchStatus(int? Id)
        {
            if (Id == null)
            {
                return RedirectToAction("Index", "Home");
            }
            var maker = context.AutoMakers.Where(e=>e.Id == Id).FirstOrDefault();
            if (maker == null)
            {
                return NotFound($"Không tìm thấy Nhà sản xuất với ID '{Id}'.");
            }
            maker.Status = !maker.Status;
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
        public async Task<IActionResult> Delete(int? Id)
        {
            if (Id == null)
            {
                return RedirectToAction("Index", "Home");
            }
            var maker = context.AutoMakers.Where(e=>e.Id == Id).FirstOrDefault();
            if (maker == null)
            {
                return NotFound($"Không tìm thấy Nhà sản xuất với ID '{Id}'.");
            }
            if (maker.CoverImage != null)
            {
                System.IO.File.Delete(Path.Combine(@"wwwroot/", maker.CoverImage));
            }
            context.Remove(maker);
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
        public async Task<IActionResult> Create(Models.AutoMaker model)
        {
            ModelState.Remove("Author");
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError(string.Empty, "Lỗi nhập liệu");
                return View();
            }
            else
            {
                var maker = Activator.CreateInstance<ShopData.Model.AutoMaker>();
                maker.Author = await userManage.GetUserAsync(User);
                maker.AuthorId = maker.Author.Id;
                maker.Name = model.Name;
                maker.Status = model.Status;
                if (model.SeoPageTitle != null)
                {
                    maker.SeoPageTitle = model.SeoPageTitle;
                }
                else
                {
                    maker.SeoPageTitle = maker.Name;
                }
                if (model.SeoAlias != null)
                {
                    if (maker.SeoAlias == model.SeoAlias)
                    {
                        maker.SeoAlias = model.SeoAlias;
                    }
                    else
                    {
                        maker.SeoAlias = model.SeoAlias;
                        var listAlias = context.AutoMakers.Where(e => e.SeoAlias.Contains(model.SeoAlias)||e.SeoAlias == maker.SeoAlias).ToList();
                        if (listAlias.Count > 0)
                        {
                            maker.SeoAlias += "-" + listAlias.Count;
                        }
                    }
                }
                else
                {
                    maker.SeoAlias = UrlSlug.GenerateSlug(model.Name, false);
                    var listAlias = context.AutoMakers.Where(e => e.SeoAlias.Contains(model.SeoAlias)||e.SeoAlias == maker.SeoAlias).ToList();
                    if (listAlias.Count > 0)
                    {
                        maker.SeoAlias += "-" + listAlias.Count;
                    }
                }
                if (model.SeoDescription != null)
                {
                    maker.SeoDescription = model.SeoDescription;
                }
                else
                {
                    if(model.Name != null)
                    maker.SeoDescription = model.Name.Length > 250 ? model.Name.Substring(0, 250) : model.Name;
                }
                maker.SeoKeywords = model.SeoKeywords;
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
                    maker.CoverImage = filePath.Substring(8);
                }
                context.Add(maker);
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
        public async Task<IActionResult> Update(int? Id)
        {
            if (Id == null)
            {
                return RedirectToAction("Index", "Home");
            }
            var maker = context.AutoMakers.Where(e => e.Id == Id).Include(e=>e.Author).FirstOrDefault();
            if (maker == null)
            {
                return NotFound($"Không tìm thấy Nhà sản xuất với ID '{Id}'.");
            }
            Models.AutoMaker model = new Models.AutoMaker()
            {
                Id = maker.Id,
                Author = maker.Author,
                AuthorId = maker.AuthorId,
                Name = maker.Name,
                SeoAlias = maker.SeoAlias,
                SeoDescription = maker.SeoDescription,
                SeoKeywords = maker.SeoKeywords,
                SeoPageTitle = maker.SeoPageTitle,
                Status = maker.Status,
                Image = maker.CoverImage,
            };
           
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Update(Models.AutoMaker model)
        {
            ModelState.Remove("Author");
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError(string.Empty, "Lỗi nhập liệu");
                return View();
            }
            else
            {
                var maker = context.AutoMakers.Where(e=>e.Id == model.Id).FirstOrDefault();
                if (maker == null)
                {
                    return NotFound($"Không tìm thấy Nhà sản xuất với ID '{model.Id}'.");
                }
                maker.Author = await userManage.GetUserAsync(User);
                maker.AuthorId = maker.Author.Id;
                maker.Name = model.Name;
                maker.Status = model.Status;
                if (model.SeoPageTitle != null)
                {
                    maker.SeoPageTitle = model.SeoPageTitle;
                }
                else
                {
                    maker.SeoPageTitle = maker.Name;
                }
                if (model.SeoAlias != null)
                {
                    if(maker.SeoAlias == model.SeoAlias)
                    {
                        maker.SeoAlias = model.SeoAlias;
                    }
                    else
                    {
                        maker.SeoAlias = model.SeoAlias;
                        var listAlias = context.AutoMakers.Where(e => e.SeoAlias.Contains(model.SeoAlias) || e.SeoAlias == maker.SeoAlias).ToList();
                        if (listAlias.Count > 0)
                        {
                            maker.SeoAlias += "-" + listAlias.Count;
                        }
                    }
                }
                else
                {
                    maker.SeoAlias = UrlSlug.GenerateSlug(model.Name, false);
                    var listAlias = context.AutoMakers.Where(e => e.SeoAlias.Contains(model.SeoAlias)||e.SeoAlias == maker.SeoAlias).ToList();
                    if (listAlias.Count > 0)
                    {
                        maker.SeoAlias += "-" + listAlias.Count;
                    }
                }

                if (model.SeoDescription != null)
                {
                    maker.SeoDescription = model.SeoDescription;
                }
                else
                {
                    maker.SeoDescription = model.Name.Length > 250 ? model.Name.Substring(0, 250) : model.Name;
                }
                maker.SeoKeywords = model.SeoKeywords;
                if (model.CoverImage != null)
                {
                    if (maker.CoverImage != null)
                    {
                        System.IO.File.Delete(Path.Combine(@"wwwroot/", maker.CoverImage));
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
                    maker.CoverImage = filePath.Substring(8);
                }

                context.Update(maker);
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
