using BangazonAPI.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace TestBangazonAPI
{
    public class TestOrders
    {
        [Fact]
        public async Task Test_Get_All_Orders()
        //asserts that a List<Order> containing at least one Order is returned
        {
            using (var client = new APIClientProvider().Client)
            {
                var response = await client.GetAsync("/orders");

                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();
                var productTypes = JsonConvert.DeserializeObject<List<ProductType>>(responseBody);

                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.True(productTypes.Count > 0);
            }
        }
        [Fact]
        public async Task Test_Get_Single_Order()
        //asserts that an Order with the specified Id is returned
        {

            using (var client = new APIClientProvider().Client)
            {
                var response = await client.GetAsync("/orders/1");

                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();
                var order = JsonConvert.DeserializeObject<Order>(responseBody);

                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Equal(1, order.CustomerId);
                Assert.Equal(3, order.PaymentTypeId);
                Assert.NotNull(order);
            }
        }

        [Fact]
        public async Task Test_Get_NonExistant_Order_Fails()
        //asserts that if a non-existant Id is passed into the GET, a 404 response is returned
        {

            using (var client = new APIClientProvider().Client)
            {
                var response = await client.GetAsync("/orders/999999999");
                Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            }
        }


        [Fact]
        public async Task Test_Create_And_Delete_Order()
        //asserts that a new Order is created correctly and deleted correctly
        {
            using (var client = new APIClientProvider().Client)
            {
                Order order = new Order
                {
                    CustomerId = 1,
                    Customer = null,
                    PaymentTypeId = null,
                    Payment = null,
                    Products = null
                };
                var orderAsJSON = JsonConvert.SerializeObject(order);


                var response = await client.PostAsync(
                    "/orders",
                    new StringContent(orderAsJSON, Encoding.UTF8, "application/json")
                );

                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();
                var newOrder = JsonConvert.DeserializeObject<Order>(responseBody);

                Assert.Equal(HttpStatusCode.Created, response.StatusCode);
                Assert.Equal(1, newOrder.CustomerId);
                Assert.Null(newOrder.Customer);
                Assert.Null(newOrder.PaymentTypeId);
                Assert.Null(newOrder.Payment);
                Assert.Null(newOrder.Products);

                var deleteResponse = await client.DeleteAsync($"/orders/{newOrder.Id}");
                deleteResponse.EnsureSuccessStatusCode();
                Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
            }
        }

        [Fact]
        public async Task Test_Delete_NonExistent_Order_Fails()
        //asserts that a non-existant Id passed into the DELETE method will return a 404 code
        {
            using (var client = new APIClientProvider().Client)
            {
                var deleteResponse = await client.DeleteAsync("/orders/600000");

                Assert.False(deleteResponse.IsSuccessStatusCode);
                Assert.Equal(HttpStatusCode.NotFound, deleteResponse.StatusCode);
            }
        }

        [Fact]
        public async Task Test_Modify_Order()
        //asserts that an existing order is modified in the expected way with PUT
        {
            int id = 1;
            int newCustomerId = 2;

            using (var client = new APIClientProvider().Client)
            {
                /*
                    initial GET section - get an Order for modification
                 */
                var initialGetOrder = await client.GetAsync($"/orders/{id}");
                initialGetOrder.EnsureSuccessStatusCode();

                string initialGetOrderBody = await initialGetOrder.Content.ReadAsStringAsync();
                Order initialOrder = JsonConvert.DeserializeObject<Order>(initialGetOrderBody);

                /*
                    PUT section - modify the initial Order
                */
                Order modifiedOrder = new Order
                {
                    Id = initialOrder.Id,
                    CustomerId = newCustomerId,
                    Customer = null,
                    PaymentTypeId = initialOrder.PaymentTypeId,
                    Payment = initialOrder.Payment,
                    Products = initialOrder.Products
                };
                var modifiedOrderAsJSON = JsonConvert.SerializeObject(modifiedOrder);

                var response = await client.PutAsync(
                    $"/orders/{id}",
                    new StringContent(modifiedOrderAsJSON, Encoding.UTF8, "application/json")
                );
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();

                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

                /*
                    GET section - get the modified order
                 */
                var getOrder = await client.GetAsync($"/orders/{id}");
                getOrder.EnsureSuccessStatusCode();

                string getOrderBody = await getOrder.Content.ReadAsStringAsync();
                Order newOrder = JsonConvert.DeserializeObject<Order>(getOrderBody);

                Assert.Equal(HttpStatusCode.OK, getOrder.StatusCode);
                Assert.Equal(newCustomerId, newOrder.CustomerId);
                Assert.Equal(initialOrder.PaymentTypeId, newOrder.PaymentTypeId);

                /*
                    reset to initial data
                */
           
                var resetOrderAsJSON = JsonConvert.SerializeObject(initialOrder);

                var resetResponse = await client.PutAsync(
                    $"/orders/{id}",
                    new StringContent(resetOrderAsJSON, Encoding.UTF8, "application/json")
                );
            }
        }
    }
}
