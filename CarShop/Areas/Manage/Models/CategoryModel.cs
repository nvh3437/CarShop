using ShopData.Model;
using System.ComponentModel.DataAnnotations;

namespace CarShop.Areas.Manage.Models
{
    public class CategoryModel
    {
        [Display(Name = "Id Thể loại")]
        [StringLength(250, ErrorMessage = "{0} Tối đa {1} Ký tự.")]
        public string? Id { get; set; }

        [Required(ErrorMessage = "{0} Là bắt buộc")]
        [StringLength(250, ErrorMessage = "{0} Tối đa {1} Ký tự.")]
        [Display(Name = "Tiêu đề")]
        public string Name { get; set; }

        [Display(Name = "Ngày sửa")]
        public string? DateModified { get; set; }

        public bool Status { get; set; }

        public string? AuthorId { get; set; }
        public User Author { get; set; }
        public IFormFile? CoverImage { get; set; }
        public string? Image { set; get; }

    }
}
