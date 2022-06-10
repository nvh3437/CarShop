using ShopData.Model;
using System.ComponentModel.DataAnnotations;

namespace CarShop.Models
{
    public class CheckoutModel
    {
        public string? Id { get; set; }

        [Required(ErrorMessage = "{0} Là bắt buộc")]
        [StringLength(250, ErrorMessage = "{0} Tối đa {1} Ký tự.")]
        [Display(Name = "Họ tên")]
        public string Name { get; set; }

        [Required(ErrorMessage = "{0} Là bắt buộc")]
        [StringLength(250, ErrorMessage = "{0} Tối đa {1} Ký tự.")]
        [Display(Name = "Địa chỉ")]
        public string Address { get; set; }

        [Required(ErrorMessage = "{0} Là bắt buộc")]
        [StringLength(16, ErrorMessage = "{0} Tối đa {1} Ký tự.")]
        [Display(Name = "Số điện thoại")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "{0} Là bắt buộc")]
        [StringLength(250, ErrorMessage = "{0} Tối đa {1} Ký tự.")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [StringLength(250, ErrorMessage = "{0} Tối đa {1} Ký tự.")]
        [Display(Name = "Ghi chú")]
        public string? Note { get; set; }
        public int ProductId { get; set; }
        public Car? Car { get; set; }
    }
}
