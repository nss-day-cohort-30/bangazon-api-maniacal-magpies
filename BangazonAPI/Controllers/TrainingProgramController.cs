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
    public class TrainingProgramController : ControllerBase
    {
        private readonly IConfiguration _config;

        public TrainingProgramController(IConfiguration config)
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

        // Gets all Training Programs in the database and see Employees who are attending
        [HttpGet]
        public async Task<IActionResult> Get(string completed)
        {
            // SQL string for Training Program query
            string sql_head = "SELECT tp.Id, tp.[Name], tp.StartDate, tp.EndDate, tp.MaxAttendees, e.FirstName, e.LastName";
            string sql_end = @"FROM TrainingProgram tp
                               JOIN EmployeeTraining et ON et.TrainingProgramId = tp.Id
                               JOIN Employee e ON et.EmployeeId = e.Id";
            string sql = $"{sql_head} {sql_end}";

            if (completed == "false?") //?completed=false?
            {
                string sql_where = "WHERE StartDate > CONVERT(date, getdate())";
                sql = $"{sql_head} {sql_end} {sql_where}";
            }


            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = sql;

                    SqlDataReader reader = await cmd.ExecuteReaderAsync();

                    List<TrainingProgram> trainingPrograms = new List<TrainingProgram>();

                    Dictionary<int, TrainingProgram> trainingProgramHash = new Dictionary<int, TrainingProgram>();

                    while (reader.Read())
                    {
                        int trainingProgramId = reader.GetInt32(reader.GetOrdinal("Id"));

                        // this isn't right
                        if (!trainingProgramHash.ContainsKey(trainingProgramId));
                        TrainingProgram trainingProgram = new TrainingProgram
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                            StartDate = reader.GetDateTime(reader.GetOrdinal("startDate")),
                            EndDate = reader.GetDateTime(reader.GetOrdinal("endDate")),
                            MaxAttendees = reader.GetInt32(reader.GetOrdinal("maxAttendees"))
                        };

                        trainingPrograms.Add(trainingProgram);
                    }

                    reader.Close();

                    return Ok(trainingPrograms);
                }
            }
        }

        // GET api/values/5
        [HttpGet("{id}", Name = "GetTrainingProgram")]
        public async Task<IActionResult> Get([FromRoute]int id)
        {
            if (!TrainingProgramExists(id))
            {
                return new StatusCodeResult(StatusCodes.Status404NotFound);
            }
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT
                            Id, Name
                        FROM TrainingProgram
                        WHERE Id =@id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = await cmd.ExecuteReaderAsync();

                    TrainingProgram trainingProgram = null;

                    if (reader.Read())
                    {
                        trainingProgram = new TrainingProgram
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Name = reader.GetString(reader.GetOrdinal("Name"))
                        };
                    }

                    reader.Close();

                    return Ok(trainingProgram);
                }
            }
        }

        // POST api/values
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] TrainingProgram trainingProgram)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    // More string interpolation
                    cmd.CommandText = @"
                        INSERT INTO TrainingProgram (Name)
                        OUTPUT INSERTED.Id
                        VALUES (@name);
                    ";
                    cmd.Parameters.Add(new SqlParameter("@name", trainingProgram.Name));

                    trainingProgram.Id = (int)await cmd.ExecuteScalarAsync();

                    return CreatedAtRoute("GetTrainingProgram", new { id = trainingProgram.Id }, trainingProgram);
                }
            }
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] TrainingProgram trainingProgram)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"
                            UPDATE TrainingProgram
                            SET Name = @Name
                            -- Set the remaining columns here
                            WHERE Id = @id
                        ";
                        cmd.Parameters.Add(new SqlParameter("@id", trainingProgram.Id));
                        cmd.Parameters.Add(new SqlParameter("@Name", trainingProgram.Name));

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
                if (!TrainingProgramExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        //DELETE api/values/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"DELETE FROM TrainingProgram WHERE Id = @id";
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
                if (!TrainingProgramExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        private bool TrainingProgramExists(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    // More string interpolation
                    cmd.CommandText = "SELECT Id FROM TrainingProgram WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = cmd.ExecuteReader();

                    return reader.Read();
                }
            }
        }
    }
}
