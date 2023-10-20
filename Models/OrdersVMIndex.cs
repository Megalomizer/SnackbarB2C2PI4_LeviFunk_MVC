using SnackbarB2C2PI4_LeviFunk_ClassLibrary;

namespace SnackbarB2C2PI4_LeviFunk_MVC.Models
{
    public class OrdersVMIndex
    {
        public List<Order>? ListOrders { get; set; }
        public List<Order>? FavoritedOrders { get; set; }
        public Customer? ActiveCustomer { get; set; }
    }
}
