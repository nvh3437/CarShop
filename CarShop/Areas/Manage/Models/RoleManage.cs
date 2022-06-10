using System.ComponentModel.DataAnnotations;

namespace CarShop.Areas.Manage.Models
{
    public class RoleManage
    {
        public string? Id { get; set; }

        [Required(ErrorMessage = "{0} Là bắt buộc")]
        [RegularExpression(@"[0-9a-zA-z^\S]*", ErrorMessage ="Chỉ chấp nhận chữ và số")]
        [StringLength(350, ErrorMessage = "{0} phải trong phạm vi {2} - {1} Ký tự.", MinimumLength = 1)]
        [Display(Name = "Tên Role")]
        public string Name { get; set; }

        [Required(ErrorMessage = "{0} Là bắt buộc")]
        [Display(Name = "Mô tả")]
        [StringLength(350, ErrorMessage = "{0} Tối đa {1} Ký tự.")]
        public string Description { get; set; }
    }
    
}
