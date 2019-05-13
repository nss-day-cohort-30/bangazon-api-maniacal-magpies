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
    public class TestComputers
    {
        [Fact]
        public async Task Test_Get_All_Computers()
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

                var response = await client.GetAsync("/computer");

                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();
                var computers = JsonConvert.DeserializeObject<List<Computer>>(responseBody);

                /*
                    ASSERT
                */
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.True(computers.Count > 0);
            }
        }

        [Fact]
        public async Task Test_Get_One_Computer()
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
                var response = await client.GetAsync("/computer/1");

                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();
                var computer = JsonConvert.DeserializeObject<Computer>(responseBody);

                /*
                    ASSERT
                */
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Equal("VivoBook", computer.Make);
                Assert.Equal("ASUS", computer.Manufacturer);
                Assert.NotNull(computer);
            }
        }

        [Fact]
        public async Task Test_Get_NonExisting_Computer_Fails()
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
                var response = await client.GetAsync("/computer/99999999");

                /*
                    ASSERT
                */
                Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            }
        }

        [Fact]
        public async Task Test_Create_And_Delete_Computer()
        {
            using (var client = new APIClientProvider().Client)
            {
                /* 
                    ARRANGE
                */
                Computer raspi = new Computer
                {
                    Make = "Model 3b",
                    Manufacturer = "Raspberry Pi",
                    PurchaseDate = System.DateTime.Today
                };
                var raspiAsJSON = JsonConvert.SerializeObject(raspi);

                /*
                    ACT
                */
                var response = await client.PostAsync(
                   "/computer",
                   new StringContent(raspiAsJSON, Encoding.UTF8, "application/json")
               );

                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();
                var newRaspi = JsonConvert.DeserializeObject<Computer>(responseBody);

                /* 
                    ASSERT
                */
                Assert.Equal(HttpStatusCode.Created, response.StatusCode);
                Assert.Equal("Model 3b", newRaspi.Make);
                Assert.Equal("Raspberry Pi", newRaspi.Manufacturer);

                var deleteResponse = await client.DeleteAsync($"/computer/{newRaspi.Id}");
                deleteResponse.EnsureSuccessStatusCode();
                Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
            }
        }

        [Fact]
        public async Task Test_Modify_Computer()
        {
            // New last name to change to and test
            string newMake = "8265NGW";

            using (var client = new APIClientProvider().Client)
            {
                /*
                    PUT section
                 */
                Computer modifiedAsus = new Computer
                {
                    Manufacturer = "ASUS",
                    Make = newMake,
                };
                var modifiedAsusAsJSON = JsonConvert.SerializeObject(modifiedAsus);

                var response = await client.PutAsync(
                    "/computer/1",
                    new StringContent(modifiedAsusAsJSON, Encoding.UTF8, "application/json")
                );
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();

                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

                /*
                    GET section
                 */
                var getAsus = await client.GetAsync("/computer/1");
                getAsus.EnsureSuccessStatusCode();

                string getAsusBody = await getAsus.Content.ReadAsStringAsync();
                Computer newAsus = JsonConvert.DeserializeObject<Computer>(getAsusBody);

                Assert.Equal(HttpStatusCode.OK, getAsus.StatusCode);
                Assert.Equal(newMake, newAsus.Make);
            }
        }
    }
}
