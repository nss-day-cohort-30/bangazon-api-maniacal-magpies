using System;
using System.Net;
using Newtonsoft.Json;
using Xunit;
using BangazonAPI.Models;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace TestBangazonAPI
{
    public class TestProductTypes
    {
        [Fact]
        public async Task Test_Get_All_ProductTypes()
        {
            using (var client = new APIClientProvider().Client)
            {
                var response = await client.GetAsync("/producttype");

                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();
                var productTypes = JsonConvert.DeserializeObject<List<ProductType>>(responseBody);

                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.True(productTypes.Count > 0);
            }
        }
            [Fact]
        public async Task Test_Get_Single_ProductType()
        {

            using (var client = new APIClientProvider().Client)
            {
                var response = await client.GetAsync("/producttype/1");

                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();
                var productType = JsonConvert.DeserializeObject<ProductType>(responseBody);

                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Equal("Books", productType.Name);
                Assert.NotNull(productType);
            }
        }

        [Fact]
        public async Task Test_Get_NonExistant_ProductType_Fails()
        {

            using (var client = new APIClientProvider().Client)
            {
                var response = await client.GetAsync("/producttype/999999999");
                Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            }
        }


        [Fact]
        public async Task Test_Create_And_Delete_ProductType()
        {
            using (var client = new APIClientProvider().Client)
            {
                ProductType toys = new ProductType
                {
                    Name = "Toys"
                };
                var toysAsJSON = JsonConvert.SerializeObject(toys);


                var response = await client.PostAsync(
                    "/producttype",
                    new StringContent(toysAsJSON, Encoding.UTF8, "application/json")
                );

                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();
                var newToys = JsonConvert.DeserializeObject<ProductType>(responseBody);

                Assert.Equal(HttpStatusCode.Created, response.StatusCode);
                Assert.Equal("Toys", newToys.Name);


                var deleteResponse = await client.DeleteAsync($"/producttype/{newToys.Id}");
                deleteResponse.EnsureSuccessStatusCode();
                Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
            }
        }

        [Fact]
        public async Task Test_Delete_NonExistent_ProductType_Fails()
        {
            using (var client = new APIClientProvider().Client)
            {
                var deleteResponse = await client.DeleteAsync("/api/producttype/600000");

                Assert.False(deleteResponse.IsSuccessStatusCode);
                Assert.Equal(HttpStatusCode.NotFound, deleteResponse.StatusCode);
            }
        }

        [Fact]
        public async Task Test_Modify_ProductType()
        {
            // New name to change to and test
            string newName = "Games";

            using (var client = new APIClientProvider().Client)
            {
                /*
                    PUT section
                 */
                ProductType modifiedProductType = new ProductType
                {
                    Name = newName
                };
                var modifiedProductTypeAsJSON = JsonConvert.SerializeObject(modifiedProductType);

                var response = await client.PutAsync(
                    "/producttype/1",
                    new StringContent(modifiedProductTypeAsJSON, Encoding.UTF8, "application/json")
                );
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();

                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

                /*
                    GET section
                 */
                var getProductType = await client.GetAsync("/producttype/1");
                getProductType.EnsureSuccessStatusCode();

                string getProductTypeBody = await getProductType.Content.ReadAsStringAsync();
                ProductType newProductType = JsonConvert.DeserializeObject<ProductType>(getProductTypeBody);

                Assert.Equal(HttpStatusCode.OK, getProductType.StatusCode);
                Assert.Equal(newName, newProductType.Name);
            }
        }
    }
}
