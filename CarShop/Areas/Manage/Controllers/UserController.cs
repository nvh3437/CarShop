using CarShop.Areas.Manage.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ShopData.Model;
using X.PagedList;

namespace CarShop.Areas.Manage.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UserController : ManageController
    {
        private readonly RoleManager<Role> roleManager;
        private readonly UserManager<User> userManager;
        public UserController(RoleManager<Role> _roleManager, UserManager<User> _userManager)
        {
            roleManager = _roleManager;
            userManager = _userManager;
        }
        public async Task<IActionResult> Index(int page = 1, int pageSize = 10)
        {
            List<UserManage> res = new List<UserManage>();

            List<User> users = userManager.Users.ToList();

            foreach (var user in users)
            {
                var roles = await userManager.GetRolesAsync(user);
                string? roleName = null;
                if (roles.Count > 0)
                {
                    roleName = string.Join(", ", roles);
                }
                var newUser = new UserManage()
                    {
                        Email = user.Email,
                        Name = user.Name,
                        UserId = user.Id,
                        UserName = user.UserName,
                        Role = roleName
                    } ;
                if (await userManager.IsLockedOutAsync(user))
                {
                    DateTimeOffset? endTimeUTC = await userManager.GetLockoutEndDateAsync(user);
                    var endTime = endTimeUTC.Value.ToLocalTime().Subtract(DateTimeOffset.Now);


                    newUser.Status = $"Bị Khóa <br>{endTime.Days} Ngày {endTime.Hours}h:{endTime.Minutes}m";
                }
                else
                {
                    newUser.Status = "Hoạt động";
                }
                res.Add(newUser);


            };
            IPagedList<UserManage> model = res.ToPagedList(page, pageSize);
            return View(model);
        }

        public async Task<IActionResult> Create()
        {
            var model = new UserManage();
            model.ListRoles = new List<string>();
            roleManager.Roles.ToList().ForEach(async role =>
            {
                model.ListRoles.Add(role.Name);
            });
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Create(UserManage model)
        {
            string? roleName;
            if (ModelState.IsValid)
            {
                User findUser = await userManager.FindByNameAsync(model.UserName);
                //Check exists UserName
                if (findUser != null)
                {
                    ModelState.AddModelError(string.Empty, "User đã tồn tại");
                    model.ListRoles = new List<string>();
                    roleManager.Roles.ToList().ForEach(async role =>
                    {
                        model.ListRoles.Add(role.Name);
                    });
                    roleName = null;
                    if (model.ListRolesSelected != null && model.ListRolesSelected.Count > 0)
                    {
                        roleName = string.Join(", ", model.ListRolesSelected);
                    }
                    model.Role = roleName;
                    return View(model);
                }
                //Check exists Email
                findUser = await userManager.FindByEmailAsync(model.Email);
                if (findUser != null)
                {
                    model.ListRoles = new List<string>();
                    roleManager.Roles.ToList().ForEach(async role =>
                    {
                        model.ListRoles.Add(role.Name);
                    });
                    roleName = null;
                    if (model.ListRolesSelected != null && model.ListRolesSelected.Count > 0)
                    {
                        roleName = string.Join(", ", model.ListRolesSelected);
                    }
                    model.Role = roleName;
                    ModelState.AddModelError(string.Empty, "Email đã tồn tại");
                    return View(model);
                }
                //Create new User Object
                User newUser = Activator.CreateInstance<User>();
                //Set UserName for new User
                await userManager.SetUserNameAsync(newUser, model.UserName);
                //Set Email for new User
                await userManager.SetEmailAsync(newUser, model.Email);
                //Set Name for new User
                newUser.Name = model.Name;
                //Create new User in Project
                var res = await userManager.CreateAsync(newUser,model.Password);
                var SystemMessage = new List<SystemMessage>();
                //Check status and add User to Role
                if (res.Succeeded)
                {
                    SystemMessage.Add(new SystemMessage() { Title = "SystemMessageSuccess", Message = "Tạo User thành công" });
                    
                    if(model.ListRolesSelected != null)
                    {
                        res = await userManager.AddToRolesAsync(newUser, model.ListRolesSelected);
                        if (!res.Succeeded)
                        {
                            SystemMessage.Add(new SystemMessage() { Title = "SystemMessageError", Message = "Gán Role thất bại" });
                        }
                    }
                }
                else
                {
                    SystemMessage.Add(new SystemMessage() { Title = "SystemMessageError", Message = "Tạo User thất bại, Kiểm tra lại" });
                }

                TempData["SystemMessage"] = JsonConvert.SerializeObject(SystemMessage);
                return RedirectToAction("Index");
            }
            model.ListRoles = new List<string>();
            roleManager.Roles.ToList().ForEach(async role =>
            {
                model.ListRoles.Add(role.Name);
            });
            roleName = null;
            if (model.ListRolesSelected != null && model.ListRolesSelected.Count > 0)
            {
                roleName = string.Join(", ", model.ListRolesSelected);
            }
            model.Role = roleName;
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string? userId)
        {
            if (userId == null)
            {
                return RedirectToAction("Index", "Home");
            }
            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound($"Không tìm thấy User với ID '{userId}'.");
            }

            var res = await userManager.DeleteAsync(user);
            var SystemMessage = new List<SystemMessage>();

            if (res.Succeeded)
            {
                SystemMessage.Add(new SystemMessage() { Title = "SystemMessageSuccess", Message = "Xóa user thành công" });
            }
            else
            {
                SystemMessage.Add(new SystemMessage() { Title = "SystemMessageError", Message = "Xóa user thất bại, Kiểm tra lại" });
            }
            TempData["SystemMessage"] = JsonConvert.SerializeObject(SystemMessage);
            return RedirectToAction("Index");

        }

        public async Task<IActionResult> Update(string? userId)
        {
            if (userId == null)
            {
                return RedirectToAction("Index", "Home");
            }
            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound($"Không tìm thấy User với ID '{userId}'.");
            }
            var model = new UserManage() { UserId = user.Id, Email = user.Email, Name = user.Name, UserName = user.UserName };
            
            model.ListRoles = new List<string>();
            roleManager.Roles.ToList().ForEach(async role =>
            {
                model.ListRoles.Add(role.Name);
            });
            var roles = await userManager.GetRolesAsync(user);
            string? roleName = null;
            if (roles.Count > 0)
            {
                roleName = string.Join(", ", roles);
            }

            model.Role = roleName;
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Update(UserManage model)
        {
            ModelState.Remove("Password");
            ModelState.Remove("ConfirmPassword");
            string? roleName = null;
            if (ModelState.IsValid)
            {
                //Get this User
                var user = await userManager.FindByIdAsync(model.UserId);
                User findUser;
                IdentityResult res;
                //Create System Message
                var SystemMessage = new List<SystemMessage>();

                //Set UserName and Email for this user
                if (user.UserName != model.UserName || user.Email != model.Email)
                {
                    //Check exists UserName
                    if (user.UserName != model.UserName)
                    {
                        findUser = await userManager.FindByNameAsync(model.UserName);
                        if (findUser != null)
                        {
                            ModelState.AddModelError(string.Empty, "User đã tồn tại");
                            model.ListRoles = new List<string>();
                            roleManager.Roles.ToList().ForEach(async role =>
                            {
                                model.ListRoles.Add(role.Name);
                            });
                            roleName = null;
                            if (model.ListRolesSelected != null && model.ListRolesSelected.Count > 0)
                            {
                                roleName = string.Join(", ", model.ListRolesSelected);
                            }
                            model.Role = roleName;
                            return View(model);
                        }
                    }
                    //Check exists Email
                    if (user.Email != model.Email)
                    {
                        findUser = await userManager.FindByEmailAsync(model.Email);
                        if (findUser != null)
                        {
                            ModelState.AddModelError(string.Empty, "Email đã tồn tại");
                            model.ListRoles = new List<string>();
                            roleManager.Roles.ToList().ForEach(async role =>
                            {
                                model.ListRoles.Add(role.Name);
                            });
                            roleName = null;
                            if (model.ListRolesSelected != null && model.ListRolesSelected.Count > 0)
                            {
                                roleName = string.Join(", ", model.ListRolesSelected);
                            }
                            model.Role = roleName;
                            return View(model);
                        }
                    }

                    //Set UserName for user
                        res = await userManager.SetUserNameAsync(user, model.UserName);
                    if (!res.Succeeded)
                    {
                        SystemMessage.Add(new SystemMessage() { Title = "SystemMessageError", Message = "Đổi UserName Thất Bại" });
                    }
                    //Set Email for user
                    res = await userManager.SetEmailAsync(user, model.Email);
                    if (!res.Succeeded)
                    {
                        SystemMessage.Add(new SystemMessage() { Title = "SystemMessageError", Message = "Đổi Email Thất Bại" });
                    }
                }
                //Set Name for user
                if(user.Name != model.Name)
                {
                    user.Name = model.Name;
                    res = await userManager.UpdateAsync(user);
                    if (!res.Succeeded)
                    {
                        SystemMessage.Add(new SystemMessage() { Title = "SystemMessageError", Message = "Đổi Tên Thất Bại" });
                    }
                }
                //Delete old Role for user
                var oldRole = await userManager.GetRolesAsync(user);
                res = await userManager.RemoveFromRolesAsync(user, oldRole);
                if (!res.Succeeded)
                {
                    SystemMessage.Add(new SystemMessage() { Title = "SystemMessageError", Message = "Xóa Role Thất Bại" });
                }
                //Check ListRoleSelected > 0 and Add User to Role
                if(model.ListRolesSelected != null && model.ListRolesSelected.Count > 0)
                {
                    //Set new Role for user
                    res = await userManager.AddToRolesAsync(user, model.ListRolesSelected);
                    if (!res.Succeeded)
                    {
                        SystemMessage.Add(new SystemMessage() { Title = "SystemMessageError", Message = "Đổi Role Thất Bại" });
                    }
                }

                SystemMessage.Add(new SystemMessage() { Title = "SystemMessageSuccess", Message = "Đã cập nhật User" });
                TempData["SystemMessage"] = JsonConvert.SerializeObject(SystemMessage);
                return RedirectToAction("Index");
            }
            //Model not IsValid
            //Get list Role in Project
            model.ListRoles = new List<string>();
            roleManager.Roles.ToList().ForEach(async role =>
            {
                model.ListRoles.Add(role.Name);
            });
            //Get Role Name selected
            roleName = null;
            if (model.ListRolesSelected != null && model.ListRolesSelected.Count > 0)
            {
                roleName = string.Join(", ", model.ListRolesSelected);
            }
            model.Role = roleName;

            return View(model);
        }

        public async Task<IActionResult> ChangePassword(string? userId)
        {
            if (userId == null)
            {
                return RedirectToAction("Index", "Home");
            }
            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound($"Không tìm thấy User với ID '{userId}'.");
            }
            UserManage model = new UserManage() { UserId = user.Id, UserName = user.UserName};
            return View(model);
        }
        
        [HttpPost]
        public async Task<IActionResult> ChangePassword(UserManage model)
        {
            ModelState.Remove("Email");
            if (ModelState.IsValid)
            {
                if (model.UserId == null)
                {
                    return RedirectToAction("Index", "Home");
                }
                var user = await userManager.FindByIdAsync(model.UserId);
                if (user == null)
                {
                    return NotFound($"Không tìm thấy User với ID '{model.UserId}'.");
                }

                var SystemMessage = new List<SystemMessage>();

                //Delete Password for user
                var res = await userManager.RemovePasswordAsync(user);
                if (!res.Succeeded)
                {
                    SystemMessage.Add(new SystemMessage() { Title = "SystemMessageSuccess", Message = "Xóa Mật Khẩu Thất Bại" });
                }

                //Set new Password for user
                res = await userManager.AddPasswordAsync(user, model.Password);
                if (!res.Succeeded)
                {
                    SystemMessage.Add(new SystemMessage() { Title = "SystemMessageSuccess", Message = "Đổi Mật Khẩu Thất Bại" });
                }

                SystemMessage.Add(new SystemMessage() { Title = "SystemMessageSuccess", Message = "Đã cập nhật User" });
                TempData["SystemMessage"] = JsonConvert.SerializeObject(SystemMessage);
                return RedirectToAction("Index");
            }
            return View(model);
        }

        public async Task<IActionResult> ManageRole(string? userId)
        {
            if (userId == null)
            {
                return RedirectToAction("Index", "Home");
            }
            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound($"Không tìm thấy User với ID '{userId}'.");
            }

            UserManage model = new UserManage() { UserId = user.Id, UserName = user.UserName, ListRoles = new List<string>(), Role = null };

            roleManager.Roles.ToList().ForEach(x =>
            {
                model.ListRoles.Add(x.Name);
            });

            var UesrRole = await userManager.GetRolesAsync(user);
            if(UesrRole != null && UesrRole.Count > 0)
            {
                model.Role = string.Join(",", UesrRole);
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ManageRole(UserManage model)
        {
            if (model.UserId == null)
            {
                return RedirectToAction("Index", "Home");
            }
            var user = await userManager.FindByIdAsync(model.UserId);
            if (user == null)
            {
                return NotFound($"Không tìm thấy User với ID '{model.UserId}'.");
            }

            //Create System Message
            var SystemMessage = new List<SystemMessage>();

            //Delete old Role for user
            var oldRole = await userManager.GetRolesAsync(user);
            var res = await userManager.RemoveFromRolesAsync(user, oldRole);
            if (!res.Succeeded)
            {
                SystemMessage.Add(new SystemMessage() { Title = "SystemMessageError", Message = "Xóa Role Thất Bại" });
            }

            //Check ListRoleSelected > 0 and Add User to Role
            if (model.ListRolesSelected != null && model.ListRolesSelected.Count > 0)
            {
                //Set new Role for user
                res = await userManager.AddToRolesAsync(user, model.ListRolesSelected);
                if (!res.Succeeded)
                {
                    SystemMessage.Add(new SystemMessage() { Title = "SystemMessageError", Message = "Đổi Role Thất Bại" });
                }
            }

            SystemMessage.Add(new SystemMessage() { Title = "SystemMessageSuccess", Message = "Đã cập nhật User" });
            TempData["SystemMessage"] = JsonConvert.SerializeObject(SystemMessage);
            return RedirectToAction("Index");

        }

        [HttpPost]
        public async Task<IActionResult> Lockout(string? userId)
        {
            if (userId == null)
            {
                return RedirectToAction("Index", "Home");
            }
            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound($"Không tìm thấy User với ID '{userId}'.");
            }
            var utcDateTime = DateTime.UtcNow.AddYears(1000);
            var dto = new DateTimeOffset(utcDateTime).ToOffset(TimeSpan.FromHours(2));

            var res = await userManager.SetLockoutEndDateAsync(user, dto);
            var SystemMessage = new List<SystemMessage>();

            if (res.Succeeded)
            {
                SystemMessage.Add(new SystemMessage() { Title = "SystemMessageSuccess", Message = "Khóa user thành công" });
            }
            else
            {
                SystemMessage.Add(new SystemMessage() { Title = "SystemMessageError", Message = "Khóa user thất bại, Kiểm tra lại" });
            }
            TempData["SystemMessage"] = JsonConvert.SerializeObject(SystemMessage);
            return RedirectToAction("Index");

        }
        
        [HttpPost]
        public async Task<IActionResult> UnLockout(string? userId)
        {
            if (userId == null)
            {
                return RedirectToAction("Index", "Home");
            }
            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound($"Không tìm thấy User với ID '{userId}'.");
            }
            var utcDateTime = DateTime.UtcNow;
            var dto = new DateTimeOffset(utcDateTime).ToOffset(TimeSpan.FromHours(2));

            var res = await userManager.SetLockoutEndDateAsync(user, dto);
            var SystemMessage = new List<SystemMessage>();

            if (res.Succeeded)
            {
                SystemMessage.Add(new SystemMessage() { Title = "SystemMessageSuccess", Message = "Mở khóa user thành công" });
            }
            else
            {
                SystemMessage.Add(new SystemMessage() { Title = "SystemMessageError", Message = "Mở khóa user thất bại, Kiểm tra lại" });
            }
            TempData["SystemMessage"] = JsonConvert.SerializeObject(SystemMessage);
            return RedirectToAction("Index");

        }
    }
}
