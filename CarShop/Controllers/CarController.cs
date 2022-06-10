using CarShop.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopData;
using ShopData.Model;
using X.PagedList;

namespace CarShop.Controllers
{
    public class CarController : Controller
    {
        private readonly Context context;
        public CarController(Context _context)
        {
            context = _context;
        }

        public IActionResult Index(Filter filter)
        {
            ViewData["DisableHeroCate"] = true;
            List<Car> ListCars;
            if (filter.keyword != null)
            {
                ListCars = context.Cars.Where(e => EF.Functions.Like(e.Name, $"%{filter.keyword}%")).ToList();
            }
            else
            {
                ListCars = context.Cars.Where(e => e.Status == true).Include(e => e.CarCategory).ToList();
            }
             
            List<Car> LastedCars = ListCars.OrderByDescending(e => e.DateCreated).Take(6).ToList();

            List<decimal> ListPrice = new List<decimal>();
            ListCars.ForEach(e => { ListPrice.Add(Decimal.Subtract(e.Price,e.PromoPrice)); });

            int MinPrice = Decimal.ToInt32(Decimal.Divide(ListPrice.Min(),1000000)) - 1;
            int MaxPrice = Decimal.ToInt32(Decimal.Divide(ListPrice.Max(), 1000000)) + 1;
            if (filter.automaker != null)
            {
                int makerId = 0;
                int.TryParse(filter.automaker,out makerId);
                List<CarAutoMaker> CarAutoMaker = context.CarAutoMakers.Include(e => e.AutoMaker).Where(e => (e.AutoMaker.SeoAlias == filter.automaker || e.AutoMaker.Id == makerId) && e.AutoMaker.Status == true).ToList();
                ListCars = ListCars.Join(CarAutoMaker, c => c.Id, cc => cc.CarId, (c, cc) => c).ToList();
            }
            if (filter.category != null)
            {
                List<CarCategory> carCategories = context.CarCategories.Include(e => e.Category).Where(e => e.Category.Id == filter.category && e.Category.Status == true).ToList();
                ListCars = ListCars.Join(carCategories, c => c.Id, cc => cc.CarId, (c, cc) => c).ToList();
            }
            //else
            //{
            //    List<CarCategory>  carCategoriesFalse = context.CarCategories.Include(e=>e.Category).Where(e=>e.Category.Status == false).ToList();
            //    ListCars.Join(carCategoriesFalse, c => c.Id, cc => cc.CarId, (c, cc) => c).ToList().ForEach(e =>
            //    {
            //        ListCars.Remove(e);
            //    });
            //}
            if (filter.maxAmount != null && filter.minAmount != null)
            {
                var x = new Decimal(filter.minAmount.Value);
                var y = new Decimal(filter.maxAmount.Value);
                x *= new Decimal(1000000);
                y *= new Decimal(1000000);
                ListCars = ListCars.Where(e => x <= e.Price - e.PromoPrice && e.Price - e.PromoPrice <= y).ToList();
            }
            else
            {
                filter.maxAmount = MaxPrice;
                filter.minAmount = MinPrice;
            }
            if (filter.sortBy != null)
            {
                switch (filter.sortBy)
                {
                    case "lasted":
                        ListCars = ListCars.OrderByDescending(e => e.DateCreated).ToList();
                        break;
                    case "price":
                        ListCars = ListCars.OrderBy(e => e.Price).ToList();
                        break;
                    case "desprice":
                        ListCars = ListCars.OrderByDescending(e => e.Price).ToList();
                        break;
                }
            }
            if(filter.page == null) {
                filter.page = 1;
            }
            if(filter.pageSize == null) {
                filter.pageSize = 10;
            }
            CarModel model = new CarModel()
            {
                ListCars = ListCars.ToPagedList(filter.page.Value, filter.pageSize.Value),
                MinPrice = MinPrice,
                MaxPrice = MaxPrice,
                Categories = context.Categories.Where(c => c.Status == true).ToList(),
                AutoMakers = context.AutoMakers.Where(c => c.Status == true).ToList(),
                CarsLasted = LastedCars,
                Filter = filter,
            };

            return View(model);
        }

