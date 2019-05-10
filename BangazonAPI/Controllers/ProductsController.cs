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
    public class ProductsController : ControllerBase
    {
        private readonly IConfiguration _config;

        public ProductsController(IConfiguration config)
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
        public async Task<IActionResult> Get()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {

                    string sql = @"SELECT p.Id ProductId, pt.Id ProductTypeId, pt.Name ProductTypeName, p.Price, p.Title, p.Description, p.Quantity, p.CustomerId, c.FirstName, c.LastName
                                    FROM Product p
                                    JOIN ProductType pt ON p.ProductTypeId = pt.Id
                                    JOIN Customer c ON c.Id = p.CustomerId";

                    cmd.CommandText = sql;

                    SqlDataReader reader = await cmd.ExecuteReaderAsync();

                    List<Product> products = new List<Product>();

                    while (reader.Read())
                    {
                        Product product = new Product
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("ProductId")),
                            ProductTypeId = reader.GetInt32(reader.GetOrdinal("ProductTypeId")),
                            ProductType = reader.GetString(reader.GetOrdinal("ProductTypeName")),
                            CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId")),
                            Customer = new Customer
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("CustomerId")),
                                FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                                LastName = reader.GetString(reader.GetOrdinal("LastName")),
                                ProductsSelling = new List<Product>(),
                                PaymentTypesUsed = new List<PaymentType>()
                            },
                            Price = reader.GetDecimal(reader.GetOrdinal("Price")),
                            Title = reader.GetString(reader.GetOrdinal("Title")),
                            Description = reader.GetString(reader.GetOrdinal("Description")),
                            Quantity = reader.GetInt32(reader.GetOrdinal("Quantity"))
                        };

                        products.Add(product);
                    }

                    reader.Close();

                    return Ok(products);
                }
            }
        }

        // GET /values/5
        [HttpGet("{id}", Name = "GetProduct")]
        public async Task<IActionResult> Get(int id)
        {
            if (!ProductExists(id))
            {
                return new StatusCodeResult(StatusCodes.Status404NotFound);
            }
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {

                    string sql = @"SELECT p.Id ProductId, pt.Id ProductTypeId, pt.Name ProductTypeName, p.Price, p.Title, p.Description, p.Quantity, p.CustomerId, c.FirstName, c.LastName
                                    FROM Product p
                                    JOIN ProductType pt ON p.ProductTypeId = pt.Id
                                    JOIN Customer c ON c.Id = p.CustomerId
                                    WHERE p.Id = @id";

                    cmd.CommandText = sql;
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = await cmd.ExecuteReaderAsync();

                    Product product = null;
                    if (reader.Read())
                    {
                        product = new Product
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("ProductId")),
                            ProductTypeId = reader.GetInt32(reader.GetOrdinal("ProductTypeId")),
                            ProductType = reader.GetString(reader.GetOrdinal("ProductTypeName")),
                            CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId")),
                            Customer = new Customer
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("CustomerId")),
                                FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                                LastName = reader.GetString(reader.GetOrdinal("LastName")),
                                ProductsSelling = new List<Product>(),
                                PaymentTypesUsed = new List<PaymentType>()
                            },
                            Price = reader.GetDecimal(reader.GetOrdinal("Price")),
                            Title = reader.GetString(reader.GetOrdinal("Title")),
                            Description = reader.GetString(reader.GetOrdinal("Description")),
                            Quantity = reader.GetInt32(reader.GetOrdinal("Quantity"))
                        };
                    }

                    reader.Close();

                    return Ok(product);
                }
            }
        }

        // POST /values
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Product product)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    // More string interpolation
                    cmd.CommandText = @"
                        INSERT INTO Product (ProductTypeId, CustomerId, Price, Title, Description, Quantity)
                        OUTPUT INSERTED.Id
                        VALUES (@ProductTypeId, @CustomerId, @Price, @Title, @Description, @Quantity)
                    ";
                    cmd.Parameters.Add(new SqlParameter("@ProductTypeId", product.ProductTypeId));
                    cmd.Parameters.Add(new SqlParameter("@CustomerId", product.CustomerId));
                    cmd.Parameters.Add(new SqlParameter("@Price", product.Price));
                    cmd.Parameters.Add(new SqlParameter("@Title", product.Title));
                    cmd.Parameters.Add(new SqlParameter("@Description", product.Description));
                    cmd.Parameters.Add(new SqlParameter("@Quantity", product.Quantity));

                    product.Id = (int)await cmd.ExecuteScalarAsync();

                    return CreatedAtRoute("GetProduct", new { id = product.Id }, product);
                }
            }
        }

        // PUT /values/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] Product product)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"
                            UPDATE Product
                            SET ProductTypeId = @ProductTypeId,
                            CustomerId = @CustomerId,
                            Price = @Price,
                            Title = @Title,
                            Description = @Description,
                            Quantity = @Quantity
                            WHERE Id = @id
                        ";
                        cmd.Parameters.Add(new SqlParameter("@id", product.Id));
                        cmd.Parameters.Add(new SqlParameter("@ProductTypeId", product.ProductTypeId));
                        cmd.Parameters.Add(new SqlParameter("@CustomerId", product.CustomerId));
                        cmd.Parameters.Add(new SqlParameter("@Price", product.Price));
                        cmd.Parameters.Add(new SqlParameter("@Title", product.Title));
                        cmd.Parameters.Add(new SqlParameter("@Description", product.Description));
                        cmd.Parameters.Add(new SqlParameter("@Quantity", product.Quantity));

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
                if (!ProductExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        // DELETE api/values/5
        //[HttpDelete("{id}")]
        //public async Task<IActionResult> Delete(int id)
        //{
        //}

        private bool ProductExists(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    // More string interpolation
                    cmd.CommandText = "SELECT Id FROM Product WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = cmd.ExecuteReader();

                    return reader.Read();
                }
            }
        }
    }
}
