using System;
using System.Net;
using Newtonsoft.Json;
using Xunit;
using BangazonAPI.Models;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Linq;


namespace TestBangazonAPI
{
    public class TestDepartment
    {
        //This method test the get all department function by ensuring department are greated than 0
        [Fact]
        public async Task Test_Get_All_Departments()
        {
            using (var client = new APIClientProvider().Client)
            {
                /*
                    ARRANGE
                */

                /*
                    ACT
                */
                var response = await client.GetAsync("/Department");


                string responseBody = await response.Content.ReadAsStringAsync();
                var departments = JsonConvert.DeserializeObject<List<Department>>(responseBody);

                /*
                    ASSERT
                */
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.True(departments.Count > 0);
            }
        }
        //this method test get a single department by comparing the values in the database to the values inputted in the test
        [Fact]
        public async Task Test_Get_Single_Department()
        {

            using (var client = new APIClientProvider().Client)
            {
                var response = await client.GetAsync("/Department/1");

                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();
                var departments = JsonConvert.DeserializeObject<Department>(responseBody);

                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Equal("IT", departments.Name);
                Assert.Equal(300000, departments.Budget);               
                Assert.NotNull(departments);
            }
        }

        //this method allows to create a department by building a new department  and that new department is then deleted using the new ID created
        [Fact]
        public async Task Test_Create_Department()
        {
            using (var client = new APIClientProvider().Client)
            {
                Department department = new Department
                {
                    Name = "DonkeyTest",
                    Budget = 789632,
                    
                };
                var departmentAsJSON = JsonConvert.SerializeObject(department);


                var response = await client.PostAsync(
                    "/Department",
                    new StringContent(departmentAsJSON, Encoding.UTF8, "application/json")
                );

                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();
                var newDepartment = JsonConvert.DeserializeObject<Department>(responseBody);

                Assert.Equal(HttpStatusCode.Created, response.StatusCode);
                Assert.Equal(789632, newDepartment.Budget);
                Assert.Equal("DonkeyTest", newDepartment.Name);

             }
        }

        //this method allow s us to update an existing department 
        [Fact]
        public async Task Test_Put_For_Department()
        {
            // New last name to change to and test
            string newName = "TestChangeDepartment";

            using (var client = new APIClientProvider().Client)
            {
                /*
                    PUT section
                 */
                Department changeCustodial = new Department
                {
                    Budget = 15000,
                    Name = newName,
                };
                var newNameAsJSON = JsonConvert.SerializeObject(changeCustodial);

                var response = await client.PutAsync(
                    "/Department/2",
                    new StringContent(newNameAsJSON, Encoding.UTF8, "application/json")
                );
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();

                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

                /*
                    GET section
                 */
                var getNewName = await client.GetAsync("/Department/2");
                getNewName.EnsureSuccessStatusCode();

                string getNewNameBody = await getNewName.Content.ReadAsStringAsync();
                Department newNameDepartment = JsonConvert.DeserializeObject<Department>(getNewNameBody);

                Assert.Equal(HttpStatusCode.OK, getNewName.StatusCode);
                Assert.Equal(newName, newNameDepartment.Name);
            }
        }

        //Test to make sure a nonexistenet department is not returned
        [Fact]
        public async Task Test_Get_NonExitant_Department_Fail()
        {

            using (var client = new APIClientProvider().Client)
            {
                var response = await client.GetAsync("/Department/123852");
                Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            }
        }
        //This method test the get all department function with get employes in department 
        [Fact]
        public async Task Test_Get_All_Departments_And_Employees()
        {
            using (var client = new APIClientProvider().Client)
            {
                /*
                    ARRANGE
                */

                /*
                    ACT
                */
                var response = await client.GetAsync("/Department?_include=employees");


                string responseBody = await response.Content.ReadAsStringAsync();
                var departments = JsonConvert.DeserializeObject<List<Department>>(responseBody);

                /*
                    ASSERT
                */
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.True(departments.Count > 0);
                                
            }
        }

    }
}