        public IActionResult Detail(string? id)
        {
            if (id == null)
            {
                return RedirectToAction("Index", "Home");
            }
            var car = context.Cars.Where(e => e.SeoAlias == id && e.Status == true)
                .Include(e => e.CarCategory).ThenInclude(e => e.Category)
                .Include(e => e.CarAutoMaker).ThenInclude(e => e.AutoMaker)
                .FirstOrDefault();
            if (car == null)
            {
                return NotFound($"Không tìm thấy Xe với ID '{id}'.");
            }

            List<string> ListCateId = new List<string>();
            car.CarCategory.ForEach(CarCategory => ListCateId.Add(CarCategory.CategoryId));
            var carCates = context.CarCategories.Where(cc => ListCateId.Contains(cc.CategoryId)).Include(cc => cc.Car).Where(x => x.Car.Status == true).ToList();

            List<int> ListMakerId = new List<int>();
            car.CarAutoMaker.ForEach(CarAutoMaker => ListMakerId.Add(CarAutoMaker.AutoMakerId));
            var carMakers = context.CarAutoMakers.Where(cm => ListMakerId.Contains(cm.AutoMakerId)).Include(cm => cm.Car).Where(x => x.Car.Status == true).ToList();



            var related = new List<Car>();

            foreach (var cate in carCates)
            {
                related.Add(cate.Car);
            }

            foreach (var maker in carMakers)
            {
                related.Add(maker.Car);
            }
            related = related.Distinct().OrderByDescending(e => e.PromoPrice).Take(4).ToList();

            ViewData["related"] = related;
            return View(car);
        }

        public IActionResult Checkout(string? id)
        {
            if (id == null)
            {
                return RedirectToAction("Index", "Home");
            }
            var car = context.Cars.Where(e => e.SeoAlias == id && e.Status == true).FirstOrDefault();
            if (car == null)
            {
                return NotFound($"Không tìm thấy Xe với ID '{id}'.");
            }
            var model = new CheckoutModel() { Car = car };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Checkout(CheckoutModel model)
        {
            model.Car = context.Cars.Where(e => e.Id == model.ProductId).FirstOrDefault();
            if (model.Car == null)
            {
                return NotFound($"Không tìm thấy Xe với ID '{model.ProductId}'.");
            }
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError(string.Empty, "sjdnjhvfdsb");
                return View(model);
            }
            else
            {
                var Bill = Activator.CreateInstance<Bill>();
                Bill.Id = Guid.NewGuid().ToString();
                Bill.Name = model.Name;
                Bill.Email = model.Email;
                Bill.Phone = Services.GeneratePhoneNumber.PhoneNumber.GeneratePhoneNumber(model.Phone);
                Bill.Note = model.Note;
                Bill.Address = model.Address;
                Bill.Type = true;
                Bill.Fee = null;
                Bill.Total = model.Car.Price - model.Car.PromoPrice;
                Bill.Status = ShopData.Enum.CheckoutStatus.Open;

                var CarBill = Activator.CreateInstance<CarBill>();
                CarBill.CarId = model.Car.Id;
                CarBill.BillId = Bill.Id;
                CarBill.Fee = null;
                CarBill.Price = model.Car.Price - model.Car.PromoPrice;

                await context.AddRangeAsync(Bill, CarBill);
                var res = context.SaveChanges();
                if (res <= 0)
                {
                    ModelState.AddModelError(string.Empty, "Có lỗi xảy ra khi xử lý yêu cầu");
                    return View(model);
                }
                return RedirectToAction("BillDetail", "Car",Bill);
            }
        }

        public IActionResult BillDetail(string? id)
        {
            if (id == null)
            {
                return RedirectToAction("Index", "Home");
            }
            var bill = context.CarBills.Where(e => e.BillId == id).Include(e=>e.Bill).Include(e=>e.Car).FirstOrDefault();
            if (bill == null)
            {
                return NotFound($"Không tìm thấy hóa đơn với ID '{id}'.");
            }
            return View(bill);
        }
    }
}
