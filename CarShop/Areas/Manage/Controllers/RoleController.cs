using CarShop.Areas.Manage.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ShopData.Model;

namespace CarShop.Areas.Manage.Controllers
{
    [Authorize(Roles = "Admin")]
    public class RoleController : ManageController
    {
        private readonly RoleManager<Role> roleManager;
        public RoleController(RoleManager<Role> _roleManager)
        {
            roleManager = _roleManager;
        }
        public IActionResult Index()
        {
            var model = new List<RoleManage>();
            roleManager.Roles.ToList().ForEach(role =>
            {
                model.Add(
                    new RoleManage() { Id = role.Id, Description = role.Description, Name = role.Name });
            });
            return View(model);
        }
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(RoleManage model)
        {
            if (ModelState.IsValid)
            {
                Role role = await roleManager.FindByNameAsync(model.Name);
                if(role != null)
                {
                    ModelState.AddModelError(string.Empty, "Role đã tồn tại");
                    return View(model);
                }

                Role newRole = Activator.CreateInstance<Role>();
                await roleManager.SetRoleNameAsync(newRole, model.Name);
                newRole.Description = model.Description;

                var res = await roleManager.CreateAsync(newRole);
                var SystemMessage = new List<SystemMessage>();

                if (res.Succeeded)
                {
                    SystemMessage.Add(new SystemMessage() { Title = "SystemMessageSuccess", Message = "Tạo role thành công" });
                }
                else
                {
                    SystemMessage.Add(new SystemMessage() { Title = "SystemMessageError", Message = "Tạo role thất bại, Kiểm tra lại" });
                }
                TempData["SystemMessage"] = JsonConvert.SerializeObject(SystemMessage);
                return RedirectToAction("Index");
            }
            ModelState.AddModelError(string.Empty, "Đầu vào chưa chính xác");
            return View(model);
        }
        public async Task<IActionResult> Update(string? roleId)
        {
            if(roleId == null)
            {
                return RedirectToAction("Index","Home");
            }
            var role = await roleManager.FindByIdAsync(roleId);
            if(role == null)
            {
                return NotFound($"Không tìm thấy Role với ID '{roleId}'.");
            }
            var model = new RoleManage() {Id = role.Id, Description = role.Description, Name = role.Name };
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Update(RoleManage model)
        {
            if (ModelState.IsValid)
            {
                Role role = await roleManager.FindByNameAsync(model.Name);
                if(role != null)
                {
                    ModelState.AddModelError(string.Empty, "Role đã tồn tại");
                    return View(model);
                }

                Role newRole = await roleManager.FindByIdAsync(model.Id);

                await roleManager.SetRoleNameAsync(newRole, model.Name);
                newRole.Description = model.Description;

                var res = await roleManager.UpdateAsync(newRole);
                var SystemMessage = new List<SystemMessage>();

                if (res.Succeeded)
                {
                    SystemMessage.Add(new SystemMessage() { Title = "SystemMessageSuccess", Message = "Thay đổi role thành công" });
                }
                else
                {
                    SystemMessage.Add(new SystemMessage() { Title = "SystemMessageError", Message = "Thay đổi role thất bại, Kiểm tra lại" });
                }
                TempData["SystemMessage"] = JsonConvert.SerializeObject(SystemMessage);
                return RedirectToAction("Index");
            }
            ModelState.AddModelError(string.Empty, "Đầu vào chưa chính xác");
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string? roleId)
        {
            if (roleId == null)
            {
                return RedirectToAction("Index", "Home");
            }
            var role = await roleManager.FindByIdAsync(roleId);
            if (role == null)
            {
                return NotFound($"Không tìm thấy Role với ID '{roleId}'.");
            }

            var res = await roleManager.DeleteAsync(role);
            var SystemMessage = new List<SystemMessage>();

            if (res.Succeeded)
            {
                SystemMessage.Add(new SystemMessage() { Title = "SystemMessageSuccess", Message = "Xóa role thành công" });
            }
            else
            {
                SystemMessage.Add(new SystemMessage() { Title = "SystemMessageError", Message = "Xóa role thất bại, Kiểm tra lại" });
            }
            TempData["SystemMessage"] = JsonConvert.SerializeObject(SystemMessage);
            return RedirectToAction("Index");

        }
    }
}
