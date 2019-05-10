using System;
using System.Collections.Generic;
using System.Data;
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
    public class CustomerController : ControllerBase
    {
        private readonly IConfiguration _config;

        public CustomerController(IConfiguration config)
        {
            _config = config;
        }

        public SqlConnection Connection
        {
            get
            {
                return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            }
        }

        [HttpGet]
        //this function gets a List of all Customers in the database
        public async Task<IActionResult> Get(string _include, string q)
        {
            //create the SQL as a string, in order to be able to add to it with the 'include' queries
            string sql_head = "SELECT c.Id, c.FirstName, c.LastName";
            string sql_end = "FROM Customer c";
            string sql = $"{sql_head} {sql_end}";

            if (_include == "products") //?_include=product
            {
                string sql_product_middle = @", p.Id AS ProductId, p.Price, p.Title, p.[Description], p.Quantity, p.ProductTypeId AS TypeId, pt.Name AS ProductType";
                string sql_product_end = @"JOIN Product p ON c.Id = p.CustomerId
                    JOIN ProductType pt ON p.ProductTypeId = pt.Id";
                sql = $"{sql_head} {sql_product_middle} {sql_end} {sql_product_end}";
            }
            else if (_include == "payments") //?_include=payments
            {
                string sql_payments_middle = ", pt.Id AS PaymentId, pt.Name, pt.AcctNumber";
                string sql_payments_end = @"JOIN PaymentType pt ON c.Id = pt.CustomerId
                    JOIN [Order] o ON pt.Id = o.PaymentTypeId";
                sql = $"{sql_head} {sql_payments_middle} {sql_end} {sql_payments_end}";
            }

            if (q != null) //?q=
            {
                string sql_q_middle = @" WHERE c.LastName LIKE @q
                    OR c.FirstName LIKE @q";
                sql = $"{sql_head} {sql_end} {sql_q_middle}";
            }

            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                     
                    cmd.CommandText = sql;
                    if (q != null)
                    {
                        cmd.Parameters.Add(new SqlParameter("@q", $"%{q}%"));

                    }

                    SqlDataReader reader = await cmd.ExecuteReaderAsync();
                    List<Customer> customers = new List<Customer>();

                    Dictionary<int, Customer> customerHash = new Dictionary<int, Customer>();

                    while (reader.Read())
                    {
                        int customerId = reader.GetInt32(reader.GetOrdinal("Id"));
                        if (!customerHash.ContainsKey(customerId))
                        {
                            customerHash[customerId] = new Customer
                            {
                                Id = customerId,
                                FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                                LastName = reader.GetString(reader.GetOrdinal("LastName")),
                                ProductsSelling = new List<Product>(),
                                PaymentTypesUsed = new List<PaymentType>()
                            };
                        };

                        if (_include == "products")
                        {
                            customerHash[customerId].ProductsSelling.Add(new Product
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("ProductId")),
                                Title = reader.GetString(reader.GetOrdinal("Title")),
                                Description = reader.GetString(reader.GetOrdinal("Description")),
                                ProductTypeId = reader.GetInt32(reader.GetOrdinal("TypeId")),
                                ProductType = reader.GetString(reader.GetOrdinal("ProductType")),
                                Price = reader.GetDecimal(reader.GetOrdinal("Price")),
                                Quantity = reader.GetInt32(reader.GetOrdinal("Quantity"))
                            });
                        };

                        if(_include == "payments")
                        {
                            customerHash[customerId].PaymentTypesUsed.Add(new PaymentType
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("PaymentId")),
                                Name = reader.GetString(reader.GetOrdinal("Name")),
                                AcctNumber = reader.GetInt32(reader.GetOrdinal("AcctNumber"))
                            });
                        };

                        customers = customerHash.Values.ToList();
                    }
                    reader.Close();

                    return Ok(customers);
                }
            }
        }

        [HttpGet("{id}", Name = "GetCustomer")]
        //this function gets a single Customer from the database, by id
        // TODO: add 'include' queries
        public async Task<IActionResult> Get([FromRoute] int id, string _include)
        {
            //create the SQL as a string, in order to be able to add to it with the 'include' queries
            string sql_head = "SELECT c.Id, c.FirstName, c.LastName";
            string sql_end = "FROM Customer c WHERE c.Id = @id";
            string sql = $"{sql_head} {sql_end}";

            if (_include == "products") //?_include=product
            {
                string sql_product_middle = @", p.Id AS ProductId, p.Price, p.Title, p.[Description], p.Quantity, p.ProductTypeId AS TypeId, pt.Name AS ProductType";
                string sql_product_end = @"JOIN Product p ON c.Id = p.CustomerId
                    JOIN ProductType pt ON p.ProductTypeId = pt.Id";
                sql = $"{sql_head} {sql_product_middle} {sql_end} {sql_product_end}";
            }
            else if (_include == "payments") //?_include=payments
            {
                string sql_payments_middle = ", pt.Id AS PaymentId, pt.Name, pt.AcctNumber";
                string sql_payments_end = @" JOIN PaymentType pt ON c.Id = pt.CustomerId
                    JOIN [Order] o ON pt.Id = o.PaymentTypeId";
                sql = $"{sql_head} {sql_payments_middle} {sql_end} {sql_payments_end}";
            }
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = sql;
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = await cmd.ExecuteReaderAsync();

                    Customer customer = null;

                    while (reader.Read())
                    {
                        if (customer == null)
                        {
                            customer = new Customer
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                                LastName = reader.GetString(reader.GetOrdinal("LastName")),
                                ProductsSelling = new List<Product>(),
                                PaymentTypesUsed = new List<PaymentType>()
                            };
                        }
                        if (_include == "products")
                        {
                            customer.ProductsSelling.Add(new Product
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("ProductId")),
                                Title = reader.GetString(reader.GetOrdinal("Title")),
                                Description = reader.GetString(reader.GetOrdinal("Description")),
                                ProductTypeId = reader.GetInt32(reader.GetOrdinal("TypeId")),
                                ProductType = reader.GetString(reader.GetOrdinal("ProductType")),
                                Price = reader.GetDecimal(reader.GetOrdinal("Price")),
                                Quantity = reader.GetInt32(reader.GetOrdinal("Quantity"))
                            });
                        }

                        if (_include == "payments")
                        {
                            customer.PaymentTypesUsed.Add(new PaymentType
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("PaymentId")),
                                Name = reader.GetString(reader.GetOrdinal("Name")),
                                AcctNumber = reader.GetInt32(reader.GetOrdinal("AcctNumber"))
                            });
                        }
                    }
                    reader.Close();

                    return Ok(customer);
                }
            }
        }

        [HttpPost]
        //this function adds a single Customer to the database
        public async Task<IActionResult> Post([FromBody] Customer customer)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO Customer (FirstName, LastName)
                                        OUTPUT INSERTED.Id
                                        VALUES (@FirstName, @LastName)";
                    cmd.Parameters.Add(new SqlParameter("@FirstName", customer.FirstName));
                    cmd.Parameters.Add(new SqlParameter("@LastName", customer.LastName));

                    int newId = (int)await cmd.ExecuteScalarAsync();
                    customer.Id = newId;
                    return CreatedAtRoute("GetCustomer", new { id = newId }, customer);
                }
            }
        }

        [HttpPut("{id}")]
        //this function updates a single Customer in the database
        public async Task<IActionResult> Put([FromRoute] int id, [FromBody] Customer customer)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"UPDATE Customer SET FirstName = @FirstName,
                                            LastName = @LastName WHERE Id = @id";
                        cmd.Parameters.Add(new SqlParameter("@FirstName", customer.FirstName));
                        cmd.Parameters.Add(new SqlParameter("@LastName", customer.LastName));

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
                if (!CustomerExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        //Customer does NOT have a Delete function

        private bool CustomerExists(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT Id, FirstName, LastName
                                        FROM Customer WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = cmd.ExecuteReader();
                    return reader.Read();
                }
            }
        }
    }
}