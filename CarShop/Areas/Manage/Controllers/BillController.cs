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
    public class BillController : ManageController
    {
        private readonly Context context;
        private readonly UserManager<User> userManage;
        public BillController(Context _context, UserManager<User> _userManage)
        {
            context = _context;
            userManage = _userManage;
        }

        public IActionResult Index(int page = 1, int pageSize = 10)
        {
            List<Bill> model = context.Bills.OrderByDescending(e=>e.DateCreated).Include(e=>e.Author).ToList();
            return View(model.ToPagedList(page, pageSize));
        }

        public async Task<IActionResult> Manager(string? Id)
        {
            if (Id == null)
            {
                return RedirectToAction("Index", "Home");
            }
            var bill = context.Bills.Where(e => e.Id == Id)
                .Include(e => e.Author)
                .Include(e => e.CarBills).ThenInclude(e => e.Car)
                .FirstOrDefault();
            if (bill == null)
            {
                return NotFound($"Không tìm thấy Đơn hàng với ID '{Id}'.");
            }
            return View(bill);
        }

        [HttpPost]
        public async Task<IActionResult> Manager(string? billId, decimal? productFee, decimal? billFee, CheckoutStatus? nextStatus, string? note, decimal? cashOut)
        {
            if (billId == null)
            {
                return RedirectToAction("Index", "Home");
            }
            var bill = context.Bills.Where(e => e.Id == billId)
                .Include(e => e.Author)
                .Include(e => e.CarBills).ThenInclude(e => e.Car)
                .FirstOrDefault();
            if (bill == null)
            {
                return NotFound($"Không tìm thấy Đơn hàng với ID '{billId}'.");
            }


            var SystemMessage = new List<SystemMessage>();
            var oldBill = bill;
            if (nextStatus > CheckoutStatus.Pending && nextStatus < CheckoutStatus.Cancelled)
            {
                if(bill.Fee == null && billFee == null)
                {
                    ModelState.AddModelError(string.Empty, "Hoàn thành phụ phí hóa đơn trước");
                    return View(oldBill);
                }
                if(bill.CarBills.Fee == null && productFee == null)
                {
                    ModelState.AddModelError(string.Empty, "Hoàn thành phụ phí sản phẩm trước");
                    return View(oldBill);
                }
            }

            //set product fee for product == Car
            if (bill.Type)
            {
                if (productFee != null)
                {
                    if (bill.CarBills.Fee != null)
                    {
                        ModelState.AddModelError(string.Empty, "Phụ phí sản phẩm chỉ có thể thêm 1 lần");
                        return View(oldBill);
                    }
                    else
                    {
                        bill.CarBills.Fee = productFee;
                    }
                }
            }
            //set product fee for product != Car
            else
            {

            }
            //set bill fee
            if (billFee != null)
            {
                if(bill.Fee != null)
                {
                    ModelState.AddModelError(string.Empty, "Phụ phí hóa đơn chỉ có thể thêm 1 lần");
                    return View(oldBill);
                }
                else
                {
                    bill.Fee = billFee;
                }    
            }
            //set bill note for guest
            bill.NoteManager = note+"";
            //set bill status
            if (nextStatus != null && bill.Status != CheckoutStatus.Completed)
            {
                 bill.Status = nextStatus.Value;
                if(nextStatus == CheckoutStatus.Completed)
                {
                    bill.CashOut = bill.Total + bill.Fee.Value;
                }
            }
            bill.Author = await userManage.GetUserAsync(User);
            bill.AuthorId = bill.Author.Id;
            context.Update(bill);
            var res = context.SaveChanges();
            if (res>0)
            {
                SystemMessage.Add(new SystemMessage() { Title = "SystemMessageSuccess", Message = "Cập nhật thành công" });
            }
            else
            {
                SystemMessage.Add(new SystemMessage() { Title = "SystemMessageError", Message = "Cập nhật thất bại, Kiểm tra lại" });
            }

            TempData["SystemMessage"] = JsonConvert.SerializeObject(SystemMessage);
            return RedirectToAction("Index");
        }
    }
}
