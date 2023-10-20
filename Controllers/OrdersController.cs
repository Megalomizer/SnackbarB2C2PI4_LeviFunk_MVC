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
            IEnumerable<Order> orders = await _apiService.GetOrders();
            List<Order> favoriteOrders = new List<Order>();

            // Get the list of all favorited orders
            foreach (Order order in orders)
            {
                if(order.IsFavorited == true)
                    favoriteOrders.Add(order);
            }

            //Get the active user
            Customer customer = await _apiService.GetCustomer(User.FindFirstValue(ClaimTypes.NameIdentifier));

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

            Order order = await _apiService.GetOrder(id);

            if (order == null)
                return NotFound();

            return View(order);
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

        // GET: Orders/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            Order order = await _apiService.GetOrder(id);

            if (order == null)
                return NotFound();

            return View(order);
        }

        // POST: Orders/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
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
        }

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
            //Re-set the list of items to the order ... since it removed it here
            order.Products = NewOrderProductList;

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
                Order = order,
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

        public async Task<IActionResult> SaveTransaction([Bind("Id, Cost, Discount, DateOfTransaction, Customer, Order")] Transaction transaction)
        {
            if (ModelState.IsValid)
                await _apiService.CreateTransaction(transaction);

            return RedirectToAction("Index");
        }

        #region NewOrderMethods

        /// <summary>
        /// Add a product to a new order
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> AddProductToOrderProductsList(int Id)
        {
            if (Id == null)
                return NotFound();

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

            if (NewOrderProductList.Contains(product))
            {
                NewOrderProductList.Remove(product);
            }
            
            return RedirectToAction("Create");
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
                Customer customer = await _apiService.GetCustomer(User.FindFirstValue(User.Identity.Name));
                order.Customer = customer;
            }

            await _apiService.CreateOrder(order);

            return RedirectToAction("OrderToTransaction", order);
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

        #endregion
    }
}
