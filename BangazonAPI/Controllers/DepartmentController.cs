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
    /// DepartmentController: A class allow developers to access the Department resource of the BangazonAPI database.
    /// Author: Antonio Jefferson
    /// Methods: 
    ///     Get -- used to get a List of all Departments in the database
    ///     GetDepartment -- used to get a single Department from the database
    ///     Post -- used to add a single Department to the database
    ///     Put -- used to update a single Department in the database
    ///     DepartmentExists -- used for verification
    /// </summary>
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
        //this function gets a List of all departments and displays a list of employes by department using parameters in the database
        public async Task<IActionResult> Get(string _include, string _filter, string q)
        {
            //set portions of the sql statements equal to variables to use i nthe if statements
            string sql_head = @"SELECT d.Id, d.[Name], d.Budget";
            string sql_end = "FROM Department d";
            string sql = $"{sql_head} {sql_end}";

            //?_include=employees
            if (_include == "employees") 
            {
                
                
                string sql_employee_middle = @", e.Id, e.FirstName, e.LastName, e.DepartmentId, e.IsSupervisor";
                string sql_employee_end = @"JOIN Employee e ON d.Id = e.DepartmentId";
                                           

                sql = $"{sql_head} {sql_employee_middle} {sql_end} {sql_employee_end}";

                //nested this if statement to utilize the sql variabl within the if above
                if (q != null)
                {
                    sql = $@"{sql_head} {sql_employee_middle} {sql_end} {sql_employee_end} AND d.Name = '{@q}'";
                }
            }
            //?_include=budget over 300000
            else if (_filter == "300000") 
            {
                string sql_budget_end = @"WHERE d.Budget >= 300000";
                sql = $"{sql_head} {sql_end} {sql_budget_end}";
            }
                    
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {

                    cmd.CommandText = sql;

                    if (q != null)
                    {
                        //passing the q variable to the sql statment if it is not null
                        cmd.Parameters.Add(new SqlParameter("@q", $"%{q}%"));

                    }

                    SqlDataReader reader = await cmd.ExecuteReaderAsync();

                    //creating a list based off the Department model named departments which will hold the DepartmentHash values. Line 129 pass the values in 
                    List<Department> departments = new List<Department>();

                    //dictionary created to hold the department and ID created below
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
                        //outlines what to display if the _include is utilized itt adds the DepartmentHash 
                        if (_include == "employees")
                        {
                            //adds the Employee object below to the list of employees on the department model based off of ID
                            DepartmentHash[departmentId].Employees.Add(new Employee
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                    FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                                    LastName = reader.GetString(reader.GetOrdinal("LastName")),
                                    DepartmentId = reader.GetInt32(reader.GetOrdinal("DepartmentId")),
                                    IsSuperVisor = reader.GetBoolean(reader.GetOrdinal("IsSuperVisor")),
                                });                            
                        };
                        //outlines what to display if the _filer is utilized
                        if (_filter == "300000")
                        {
                            if (!reader.IsDBNull(reader.GetOrdinal("Name")))
                            {
                                //creates a new department that includes the budget and add to the Departmenthash
                                DepartmentHash[departmentId] = new Department
                                {
                                    Id = departmentId,
                                    Name = reader.GetString(reader.GetOrdinal("Name")),
                                    Budget = reader.GetInt32(reader.GetOrdinal("Budget")),
                                   
                                };
                            }
                        };
                        //departments list is set equal to the DepartmentHash.Values.ToList method to create a new list of objects in departments
                        departments = DepartmentHash.Values.ToList();
                    }
                    reader.Close();
                    //returning the list
                    return Ok(departments);
                }
            }
        }
        [HttpGet("{id}", Name = "GetDepartment")]
        //this function gets a single Department from the database, by id
        public async Task<IActionResult> Get([FromRoute] int id, string _include, string _filter, string q)
        {
            try
            {
                //create the SQL as a string, in order to be able to add to it with the 'include' queries
                string sql_head = @"SELECT d.Id, d.[Name], d.Budget";
                string sql_from = @"FROM Department d";
                string sql_where = @"WHERE d.Id = @id";
                string sql = $"{sql_head} {sql_from} {sql_where}";

                if (_include == "employees") //?_include=employees
                {
                    string sql_employee_middle = @", e.Id, e.FirstName, e.LastName, e.DepartmentId, e.IsSupervisor";
                    string sql_employee_end = @"JOIN Employee e ON d.Id = e.DepartmentId";
                    sql = $"{sql_head} {sql_employee_middle} {sql_from} {sql_employee_end} {sql_where}";

                    if (q != null)
                    {
                        sql = $@"{sql_head} {sql_employee_middle} {sql_from} {sql_employee_end} WHERE 2=2 AND d.Id = @id AND d.Name = '{@q}'";
                    }
                }
                else if (_filter == "300000") //?_filter=300000
                {
                    string sql_budget_end = @" WHERE 2=2 AND d.Budget >= 300000 AND d.Id = @id";
                    sql = $"{sql_head} {sql_from} {sql_budget_end}";
                    
                }
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = sql;
                        cmd.Parameters.Add(new SqlParameter("@id", id));

                        SqlDataReader reader = await cmd.ExecuteReaderAsync();

                        Department department = null;

                        while (reader.Read())
                        {
                            if (department == null)
                            {
                                department = new Department
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                    Name = reader.GetString(reader.GetOrdinal("Name")),
                                    Budget = reader.GetInt32(reader.GetOrdinal("Budget")),
                                    Employees = new List<Employee>(),

                                };
                            }
                            if (_include == "employees")
                            {
                              
                                    department.Employees.Add(new Employee
                                    {
                                        Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                        FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                                        LastName = reader.GetString(reader.GetOrdinal("LastName")),
                                        DepartmentId = reader.GetInt32(reader.GetOrdinal("DepartmentId")),
                                        IsSuperVisor = reader.GetBoolean(reader.GetOrdinal("IsSuperVisor")),
                                    });
                                
                            }

                            if (_filter == "300000")
                            {
                               
                                department = new Department
                                    {
                                        Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                        Name = reader.GetString(reader.GetOrdinal("Name")),
                                        Budget = reader.GetInt32(reader.GetOrdinal("Budget")),

                                    };
                                
                            }
                        }
                        reader.Close();

                        if (department == null)
                        {
                            return new StatusCodeResult(StatusCodes.Status404NotFound);
                            throw new Exception("No rows affected");
                        }
                        else
                        {
                            return Ok(department);
                        }
                    }
                }
            }
            catch (Exception)
            {
                if (!DepartmentExists(id))
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
        //this function adds a single Department to the database
        //it takes a single parameter of type Department to be parsed for input
        public async Task<IActionResult> Post([FromBody] Department department)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO Department (Name, Budget)
                                        OUTPUT INSERTED.Id
                                        VALUES (@Name, @Budget)";
                    cmd.Parameters.Add(new SqlParameter("@Name", department.Name));
                    cmd.Parameters.Add(new SqlParameter("@Budget", department.Budget));

                    int newId = (int)await cmd.ExecuteScalarAsync();
                    department.Id = newId;
                    return CreatedAtRoute("GetDepartment", new { id = newId }, department);
                }
            }
        }
        [HttpPut("{id}")]
        //this function updates a single Department in the database
        //the id parameter indicates which database record should be updated
        //the Department type parameter contains the data to be updated into the indicated database record
        public async Task<IActionResult> Put([FromRoute] int id, [FromBody] Department department)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"UPDATE Department SET Name = @Name,
                                            Budget = @Budget WHERE Id = @id";
                        cmd.Parameters.Add(new SqlParameter("@Name", department.Name));
                        cmd.Parameters.Add(new SqlParameter("@Budget", department.Budget));
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
            //if the department doesn't exist a 404 error code will be presented
            catch (Exception)
            {
                if (!DepartmentExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        //Department does NOT have a Delete function
        //this function checks the database for the existence of a record matching the id parameter, and returns true or false
        //the id parameter indicates which database record should be pulled

        private bool DepartmentExists(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT Id, Name, Budget
                                        FROM Department WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = cmd.ExecuteReader();
                    return reader.Read();
                }
            }
        }


    }
}