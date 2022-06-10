using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace CarShop.Models
{
    public class ContactModel
    {
        [DisplayName("Họ Tên")]
        [Required(ErrorMessage = "{0} Là bắt buộc")]
        [StringLength(250, ErrorMessage = "{0} Tối đa {1} Ký tự.")]
        public string Name { get; set; }

        [Phone]
        [DisplayName("Điện Thoại")]
        [Required(ErrorMessage = "{0} Là bắt buộc")]
        [StringLength(16, ErrorMessage = "{0} Tối đa {1} Ký tự.")]
        public string Phone { get; set; }

        [EmailAddress]
        [DisplayName("Email")]
        [Required(ErrorMessage = "{0} Là bắt buộc")]
        [StringLength(250, ErrorMessage = "{0} Tối đa {1} Ký tự.")]
        public string Email { get; set; }

        [DisplayName("Nội Dung")]
        [Required(ErrorMessage = "{0} Là bắt buộc")]
        public string Content { get; set; }
    }
}
