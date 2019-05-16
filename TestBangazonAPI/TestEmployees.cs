using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using System.Linq;
using BangazonAPI.Models;

namespace TestBangazonAPI
{
    public class TestEmployees
    {
        [Fact]
        public async Task Test_Get_All_Employees()
        {
            using (var client = new APIClientProvider().Client)
            {
                /*
                    ARRANGE
                */

                //no arrange for gets

                /*
                    ACT
                */

                var response = await client.GetAsync("/employee");

                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();
                var employees = JsonConvert.DeserializeObject<List<Employee>>(responseBody);

                /*
                    ASSERT
                */
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.True(employees.Count > 0);
            }
        }

        [Fact]
        public async Task Test_Get_One_Employee()
        {
            using (var client = new APIClientProvider().Client)
            {
                /*
                    ARRANGE
                */

                //no arrange for gets

                /*
                    ACT
                */
                var response = await client.GetAsync("/employee/1");

                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();
                var employee = JsonConvert.DeserializeObject<Employee>(responseBody);

                /*
                    ASSERT
                */
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Equal("Leonard", employee.FirstName);
                Assert.Equal("Snart", employee.LastName);
                Assert.NotNull(employee);
            }
        }

        [Fact]
        public async Task Test_Get_NonExisting_Employee_Fails()
        {
            using (var client = new APIClientProvider().Client)
            {
                /*
                   ARRANGE
               */

                //no arrange for gets

                /*
                    ACT
                */
                var response = await client.GetAsync("/employee/99999999");

                /*
                    ASSERT
                */
                Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            }
        }

        [Fact]
        public async Task Test_Create_Employee()
        {
            using (var client = new APIClientProvider().Client)
            {
                /*
                    ARRANGE
                */
                Employee helen = new Employee
                {
                    FirstName = "Helen",
                    LastName = "Chalmers",
                    DepartmentId = 2
                };
                var helenAsJSON = JsonConvert.SerializeObject(helen);

                /*
                    ACT
                */
                var response = await client.PostAsync(
                   "/employee",
                   new StringContent(helenAsJSON, Encoding.UTF8, "application/json")
               );

                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();
                var newHelen = JsonConvert.DeserializeObject<Employee>(responseBody);

                /*
                    ASSERT
                */
                Assert.Equal(HttpStatusCode.Created, response.StatusCode);
                Assert.Equal("Helen", newHelen.FirstName);
                Assert.Equal("Chalmers", newHelen.LastName);
            }
        }

        [Fact]
        public async Task Test_Modify_Employee()
        {
            // New last name to change to and test
            string newLastName = "Snart-Allen";

            using (var client = new APIClientProvider().Client)
            {
                /*
                    PUT section
                 */
                Employee modifiedLeonard = new Employee
                {
                    FirstName = "Leonard",
                    LastName = newLastName,
                    DepartmentId = 1
                };
                var modifiedLeonardAsJSON = JsonConvert.SerializeObject(modifiedLeonard);

                var response = await client.PutAsync(
                    "/employee/1",
                    new StringContent(modifiedLeonardAsJSON, Encoding.UTF8, "application/json")
                );
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();

                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

                /*
                    GET section
                 */
                var getLeonard = await client.GetAsync("/employee/1");
                getLeonard.EnsureSuccessStatusCode();

                string getLeonardBody = await getLeonard.Content.ReadAsStringAsync();
                Employee newLeonard = JsonConvert.DeserializeObject<Employee>(getLeonardBody);

                Assert.Equal(HttpStatusCode.OK, getLeonard.StatusCode);
                Assert.Equal(newLastName, newLeonard.LastName);

                ///////////////
                /*
                    Reset data
                 */
                Employee resetLeonard = new Employee
                {
                    FirstName = "Leonard",
                    LastName = "Snart",
                    DepartmentId = 1
                };
                var resetLeonardAsJSON = JsonConvert.SerializeObject(resetLeonard);

                var resetResponse = await client.PutAsync(
                    "/employee/1",
                    new StringContent(resetLeonardAsJSON, Encoding.UTF8, "application/json")
                );



            }
        }
    }
}