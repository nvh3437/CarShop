using System.ComponentModel.DataAnnotations;

namespace CarShop.Areas.Manage.Models
{
    public class UserLogin
    {
        [Required(ErrorMessage = "{0} là bắt buộc")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "{0} là bắt buộc")]
        [StringLength(100, ErrorMessage = "{0} phải trong phạm vi {2} - {1} Ký tự.", MinimumLength = 8)]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string Password { get; set; }

        public bool RememberMe { get; set; }
    }
}
