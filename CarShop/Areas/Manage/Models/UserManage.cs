using System.ComponentModel.DataAnnotations;

namespace CarShop.Areas.Manage.Models
{
    public class UserManage
    {
        public string? UserId { get; set; }

        [Required(ErrorMessage = "{0} Là bắt buộc")]
        [RegularExpression(@"[0-9a-zA-z^\S]*", ErrorMessage = "Chỉ chấp nhận chữ và số")]
        [StringLength(350, ErrorMessage = "{0} phải trong phạm vi {2} - {1} Ký tự.", MinimumLength = 1)]
        [Display(Name = "UserName")]
        public string UserName { get; set; }

        [Display(Name = "Họ Tên")]
        [StringLength(350, ErrorMessage = "{0} Tối đa {1} Ký tự.")]
        public string? Name { get; set; }

        [Required(ErrorMessage = "{0} là bắt buộc")]
        [EmailAddress(ErrorMessage = "Định dạng phải là email")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "{0} là bắt buộc")]
        [StringLength(100, ErrorMessage = "{0} phải trong phạm vi {2} - {1} Ký tự.", MinimumLength = 8)]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Xác nhận mật khẩu")]
        [Compare("Password", ErrorMessage = "Mật khẩu không khớp")]
        public string? ConfirmPassword { get; set; }
        // "Admin, Seller..."
        public string? Role { get; set; }

        //List Roles of User
        [Display(Name = "Phân Quyền")]
        public List<string>? ListRolesSelected { get; set; }
        //List Role in Project
        public List<string>? ListRoles { get; set; }

        public string? Status { set; get; }
        public bool? LockOut { get; set; }
    }
}
