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
    public class EmployeeController : ControllerBase
    {
        private readonly IConfiguration _config;

        public EmployeeController(IConfiguration config)
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
        //this function gets a List of all Employees in the database
        public async Task<IActionResult> Get()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {

                    cmd.CommandText = $@"SELECT e.Id AS EmployeeId, e.FirstName, e.LastName, 
                                e.DepartmentId, d.Name AS DepartmentName, 
                                c.Id AS ComputerId, c.Make, c.Manufacturer FROM Employee e
	                            LEFT JOIN Department d ON e.DepartmentId = d.Id
	                            LEFT JOIN ComputerEmployee ce ON e.Id = ce.EmployeeId
	                            LEFT JOIN Computer c ON ce.ComputerId = c.Id";

                    SqlDataReader reader = await cmd.ExecuteReaderAsync();
                    List<Employee> employees = new List<Employee>();

                    while (reader.Read())
                    {
                        Employee employee = new Employee
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("EmployeeId")),
                            FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                            LastName = reader.GetString(reader.GetOrdinal("LastName")),
                            Department = new Department
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("DepartmentId")),
                                Name = reader.GetString(reader.GetOrdinal("DepartmentName"))
                            },
                            Computer = new Computer
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("ComputerId")),
                                Make = reader.GetString(reader.GetOrdinal("Make")),
                                Manufacturer = reader.GetString(reader.GetOrdinal("Manufacturer"))
                            }
                        };
                        employees.Add(employee);
                    }
                    reader.Close();

                    return Ok(employees);
                }
            }
        }

        [HttpGet("{id}", Name = "GetEmployee")]
        //this function gets a single Employee from the database, by id
        public async Task<IActionResult> Get([FromRoute] int id)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = $@"SELECT e.Id AS EmployeeId, e.FirstName, e.LastName, 
                                e.DepartmentId, d.Name AS DepartmentName, 
                                c.Id AS ComputerId, c.Make, c.Manufacturer FROM Employee e
                                LEFT JOIN Department d ON e.DepartmentId = d.Id
                                LEFT JOIN ComputerEmployee ce ON e.Id = ce.EmployeeId
                                LEFT JOIN Computer c ON ce.ComputerId = c.Id
                                WHERE e.Id = @id";
                        cmd.Parameters.Add(new SqlParameter("@id", id));

                        SqlDataReader reader = await cmd.ExecuteReaderAsync();

                        Employee employee = null;

                        while (reader.Read())
                        {
                            if (employee == null)
                            {
                                employee = new Employee
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("EmployeeId")),
                                    FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                                    LastName = reader.GetString(reader.GetOrdinal("LastName")),
                                    Department = new Department
                                    {
                                        Id = reader.GetInt32(reader.GetOrdinal("DepartmentId")),
                                        Name = reader.GetString(reader.GetOrdinal("DepartmentName"))
                                    },
                                    Computer = new Computer
                                    {
                                        Id = reader.GetInt32(reader.GetOrdinal("ComputerId")),
                                        Make = reader.GetString(reader.GetOrdinal("Make")),
                                        Manufacturer = reader.GetString(reader.GetOrdinal("Manufacturer"))
                                    }
                                };
                            }
                        }
                        reader.Close();

                        if (employee == null)
                        {
                            return new StatusCodeResult(StatusCodes.Status404NotFound);
                            throw new Exception("No rows affected");
                        }
                        else
                        {
                            return Ok(employee);
                        }
                    }
                }
            }
            catch (Exception)
            {
                if (!EmployeeExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        [HttpPost]
        //this function adds a single Employee to the database
        public async Task<IActionResult> Post([FromBody] Employee employee)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO Employee (FirstName, LastName, DepartmentId)
                                        OUTPUT INSERTED.Id
                                        VALUES (@FirstName, @LastName, @DepartmentId)";
                    cmd.Parameters.Add(new SqlParameter("@FirstName", employee.FirstName));
                    cmd.Parameters.Add(new SqlParameter("@LastName", employee.LastName));
                    cmd.Parameters.Add(new SqlParameter("@DepartmentId", employee.DepartmentId));

                    int newId = (int)await cmd.ExecuteScalarAsync();
                    employee.Id = newId;
                    return CreatedAtRoute("GetEmployee", new { id = newId }, employee);
                }
            }
        }

        [HttpPut("{id}")]
        //this function updates a single Employee in the database
        public async Task<IActionResult> Put([FromRoute] int id, [FromBody] Employee employee)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"UPDATE Employee SET FirstName = @FirstName,
                                            LastName = @LastName, DepartmentId = @DepartmentId
                                            WHERE Id = @id";
                        cmd.Parameters.Add(new SqlParameter("@FirstName", employee.FirstName));
                        cmd.Parameters.Add(new SqlParameter("@LastName", employee.LastName));
                        cmd.Parameters.Add(new SqlParameter("@DepartmentId", employee.DepartmentId));
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
                if (!EmployeeExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        //Employee does NOT have a Delete function

        private bool EmployeeExists(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT Id, FirstName, LastName
                                        FROM Employee WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = cmd.ExecuteReader();
                    return reader.Read();
                }
            }
        }
    }
}