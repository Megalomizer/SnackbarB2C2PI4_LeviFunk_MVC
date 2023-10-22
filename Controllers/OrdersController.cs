using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Humanizer.Localisation.TimeToClockNotation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SnackbarB2C2PI4_LeviFunk_ClassLibrary;
using SnackbarB2C2PI4_LeviFunk_MVC.Data;
using SnackbarB2C2PI4_LeviFunk_MVC.Models;

namespace SnackbarB2C2PI4_LeviFunk_MVC.Controllers
{
    public class OrdersController : Controller
    {
        private readonly ApiService _apiService;
        private static List<Product> NewOrderProductList = new List<Product>();

        public OrdersController(ApiService apiService)
        {
            _apiService = apiService;
        }

        // GET: Orders
        public async Task<IActionResult> Index()
        {
            // Get the active customer
            Customer customer = await _apiService.GetCustomer(User.FindFirstValue(ClaimTypes.NameIdentifier));

            // Get vm data
            IEnumerable<Order> orders = await _apiService.GetOrders(customer.Id);
            List<Order> favoriteOrders = new List<Order>();

            // Get the list of all favorited orders
            foreach (Order order in orders)
            {
                if(order.IsFavorited == true)
                    favoriteOrders.Add(order);
            }

            // Create the viewmodel
            OrdersVMIndex ordersVM = new OrdersVMIndex()
            {
                ListOrders = orders.ToList(),
                FavoritedOrders = favoriteOrders,
                ActiveCustomer = customer,
            };

            return View(ordersVM);
        }

        // GET: Orders/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            // Get the order & the customer of the order
            Order order = await _apiService.GetOrder(id);

            if (order == null)
                return NotFound();

            order.Customer = await _apiService.GetCustomer((int)order.CustomerId);

            // Get the productlist
            IEnumerable<OrderProduct> orderProducts = await _apiService.GetOrderProducts(order.Id);
            List<Product> products = new List<Product>();
            foreach (OrderProduct op in orderProducts)
            {
                for (int i = 0; i < op.Amount; i++)
                {
                    Product product = await _apiService.GetProduct(op.ProductId);
                    products.Add(product);
                }
            }

            // Create the viewmodel
            OrdersVMDetails ordersVMDetails = new OrdersVMDetails()
            {
                Order = order,
                Products = products,
            };

            return View(ordersVMDetails);
        }

        // GET: Orders/Create
        public async Task<IActionResult> Create()
        {
            // Get a list of all products
            IEnumerable<Product> products = await _apiService.GetProducts();

            // Create the new viewmodel
            OrdersVMCreate ordersVM = new OrdersVMCreate()
            {
                Order = new Order(),
                Products = NewOrderProductList,
                AllProducts = products.ToList(),
            };

            return View(ordersVM);
        }

        // POST: Orders/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Cost,DateOfOrder,IsFavorited,Status")] Order order)
        {
            if (ModelState.IsValid)
            {
                Customer customer = await _apiService.GetCustomer(User.FindFirstValue(ClaimTypes.NameIdentifier));
                order.Customer = customer;

                // Add the itemlist
                order.Products = NewOrderProductList;

                await _apiService.CreateOrder(order);
                NewOrderProductList.Clear();

                return RedirectToAction("Index");
            }

            return View(order);
        }

        public async Task<IActionResult> EditStart(int? id)
        {
            // Get the list of all products in the order
            IEnumerable<OrderProduct> orderproducts = await _apiService.GetOrderProducts((int)id);
            List<Product> products = new List<Product>();
            foreach (OrderProduct op in orderproducts)
            {
                for (int i = 0; i < op.Amount; i++)
                {
                    Product product = await _apiService.GetProduct(op.ProductId);
                    products.Add(product);
                }
            }

            // Set the product list
            NewOrderProductList = products;
            return RedirectToAction("Edit", new {id=id});
        }

        // GET: Orders/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            // Get all data
            Order order = await _apiService.GetOrder(id);
            IEnumerable<Product> productsCatalog = await _apiService.GetProducts();

            if (order == null)
                return NotFound();

            /*// Get a list of all products in the order
            IEnumerable<OrderProduct> orderproducts = await _apiService.GetOrderProducts(order.Id);
            List<Product> products = new List<Product>();
            foreach(OrderProduct op in orderproducts)
            {
                for (int i = 0; i < op.Amount; i++)
                {
                    Product product = await _apiService.GetProduct(op.ProductId);
                    products.Add(product);
                }                
            }*/

            // Create vm
            OrdersVMCreate orderVM = new OrdersVMCreate()
            {
                Order = order,
                Products = NewOrderProductList,
                AllProducts = productsCatalog.ToList(),
            };

