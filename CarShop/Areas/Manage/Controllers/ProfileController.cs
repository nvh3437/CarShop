using CarShop.Areas.Manage.Models;
using CarShop.Services.Email;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;
using ShopData.Model;
using System.IO;
using System.Text;
using System.Text.Encodings.Web;

namespace CarShop.Areas.Manage.Controllers
{
    public class ProfileController : ManageController
    {
        private readonly UserManager<User> userManager;
        private readonly SignInManager<User> signinManager;
        private readonly IEmailSender emailSender;
        private readonly ILogger<ProfileController> logger;

        public ProfileController(UserManager<User> _userManager, SignInManager<User> _signinManager, IEmailSender _emailSender, ILogger<ProfileController> _logger)
        {
            userManager = _userManager;
            signinManager = _signinManager;
            emailSender = _emailSender;
            logger = _logger;
        }
        public async Task<IActionResult> Index()
        {
            var user = await userManager.GetUserAsync(User);
            var model = new UserProfile() {
                Address = user.Address,
                Birthday = user.BirthDay,
                Email = user.Email,
                Phone = user.PhoneNumber,
                Name = user.Name,
                Avatar = user.Avatar,
                ConfirmEmail = user.EmailConfirmed,
                UserId = user.Id,
                UserName = user.UserName
            };
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Index(UserProfile model)
        {
            if (!ModelState.IsValid)
            {
                //ModelState.AddModelError(string.Empty, "Có lỗi xảy ra");
                return View(model);
            }
            var user = await userManager.GetUserAsync(User);
            var SystemMessage = new List<SystemMessage>();

            if (!string.IsNullOrEmpty(model.OldPassword) && !string.IsNullOrEmpty(model.NewPassword))
            {
                var res = await userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
                if (res.Succeeded)
                {
                    SystemMessage.Add(new SystemMessage() { Title = "SystemMessageSuccess", Message = "Cập nhật mật khẩu thành công" });
                }
                else
                {
                    SystemMessage.Add(new SystemMessage() { Title = "SystemMessageError", Message = "Cập nhật mật khẩu thất bại, Kiểm tra lại" });
                }
            }
            if (!string.IsNullOrEmpty(model.Email) && model.Email != user.Email)
            {
                var finduser = await userManager.FindByEmailAsync(model.Email);
                if (finduser != null)
                {
                    SystemMessage.Add(new SystemMessage() { Title = "SystemMessageError", Message = "Email đã tồn tại" });
                }
                else
                {

                    var code = await userManager.GenerateChangeEmailTokenAsync(user, model.Email);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    string? VerifyLink = Url.Action("ConfirmChangeEmail", "Profile", new
                    {
                        Area = "admin",
                        email = model.Email,
                        userId = await userManager.GetUserIdAsync(user),
                        code = code
                    }, HttpContext.Request.Scheme);
                    await emailSender.SendEmailAsync(
                        model.Email,
                        "Xác nhận tài khoản",
                        $"Bấm vào link sau để xác nhận Thay đổi Email <h3><a href='{HtmlEncoder.Default.Encode(VerifyLink)}'>Xác nhận</a</h3>");
                    SystemMessage.Add(new SystemMessage() { Title = "SystemMessageSuccess", Message = "Cập nhật Email thành công, Kiểm tra Email để xác nhận" });
                }
            }
            if (!string.IsNullOrEmpty(model.UserName) && model.UserName != user.UserName)
            {
                var finduser = await userManager.FindByNameAsync(model.UserName);
                if (finduser != null)
                {
                    SystemMessage.Add(new SystemMessage() { Title = "SystemMessageError", Message = "UserName đã tồn tại" });
                }
                else
                {
                    await userManager.SetUserNameAsync(user, model.UserName);
                    SystemMessage.Add(new SystemMessage() { Title = "SystemMessageSuccess", Message = "Cập nhật UserName thành công" });
                }
            }
            if (!string.IsNullOrEmpty(model.Phone) && model.Phone != user.PhoneNumber)
            {
                var res = await userManager.SetPhoneNumberAsync(user, model.Phone);
                if (res.Succeeded)
                {
                    SystemMessage.Add(new SystemMessage() { Title = "SystemMessageSuccess", Message = "Cập nhật số điện thoại thành công" });
                }
                else
                {
                    SystemMessage.Add(new SystemMessage() { Title = "SystemMessageError", Message = "Cập nhật số điện thoại thất bại, Kiểm tra lại" });
                }
            }
            if (!string.IsNullOrEmpty(model.Name) && model.Name != user.Name)
            {
                user.Name = model.Name;
                var res = await userManager.UpdateAsync(user);
                if (res.Succeeded)
                {
                    SystemMessage.Add(new SystemMessage() { Title = "SystemMessageSuccess", Message = "Cập nhật Tên thành công" });
                }
                else
                {
                    SystemMessage.Add(new SystemMessage() { Title = "SystemMessageError", Message = "Cập nhật Tên thất bại, Kiểm tra lại" });
                }
            }
            if (!string.IsNullOrEmpty(model.Address) && model.Address != user.Address)
            {
                user.Address = model.Address;
                var res = await userManager.UpdateAsync(user);
                if (res.Succeeded)
                {
                    SystemMessage.Add(new SystemMessage() { Title = "SystemMessageSuccess", Message = "Cập nhật địa chỉ thành công" });
                }
                else
                {
                    SystemMessage.Add(new SystemMessage() { Title = "SystemMessageError", Message = "Cập nhật địa chỉ thất bại, Kiểm tra lại" });
                }
            }
            if (model.Birthday != null && model.Birthday != user.BirthDay)
            {
                user.BirthDay = model.Birthday;
                var res = await userManager.UpdateAsync(user);
                if (res.Succeeded)
                {
                    SystemMessage.Add(new SystemMessage() { Title = "SystemMessageSuccess", Message = "Cập nhật ngày sinh thành công" });
                }
                else
                {
                    SystemMessage.Add(new SystemMessage() { Title = "SystemMessageError", Message = "Cập nhật ngày sinh thất bại, Kiểm tra lại" });
                }
            }
            if (model.FileAvatar != null)
            {
                if(user.Avatar != null)
                {
                    System.IO.File.Delete(Path.Combine(@"wwwroot/", user.Avatar));
                }
                var folder = string.Format(@"wwwroot/FileUpload/"+user.Id+"/");
                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }
                string fileName = Guid.NewGuid() + Path.GetExtension(model.FileAvatar.FileName);
                var filePath = Path.Combine(folder, fileName);

                using (var stream = System.IO.File.Create(filePath))
                {
                    await model.FileAvatar.CopyToAsync(stream);
                }
                user.Avatar = filePath.Substring(8);
                var res = await userManager.UpdateAsync(user);
                if (res.Succeeded)
                {
                    SystemMessage.Add(new SystemMessage() { Title = "SystemMessageSuccess", Message = "Cập nhật Avatar thành công" });
                }
                else
                {
                    SystemMessage.Add(new SystemMessage() { Title = "SystemMessageError", Message = "Cập nhật Avatar thất bại, Kiểm tra lại" });
                }
                logger.LogInformation("Uploaded a file");
            }
            TempData["SystemMessage"] = JsonConvert.SerializeObject(SystemMessage);
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> ConfirmChangeEmail(string email, string userId, string code)
        {
            if (userId == null || code == null)
            {
                return RedirectToAction("Index", "Home", new { area = "" });
            }
            else
            {
                User user = await userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return NotFound($"Không tìm thấy User với ID '{userId}'.");
                }
                else
                {
                    // Decode 
                    code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
                    var res = await userManager.ChangeEmailAsync(user, email, code);
                    var SystemMessage = new List<SystemMessage>();
                    SystemMessage.Add(new SystemMessage() { Title = "SystemMessageSuccess", Message = "Xác nhận Email thành công!" });
                    TempData["SystemMessage"] = JsonConvert.SerializeObject(SystemMessage);
                    if (signinManager.IsSignedIn(User))
                        return RedirectToAction("Index", "Home", new { area = "admin" });
                    return RedirectToAction("Index", "Login", new { area = "admin" });
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteAvatar(UserProfile model)
        {
            var user = await userManager.GetUserAsync(User);
            if (user.Avatar != null)
            {
                var SystemMessage = new List<SystemMessage>();
                System.IO.File.Delete(Path.Combine(@"wwwroot/", user.Avatar));
                user.Avatar = null;
                var res = await userManager.UpdateAsync(user);
                if (res.Succeeded)
                {
                    SystemMessage.Add(new SystemMessage() { Title = "SystemMessageSuccess", Message = "Cập nhật Avatar thành công" });
                }
                else
                {
                    SystemMessage.Add(new SystemMessage() { Title = "SystemMessageError", Message = "Cập nhật Avatar thất bại, Kiểm tra lại" });
                }
                TempData["SystemMessage"] = JsonConvert.SerializeObject(SystemMessage);
            }
            return View("Index", model);
        }
    }
}
