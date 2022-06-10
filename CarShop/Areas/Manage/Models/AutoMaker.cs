using Microsoft.AspNetCore.Mvc;
using ShopData.Model;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CarShop.Areas.Manage.Models
{
    public class AutoMaker
    {
        [Display(Name = "Id nhà sản xuất")]
        [Required(ErrorMessage = "{0} Là bắt buộc")]
        public int Id { get; set; }

        [Required(ErrorMessage = "{0} Là bắt buộc")]
        [StringLength(250, ErrorMessage = "{0} Tối đa {1} Ký tự.")]
        [Display(Name = "Tiêu đề")]
        public string Name { get; set; }

        [Display(Name = "Ngày sửa")]
        public string? DateModified { get; set; }

        [StringLength(250, ErrorMessage = "{0} Tối đa {1} Ký tự.")]
        [Display(Name = "Tiêu đề SEO")]
        public string? SeoPageTitle { get; set; }


        [StringLength(250, ErrorMessage = "{0} Tối đa {1} Ký tự.")]
        [Display(Name = "Đường dẫn SEO")]
        //[RegularExpression(@"[^\W+-][+-]*", ErrorMessage = "Chỉ chấp nhận chữ và số và dấu +-")]
        [RegularExpression(@"^[a-z0-9]+(?:-[a-z0-9]+)*$", ErrorMessage = "Chỉ chấp nhận chữ không dấu và số và dấu ' - '")]
        public string? SeoAlias { get; set; }

        [StringLength(250, ErrorMessage = "{0} Tối đa {1} Ký tự.")]
        [Display(Name = "Từ khóa SEO")]
        public string? SeoKeywords { get; set; }

        [StringLength(250, ErrorMessage = "{0} Tối đa {1} Ký tự.")]
        [Display(Name = "Mô tả SEO")]
        public string? SeoDescription { get; set; }

        public bool Status { get; set; }

        public string? AuthorId { get; set; }
        public virtual User Author { get; set; }
        public IFormFile? CoverImage { get; set; }
        public string? Image { set; get; }
    }
}
