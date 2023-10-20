using SnackbarB2C2PI4_LeviFunk_ClassLibrary;

namespace SnackbarB2C2PI4_LeviFunk_MVC.Models
{
    public class HomeVMIndex
    {
        #region Properties

        public Customer Customer { get; set; } // The active user
        public List<Product> Products { get; set; } // All products

        #endregion
    }
}
