using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace CarShop.Areas.Manage.Models
{
    public class UserProfile
    {
        [Display(Name = "User Id")]
        [Required(ErrorMessage = "{0} Là bắt buộc")]
        public string UserId { get; set; }

        [RegularExpression(@"[0-9a-zA-z^\S]*", ErrorMessage = "Chỉ chấp nhận chữ và số")]
        [StringLength(350, ErrorMessage = "{0} phải trong phạm vi {2} - {1} Ký tự.", MinimumLength = 1)]
        [Display(Name = "UserName")]
        public string? UserName { get; set; }

        [StringLength(350, ErrorMessage = "{0} phải trong phạm vi {2} - {1} Ký tự.", MinimumLength = 8)]
        [Display(Name = "Họ tên")]
        public string? Name { get; set; }

        [EmailAddress(ErrorMessage = "Định dạng phải là email")]
        [Display(Name = "Email")]
        public string? Email { get; set; }

        [StringLength(15, ErrorMessage = "{0} phải trong phạm vi {2} - {1} Ký tự.", MinimumLength = 9)]
        [Display(Name = "Số điện thoại")]
        [RegularExpression(@"[0-9 +-\\()]*")]
        public string? Phone { get; set; }

        [StringLength(350, ErrorMessage = "{0} phải trong phạm vi {2} - {1} Ký tự.", MinimumLength = 8)]
        [Display(Name = "Địa chỉ")]
        public string? Address { get; set; }

        [Display(Name = "Ngày sinh")]
        public DateTime? Birthday { get; set; }

        
        [StringLength(100, ErrorMessage = "{0} phải trong phạm vi {2} - {1} Ký tự.", MinimumLength = 8)]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string? OldPassword { get; set; }
        
        [StringLength(100, ErrorMessage = "{0} phải trong phạm vi {2} - {1} Ký tự.", MinimumLength = 8)]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string? NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Xác nhận mật khẩu")]
        [Compare("NewPassword", ErrorMessage = "Mật khẩu không khớp")]
        public string? ConfirmPassword { get; set; }

        public IFormFile? FileAvatar { get; set; }
        public string? Avatar { set; get; }
        public bool? ConfirmEmail { set; get; }
    }
}
