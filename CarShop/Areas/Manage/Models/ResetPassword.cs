using System.ComponentModel.DataAnnotations;

namespace CarShop.Areas.Manage.Models
{
    public class ResetPassword
    {
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
        [Required(ErrorMessage = "{0} là bắt buộc")]
        [Display(Name = "Xác nhận mật khẩu")]
        [Compare("Password", ErrorMessage = "Mật khẩu không khớp")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "{0} là bắt buộc")]
        [Display(Name = "Mã xác nhận")]
        public string Code { get; set; }
    }
}
