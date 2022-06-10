using ShopData.Model;

namespace CarShop.Models
{
    public class AutoMakerModel
    {
        public AutoMaker AutoMaker { get; set; }
        public List<Category> Categories { get; set; }
        public List<Car> Cars { get; set; }
    }
}
