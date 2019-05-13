<<<<<<< HEAD
using System;
using System.Net;
using Newtonsoft.Json;
using Xunit;
using BangazonAPI.Models;
using System.Threading.Tasks;
using System.Collections.Generic;
=======
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using System.Linq;
using BangazonAPI.Models;

>>>>>>> master

namespace TestBangazonAPI
{
    public class TestCustomers
    {
        [Fact]
        public async Task Test_Get_All_Customers()
        {
            using (var client = new APIClientProvider().Client)
            {
                /*
                    ARRANGE
                */
<<<<<<< HEAD


                /*
                    ACT
                */
                var response = await client.GetAsync("/api/customers");

=======
                    
                //no arrange for gets
              
                /*
                    ACT
                */

                var response = await client.GetAsync("/customer");

                response.EnsureSuccessStatusCode();
>>>>>>> master

                string responseBody = await response.Content.ReadAsStringAsync();
                var customers = JsonConvert.DeserializeObject<List<Customer>>(responseBody);

                /*
                    ASSERT
                */
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.True(customers.Count > 0);
            }
        }
<<<<<<< HEAD
=======

        [Fact]
        public async Task Test_Get_One_Customer()
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
                var response = await client.GetAsync("/customer/1");

                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();
                var customer = JsonConvert.DeserializeObject<Customer>(responseBody);

                /*
                    ASSERT
                */
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Equal("Summer", customer.FirstName);
                Assert.Equal("Rainault", customer.LastName);
                Assert.NotNull(customer);
            }
        }

        [Fact]
        public async Task Test_Get_NonExisting_Customer_Fails()
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
                var response = await client.GetAsync("/customer/99999999");

                /*
                    ASSERT
                */
                Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            }
        }

        [Fact]
        public async Task Test_Create_Customer()
        {
            using (var client = new APIClientProvider().Client)
            {
                /* 
                    ARRANGE
                */
                 Customer helen = new Customer
                 {
                     FirstName = "Helen",
                     LastName = "Chalmers",
                 };
                var helenAsJSON = JsonConvert.SerializeObject(helen);

                /*
                    ACT
                */
                var response = await client.PostAsync(
                   "/customer",
                   new StringContent(helenAsJSON, Encoding.UTF8, "application/json")
               );

                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();
                var newHelen = JsonConvert.DeserializeObject<Customer>(responseBody);

                /* 
                    ASSERT
                */
                Assert.Equal(HttpStatusCode.Created, response.StatusCode);
                Assert.Equal("Helen", newHelen.FirstName);
                Assert.Equal("Chalmers", newHelen.LastName);
            }
        }

        [Fact]
        public async Task Test_Modify_Customer()
        {
            // New last name to change to and test
            string newLastName = "Rainault-Allen";

            using (var client = new APIClientProvider().Client)
            {
                /*
                    PUT section
                 */
                Customer modifiedSummer = new Customer
                {
                    FirstName = "Summer",
                    LastName = newLastName,
                };
                var modifiedSummerAsJSON = JsonConvert.SerializeObject(modifiedSummer);

                var response = await client.PutAsync(
                    "/customer/1",
                    new StringContent(modifiedSummerAsJSON, Encoding.UTF8, "application/json")
                );
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();

                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

                /*
                    GET section
                 */
                var getSummer = await client.GetAsync("/customer/1");
                getSummer.EnsureSuccessStatusCode();

                string getSummerBody = await getSummer.Content.ReadAsStringAsync();
                Customer newSummer = JsonConvert.DeserializeObject<Customer>(getSummerBody);

                Assert.Equal(HttpStatusCode.OK, getSummer.StatusCode);
                Assert.Equal(newLastName, newSummer.LastName);
            }
        }
>>>>>>> master
    }
}
