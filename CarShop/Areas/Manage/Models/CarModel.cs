using ShopData.Model;
using System.ComponentModel.DataAnnotations;

namespace CarShop.Areas.Manage.Models
{
    public class CarModel
    {
        [Display(Name = "Id Thể loại")]
        [Required(ErrorMessage = "{0} Là bắt buộc")]
        public int Id { get; set; }

        [Required(ErrorMessage = "{0} Là bắt buộc")]
        [StringLength(250, ErrorMessage = "{0} Tối đa {1} Ký tự.")]
        [Display(Name = "Tiêu đề")]
        public string Name { get; set; }


        [Required(ErrorMessage = "{0} Là bắt buộc")]
        [Display(Name = "Nội dung")]
        public string Content { get; set; }

        [Display(Name = "Mô tả ngắn")]
        public string? SortDescription { get; set; }

        [Required(ErrorMessage = "{0} Là bắt buộc")]
        [Display(Name = "Giá")]
        public decimal Price { get; set; }

        [Display(Name = "Giảm Giá")]
        [Required(ErrorMessage = "{0} Là bắt buộc")]
        public decimal PromoPrice { get; set; }

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

        [Display(Name = "Kích hoạt?")]
        public bool Status { get; set; }

        [Display(Name = "Đề xuất?")]
        public bool Recommended { get; set; }

        public string? AuthorId { get; set; }
        public User Author { get; set; }

        public List<CarCategory> CarCategory { get; set; }

        public List<Category> Categories { get; set; }
        public List<string> NewCategoriesId { get; set; }
        public List<string> OldCategoriesId { get; set; }
        
        public List<CarAutoMaker> CarAutoMaker { get; set; }
        public List<ShopData.Model.AutoMaker> AutoMakers { get; set; }
        public List<int> NewAutoMakersId { get; set; }
        public List<int> OldAutoMakersId { get; set; }

        public IFormFile? CoverImage { get; set; }
        public string? Image { set; get; }



    }
}
