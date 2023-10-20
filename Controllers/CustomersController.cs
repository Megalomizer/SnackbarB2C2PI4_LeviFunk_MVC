using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SnackbarB2C2PI4_LeviFunk_ClassLibrary;
using SnackbarB2C2PI4_LeviFunk_MVC.Data;
using SnackbarB2C2PI4_LeviFunk_MVC.Models;

namespace SnackbarB2C2PI4_LeviFunk_MVC.Controllers
{
    [Authorize]
    public class CustomersController : Controller
    {
        private readonly ApiService _apiService;

        public CustomersController(ApiService apiService)
        {
            _apiService = apiService;
        }

        // GET: Customers
        public async Task<IActionResult> Index()
        {
            Customer customer = await _apiService.GetCustomer(User.FindFirstValue(ClaimTypes.NameIdentifier));

            if(customer.FirstName == null)
                return RedirectToAction("Create");

            CustomersVMIndex customerVM = new CustomersVMIndex()
            {
                Customer = customer
            };

            return View(customerVM);
        }

        // GET: Customers/Details/5
        public async Task<IActionResult> Details(int id)
        {
            Customer customer = await _apiService.GetCustomer(id);

            if (customer == null)
                return NotFound();

            CustomersVMIndex customerVM = new CustomersVMIndex() { Customer = customer };

            return View(customerVM);
        }

        // GET: Customers/Create
        public IActionResult Create()
        {
            Customer customer = new Customer()
            {
                Email = User.Identity.Name,
                AuthenticationId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            };

            return View(customer);
        }

        // POST: Customers/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,FirstName,LastName,Email,Phone,AuthenticationId")] Customer customer)
        {
            if (ModelState.IsValid)
            {
                await _apiService.CreateCustomer(customer);
                return RedirectToAction("Index");
            }

            return View(customer);
        }

        // GET: Customers/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            if (id == null)
                return NotFound();

            Customer customer = await _apiService.GetCustomer(id);

            if (customer == null)
                return RedirectToAction("Create");

            CustomersVMIndex customerVM = new()
            {
                Customer = customer
            };

            return View(customerVM);
        }

        // POST: Customers/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,AuthenticationId,Email,FirstName,LastName,Phone")] Customer customer)
        {
            if (id != customer.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                await _apiService.UpdateCustomer(customer, customer.Id);
                return RedirectToAction("Index");
            }
            
            return View(customer);
        }

        // GET: Customers/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            if (id == null)
                return NotFound();

            Customer customer = await _apiService.GetCustomer(id);

            if (customer == null)
                return NotFound();

            CustomersVMIndex customerVM = new CustomersVMIndex()
            {
                Customer = customer
            };

            return View(customerVM);
        }

        // POST: Customers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _apiService.DeleteCustomer(id);

            return RedirectToAction("Index");
        }

        private async Task<bool> CustomerExists(int id)
        {
            return await _apiService.DoesCustomerExist(id);
        }
    }
}
