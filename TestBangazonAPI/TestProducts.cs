using System.Net;
using Newtonsoft.Json;
using Xunit;
using BangazonAPI.Models;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Data.SqlClient;


namespace TestBangazonAPI
{
    public class TestProduct
    {
        [Fact]
        public async Task Test_Get_All_Products()
        {
            using (var client = new APIClientProvider().Client)
            {
                /*
                    ARRANGE
                */

                /*
                    ACT
                */
                var response = await client.GetAsync("/products");


                string responseBody = await response.Content.ReadAsStringAsync();
                var products = JsonConvert.DeserializeObject<List<Product>>(responseBody);

                /*
                    ASSERT
                */
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.True(products.Count > 0);
            }
        }

        [Fact]
        public async Task Test_Get_Single_Product()
        {

            using (var client = new APIClientProvider().Client)
            {
                var response = await client.GetAsync("/products/1");

                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();
                var product = JsonConvert.DeserializeObject<Product>(responseBody);

                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Equal(1, product.ProductTypeId);
                Assert.Equal(1, product.CustomerId);
                Assert.Equal(11.50M, product.Price);
                Assert.Equal("Electronic Thingy", product.Title);
                Assert.Equal("It's a thing and it's electronic", product.Description);
                Assert.Equal(5, product.Quantity);
                Assert.NotNull(product);


            }
        }

        [Fact]
        public async Task Test_Create_And_Delete_Product()
        {
            using (var client = new APIClientProvider().Client)
            {
                Product testProduct = new Product
                {
                    ProductTypeId = 1,
                    CustomerId = 1,
                    Price = 11.50M,
                    Title = "Test Product",
                    Description = "This is a test",
                    Quantity = 100
                };
                var testProductAsJSON = JsonConvert.SerializeObject(testProduct);


                var response = await client.PostAsync(
                    "/products",
                    new StringContent(testProductAsJSON, Encoding.UTF8, "application/json")
                );

                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();
                var newTestProduct = JsonConvert.DeserializeObject<Product>(responseBody);

                Assert.Equal(HttpStatusCode.Created, response.StatusCode);
                Assert.Equal(1, newTestProduct.ProductTypeId);
                Assert.Equal(1, newTestProduct.CustomerId);
                Assert.Equal(11.50M, newTestProduct.Price);
                Assert.Equal("Test Product", newTestProduct.Title);
                Assert.Equal("This is a test", newTestProduct.Description);
                Assert.Equal(100, newTestProduct.Quantity);


                var deleteResponse = await client.DeleteAsync($"/products/{newTestProduct.Id}");
                deleteResponse.EnsureSuccessStatusCode();
                Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
            }
        }

        [Fact]
        public async Task Test_Put_For_Product()
        {
            // New title to change to and test
            string newTitle = "Electronic Test Thingy";

            using (var client = new APIClientProvider().Client)
            {
                /*
                    PUT section
                 */
                Product changeTestProduct = new Product
                {
                    Id = 1,
                    ProductTypeId = 1,
                    CustomerId = 1,
                    Price = 11.50M,
                    Title = newTitle,
                    Description = "It's a thing and it's electronic",
                    Quantity = 5
                };
                var newTitleProductAsJSON = JsonConvert.SerializeObject(changeTestProduct);

                var response = await client.PutAsync(
                    "/products/1",
                    new StringContent(newTitleProductAsJSON, Encoding.UTF8, "application/json")
                );
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();

                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

                /*
                    GET section
                 */
                var getNewProduct = await client.GetAsync("/products/1");
                getNewProduct.EnsureSuccessStatusCode();

                string getNewProductBody = await getNewProduct.Content.ReadAsStringAsync();
                Product newTitleProductCard = JsonConvert.DeserializeObject<Product>(getNewProductBody);

                Assert.Equal(HttpStatusCode.OK, getNewProduct.StatusCode);
                Assert.Equal(newTitle, newTitleProductCard.Title);


                /*
                    reset data
                 */
                Product resetTestProduct = new Product
                {
                    Id = 1,
                    ProductTypeId = 1,
                    CustomerId = 1,
                    Price = 11.50M,
                    Title = "Electronic Thingy",
                    Description = "It's a thing and it's electronic",
                    Quantity = 5
                };
                var resetTestProductAsJSON = JsonConvert.SerializeObject(resetTestProduct);

                var resetResponse = await client.PutAsync(
                    "/products/1",
                    new StringContent(resetTestProductAsJSON, Encoding.UTF8, "application/json")
                );
            }
        }

    }
}
