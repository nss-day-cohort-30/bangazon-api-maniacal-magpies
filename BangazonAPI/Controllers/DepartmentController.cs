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
        //this function gets a List of all Customers in the database
        public async Task<IActionResult> Get(string _include, string _filter, string q)
        {
            //create the SQL as a string, in order to be able to add to it with the 'include' queries
            //string sql = 
            //string sql = @"SELECT d.Id, d.[Name], d.Budget, e.Id, e.FirstName, e.LastName, 
            //              e.DepartmentId, e.IsSupervisor
            //              FROM Department d
            //              JOIN Employee e ON d.Id = e.Department.Id
            //              WHERE 2=2";

            string sql_head = @"SELECT d.Id, d.[Name], d.Budget";
            string sql_end = "FROM Department d";
            string sql = $"{sql_head} {sql_end}";
                       
            if (_include == "employees") //?_include=employees
            {
                
                
                string sql_employee_middle = @", e.Id, e.FirstName, e.LastName, e.DepartmentId, e.IsSupervisor";
                string sql_employee_end = @"JOIN Employee e ON d.Id = e.DepartmentId";
                                           

                sql = $"{sql_head} {sql_employee_middle} {sql_end} {sql_employee_end}";
                Console.WriteLine("LoOOOOOK", sql);

                    if (q != null)
                {
                    sql = $"{sql_head} {sql_employee_middle} {sql_end} {sql_employee_end} AND d.[Name] == @q";
                }
            }
            else if (_filter == "300000") //?_include=budget over 300000
            {
                string sql_budget_end = @"WHERE d.Budget >= 300000";
                sql = $"{sql_head} {sql_end} {sql_budget_end}";
            }
            //else if (q != null)
            //{
            //    sql = $"{sql} AND d.[Name] == @q";
            //}
          
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
                    List<Department> departments = new List<Department>();

                    Dictionary<int, Department> DepartmentHash = new Dictionary<int, Department>();

                    while (reader.Read())
                    {
                        int departmentId = reader.GetInt32(reader.GetOrdinal("Id"));
                        if (!DepartmentHash.ContainsKey(departmentId))
                        {
                            DepartmentHash[departmentId] = new Department
                            {
                                Id = departmentId,
                                Name = reader.GetString(reader.GetOrdinal("Name")),
                                Budget = reader.GetInt32(reader.GetOrdinal("Budget")),
                                Employees = new List<Employee>(),                               
                            };
                        };

                        if (_include == "employees")
                        {
                            DepartmentHash[departmentId].Employees.Add(new Employee
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                    FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                                    LastName = reader.GetString(reader.GetOrdinal("LastName")),
                                    DepartmentId = reader.GetInt32(reader.GetOrdinal("DepartmentId")),
                                    IsSuperVisor = reader.GetBoolean(reader.GetOrdinal("IsSuperVisor")),
                                });                            
                        };

                        if (_include == "filter")
                        {
                            if (!reader.IsDBNull(reader.GetOrdinal("Name")))
                            {
                                DepartmentHash[departmentId] = new Department
                                {
                                    Id = departmentId,
                                    Name = reader.GetString(reader.GetOrdinal("Name")),
                                    Budget = reader.GetInt32(reader.GetOrdinal("Budget")),
                                   
                                };
                            }
                        };

                        departments = DepartmentHash.Values.ToList();
                    }
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