using ShopData.Enum;

namespace CarShop
{
    public static class CheckoutStatusExtendsion
    {
        public static string GetStringName(this CheckoutStatus status)
        {
            switch (status)
            {
                case CheckoutStatus.Open: return "Đang chờ duyệt";
                    break;
                case CheckoutStatus.Pending: return "Đang xác nhận thông tin";
                    break;
                case CheckoutStatus.UnPaid: return "Đang chờ thanh toán";
                    break;
                case CheckoutStatus.Paid: return "Đã thanh toán, chờ vận chuyển";
                    break;
                case CheckoutStatus.Confirmed: return "Đã xác nhận thông tin, chờ vận chuyển";
                    break;
                case CheckoutStatus.Shipping: return "Đang trong quá trình vận chuyển";
                    break;
                case CheckoutStatus.Completed: return "Đã hoàn thành";
                    break;
                case CheckoutStatus.Cancelled: return "Đã hủy";
                    break;
                default: return "Không tồn tại";
                    break;
            }
        }
        
        public static List<CheckoutStatus> GetNextStatus(this CheckoutStatus status)
        {
            switch (status)
            {
                case CheckoutStatus.Open: return new List<CheckoutStatus>() { 
                                                            CheckoutStatus.Pending, 
                                                            CheckoutStatus.UnPaid, 
                                                            CheckoutStatus.Confirmed, 
                                                            CheckoutStatus.Completed, 
                                                            CheckoutStatus.Cancelled};
                    break;
                case CheckoutStatus.Pending: return new List<CheckoutStatus>(){
                                                                    CheckoutStatus.Confirmed,
                                                                    CheckoutStatus.UnPaid,
                                                                    CheckoutStatus.Completed,
                                                                    CheckoutStatus.Cancelled};
                    break;
                case CheckoutStatus.UnPaid: return new List<CheckoutStatus>(){
                                                                    CheckoutStatus.Paid,
                                                                    CheckoutStatus.Confirmed,
                                                                    CheckoutStatus.Completed,
                                                                    CheckoutStatus.Cancelled};
                    break;
                case CheckoutStatus.Paid: return new List<CheckoutStatus>(){
                                                                    CheckoutStatus.Shipping,
                                                                    CheckoutStatus.Completed,
                                                                    CheckoutStatus.Cancelled};
                    break;
                case CheckoutStatus.Confirmed: return new List<CheckoutStatus>(){
                                                                    CheckoutStatus.Shipping,
                                                                    CheckoutStatus.Completed,
                                                                    CheckoutStatus.Cancelled};
                    break;
                case CheckoutStatus.Shipping: return new List<CheckoutStatus>(){
                                                                    CheckoutStatus.Completed,
                                                                    CheckoutStatus.Cancelled};
                    break;
                case CheckoutStatus.Completed: return new List<CheckoutStatus>();
                    break;
                case CheckoutStatus.Cancelled: return new List<CheckoutStatus>();
                    break;
                default: return new List<CheckoutStatus>();
                    break;
            }
        }
        
        public static string GetContent(this CheckoutStatus status)
        {
            switch (status)
            {
                case CheckoutStatus.Open: return "Chúng tôi sẽ liên hệ với bạn qua số điện thoại hoặc email. Tối đa 3 ngày kể từ khi tạo đơn hàng";
                    break;
                case CheckoutStatus.Pending: return "Đang chờ xác nhận thông tin từ phía bạn";
                    break;
                case CheckoutStatus.UnPaid: return "Đang chờ thanh toán. Nếu gặp vấn đề hãy thử liên hệ CSKH của ngân hàng để kiểm tra";
                    break;
                case CheckoutStatus.Paid: return "Đã thanh toán, Sản phẩm của bạn đang được chuẩn bị để vận chuyển sớm nhất";
                    break;
                case CheckoutStatus.Confirmed: return "Đã xác nhận thông tin, Sản phẩm của bạn đang được chuẩn bị để vận chuyển sớm nhất";
                    break;
                case CheckoutStatus.Shipping: return "Đang trong quá trình vận chuyển, Sản phẩm của bạn đang được vận chuyển tới địa chỉ giao hàng. Hãy chú ý điện thoại để chúng tôi có thể liên hệ ngay khi tới";
                    break;
                case CheckoutStatus.Completed: return "Đã hoàn thành";
                    break;
                case CheckoutStatus.Cancelled: return "Đã hủy. Liên hệ CSKH của chúng tôi để biết thêm chi tiết hoặc khiếu nại";
                    break;
                default: return "Không tồn tại. Liên hệ CSKH của chúng tôi để biết thêm chi tiết hoặc khiếu nại";
                    break;
            }
        }
        
        public static string GetColor(this CheckoutStatus status)
        {
            switch (status)
            {
                case CheckoutStatus.Open: return "text-warning";
                    break;
                case CheckoutStatus.Pending: return "text-warning";
                    break;
                case CheckoutStatus.UnPaid: return "text-danger";
                    break;
                case CheckoutStatus.Paid: return "text-success";
                    break;
                case CheckoutStatus.Confirmed: return "text-success";
                    break;
                case CheckoutStatus.Shipping: return "text-success";
                    break;
                case CheckoutStatus.Completed: return "text-primary";
                    break;
                case CheckoutStatus.Cancelled: return "text-danger";
                    break;
                default: return "text-danger";
                    break;
            }
        }
    }
}