            return View(orderVM);
        }

        // POST: Orders/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        /*[HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Cost,DateOfOrder,IsFavorited,Status")] Order order)
        {
            if (id != order.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                await _apiService.UpdateOrder(order, id);

                return RedirectToAction("Index");
            }

            return View(order);
        }*/

        // GET: Orders/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            Order order = await _apiService.GetOrder(id);

            if (order == null)
                return NotFound();

            return View(order);
        }

        // POST: Orders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _apiService.DeleteOrder(id);
            return RedirectToAction("Index");
        }

        private Task<bool> OrderExists(int id)
        {
            return (_apiService.DoesOrderExist(id)); ;
        }

        // Transform Order to Transaction (From Saved order)
        public async Task<IActionResult> OrderToTransaction(Order order)
        {
            //Re-set the list of items to the order and the customer... since it removed it here
            order.Products = NewOrderProductList;
            order.Customer = await _apiService.GetCustomer((int)order.CustomerId);

            // Get the total discount
            int? discount = 0;
            foreach(Product product in order.Products)
                discount += product.Discount;

            // Create the new transaction
            Transaction transaction = new Transaction()
            {
                Id = 0,
                Cost = order.Cost,
                Discount = discount,
                DateOfTransaction = DateTime.UtcNow,
                Customer = order.Customer,
                CustomerId = order.CustomerId,
                Order = order,
                OrderId = order.Id
            };

            // Create the viewmodel
            OrdersVMCreateTransaction createTransaction = new OrdersVMCreateTransaction()
            {
                Order = order,
                Transaction = transaction,
                Products = order.Products.ToList(),
            };

            return View(createTransaction);
        }

        public async Task<IActionResult> SaveTransaction(int? id, [Bind("Id, Cost, Discount, DateOfTransaction, Customer, Order")] Transaction transaction)
        {
            if (id == null)
                return NotFound();

            Order order = await _apiService.GetOrder(id);
            transaction.Order = order;
            transaction.DateOfTransaction = DateTime.UtcNow;

            if (ModelState.IsValid)
                await _apiService.CreateTransaction(transaction);

            return RedirectToAction("Index");
        }

        #region NewOrderMethods & EditOrderMethods

        /// <summary>
        /// Add a product to a new order
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> AddProductToOrderProductsList(int Id)
        {
            Product product = await _apiService.GetProduct(Id);

            if (product == null)
                return NotFound();

            NewOrderProductList.Add(product);
            return RedirectToAction("Create");
        }

        /// <summary>
        /// Remove a product from a new order
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> RemoveProductFromOrderProductsList(int Id)
        {
            Product product = await _apiService.GetProduct(Id);

            if (product == null)
                return NotFound();

            NewOrderProductList.Remove(product);
            
            return RedirectToAction("Create");
        }

        /// <summary>
        /// Add a product to an edited order
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> AddProductToOrderProductsListEdit(int Id, int orderId)
        {
            Product product = await _apiService.GetProduct(Id);

            if (product == null)
                return NotFound();

            NewOrderProductList.Add(product);

            // Get the correct order
            Order order = await _apiService.GetOrder(orderId);

            return RedirectToAction("Edit", new { id = order.Id });
        }

        /// <summary>
        /// Remove a product from an edited order
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> RemoveProductFromOrderProductsListEdit(int Id, int orderId)
        {
            Product product = await _apiService.GetProduct(Id);

            if (product == null)
                return NotFound();

            // Find the item in NewOrderProductList
            Product productListItem = NewOrderProductList.Find(p => p.Id == product.Id);

            NewOrderProductList.Remove(productListItem);

            //Get the correct order
            Order order = await _apiService.GetOrder(orderId);

            return RedirectToAction("Edit", new { id = order.Id });
        }

        /// <summary>
        /// Save the newly created order
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> SaveNewOrder()
        {
            // Create the cost
            decimal cost = 0;
            foreach(Product product in NewOrderProductList)
                cost += product.Price;

            // Create the order
            Order order = new Order()
            {
                Id = 0,
                IsFavorited = false,
                DateOfOrder = DateTime.Now,
                Status = "Not Ordered",
                Cost = cost,
                Products = NewOrderProductList
            };

            // If the user is authenticated, get the corresponding customer and add it to the order
            if (User.Identity.IsAuthenticated)
            {
                Customer customer = await _apiService.GetCustomer(User.FindFirstValue(ClaimTypes.NameIdentifier));
                order.Customer = customer;
                order.CustomerId = customer.Id;
            }

            await _apiService.CreateOrder(order);

            return RedirectToAction("OrderToTransaction", order);
        }

        /// <summary>
        /// Save the newly edited order
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> SaveEditedOrder(int id)
        {
            // Get the old order
            Order order = await _apiService.GetOrder(id);

            // Get the active Customer
            order.Customer = await _apiService.GetCustomer(User.FindFirstValue(ClaimTypes.NameIdentifier));

            // Create the cost
            decimal cost = 0;
            foreach (Product product in NewOrderProductList)
                cost += product.Price;

            // Create the order
            Order orderEdited = new Order()
            {
                Id = id,
                IsFavorited = order.IsFavorited,
                DateOfOrder = DateTime.Now,
                Status = order.Status,
                Cost = cost,
                Products = NewOrderProductList,
                CustomerId = order.CustomerId,
            };

            await _apiService.UpdateOrder(orderEdited, id);

            return RedirectToAction("Index");
        }

        /// <summary>
        /// Cancel the newly created order
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> CancelNewOrder()
        {
            NewOrderProductList.Clear();
            return RedirectToAction("Index");
        }

        /// <summary>
        /// Cancel the editing order
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> CancelEditedOrder(int id)
        {
            NewOrderProductList.Clear();
            return RedirectToAction("Details", new { id = id });
        }

        #endregion
    }
}
