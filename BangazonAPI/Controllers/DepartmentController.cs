using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Data.SqlClient;
using BangazonAPI.Models;
using Microsoft.AspNetCore.Http;

namespace BangazonAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class DepartmentController : ControllerBase
    {
        private readonly IConfiguration _config;

        public DepartmentController(IConfiguration config)
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
        public async Task<IActionResult> Get_All_Departments(int budget, string departmentName)
        {
            string sql = @"SELECT 
                            d.Id, d.Name, d.budget,
                            e.Id, e.FirstName, e.LastName,
                            e.DepartmentId, e.IsSupervisor
                        FROM Department d
                        JOIN Employee e ON e.DepartmentId = d.Id
                        WHERE 2 = 2";                   
                       

            if (departmentName != null)
            {
                sql = $"{sql} AND d.Name = @departmentName";
            }

            if (budget != 0)
            {
                sql = $@"{sql} AND d.budget >= @budget";         
                    
            }

           using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = sql;
                    if (departmentName != null)
                    {
                        cmd.Parameters.Add(new SqlParameter("@departmentName", $"%{departmentName}%"));
                    }
                    if (budget != 0)
                    {
                        cmd.Parameters.Add(new SqlParameter("@budget", $"%{budget}%"));

                    }
                    SqlDataReader reader = await cmd.ExecuteReaderAsync();

                    Dictionary<int, Department> departmentHash = new Dictionary<int, Department>();

                    while (reader.Read())
                    {
                        int departmentId = reader.GetInt32(reader.GetOrdinal("Id"));

                        if (!departmentHash.ContainsKey(departmentId))
                        {
                            departmentHash[departmentId] = new Department
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                Name = reader.GetString(reader.GetOrdinal("Name")),
                                Budget = reader.GetInt32(reader.GetOrdinal("Budget")),
                                Employees = new List<Employee>
                                {
                                    
                                }
                                                               
                            };
                        }

                     }
                    List<Department> departments = departmentHash.Values.ToList();
                    reader.Close();

                    return Ok(departments);
                }
            }
        }

        //[HttpGet("{id}", Name = "GetPaymentType")]
        //public async Task<IActionResult> Get([FromRoute] int id)
        //{
        //    if (!PaymentTypeExists(id))
        //    {
        //        return new StatusCodeResult(StatusCodes.Status404NotFound);
        //    }
        //    using (SqlConnection conn = Connection)
        //    {
        //        conn.Open();
        //        using (SqlCommand cmd = conn.CreateCommand())
        //        {
        //            cmd.CommandText = @"SELECT
        //                            pt.Id, pt.AcctNumber, pt.Name, pt.CustomerId,
        //                            c.Id, c.FirstName, c.LastName
        //                            FROM PaymentType pt
        //                            JOIN Customer c ON pt.CustomerId = c.Id 
        //                            WHERE pt.Id = @id";
        //            cmd.Parameters.Add(new SqlParameter("@id", id));
        //            SqlDataReader reader = await cmd.ExecuteReaderAsync();

        //            PaymentType paymentType = null;

        //            while (reader.Read())
        //            {
        //                paymentType = new PaymentType
        //                {
        //                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
        //                    AcctNumber = reader.GetInt32(reader.GetOrdinal("AcctNumber")),
        //                    Name = reader.GetString(reader.GetOrdinal("Name")),
        //                    CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId")),
        //                    Customer = new Customer
        //                    {
        //                        Id = reader.GetInt32(reader.GetOrdinal("Id")),
        //                        FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
        //                        LastName = reader.GetString(reader.GetOrdinal("LastName")),
        //                    }
        //                };
        //            }
        //            reader.Close();

        //            return Ok(paymentType);
        //        }
        //    }
        //}

        //[HttpPost]
        //public async Task<IActionResult> Post([FromBody] PaymentType paymentType)
        //{
        //    using (SqlConnection conn = Connection)
        //    {
        //        conn.Open();
        //        using (SqlCommand cmd = conn.CreateCommand())
        //        {
        //            cmd.CommandText = @"INSERT INTO PaymentType (AcctNumber, Name, CustomerId)
        //                                        OUTPUT INSERTED.Id
        //                                        VALUES (@acctNumber, @name, @customerId)";
        //            cmd.Parameters.Add(new SqlParameter("@acctNumber", paymentType.AcctNumber));
        //            cmd.Parameters.Add(new SqlParameter("@name", paymentType.Name));
        //            cmd.Parameters.Add(new SqlParameter("@customerId", paymentType.CustomerId));

        //            paymentType.Id = (int)await cmd.ExecuteScalarAsync();

        //            return CreatedAtRoute("GetPaymentType", new { id = paymentType.Id }, paymentType);
        //        }
        //    }
        //}

        //[HttpPut("{id}")]
        //public async Task<IActionResult> Put([FromRoute] int id, [FromBody] PaymentType paymentType)
        //{
        //    try
        //    {
        //        using (SqlConnection conn = Connection)
        //        {
        //            conn.Open();
        //            using (SqlCommand cmd = conn.CreateCommand())
        //            {
        //                cmd.CommandText = @"UPDATE PaymentType
        //                                            SET AcctNumber = @acctNumber,
        //                                                Name = @name,
        //                                                CustomerId = @customerId
        //                                            WHERE Id = @id";
        //                cmd.Parameters.Add(new SqlParameter("@acctNumber", paymentType.AcctNumber));
        //                cmd.Parameters.Add(new SqlParameter("@name", paymentType.Name));
        //                cmd.Parameters.Add(new SqlParameter("@customerId", paymentType.CustomerId));
        //                cmd.Parameters.Add(new SqlParameter("@id", id));

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
        //        if (!PaymentTypeExists(id))
        //        {
        //            return NotFound();
        //        }
        //        else
        //        {
        //            throw;
        //        }
        //    }
        //}

        //[HttpDelete("{id}")]
        //public async Task<IActionResult> Delete([FromRoute] int id)
        //{
        //    try
        //    {
        //        using (SqlConnection conn = Connection)
        //        {
        //            conn.Open();
        //            using (SqlCommand cmd = conn.CreateCommand())
        //            {
        //                cmd.CommandText = @"DELETE FROM PaymentType WHERE Id = @id";
        //                cmd.Parameters.Add(new SqlParameter("@id", id));

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
        //        if (!PaymentTypeExists(id))
        //        {
        //            return NotFound();
        //        }
        //        else
        //        {
        //            throw;
        //        }
        //    }
        //}

        //private bool PaymentTypeExists(int id)
        //{
        //    using (SqlConnection conn = Connection)
        //    {
        //        conn.Open();
        //        using (SqlCommand cmd = conn.CreateCommand())
        //        {
        //            cmd.CommandText = @"
        //                        SELECT Id, AcctNumber, Name, CustomerId
        //                        FROM PaymentType
        //                        WHERE Id = @id";
        //            cmd.Parameters.Add(new SqlParameter("@id", id));

        //            SqlDataReader reader = cmd.ExecuteReader();
        //            return reader.Read();
        //        }
        //    }
        //}
    }
}