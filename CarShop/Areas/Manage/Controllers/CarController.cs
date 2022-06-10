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
    public class CarController : ManageController
    {
        private readonly ILogger<AutoMakerController> logger;
        private readonly Context context;
        private readonly UserManager<User> userManage;
        public CarController(ILogger<AutoMakerController> _logger, Context _context, UserManager<User> _userManage)
        {
            logger = _logger;
            context = _context;
            userManage = _userManage;
        }

        public IActionResult Index(int page = 1, int pageSize = 10)
        {
            List<CarModel> model = new List<CarModel>();
            context.Cars
                .Include(e => e.Author)
                .Include(e=>e.CarCategory).ThenInclude(e=>e.Category).OrderByDescending(e=>e.DateCreated)
                .ToList().ForEach(e => {
                var car = new CarModel
                {
                    Id = e.Id,
                    Name = e.Name,
                    Content = e.Content,
                    Price = e.Price,
                    PromoPrice = e.PromoPrice,
                    Author = e.Author,
                    CarCategory = e.CarCategory,
                    CarAutoMaker = e.CarAutoMaker,
                    SeoAlias = e.SeoAlias,
                    Status = e.Status
                };
                if(e.DateModified == null)
                {
                        car.DateModified = e.DateCreated.Value.ToString("dd/MM/yyyy");
                }
                else
                {
                        car.DateModified = e.DateModified.Value.ToString("dd/MM/yyyy");
                }
                string content = Regex.Replace(car.Content, "<[^>]*>", " ");
                car.Content = content.Length > 250 ? content.Substring(0, 250) : content;
                model.Add(car);
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
            var car = context.Cars.Where(e=>e.Id == Id).FirstOrDefault();
            if (car == null)
            {
                return NotFound($"Không tìm thấy Xe với ID '{Id}'.");
            }
            car.Status = !car.Status;
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
            var car = context.Cars.Where(e=>e.Id == Id).FirstOrDefault();
            if (car == null)
            {
                return NotFound($"Không tìm thấy Xe với ID '{Id}'.");
            }
            if (car.CoverImage != null)
            {
                System.IO.File.Delete(Path.Combine(@"wwwroot/", car.CoverImage));
            }
            context.Remove(car);
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
            var model = new CarModel();
            model.Categories = context.Categories.ToList();
            model.AutoMakers = context.AutoMakers.ToList();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CarModel model)
        {
            ModelState.Remove("Author");
            ModelState.Remove("CarCategory");
            ModelState.Remove("Categories");
            ModelState.Remove("OldCategoriesId");
            ModelState.Remove("NewCategoriesId");
            ModelState.Remove("CarAutoMaker");
            ModelState.Remove("AutoMakers");
            ModelState.Remove("OldAutoMakersId");
            ModelState.Remove("NewAutoMakersId");
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError(string.Empty, "Lỗi nhập liệu");
                model.Categories = context.Categories.ToList();
                model.AutoMakers = context.AutoMakers.ToList();
                return View(model);
            }
            else
            {
                var car = Activator.CreateInstance<Car>();
                car.Author = await userManage.GetUserAsync(User);
                car.AuthorId = car.Author.Id;
                car.Name = model.Name;
                car.SortDescription = model.SortDescription;
                car.Content = model.Content;
                car.Price = model.Price;
                car.PromoPrice = model.PromoPrice;
                car.Status = model.Status;
                car.Recommended = model.Recommended;
                if (model.SeoPageTitle != null)
                {
                    car.SeoPageTitle = model.SeoPageTitle;
                }
                else
                {
                    car.SeoPageTitle = car.Name;
                }
                if (model.SeoAlias != null)
                {
                    if (car.SeoAlias == model.SeoAlias)
                    {
                        car.SeoAlias = model.SeoAlias;
                    }
                    else
                    {
                        car.SeoAlias = model.SeoAlias;
                        var listAlias = context.Cars.Where(e => e.SeoAlias.Contains(model.SeoAlias) || e.SeoAlias == car.SeoAlias).ToList();
                        if (listAlias.Count > 0)
                        {
                            car.SeoAlias += "-" + listAlias.Count;
                        }
                    }
                }
                else
                {
                    car.SeoAlias = UrlSlug.GenerateSlug(model.Name, false);
                    var listAlias = context.Cars.Where(e => e.SeoAlias.Contains(model.SeoAlias)||e.SeoAlias == car.SeoAlias).ToList();
                    if (listAlias.Count > 0)
                    {
                        car.SeoAlias += "-" + listAlias.Count;
                    }
                }
                if (model.SeoDescription != null)
                {
                    car.SeoDescription = model.SeoDescription;
                }
                else
                {
                    string content = Regex.Replace(model.Content, "<[^>]*>", " ");
                    car.SeoDescription = content.Length > 250 ? content.Substring(0, 250) : content;
                }
                car.SeoKeywords = model.SeoKeywords;
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
                    car.CoverImage = filePath.Substring(8);
                }
                context.Add(car);
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

                if(model.NewAutoMakersId != null && model.NewAutoMakersId.Count > 0)
                {
                    List<CarAutoMaker> ListCarAutoMaker = new List<CarAutoMaker>();
                    foreach (var item in model.NewAutoMakersId)
                    {
                        CarAutoMaker carMaker = Activator.CreateInstance<CarAutoMaker>();
                        carMaker.CarId = car.Id;
                        carMaker.AutoMakerId = item;
                        ListCarAutoMaker.Add(carMaker);
                    }
                    await context.AddRangeAsync(ListCarAutoMaker);
                    res = context.SaveChanges();
                    if (res <= 0)
                    {
                        SystemMessage.Add(new SystemMessage() { Title = "SystemMessageError", Message = "Thêm Xe vào Nhà sản xuất thất bại" });
                    }
                }
                
                if(model.NewCategoriesId != null && model.NewCategoriesId.Count > 0)
                {
                    List<CarCategory> ListCarCategory = new List<CarCategory>();
                    foreach (var item in model.NewCategoriesId)
                    {
                        CarCategory carCate = Activator.CreateInstance<CarCategory>();
                        carCate.CarId = car.Id;
                        carCate.CategoryId = item;
                        ListCarCategory.Add(carCate);
                    }
                    await context.AddRangeAsync(ListCarCategory);
                    res = context.SaveChanges();
                    if (res <= 0)
                    {
                        SystemMessage.Add(new SystemMessage() { Title = "SystemMessageError", Message = "Thêm Xe vào Thể loại thất bại" });
                    }
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
            var car = context.Cars.Where(e => e.Id == Id)
                .Include(e => e.Author)
                .Include(e => e.CarCategory).ThenInclude(e => e.Category)
                .Include(e => e.CarAutoMaker).ThenInclude(e => e.AutoMaker)
                .FirstOrDefault();
            if (car == null)
            {
                return NotFound($"Không tìm thấy Xe với ID '{Id}'.");
            }
            CarModel model = new CarModel()
            {
                Id = car.Id,
                Author = car.Author,
                AuthorId = car.AuthorId,
                Content = car.Content,
                SortDescription = car.SortDescription,
                Price = car.Price,
                PromoPrice = car.PromoPrice,
                Name = car.Name,
                SeoAlias = car.SeoAlias,
                SeoDescription = car.SeoDescription,
                SeoKeywords = car.SeoKeywords,
                SeoPageTitle = car.SeoPageTitle,
                Status = car.Status,
                Recommended = car.Recommended.Value,
                Categories = context.Categories.ToList(),
                AutoMakers = context.AutoMakers.ToList(),
                Image = car.CoverImage,
                OldCategoriesId = new List<string>(),
                OldAutoMakersId = new List<int>(),
             };
            car.CarCategory.ForEach(e =>
            {
                model.OldCategoriesId.Add(e.CategoryId);
            });
            car.CarAutoMaker.ForEach(e =>
            {
                model.OldAutoMakersId.Add(e.AutoMakerId);
            });

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Update(CarModel model)
        {
            ModelState.Remove("Author");
            ModelState.Remove("CarCategory");
            ModelState.Remove("Categories");
            ModelState.Remove("OldCategoriesId");
            ModelState.Remove("NewCategoriesId");

            ModelState.Remove("CarAutoMaker");
            ModelState.Remove("AutoMakers");
            ModelState.Remove("OldAutoMakersId");
            ModelState.Remove("NewAutoMakersId");
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError(string.Empty, "Lỗi nhập liệu");
                model.Categories = context.Categories.ToList();
                model.AutoMakers = context.AutoMakers.ToList();
                return View(model);
            }
            else
            {
                var car = context.Cars.Where(e => e.Id == model.Id)
                    .Include(e => e.Author)
                    .Include(e => e.CarCategory).ThenInclude(e => e.Category)
                    .Include(e => e.CarAutoMaker).ThenInclude(e => e.AutoMaker)
                    .FirstOrDefault();
                if (car == null)
                {
                    return NotFound($"Không tìm thấy Xe với ID '{model.Id}'.");
                }
                car.Author = await userManage.GetUserAsync(User);
                car.AuthorId = car.Author.Id;
                car.Name = model.Name;
                car.Content = model.Content;
                car.SortDescription = model.SortDescription;
                car.Price = model.Price;
                car.PromoPrice = model.PromoPrice;
                car.Status = model.Status;
                car.Recommended = model.Recommended;
                if (model.SeoPageTitle != null)
                {
                    car.SeoPageTitle = model.SeoPageTitle;
                }
                else
                {
                    car.SeoPageTitle = car.Name;
                }
                if (model.SeoAlias != null)
                {
                    if (car.SeoAlias == model.SeoAlias)
                    {
                        car.SeoAlias = model.SeoAlias;
                    }
                    else
                    {
                        car.SeoAlias = model.SeoAlias;
                        var listAlias = context.Cars.Where(e => e.SeoAlias.Contains(model.SeoAlias) || e.SeoAlias == car.SeoAlias).ToList();
                        if (listAlias.Count > 0)
                        {
                            car.SeoAlias += "-" + listAlias.Count;
                        }
                    }
                }
                else
                {
                    car.SeoAlias = UrlSlug.GenerateSlug(model.Name, false);
                    var listAlias = context.Cars.Where(e => e.SeoAlias.Contains(model.SeoAlias)||e.SeoAlias == car.SeoAlias).ToList();
                    if (listAlias.Count > 0)
                    {
                        car.SeoAlias += "-" + listAlias.Count;
                    }
                }
                if (model.SeoDescription != null)
                {
                    car.SeoDescription = model.SeoDescription;
                }
                else
                {
                    string content = Regex.Replace(model.Content, "<[^>]*>", " ");
                    car.SeoDescription = content.Length > 250 ? content.Substring(0, 250) : content;
                }
                car.SeoKeywords = model.SeoKeywords;
                if (model.CoverImage != null)
                {
                    if (car.CoverImage != null)
                    {
                        System.IO.File.Delete(Path.Combine(@"wwwroot/", car.CoverImage));
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
                    car.CoverImage = filePath.Substring(8);
                }
                context.Update(car);
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
                context.RemoveRange(car.CarCategory);
                context.RemoveRange(car.CarAutoMaker);
                if (model.NewAutoMakersId != null && model.NewAutoMakersId.Count > 0)
                {
                    var ListCarAutoMaker = new List<CarAutoMaker>();
                    foreach (var item in model.NewAutoMakersId)
                    {
                        CarAutoMaker carMaker = Activator.CreateInstance<CarAutoMaker>();
                        carMaker.CarId = car.Id;
                        carMaker.AutoMakerId = item;
                        ListCarAutoMaker.Add(carMaker);
                    }
                    await context.AddRangeAsync(ListCarAutoMaker);
                }
                
                if (model.NewCategoriesId != null && model.NewCategoriesId.Count > 0)
                {
                    var ListCarCategory = new List<CarCategory>();
                    foreach (var item in model.NewCategoriesId)
                    {
                        CarCategory carCate = Activator.CreateInstance<CarCategory>();
                        carCate.CarId = car.Id;
                        carCate.CategoryId = item;
                        ListCarCategory.Add(carCate);
                    }
                    await context.AddRangeAsync(ListCarCategory);
                }

                res = context.SaveChanges();
                

                TempData["SystemMessage"] = JsonConvert.SerializeObject(SystemMessage);
                return RedirectToAction("Index");
            }
        }

        public async Task<IActionResult> Detail(int? Id, string? alias)
        {
            if (Id == null)
            {
                return RedirectToAction("Index", "Home");
            }
            Car car = null;
            if (alias != null)
            {
                car = context.Cars.Where(e => e.SeoAlias == alias)
                    .Include(e=>e.CarCategory).ThenInclude(e=>e.Category)
                    .Include(e=>e.CarAutoMaker).ThenInclude(e=>e.AutoMaker)
                    .FirstOrDefault();
            }
            else
            {
                car = context.Cars.Where(e => e.Id == Id)
                    .Include(e => e.CarCategory).ThenInclude(e => e.Category)
                    .Include(e => e.CarAutoMaker).ThenInclude(e => e.AutoMaker)
                    .FirstOrDefault();
            }
            if (car == null)
            {
                return NotFound($"Không tìm thấy Xe với ID '{Id}'.");
            }
            var model = new CarModel
            {
                Id = car.Id,
                Name = car.Name,
                Content = car.Content,
                SortDescription = car.SortDescription,
                Image = car.CoverImage,
                Price = car.Price,
                PromoPrice = car.PromoPrice,
                Categories = new List<Category>(),
                AutoMakers = new List<ShopData.Model.AutoMaker>(),
            };
            if (car.DateModified == null)
            {
                model.DateModified = car.DateCreated.Value.ToString("dd/MM/yyyy");
            }
            else
            {
                model.DateModified = car.DateModified.Value.ToString("dd/MM/yyyy");
            }
            if (car.CarCategory != null && car.CarCategory.Count > 0)
            {
                car.CarCategory.ForEach(e =>
                {
                    model.Categories.Add(e.Category);
                });
            }
            if (car.CarAutoMaker != null && car.CarAutoMaker.Count > 0)
            {
                car.CarAutoMaker.ForEach(e =>
                {
                    model.AutoMakers.Add(e.AutoMaker);
                });
            }
            return View(model);
        }
    }
}
