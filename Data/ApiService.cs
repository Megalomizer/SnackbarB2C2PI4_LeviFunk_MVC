using Newtonsoft.Json;
using NuGet.Protocol;
using SnackbarB2C2PI4_LeviFunk_ClassLibrary;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SnackbarB2C2PI4_LeviFunk_MVC.Data
{
    public class ApiService
    {
        private readonly HttpClient _client;

        public ApiService(HttpClient client)
        {
            _client = client;
        }

        #region Product CRUD

        /// <summary>
        /// Get a list of all products
        /// </summary>
        /// <returns>List(Product)</returns>
        public async Task<IEnumerable<Product>> GetProducts()
        {
            List<Product>? products = null;

            HttpResponseMessage response = await _client.GetAsync("api/Products/");

            if (response.IsSuccessStatusCode)
            {
                // Get the result --> its already in json
                var JsonResponse = response.Content.ReadAsStringAsync().Result;
                
                // convert from json to list of products
                products = JsonConvert.DeserializeObject<List<Product>>(JsonResponse);
            }

            if(products == null)
                products = new List<Product>();

            return products;
        }

        /// <summary>
        /// Get a specific product based on Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Product</returns>
        public async Task<Product> GetProduct(int? id)
        {
            Product? product = null;

            HttpResponseMessage response = await _client.GetAsync("api/Products/" + id.ToString());

            if (response.IsSuccessStatusCode)
            {
                var JsonResponseResult = response.Content.ReadAsStringAsync().Result;
                product = JsonConvert.DeserializeObject<Product>(JsonResponseResult);
            }

            if (product == null)
                product = new Product();

            return product;
        }

        /// <summary>
        /// Create a new product
        /// </summary>
        /// <param name="product"></param>
        public async Task<HttpResponseMessage> CreateProduct(Product product) // Error 400 --> Bad Request (probably json)
        {
            var productJson = product.ToJson();
            HttpContent content = new StringContent(productJson, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _client.PostAsync("api/Products/", content);

            return response; 
        }

        /// <summary>
        /// Update a product
        /// </summary>
        /// <param name="product"></param>
        /// <returns></returns>
        public async Task<Product> UpdateProduct(Product product, int id)
        {
            var productJson = product.ToJson();
            HttpContent content = new StringContent(productJson, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _client.PutAsync("api/Products/" + id.ToString(), content);

            Product p = null;

            if(response.IsSuccessStatusCode)
            {
                HttpResponseMessage getResponse = await _client.GetAsync("api/Products/" +  product.Id.ToString());

                if (getResponse.IsSuccessStatusCode)
                {
                    var JsonResponseResult = getResponse.Content.ReadAsStringAsync().Result;
                    p = JsonConvert.DeserializeObject<Product>(JsonResponseResult);
                }
            }

            if (p == null)
                p = product;

            return p;
        }

        /// <summary>
        /// Delete a product based on productId
        /// </summary>
        /// <param name="product"></param>
        public async Task DeleteProduct(int id)
        {
            HttpResponseMessage response = await _client.DeleteAsync("Api/Products/" + id.ToString());
        }

        /// <summary>
        /// Check if a product already exists
        /// </summary>
        /// <returns></returns>
        public async Task<bool> DoesProductExist(int id)
        {
            Product product = null;
            product = await GetProduct(id);
            if (product == null)
                return false;
            return true;
        }

        #endregion

        #region Order CRUD

        /// <summary>
        /// Get a list of all orders
        /// </summary>
        /// <returns>List(Order)</returns>
        public async Task<IEnumerable<Order>> GetOrders()
        {
            List<Order>? orders = null;

            HttpResponseMessage response = await _client.GetAsync("api/Orders/");

            if (response.IsSuccessStatusCode)
            {
                // Get the result --> its already in json
                var JsonResponse = response.Content.ReadAsStringAsync().Result;

                // convert from json to list of products
                orders = JsonConvert.DeserializeObject<List<Order>>(JsonResponse);
            }

            if (orders== null)
                orders = new List<Order>();

            return orders;
        }

        /// <summary>
        /// Get a list of all orders from a customer
        /// </summary>
        /// <returns>List(Order)</returns>
        public async Task<IEnumerable<Order>> GetOrders(int id)
        {
            List<Order>? orders = null;

            HttpResponseMessage response = await _client.GetAsync("api/Orders/CustomerOrders/" + id.ToString());

            if (response.IsSuccessStatusCode)
            {
                // Get the result --> its already in json
                var JsonResponse = response.Content.ReadAsStringAsync().Result;

                // convert from json to list of products
                orders = JsonConvert.DeserializeObject<List<Order>>(JsonResponse);
            }

            if (orders == null)
                orders = new List<Order>();

            return orders;
        }

        /// <summary>
        /// Get a specific order based on Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Order</returns>
        public async Task<Order> GetOrder(int? id)
        {
            // Get the order
            Order? order = null;

            HttpResponseMessage response = await _client.GetAsync("api/Orders/SpecificOrder/" + id.ToString());

            if (response.IsSuccessStatusCode)
            {
                var JsonResponseResult = response.Content.ReadAsStringAsync().Result;
                order = JsonConvert.DeserializeObject<Order>(JsonResponseResult);
            }

            if (order == null)
                order = new Order();

            // Get orderProducts
            IEnumerable<OrderProduct> orderProducts = await GetOrderProducts(order.Id);
            List<Product> products = new List<Product>();
            foreach(OrderProduct op in orderProducts)
            {
                for(int i = 0; i < op.Amount; i++)
                {
                    Product product = await GetProduct(op.ProductId);
                    products.Add(product);
                }
            }

            order.Products = products;

            return order;
        }

        /// <summary>
        /// Create a new Order
        /// </summary>
        /// <param name="order"></param>
        public async Task<HttpResponseMessage> CreateOrder(Order order)
        {
            // Convert Object to Json
            var jsonObject = JsonConvert.SerializeObject(order);
            
            // Create an Content-object with the Json data
            HttpContent content = new StringContent(jsonObject, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _client.PostAsync("api/Orders", content);

            return response;
        }

        /// <summary>
        /// Update an order but not the products
        /// </summary>
        /// <param name="product"></param>
        /// <returns></returns>
        public async Task<Order> UpdateOrder(Order order, int id)
        {
            // Update order
            var orderJson = JsonConvert.SerializeObject(order);
            HttpContent content = new StringContent(orderJson, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _client.PutAsync("api/Orders/" + id.ToString(), content);

            // return the updated order
            Order o = null;

            if (response.IsSuccessStatusCode)
            {
                HttpResponseMessage getResponse = await _client.GetAsync("api/Orders/" + order.Id.ToString());

                if (getResponse.IsSuccessStatusCode)
                {
                    var JsonResponseResult = getResponse.Content.ReadAsStringAsync().Result;
                    o = JsonConvert.DeserializeObject<Order>(JsonResponseResult);
                }
            }

            if (o == null)
                o = order;

            return o;
        }

        /// <summary>
        /// Delete an order based on OrdertId
        /// </summary>
        /// <param name="product"></param>
        public async Task DeleteOrder(int id)
        {
            await _client.DeleteAsync("Api/Orders/" + id.ToString());
        }

        /// <summary>
        /// Check if an owner already exists
        /// </summary>
        /// <returns></returns>
        public async Task<bool> DoesOrderExist(int id)
        {
            Owner owner = null;
            owner = await GetOwner(id);
            if (owner == null)
                return false;
            return true;
        }

        #endregion

        #region OrderProduct CRUD

        /// <summary>
        /// Get a list of all orderproducts for an order
        /// </summary>
        /// <param name="order"></param>
        /// <returns>List(OrderProduct)</returns>
        public async Task<IEnumerable<OrderProduct>> GetOrderProducts(int orderId)
        {
            List<OrderProduct>? orderproduct = null;

            HttpResponseMessage response = await _client.GetAsync("api/OrderProducts/" + orderId.ToString());

            if (response.IsSuccessStatusCode)
            {
                // Get the result --> its already in json
                var JsonResponse = response.Content.ReadAsStringAsync().Result;

                // convert from json to list of products
                orderproduct = JsonConvert.DeserializeObject<List<OrderProduct>>(JsonResponse);
            }

            if (orderproduct == null)
                orderproduct = new List<OrderProduct>();

            return orderproduct;
        }

        /// <summary>
        /// Create a new orderproduct
        /// </summary>
        /// <param name="orderproduct"></param>
        public async Task<HttpResponseMessage> CreateOrderProduct(OrderProduct orderproduct)
        {
            var orderproductJson = orderproduct.ToJson();
            HttpContent content = new StringContent(orderproductJson, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _client.PostAsync("api/OrderProducts/", content);

            return response;
        }

        // Maybe one for adding a list of order products?
        /// <summary>
        /// Create new orderproducts
        /// </summary>
        /// <param name="orderproducts"></param>
        /// <returns></returns>
        public async Task<HttpResponseMessage> CreateOrderProducts(List<OrderProduct> orderproducts)
        {
            var orderproductsJson = orderproducts.ToJson();
            HttpContent content = new StringContent(orderproductsJson, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _client.PostAsync("api/OrderProducts/", content);
            return response;
        }

        /// <summary>
        /// Update an orderproduct
        /// </summary>
        /// <param name="orderproduct"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<OrderProduct> UpdateOrderProduct(OrderProduct orderproduct, int id)
        {
            var orderproductJson = orderproduct.ToJson();
            HttpContent content = new StringContent(orderproductJson, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _client.PutAsync("api/OrderProducts/" + id.ToString(), content);

            OrderProduct op = null;

            if (response.IsSuccessStatusCode)
            {
                HttpResponseMessage getResponse = await _client.GetAsync("api/OrderProducts/" + orderproduct.ProductId.ToString());

                if (getResponse.IsSuccessStatusCode)
                {
                    var JsonResponseResult = getResponse.Content.ReadAsStringAsync().Result;
                    op = JsonConvert.DeserializeObject<OrderProduct>(JsonResponseResult);
                }
            }

            if (op == null)
                op = orderproduct;

            return op;
        }

        /// <summary>
        /// Update all orderproducts for a specific order
        /// </summary>
        /// <param name="orderproducts"></param>
        /// <returns></returns>
        public async Task<List<OrderProduct>> UpdateOrderProducts(List<OrderProduct> orderproducts)
        {
            var orderproductsJson = orderproducts.ToJson();
            HttpContent content = new StringContent(orderproductsJson, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _client.PutAsync("api/OrderProducts/AllOrderProducts/" + orderproducts.First().OrderId.ToString(), content);

            List<OrderProduct> orderProducts = null;
            if(response.IsSuccessStatusCode)
            {
                HttpResponseMessage getResponse = await _client.GetAsync("api/OrderProducts/" + orderProducts.First().OrderId.ToString());
                if(getResponse.IsSuccessStatusCode)
                {
                    var JsonResponseResult = getResponse.Content.ReadAsStringAsync().Result;
                    orderProducts = JsonConvert.DeserializeObject<List<OrderProduct>>(JsonResponseResult);
                }
            }

            if(orderProducts == null)
                orderProducts = new List<OrderProduct>();

            return orderProducts;
        }

        /// <summary>
        /// Delete a orderproduct based on Id
        /// </summary>
        /// <param name="id"></param>
        public async Task DeleteOrderProduct(int orderId, int productId)
        {
            await _client.DeleteAsync("Api/OrderProducts/" + orderId.ToString() + "/" + productId.ToString());
        }

        /// <summary>
        /// Check if an orderproduct already exists
        /// </summary>
        /// <returns></returns>
        public async Task<bool> DoesOrderProductExist(int id)
        {
            Order order = null;
            order = await GetOrder(id);
            if (order == null)
                return false;
            return true;
        }

        #endregion

        #region Transaction CRUD

        /// <summary>
        /// Get a list of all transactions
        /// </summary>
        /// <returns>List(Transaction)</returns>
        public async Task<IEnumerable<Transaction>> GetTransactions()
        {
            List<Transaction>? transactions = null;

            HttpResponseMessage response = await _client.GetAsync("api/Transactions/");

            if (response.IsSuccessStatusCode)
            {
                // Get the result --> its already in json
                var JsonResponse = response.Content.ReadAsStringAsync().Result;

                // convert from json to list of products
                transactions = JsonConvert.DeserializeObject<List<Transaction>>(JsonResponse);
            }

            if (transactions == null)
                transactions = new List<Transaction>();

            return transactions;
        }

        /// <summary>
        /// Get a specific transaction based on Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Transaction</returns>
        public async Task<Transaction> GetTransaction(int? id)
        {
            Transaction? transaction = null;

            HttpResponseMessage response = await _client.GetAsync("api/Transactions/" + id.ToString());

            if (response.IsSuccessStatusCode)
            {
                var JsonResponseResult = response.Content.ReadAsStringAsync().Result;
                transaction = JsonConvert.DeserializeObject<Transaction>(JsonResponseResult);
            }

            if (transaction == null)
                transaction = new Transaction();

            return transaction;
        }

        /// <summary>
        /// Create a new transaction
        /// </summary>
        /// <param name="transaction"></param>
        public async Task<HttpResponseMessage> CreateTransaction(Transaction transaction)
        {
            var transactionJson = JsonConvert.SerializeObject(transaction);
            HttpContent content = new StringContent(transactionJson, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _client.PostAsync("api/Transactions", content);

            return response;
        }

        /// <summary>
        /// Update an transaction
        /// </summary>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public async Task<Transaction> UpdateTransaction(Transaction transaction, int id)
        {
            var transactionJson = transaction.ToJson();
            HttpContent content = new StringContent(transactionJson, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _client.PutAsync("api/Transactions/" + id.ToString(), content);

            Transaction t = null;

            if (response.IsSuccessStatusCode)
            {
                HttpResponseMessage getResponse = await _client.GetAsync("api/Transactions/" + transaction.Id.ToString());

                if (getResponse.IsSuccessStatusCode)
                {
                    var JsonResponseResult = getResponse.Content.ReadAsStringAsync().Result;
                    t = JsonConvert.DeserializeObject<Transaction>(JsonResponseResult);
                }
            }

            if (t == null)
                t = transaction;

            return t;
        }

        /// <summary>
        /// Delete a transaction based on TransactionId
        /// </summary>
        /// <param name="customer"></param>
        public async Task DeleteTransaction(int id)
        {
            await _client.DeleteAsync("Api/Transactions/" + id.ToString());
        }
        
        /// <summary>
         /// Check if the transaction already exists
         /// </summary>
         /// <returns></returns>
        public async Task<bool> DoesTransactionExist(int id)
        {
            Transaction transaction = null;
            transaction = await GetTransaction(id);
            if (transaction == null)
                return false;
            return true;
        }

        #endregion

        #region Customer CRUD

        /// <summary>
        /// Get a list of all customers
        /// </summary>
        /// <returns>List(Customer)</returns>
        public async Task<IEnumerable<Customer>> GetCustomers()
        {
            List<Customer>? customers = null;

            HttpResponseMessage response = await _client.GetAsync("api/Customers/");

            if (response.IsSuccessStatusCode)
            {
                // Get the result --> its already in json
                var JsonResponse = response.Content.ReadAsStringAsync().Result;

                // convert from json to list of products
                customers = JsonConvert.DeserializeObject<List<Customer>>(JsonResponse);
            }

            if (customers == null)
                customers = new List<Customer>();

            return customers;
        }

        /// <summary>
        /// Get a specific customer based on authentication Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Customer</returns>
        public async Task<Customer> GetCustomer(string? id)
        {
            Customer? customer = null;

            HttpResponseMessage response = await _client.GetAsync("api/Customers/Authentication/" + id);

            if (response.IsSuccessStatusCode)
            {
                var JsonResponseResult = response.Content.ReadAsStringAsync().Result;
                customer = JsonConvert.DeserializeObject<Customer>(JsonResponseResult);
            }

            if (customer == null)
                customer = new Customer();

            return customer;
        }

        /// <summary>
        /// Get a specific customer based on Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Customer</returns>
        public async Task<Customer> GetCustomer(int id)
        {
            Customer? customer = null;

            HttpResponseMessage response = await _client.GetAsync("api/Customers/" + id);

            if (response.IsSuccessStatusCode)
            {
                var JsonResponseResult = response.Content.ReadAsStringAsync().Result;
                customer = JsonConvert.DeserializeObject<Customer>(JsonResponseResult);
            }

            if (customer == null)
                customer = new Customer();

            return customer;
        }

        /// <summary>
        /// Create a new customer
        /// </summary>
        /// <param name="customer"></param>
        public async Task<HttpResponseMessage> CreateCustomer(Customer customer)
        {
            var customerJson = customer.ToJson();
            HttpContent content = new StringContent(customerJson, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _client.PostAsync("api/Customers/", content);

            return response;
        }

        /// <summary>
        /// Update an customer
        /// </summary>
        /// <param name="customer"></param>
        /// <returns></returns>
        public async Task<Customer> UpdateCustomer(Customer customer, int id)
        {
            var customerJson = customer.ToJson();
            HttpContent content = new StringContent(customerJson, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _client.PutAsync("api/Customers/" + id.ToString(), content);

            Customer c = null;

            if (response.IsSuccessStatusCode)
            {
                HttpResponseMessage getResponse = await _client.GetAsync("api/Customers/" + customer.Id);

                if (getResponse.IsSuccessStatusCode)
                {
                    var JsonResponseResult = getResponse.Content.ReadAsStringAsync().Result;
                    c = JsonConvert.DeserializeObject<Customer>(JsonResponseResult);
                }
            }

            if (c == null)
                c = customer;

            return c;
        }

        /// <summary>
        /// Delete an customer based on AuthenticationId
        /// </summary>
        /// <param name="customer"></param>
        public async Task DeleteCustomer(string id)
        {
            await _client.DeleteAsync("Api/Customers/Authentication/" + id);
        }

        /// <summary>
        /// Delete an customer based on CustomerId
        /// </summary>
        /// <param name="customer"></param>
        public async Task DeleteCustomer(int id)
        {
            await _client.DeleteAsync("Api/Customers/" + id.ToString());
        }

        /// <summary>
        /// Check if a customer already exists
        /// </summary>
        /// <returns></returns>
        public async Task<bool> DoesCustomerExist(int id)
        {
            Customer customer = null;
            customer = await GetCustomer(id);
            if (customer == null)
                return false;
            return true;
        }

        #endregion

        #region Owner CRUD

        /// <summary>
        /// Get a list of all owner
        /// </summary>
        /// <returns>List(Owner)</returns>
        public async Task<IEnumerable<Owner>> GetOwners()
        {
            List<Owner>? owners = null;

            HttpResponseMessage response = await _client.GetAsync("api/Owners/");

            if (response.IsSuccessStatusCode)
            {
                // Get the result --> its already in json
                var JsonResponse = response.Content.ReadAsStringAsync().Result;

                // convert from json to list of products
                owners = JsonConvert.DeserializeObject<List<Owner>>(JsonResponse);
            }

            if (owners == null)
                owners = new List<Owner>();

            return owners;
        }

        /// <summary>
        /// Get a specific owner based on Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Owner</returns>
        public async Task<Owner> GetOwner(int? id)
        {
            Owner? owner = null;

            HttpResponseMessage response = await _client.GetAsync("api/Owners/" + id.ToString());

            if (response.IsSuccessStatusCode)
            {
                var JsonResponseResult = response.Content.ReadAsStringAsync().Result;
                owner = JsonConvert.DeserializeObject<Owner>(JsonResponseResult);
            }

            if (owner == null)
                owner = new Owner();

            return owner;
        }

        /// <summary>
        /// Create a new owner
        /// </summary>
        /// <param name="owner"></param>
        public async Task<HttpResponseMessage> CreateOwner(Owner owner)
        {
            var ownerJson = owner.ToJson();
            HttpContent content = new StringContent(ownerJson, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _client.PostAsync("api/Owners/", content);

            return response;
        }

        /// <summary>
        /// Update an owner
        /// </summary>
        /// <param name="owner"></param>
        /// <returns></returns>
        public async Task<Owner> UpdateOwner(Owner owner, int id)
        {
            var ownerJson = owner.ToJson();
            HttpContent content = new StringContent(ownerJson, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _client.PutAsync("api/Owners/" + id.ToString(), content);

            Owner o = null;

            if (response.IsSuccessStatusCode)
            {
                HttpResponseMessage getResponse = await _client.GetAsync("api/Owners/" + owner.Id.ToString());

                if (getResponse.IsSuccessStatusCode)
                {
                    var JsonResponseResult = getResponse.Content.ReadAsStringAsync().Result;
                    o = JsonConvert.DeserializeObject<Owner>(JsonResponseResult);
                }
            }

            if (o == null)
                o = owner;

            return o;
        }

        /// <summary>
        /// Delete an owner based on OwnerId
        /// </summary>
        /// <param name="owner"></param>
        public async Task DeleteOwner(int id)
        {
            await _client.DeleteAsync("Api/Owners/" + id.ToString());
        }

        /// <summary>
        /// Check if an owner already exists
        /// </summary>
        /// <returns></returns>
        public async Task<bool> DoesOwnerExist(int id)
        {
            Owner owner = null;
            owner = await GetOwner(id);
            if (owner == null)
                return false;
            return true;
        }

        #endregion
    }
}