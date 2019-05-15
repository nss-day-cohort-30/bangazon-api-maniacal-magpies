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
    [Route("[controller]")]
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

        // GET /values
        [HttpGet]
        public async Task<IActionResult> Get(bool? completed, string _include)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    //===========================
                    //|      GET QUERIES        |
                    //===========================

                    //default SQL statement
                    string sql_select = @"SELECT o.Id, o.CustomerId, o.PaymentTypeId,
                                        pt.AcctNumber PaymentAccount, pt.Name PaymentName
                                        ";

                    string sql_join = @" FROM [Order] o LEFT JOIN PaymentType pt ON pt.Id = o.PaymentTypeId";
                    string sql_end = @" WHERE 1=1";

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

                    //'_include' query can equal 'products' and/or 'customers' to return relevant information with the order

                    bool includeCustomers = false;
                    bool includeProducts = false;

                    if (_include != null)
                    {
                        if (_include == "customers")  // ?_include=customers
                        {
                            includeCustomers = true;
                            sql_select += ", c.FirstName, c.LastName";
                            sql_join += " JOIN Customer c ON c.Id = o.CustomerId";
                        }
                        else if (_include == "products")  // ?_include=products
                        {
                            includeProducts = true;
                            sql_select += @", c.FirstName, c.LastName,
                                          p.Id ProductId, p.Price, p.Title ProductTitle, p.Description ProductDescription, p.Quantity ProductQuantity, p.CustomerId SellerId,
                                          prodtype.Id ProductTypeId, prodtype.Name ProductTypeName";
                            sql_join += @" JOIN Customer c ON c.Id = o.CustomerId
                                          JOIN OrderProduct op ON op.OrderId = o.Id
                                          JOIN Product p ON op.ProductId = p.Id
                                          JOIN ProductType prodtype ON p.ProductTypeId = prodtype.Id";
                        }
                        //TODO: if an invalid query is used with 'include' throw an error?

                    }

                    //construct the final SQL statement based on queries used
                    string sql = sql_select + sql_join + sql_end;

                    cmd.CommandText = sql;

                    SqlDataReader reader = await cmd.ExecuteReaderAsync();

                    Dictionary<int, Order> ordersHash = new Dictionary<int, Order>();

                    while (reader.Read())
                    {
                        //id of current order for hash usage
                        int orderId = reader.GetInt32(reader.GetOrdinal("Id"));

                        //set initial values to null, to be changed based on queries used
                        int? paymentTypeId = null;
                        PaymentType payment = null;
                        Customer customer = null;
                        List<Product> products = null;

                        //check for non-null payment type, and initialize PaymentType
                        if (!reader.IsDBNull(reader.GetOrdinal("PaymentTypeId")))
                        {
                            paymentTypeId = reader.GetInt32(reader.GetOrdinal("PaymentTypeId"));
                            payment = new PaymentType
                            {
                                Id = (int)paymentTypeId,
                                AcctNumber = reader.GetInt32(reader.GetOrdinal("PaymentAccount")),
                                Name = reader.GetString(reader.GetOrdinal("PaymentName")),
                                CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId")),
                                Customer = null
                            };
                        }

                        //check if customers should be included, and initialize
                        if (includeCustomers)
                        {
                            customer = new Customer
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("CustomerId")),
                                FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                                LastName = reader.GetString(reader.GetOrdinal("LastName")),
                                ProductsSelling = null,
                                PaymentTypesUsed = null
                            };
                        }

                        if (includeProducts)
                        {
                            products = new List<Product>();
                        }

                        //initialize the new Order in the hash OR add new product to order already in the hash
                        if (!ordersHash.ContainsKey(orderId))
                        {
                            ordersHash[orderId] = new Order
                            {
                                Id = orderId,
                                CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId")),
                                Customer = customer,
                                PaymentTypeId = paymentTypeId,
                                Payment = payment,
                                Products = products
                            };
                        }

                        //check if products should be included, and use hash table to populate List
                        if (includeProducts)
                        {
                            ordersHash[orderId].Products.Add(new Product
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("ProductId")),
                                ProductTypeId = reader.GetInt32(reader.GetOrdinal("ProductTypeId")),
                                ProductType = reader.GetString(reader.GetOrdinal("ProductTypeName")),
                                CustomerId = reader.GetInt32(reader.GetOrdinal("SellerId")),
                                Customer = null,
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

        // GET /values/5
        [HttpGet("{id}", Name = "GetOrder")]
        public async Task<IActionResult> Get(int id)
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

                    //===========================
                    //|    GET QUERIES          |
                    //===========================
                    //construct the SQL statement based on queries used
                    string sql_head = @"SELECT o.Id, o.CustomerId, o.PaymentTypeId FROM [Order] o";
                    string sql_end = @" WHERE o.Id = @id";
                    string sql = sql_head + sql_end;

                    cmd.CommandText = sql;
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = await cmd.ExecuteReaderAsync();

                    Order order = null;

                    if (reader.Read())
                    {

                        // set initial values to null, to be changed based on queries used
                        int? paymentTypeId = null;
                        PaymentType payment = null;
                        Customer customer = null;
                        List<Product> products = null;

                        if (!reader.IsDBNull(reader.GetOrdinal("PaymentTypeId")))
                        {
                            paymentTypeId = reader.GetInt32(reader.GetOrdinal("PaymentTypeId"));
                        }

                        order = new Order
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId")),
                            Customer = customer,
                            PaymentTypeId = paymentTypeId,
                            Payment = payment,
                            Products = products
                        };
                    }

                    reader.Close();

                    return Ok(order);
                }
            }
        }

        //// POST /values
        //[HttpPost]
        //public async Task<IActionResult> Post([FromBody] Product product)
        //{
        //    using (SqlConnection conn = Connection)
        //    {
        //        conn.Open();
        //        using (SqlCommand cmd = conn.CreateCommand())
        //        {
        //            // More string interpolation
        //            cmd.CommandText = @"
        //                INSERT INTO Product (ProductTypeId, CustomerId, Price, Title, Description, Quantity)
        //                OUTPUT INSERTED.Id
        //                VALUES (@ProductTypeId, @CustomerId, @Price, @Title, @Description, @Quantity)
        //            ";
        //            cmd.Parameters.Add(new SqlParameter("@ProductTypeId", product.ProductTypeId));
        //            cmd.Parameters.Add(new SqlParameter("@CustomerId", product.CustomerId));
        //            cmd.Parameters.Add(new SqlParameter("@Price", product.Price));
        //            cmd.Parameters.Add(new SqlParameter("@Title", product.Title));
        //            cmd.Parameters.Add(new SqlParameter("@Description", product.Description));
        //            cmd.Parameters.Add(new SqlParameter("@Quantity", product.Quantity));

        //            product.Id = (int)await cmd.ExecuteScalarAsync();

        //            return CreatedAtRoute("GetProduct", new { id = product.Id }, product);
        //        }
        //    }
        //}

        //// PUT /values/5
        //[HttpPut("{id}")]
        //public async Task<IActionResult> Put(int id, [FromBody] Product product)
        //{
        //    try
        //    {
        //        if (!OrderExists(id))
        //        {
        //            return NotFound();
        //        }
        //        using (SqlConnection conn = Connection)
        //        {
        //            conn.Open();
        //            using (SqlCommand cmd = conn.CreateCommand())
        //            {
        //                cmd.CommandText = @"
        //                    UPDATE Product
        //                    SET ProductTypeId = @ProductTypeId,
        //                    CustomerId = @CustomerId,
        //                    Price = @Price,
        //                    Title = @Title,
        //                    Description = @Description,
        //                    Quantity = @Quantity
        //                    WHERE Id = @id
        //                ";
        //                cmd.Parameters.Add(new SqlParameter("@id", product.Id));
        //                cmd.Parameters.Add(new SqlParameter("@ProductTypeId", product.ProductTypeId));
        //                cmd.Parameters.Add(new SqlParameter("@CustomerId", product.CustomerId));
        //                cmd.Parameters.Add(new SqlParameter("@Price", product.Price));
        //                cmd.Parameters.Add(new SqlParameter("@Title", product.Title));
        //                cmd.Parameters.Add(new SqlParameter("@Description", product.Description));
        //                cmd.Parameters.Add(new SqlParameter("@Quantity", product.Quantity));

        //                int rowsAffected = await cmd.ExecuteNonQueryAsync();

        //                if (rowsAffected > 0)
        //                {
        //                    return new StatusCodeResult(StatusCodes.Status204NoContent);
        //                }

        //                throw new Exception("No rows affected");
        //            }
        //        }
        //    }
        //    catch (Exception)
        //    {
        //        if (!OrderExists(id))
        //        {
        //            return NotFound();
        //        }
        //        else
        //        {
        //            throw;
        //        }
        //    }
        //}

        ////DELETE api/values/5
        //[HttpDelete("{id}")]
        //public async Task<IActionResult> Delete(int id)
        //{
        //    if (!OrderExists(id))
        //    {
        //        return NotFound();
        //    }
        //    using (SqlConnection conn = Connection)
        //    {
        //        conn.Open();
        //        using (SqlCommand cmd = conn.CreateCommand())
        //        {
        //            cmd.CommandText = @"
        //                DELETE FROM Product WHERE Id = @id
        //            ";
        //            cmd.Parameters.Add(new SqlParameter("@id", id));

        //            int rowsAffected = await cmd.ExecuteNonQueryAsync();

        //            if (rowsAffected > 0)
        //            {
        //                return new StatusCodeResult(StatusCodes.Status204NoContent);
        //            }

        //            throw new Exception("No rows affected");
        //        }
        //    }
        //}

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
