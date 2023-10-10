using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using Newtonsoft.Json;
using SnackbarB2C2PI4_LeviFunk_ClassLibrary;
using SnackbarB2C2PI4_LeviFunk_MVC.Data;
using SnackbarB2C2PI4_LeviFunk_MVC.Models;

namespace SnackbarB2C2PI4_LeviFunk_MVC.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ILogger<ProductsController> logger;
        private readonly ApiService apiService;

        public ProductsController(ILogger<ProductsController> _logger, ApiService _apiService)
        {
            logger = _logger;
            apiService = _apiService;
        }

        // GET: Products 
        public async Task<IActionResult> Index()
        {
            //IList<Product> products = await _context.Products.ToListAsync();
            IEnumerable<Product> products = await apiService.GetProducts();
            return View(products);
        }

        // GET: Products/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();
            
            Product product = await apiService.GetProduct(id);

            if (product == null)
                return NotFound();

            return View(product);
        }

        // GET: Products/Create 
        public IActionResult Create()
        {
            return View();
        }

        // POST: Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Price,Discount,Stock,ImgPath,Description")] Product product)
        {
            if (ModelState.IsValid)
            {
                HttpResponseMessage response = await apiService.CreateProduct(product);

                if (response.IsSuccessStatusCode)
                {
                    // When created --> Return back to list
                    return RedirectToAction("Index");
                }
            }

            // When not created --> Return back to create screen
            return View(product);
        }

        // GET: Products/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            // Get product
            Product product = await apiService.GetProduct(id);

            if (product == null)
                return NotFound();

            return View(product);
        }

        // POST: Products/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Price,Discount,Stock,ImgPath,Description")] Product product, string price)
        {
            if (id != product.Id)
                return NotFound();

            Decimal.TryParse(price, out var priceValue);
            product.Price = priceValue;

            if (ModelState.IsValid)
            {
                Product p = await apiService.UpdateProduct(product, id);
                if(p != product)
                    return RedirectToAction("Index");
            }

            return View(product);
        }

        // GET: Products/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            Product product = await apiService.GetProduct(id);

            if (product == null)
                return NotFound();

            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await apiService.DeleteProduct(id);
            return RedirectToAction("Index");
        }
    }
}
