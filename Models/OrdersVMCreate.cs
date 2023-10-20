using SnackbarB2C2PI4_LeviFunk_ClassLibrary;

namespace SnackbarB2C2PI4_LeviFunk_MVC.Models
{
    public class OrdersVMCreate
    {
        public Order? Order { get; set; }
        public List<Product>? Products { get; set; }
        public List<Product>? AllProducts { get; set; }
    }
}
