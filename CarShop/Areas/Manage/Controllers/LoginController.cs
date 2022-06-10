using CarShop.Areas.Manage.Models;
using CarShop.Services.Email;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;
using ShopData.Model;
using System.Text;
using System.Text.Encodings.Web;

namespace CarShop.Areas.Manage.Controllers
{
    [Area("Manage")]
    public class LoginController : Controller
    {
        private readonly UserManager<User> userManager;
        private readonly SignInManager<User> signinManager;
        private readonly IEmailSender emailSender;

        public LoginController(UserManager<User> _userManager, SignInManager<User> _signinManager, IEmailSender _emailSender)
        {
            userManager = _userManager;
            signinManager = _signinManager;
            emailSender = _emailSender;
        }
        public IActionResult Index(string? ReturnUrl)
        {
            ViewData["ReturnUrl"] = ReturnUrl;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(UserLogin model, string? ReturnUrl)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError(string.Empty, "Nội dung không đúng định dạng");
                return View(model);
            }
            else
            {
                var result = await signinManager.PasswordSignInAsync(model.Email, model.Password,model.RememberMe,lockoutOnFailure:true);
                if (!result.Succeeded)
                { 
                    //Find User by email and login 
                    var loginUser = await userManager.FindByEmailAsync(model.Email);
                    if(loginUser != null)
                        result = await signinManager.PasswordSignInAsync(loginUser, model.Password, model.RememberMe, lockoutOnFailure: true);
                }
                if (result.IsLockedOut)
                {
                    User user = await userManager.FindByEmailAsync(model.Email);
                    DateTimeOffset? endTimeUTC = await userManager.GetLockoutEndDateAsync(user);
                    var endTime = endTimeUTC.Value.ToLocalTime().Subtract(DateTimeOffset.Now);

                    ModelState.AddModelError(string.Empty, $"Tài khoản bị tạm khóa hãy thử lại sau {endTime.Days} Ngày {endTime.Hours}h:{endTime.Minutes}m!");
                    return View(model);
                }
                if (result.Succeeded)
                {
                    if(ReturnUrl != null)
                        return Redirect(ReturnUrl);
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Tài khoản hoặc mật khẩu không chính xác!");
                }
                return View(model);
            }
        }

        public async Task<IActionResult> Logout()
        {
            await signinManager.SignOutAsync();
            return RedirectToAction("Index", "Login");
        }

        [HttpGet]
        public IActionResult ForgotPassword(string? email)
        {
            UserLogin model = new UserLogin() { Email = email};
            return View(model);
        }
        
        [HttpPost]
        public async Task<IActionResult> ForgotPassword([Bind("Email")] UserLogin model)
        {
            ModelState.Remove("Password");
            ModelState.Remove("RememberMe");
            if(ModelState.IsValid)
            {
                var userForgot = await userManager.FindByEmailAsync(model.Email);
                if(userForgot == null)
                {
                    ModelState.AddModelError(string.Empty, "Email không tồn tại");
                    return View(model);
                }
                string code = await userManager.GeneratePasswordResetTokenAsync(userForgot);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                string? ResetLink = Url.Action("ResetPassword", "Login", 
                                    new {Area = "admin",email = model.Email, code = code }, Request.Scheme);
                await emailSender.SendEmailAsync(model.Email, "Xác nhận đặt lại mật khẩu CarShop",
                    $"Bạn đã yêu cầu đạt lại mật khẩu tại trang web <a href='{Request.Scheme}'>CarShop</a> nhấn vào link bên dưới để đặt lại mật khẩu<br> <a href='{ResetLink}'><button>Đặt lại mật khẩu</button></a>");
                var SystemMessage = new List<SystemMessage>();
                SystemMessage.Add(new SystemMessage() { Title = "SystemMessageSuccess", Message = "Chúng tôi đã gửi một thư tới Email của bạn!" });
                TempData["SystemMessage"] = JsonConvert.SerializeObject(SystemMessage);
                return RedirectToAction("Index", "Login");
            }
            else
            {
                return View(model);
            }
        }
        
        public async Task<IActionResult> ResetPassword(string email, string code)
        {
            if (email == null || code == null)
            {
                return RedirectToAction("Index", "Home", new {area =""});
            }
            else
            {
                User user = await userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    return NotFound($"Không tìm thấy User với Email '{email}'.");
                }
                else
                {
                    var model = new ResetPassword() {Email=email,Code=code};
                    return View(model);
                    
                }
            }
        }
        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPassword model)
        {
            if (ModelState.IsValid)
            {
                User user = await userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    return NotFound($"Không tìm thấy User với Email '{model.Email}'.");
                }
                var res = await userManager.ResetPasswordAsync(user, Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(model.Code)),model.Password);

                var SystemMessage = new List<SystemMessage>();
                if (res.Succeeded)
                {
                    SystemMessage.Add(new SystemMessage() { Title = "SystemMessageSuccess", Message = "Đặt lại mật khẩu thành công!" });

                }
                else
                {
                    SystemMessage.Add(new SystemMessage() { Title = "SystemMessageSuccess", Message = "Có lỗi xảy ra khi đặt lại mật khẩu.!" });
                }
                TempData["SystemMessage"] = JsonConvert.SerializeObject(SystemMessage);
                return RedirectToAction("Index", "login", new {area="admin"});
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Có lỗi xảy ra.");
                return View(model);
            }
        }
    }
}
