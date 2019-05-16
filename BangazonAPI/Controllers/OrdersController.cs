using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using BangazonAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace BangazonAPI.Controllers
{
    [Route("[controller]")]  //  /orders
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IConfiguration _config;

        public OrdersController(IConfiguration config)
        {
            _config = config;
        }

        private SqlConnection Connection
        {
            get
            {
                return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            }
        }

        /* 
            Method to get all orders, with products embedded.
            query parameters:
                ?_include=customers will return buyer information on the order
                ?completed=true will return completed orders
                ?completed=false will return open orders
        */

        //GET /orders
        [HttpGet]
        public async Task<IActionResult> Get(bool? completed, string _include)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    //=============================
                    //| QUERIES AND SQL STATEMENT |
                    //=============================

                    //default SQL statement
                    string sql_select = @"SELECT o.Id, o.CustomerId BuyerId, o.PaymentTypeId,
                                        pt.AcctNumber PaymentAccount, pt.Name PaymentName,
                                        p.Id ProductId, p.Price, p.Title ProductTitle, p.Description ProductDescription, p.Quantity ProductQuantity, p.CustomerId SellerId,
                                        prodtype.Id ProductTypeId, prodtype.Name ProductTypeName,
                                        seller.FirstName SellerFirstName, seller.LastName SellerLastName
                                        ";

                    string sql_join = @" FROM [Order] o LEFT JOIN PaymentType pt ON pt.Id = o.PaymentTypeId
                                         LEFT JOIN OrderProduct op ON op.OrderId = o.Id
                                         LEFT JOIN Product p ON op.ProductId = p.Id
                                         LEFT JOIN ProductType prodtype ON p.ProductTypeId = prodtype.Id
                                         LEFT JOIN Customer seller ON seller.Id = p.CustomerId";
                    string sql_end = @" WHERE 1=1
                                        ";

                    //==================
                    //|  ?completed=   |
                    //==================
                    //'completed' query will include completed(true) or non-completed(false) orders
                    if (completed != null)
                    {
                        if (completed == false)   // ?completed=false
                        {
                            sql_end += " AND o.PaymentTypeId IS NULL";
                        }
                        else   // ?completed=true
                        {
                            sql_end += " AND o.PaymentTypeId IS NOT NULL";
                        }
                    }


                    //==================
                    //|   ?_include=   |
                    //==================
                    //'_include' query can equal 'customers' to return relevant information with the order
                    bool includeCustomers = false;

                    if (_include != null && _include == "customers") // ?_include=customers
                    {
                        includeCustomers = true;
                        sql_select += ", c.FirstName BuyerFirstName, c.LastName BuyerLastName";
                        sql_join += " JOIN Customer c ON c.Id = o.CustomerId";
                    }
                    //TODO: if an invalid query is used with 'include' throw an error?


                    //construct the final SQL statement based on queries used
                    string sql = sql_select + sql_join + sql_end;

                    cmd.CommandText = sql;
                    SqlDataReader reader = await cmd.ExecuteReaderAsync();

                    Dictionary<int, Order> ordersHash = new Dictionary<int, Order>();

                    //read through all rows of data, adding each order to the dictionary once and each individual product to the relevant order
                    while (reader.Read())
                    {
                        int orderId = reader.GetInt32(reader.GetOrdinal("Id"));

                        //set initial values to null, to be changed based on queries used
                        int? paymentTypeId = null;
                        PaymentType payment = null;
                        Customer customer = null;

                        //check for non-null payment type, and initialize PaymentType
                        if (!reader.IsDBNull(reader.GetOrdinal("PaymentTypeId")))
                        {
                            paymentTypeId = reader.GetInt32(reader.GetOrdinal("PaymentTypeId"));
                            payment = new PaymentType
                            {
                                Id = (int)paymentTypeId,
                                AcctNumber = reader.GetInt32(reader.GetOrdinal("PaymentAccount")),
                                Name = reader.GetString(reader.GetOrdinal("PaymentName")),
                                CustomerId = reader.GetInt32(reader.GetOrdinal("BuyerId")),
                                Customer = null
                            };
                        }

                        //check if customers should be included, and initialize
                        if (includeCustomers)
                        {
                            customer = new Customer
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("BuyerId")),
                                FirstName = reader.GetString(reader.GetOrdinal("BuyerFirstName")),
                                LastName = reader.GetString(reader.GetOrdinal("BuyerLastName")),
                                ProductsSelling = null,
                                PaymentTypesUsed = null
                            };
                        }

                        //initialize the new Order in the hash OR add new product to order already in the hash
                        if (!ordersHash.ContainsKey(orderId))
                        {
                            ordersHash[orderId] = new Order
                            {
                                Id = orderId,
                                CustomerId = reader.GetInt32(reader.GetOrdinal("BuyerId")),
                                Customer = customer,
                                PaymentTypeId = paymentTypeId,
                                Payment = payment,
                                Products = new List<Product>()
                            };
                        }

                        //make sure there's a product then instantiate and add product from current row to order
                        if (!reader.IsDBNull(reader.GetOrdinal("ProductId")))
                        {
                            ordersHash[orderId].Products.Add(new Product
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("ProductId")),
                                ProductTypeId = reader.GetInt32(reader.GetOrdinal("ProductTypeId")),
                                ProductType = reader.GetString(reader.GetOrdinal("ProductTypeName")),
                                CustomerId = reader.GetInt32(reader.GetOrdinal("SellerId")),
                                Customer = new Customer
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("SellerId")),
                                    FirstName = reader.GetString(reader.GetOrdinal("SellerFirstName")),
                                    LastName = reader.GetString(reader.GetOrdinal("SellerLastName")),
                                    ProductsSelling = null,
                                    PaymentTypesUsed = null
                                },
                                Price = reader.GetDecimal(reader.GetOrdinal("Price")),
                                Title = reader.GetString(reader.GetOrdinal("ProductTitle")),
                                Description = reader.GetString(reader.GetOrdinal("ProductDescription")),
                                Quantity = reader.GetInt32(reader.GetOrdinal("ProductQuantity")),
                            });
                        }

                    }

                    List<Order> orders = ordersHash.Values.ToList();

                    reader.Close();

                    return Ok(orders);
                }
            }
        }

        /* 
            Method to get a single order, with products embedded.
            query parameters:
                ?_include=customers will return buyer information on the order
        */

        // GET /orders/5
        [HttpGet("{id}", Name = "GetOrder")]
        public async Task<IActionResult> Get(int id, string _include)
        {
            if (!OrderExists(id))
            {
                return new StatusCodeResult(StatusCodes.Status404NotFound);
            }
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {

                    //=============================
                    //| QUERIES AND SQL STATEMENT |
                    //=============================

                    //default SQL statement
                    string sql_select = @"SELECT o.Id, o.CustomerId BuyerId, o.PaymentTypeId,
                                        pt.AcctNumber PaymentAccount, pt.Name PaymentName,
                                        p.Id ProductId, p.Price, p.Title ProductTitle, p.Description ProductDescription, p.Quantity ProductQuantity, p.CustomerId SellerId,
                                        prodtype.Id ProductTypeId, prodtype.Name ProductTypeName,
                                        seller.FirstName SellerFirstName, seller.LastName SellerLastName
                                        ";

                    string sql_join = @" FROM [Order] o LEFT JOIN PaymentType pt ON pt.Id = o.PaymentTypeId
                                         LEFT JOIN OrderProduct op ON op.OrderId = o.Id
                                         LEFT JOIN Product p ON op.ProductId = p.Id
                                         LEFT JOIN ProductType prodtype ON p.ProductTypeId = prodtype.Id
                                         LEFT JOIN Customer seller ON seller.Id = p.CustomerId";
                    string sql_end = @" WHERE o.Id = @id";


                    //==================
                    //|   ?_include=   |
                    //==================
                    //'_include' query can equal 'customers' to return relevant information with the order
                    bool includeCustomers = false;

                    if (_include != null && _include == "customers") // ?_include=customers
                    {
                        includeCustomers = true;
                        sql_select += ", c.FirstName BuyerFirstName, c.LastName BuyerLastName";
                        sql_join += " JOIN Customer c ON c.Id = o.CustomerId";
                    }

                    /*
                        TODO: if an invalid query is used with 'include' throw an error?
                    */

                    //construct the final SQL statement based on queries used
                    string sql = sql_select + sql_join + sql_end;

                    cmd.CommandText = sql;
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = await cmd.ExecuteReaderAsync();

                    Order order = null;
                    Dictionary<int, Order> orderHash = new Dictionary<int, Order>();

                    //read through all rows of data, adding each order to the dictionary once and each individual product to the relevant order
                    while (reader.Read())
                    {

                        int orderId = reader.GetInt32(reader.GetOrdinal("Id"));

                        //set initial values to null, to be changed based on queries used
                        int? paymentTypeId = null;
                        PaymentType payment = null;
                        Customer customer = null;

                        //check for non-null payment type, and initialize PaymentType
                        if (!reader.IsDBNull(reader.GetOrdinal("PaymentTypeId")))
                        {
                            paymentTypeId = reader.GetInt32(reader.GetOrdinal("PaymentTypeId"));
                            payment = new PaymentType
                            {
                                Id = (int)paymentTypeId,
                                AcctNumber = reader.GetInt32(reader.GetOrdinal("PaymentAccount")),
                                Name = reader.GetString(reader.GetOrdinal("PaymentName")),
                                CustomerId = reader.GetInt32(reader.GetOrdinal("BuyerId")),
                                Customer = null
                            };
                        }

                        //check if customers should be included, and initialize
                        if (includeCustomers)
                        {
                            customer = new Customer
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("BuyerId")),
                                FirstName = reader.GetString(reader.GetOrdinal("BuyerFirstName")),
                                LastName = reader.GetString(reader.GetOrdinal("BuyerLastName")),
                                ProductsSelling = null,
                                PaymentTypesUsed = null
                            };
                        }

                        //initialize the new Order in the hash OR add new product to order already in the hash
                        if (!orderHash.ContainsKey(orderId))
                        {
                            orderHash[orderId] = new Order
                            {
                                Id = orderId,
                                CustomerId = reader.GetInt32(reader.GetOrdinal("BuyerId")),
                                Customer = customer,
                                PaymentTypeId = paymentTypeId,
                                Payment = payment,
                                Products = new List<Product>()
                            };
                        }

                        //make sure there's a product then instantiate and add product from current row to order
                        if (!reader.IsDBNull(reader.GetOrdinal("ProductId")))
                        {
                            orderHash[orderId].Products.Add(new Product
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("ProductId")),
                                ProductTypeId = reader.GetInt32(reader.GetOrdinal("ProductTypeId")),
                                ProductType = reader.GetString(reader.GetOrdinal("ProductTypeName")),
                                CustomerId = reader.GetInt32(reader.GetOrdinal("SellerId")),
                                Customer = new Customer
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("SellerId")),
                                    FirstName = reader.GetString(reader.GetOrdinal("SellerFirstName")),
                                    LastName = reader.GetString(reader.GetOrdinal("SellerLastName")),
                                    ProductsSelling = null,
                                    PaymentTypesUsed = null
                                },
                                Price = reader.GetDecimal(reader.GetOrdinal("Price")),
                                Title = reader.GetString(reader.GetOrdinal("ProductTitle")),
                                Description = reader.GetString(reader.GetOrdinal("ProductDescription")),
                                Quantity = reader.GetInt32(reader.GetOrdinal("ProductQuantity")),
                            });
                        }

                        order = orderHash[orderId];
                    }

                    reader.Close();

                    return Ok(order);
                }
            }
        }


        /* 
            Method to post a new order.
        */
        // POST /orders
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Order order)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO [Order] (CustomerId, PaymentTypeId)
                        OUTPUT INSERTED.Id
                        VALUES (@CustomerId, @PaymentTypeId)
                    ";
                    cmd.Parameters.Add(new SqlParameter("@CustomerId", order.CustomerId));
                    cmd.Parameters.Add(new SqlParameter("@PaymentTypeId", order.PaymentTypeId != null ? (object)order.PaymentTypeId : DBNull.Value));

                    order.Id = (int)await cmd.ExecuteScalarAsync();

                    return CreatedAtRoute("GetProduct", new { id = order.Id }, order);
                }
            }
        }


        /*
            Method to update an existing order 
        */
        // PUT /orders/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] Order order)
        {
            try
            {
                if (!OrderExists(id))
                {
                    return NotFound();
                }
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = $@"UPDATE [Order]
                            SET CustomerId = @CustomerId,
                            PaymentTypeId = @PaymentTypeId
                            WHERE Id = @id
                        ";
                        cmd.Parameters.Add(new SqlParameter("@id", order.Id));
                        cmd.Parameters.Add(new SqlParameter("@CustomerId", order.CustomerId));
                        cmd.Parameters.Add(new SqlParameter("@PaymentTypeId", order.PaymentTypeId != null ? (object)order.PaymentTypeId : DBNull.Value));

                        int rowsAffected = await cmd.ExecuteNonQueryAsync();

                        if (rowsAffected > 0)
                        {
                            return new StatusCodeResult(StatusCodes.Status204NoContent);
                        }

                        throw new Exception("No rows affected");
                    }
                }
            }
            catch (Exception)
            {
                if (!OrderExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        /*
            Method to delete a single order, along with any OrderProducts associated with it
        */
        //DELETE api/values/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (!OrderExists(id))
            {
                return NotFound();
            }
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        DELETE FROM [Order] WHERE Id = @id
                    ";
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    int rowsAffected = await cmd.ExecuteNonQueryAsync();

                    if (rowsAffected > 0)
                    {
                        return new StatusCodeResult(StatusCodes.Status204NoContent);
                    }

                    throw new Exception("No rows affected");
                }
            }
        }

        private bool OrderExists(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    // More string interpolation
                    cmd.CommandText = "SELECT Id FROM [Order] WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = cmd.ExecuteReader();

                    return reader.Read();
                }
            }
        }
    }
}
