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
    /// <summary>
    /// ProductsController: A class allow developers to access the Products resource of the BangazonAPI database.
    /// Author: Brian Neal
    /// Methods: 
    ///     Get -- used to get a List of all Productss in the database
    ///     GetProducts -- used to get a single Products from the database
    ///     Post -- used to add a single Products to the database
    ///     Put -- used to update a single Products in the database
    ///     Delete -- used to remove a single PaymentType from the database
    ///     ProductsExists -- used for verification
    /// </summary>
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

        //this function gets a List of all Customers in the database
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

        //this function gets a single Customer from the database, by id
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

        //this function adds a single Products to the database
        //it takes a single parameter of type Priducts to be parsed for input
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

        //this function updates a single Product in the database
        //the id parameter indicates which database record should be updated
        //the Product type parameter contains the data to be updated into the indicated database record
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] Product product)
        {
            try
            {
                if (!ProductExists(id))
                {
                    return NotFound();
                }
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

        //this function deletes a single Product in the database
        //the id parameter indicates which database record should be deleted
       
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (!ProductExists(id))
            {
                return NotFound();
            }
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        DELETE FROM Product WHERE Id = @id
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

        //this function checks the database for the existence of a record matching the id parameter, and returns true or false
        //the id parameter indicates which database record should be pulled
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
