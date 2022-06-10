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
    public class RegisterController : Controller
    {
        private readonly UserManager<User> userManager;
        private readonly SignInManager<User> signInManager;
        private readonly ILogger<RegisterController> logger;
        private readonly IUserStore<User> userStore;
        private readonly IUserEmailStore<User> emailStore;
        private readonly IEmailSender emailSender;
        public RegisterController(UserManager<User> _userManager,
            SignInManager<User> _signInManager, 
            IUserStore<User> _userStore,
            ILogger<RegisterController> _logger,
            IEmailSender _emailSender)
        {
            userManager = _userManager;
            signInManager = _signInManager;
            userStore = _userStore;
            logger = _logger;
            emailStore = (IUserEmailStore<User>)_userStore;
            emailSender = _emailSender;
        }
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(UserRegister model)
        {
            if (ModelState.IsValid)
            {
                var finduser = await userManager.FindByEmailAsync(model.Email);
                if(finduser != null)
                {
                    ModelState.AddModelError(string.Empty, "Email đã tồn tại");
                    return View(model);
                }
                var newUser = Activator.CreateInstance<User>();
                int indexOfA = model.Email.IndexOf("@");
                string mailName = model.Email.Substring(0,indexOfA);
                await userStore.SetUserNameAsync(newUser, mailName, CancellationToken.None);
                await emailStore.SetEmailAsync(newUser,model.Email,CancellationToken.None);
                var res = await userManager.CreateAsync(newUser, model.Password);
                if (res.Succeeded)
                {
                    logger.LogInformation(string.Empty, "Add new member Success");
                    await signInManager.SignInAsync(newUser, model.RememberMe);

                    // Generate Email Confirmation
                    var code = await userManager.GenerateEmailConfirmationTokenAsync(newUser);
                    // Encode to Url
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    // Generate Url Confirmation
                    string? VerifyLink = Url.Action("ConfirmEmail", "Register", new
                    {
                        Area = "admin",
                        userId = await userManager.GetUserIdAsync(newUser), 
                        code = code
                    }, HttpContext.Request.Scheme);
                    await emailSender.SendEmailAsync(
                        newUser.Email, 
                        "Xác nhận tài khoản",
                        $"Bấm vào link sau để xác nhận tài khoản <h3><a href='{HtmlEncoder.Default.Encode(VerifyLink)}'>Xác nhận</a</h3>");
                    var SystemMessage = new List<SystemMessage>();
                    SystemMessage.Add(new SystemMessage() { Title = "SystemMessageSuccess", Message = "Đăng ký thành công, Kiểm tra Email của bạn!" });
                    TempData["SystemMessage"] = JsonConvert.SerializeObject(SystemMessage);
                    return RedirectToAction("Index", "Home");
                    
                }
                else
                {
                    logger.LogInformation(string.Empty, "Add new member Err");
                    foreach(var error in res.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return View(model);
                }
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Có lỗi xảy ra");
                return View(model);
            }
            
        }

        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string userId, string code)
        {
            if(userId == null || code ==null)
            {
                return RedirectToAction("Index", "Home", new { area = "" });
            }
            else
            {
                User user = await userManager.FindByIdAsync(userId);
                if(user == null)
                {
                    return NotFound($"Không tìm thấy User với ID '{userId}'.");
                }
                else
                {
                    // Decode 
                    code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
                    var res = await userManager.ConfirmEmailAsync(user, code);
                    var SystemMessage = new List<SystemMessage>();
                    SystemMessage.Add(new SystemMessage() { Title = "SystemMessageSuccess", Message = "Xác nhận Email thành công!" });
                    TempData["SystemMessage"] = JsonConvert.SerializeObject(SystemMessage); if (signInManager.IsSignedIn(User))
                        return RedirectToAction("Index", "Home", new {area="admin"});
                    return RedirectToAction("Index", "Login", new {area="admin"});
                }
            }
        }

        public async Task<IActionResult> ResendEmail(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Index", "Home", new { area = "" });
            }
            else
            {

                var user = await userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return NotFound($"Không tìm thấy User với ID '{userId}'.");
                }
                else
                {
                    var code = await userManager.GenerateEmailConfirmationTokenAsync(user);
                    // Encode to Url
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    // Generate Url Confirmation
                    string? VerifyLink = Url.Action("ConfirmEmail", "Register", new
                    {
                        Area = "admin",
                        userId = await userManager.GetUserIdAsync(user),
                        code = code
                    }, HttpContext.Request.Scheme);
                    await emailSender.SendEmailAsync(
                        user.Email,
                        "Xác nhận tài khoản",
                        $"Bấm vào link sau để xác nhận tài khoản <h3><a href='{HtmlEncoder.Default.Encode(VerifyLink)}'>Xác nhận</a</h3>");

                    var LastPage = Request.Headers["Referer"].ToString();

                    if (LastPage != null)
                    {
                        return Redirect(LastPage);
                    }
                    return RedirectToAction("Index", "Home", null);
                }
            }
        }
    }
}
