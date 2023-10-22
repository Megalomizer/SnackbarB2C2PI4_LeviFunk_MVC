using SnackbarB2C2PI4_LeviFunk_ClassLibrary;

namespace SnackbarB2C2PI4_LeviFunk_MVC.Models
{
    public class OrdersVMDetails
    {
        public Order Order { get; set; }
        public List<Product> Products { get; set; }
    }
}
