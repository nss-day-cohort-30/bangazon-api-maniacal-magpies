using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using BangazonAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace BangazonAPI.Controllers
{
    /// <summary>
    /// ComputerController: A class allow developers to access the Computer resource of the BangazonAPI database.
    /// Author: Panya Farnette
    /// Methods: 
    ///     Get -- used to get a List of all Computers in the database
    ///     GetComputer -- used to get a single Computer from the database
    ///     Post -- used to add a single Computer to the database
    ///     Put -- used to update a single Computer in the database
    ///     Delete -- used to remove a single Computer from the database
    ///     ComputerExists -- used for verification
    /// </summary>
    [Route("[controller]")]
    [ApiController]
    public class ComputerController : ControllerBase
    {
        private readonly IConfiguration _config;

        public ComputerController(IConfiguration config)
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
        //this function gets a List of all Computers in the database
        public async Task<IActionResult> Get()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = $@"SELECT c.Id, c.Make, c.Manufacturer, c.PurchaseDate
                                FROM Computer c";

                    SqlDataReader reader = await cmd.ExecuteReaderAsync();
                    List<Computer> computers = new List<Computer>();

                    while (reader.Read())
                    {
                        Computer computer = new Computer
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Make = reader.GetString(reader.GetOrdinal("Make")),
                            Manufacturer = reader.GetString(reader.GetOrdinal("Manufacturer")),
                            PurchaseDate = reader.GetDateTime(reader.GetOrdinal("PurchaseDate"))
                        };

                        computers.Add(computer);
                    }
                    reader.Close();

                    return Ok(computers);
                }
            }
        }

        [HttpGet("{id}", Name = "GetComputer")]
        //this function gets a single Computer from the database, by id
        public async Task<IActionResult> Get([FromRoute] int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = $@"SELECT c.Id, c.Make, c.Manufacturer, c.PurchaseDate
                                FROM Computer c WHERE id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = await cmd.ExecuteReaderAsync();

                    Computer computer = null;

                    if (reader.Read())
                    {
                        if (computer == null)
                        {
                            computer = new Computer
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                Make = reader.GetString(reader.GetOrdinal("Make")),
                                Manufacturer = reader.GetString(reader.GetOrdinal("Manufacturer")),
                                PurchaseDate = reader.GetDateTime(reader.GetOrdinal("PurchaseDate"))
                            };
                        }
                    }
                    reader.Close();

                    if (computer == null)
                    {
                        return new StatusCodeResult(StatusCodes.Status404NotFound);
                        throw new Exception("No rows affected");
                    }
                    else
                    {
                        return Ok(computer);
                    }
                }
            }
        }

        [HttpPost]
        //this function adds a single Computer to the database
        public async Task<IActionResult> Post([FromBody] Computer computer)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = $@"INSERT INTO Computer (Make, Manufacturer, PurchaseDate)
                                    OUTPUT INSERTED.Id
	                                VALUES (@Make, @Manufacturer, @PurchaseDate)";
                    cmd.Parameters.Add(new SqlParameter("@Make", computer.Make));
                    cmd.Parameters.Add(new SqlParameter("@Manufacturer", computer.Manufacturer));
                    cmd.Parameters.Add(new SqlParameter("@PurchaseDate", computer.PurchaseDate));

                    int newId = (int)await cmd.ExecuteScalarAsync();
                    computer.Id = newId;
                    return CreatedAtRoute("GetComputer", new { id = newId }, computer);
                }
            }
        }

        [HttpPut("{id}")]
        //this function updates a single Computer in the database
        public async Task<IActionResult> Put([FromRoute] int id, [FromBody] Computer computer)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = $@"UPDATE Computer SET Make = @Make,
                            Manufacturer = @Manufacturer, PurchaseDate = @PurchaseDate
                            WHERE id = @id";
                        cmd.Parameters.Add(new SqlParameter("@id", id));
                        cmd.Parameters.Add(new SqlParameter("@Make", computer.Make));
                        cmd.Parameters.Add(new SqlParameter("@Manufacturer", computer.Manufacturer));
                        cmd.Parameters.Add(new SqlParameter("@PurchaseDate", computer.PurchaseDate));

                        int rowsAffected = await cmd.ExecuteNonQueryAsync();
                        if (rowsAffected > 0) {
                            return new StatusCodeResult(StatusCodes.Status204NoContent);
                        }
                        throw new Exception("No rows affected");
                    }
                }
            }
            catch (Exception)
            {
                if (!ComputerExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        [HttpDelete("{id}")]
        //this function deletes a single Computer from the database
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = $"DELETE FROM Computer WHERE Id = @id";
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
            catch (Exception)
            {
                if (!ComputerExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        private bool ComputerExists(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = $@"SELECT Id, Make, Manufacturer, PurchaseDate 
                                    FROM Computer WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = cmd.ExecuteReader();
                    return reader.Read();
                }
            }
        }
    }
}
