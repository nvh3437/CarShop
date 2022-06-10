using ShopData.Model;
using X.PagedList;

namespace CarShop.Models
{
    public class CarModel
    {
        public IPagedList<Car> ListCars { set; get; }
        public int MinPrice { get; set; }
        public int MaxPrice { get; set; }
        public List<Category> Categories { get; set; }
        public List<AutoMaker> AutoMakers { get; set; }
        public List<Car> CarsLasted { get; set; }
        public Filter? Filter { get; set; }

    }
    public class Filter
    {
        public string? category { set; get; }
        public string? automaker { set; get; }
        public int? maxAmount { set; get; }
        public int? minAmount { set; get; }
        public int? page { set; get; }
        public int? pageSize { set; get; }
        public string sortBy { set; get; }
        public string keyword { set; get; }
    }
}
