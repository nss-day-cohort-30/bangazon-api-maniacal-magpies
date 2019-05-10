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
    public class TestPaymentType
    {
        [Fact]
        public async Task Test_Get_All_PaymentTypes()
        {
            using (var client = new APIClientProvider().Client)
            {
                /*
                    ARRANGE
                */

                /*
                    ACT
                */
                var response = await client.GetAsync("/PaymentType");


                string responseBody = await response.Content.ReadAsStringAsync();
                var paymentTypes = JsonConvert.DeserializeObject<List<PaymentType>>(responseBody);

                /*
                    ASSERT
                */
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.True(paymentTypes.Count > 0);
            }
        }

        [Fact]
        public async Task Test_Get_Single_PaymentType()
        {

            using (var client = new APIClientProvider().Client)
            {
                var response = await client.GetAsync("/PaymentType/1");

                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();
                var paymentType = JsonConvert.DeserializeObject<PaymentType>(responseBody);

                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Equal(123456, paymentType.AcctNumber);
                Assert.Equal("Visa", paymentType.Name);
                Assert.Equal("Antonio", paymentType.Customer.FirstName);
                Assert.Equal("Jefferson", paymentType.Customer.LastName);
                Assert.NotNull(paymentType);
            }
        }
        [Fact]
        public async Task Test_Create_And_Delete_PaymentType()
        {
            using (var client = new APIClientProvider().Client)
            {
                PaymentType amex = new PaymentType
                {
                    AcctNumber = 789632145,
                    Name = "American Express",
                    CustomerId = 1
                };
                var amexAsJSON = JsonConvert.SerializeObject(amex);


                var response = await client.PostAsync(
                    "/PaymentType",
                    new StringContent(amexAsJSON, Encoding.UTF8, "application/json")
                );

                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();
                var newAmex = JsonConvert.DeserializeObject<PaymentType>(responseBody);

                Assert.Equal(HttpStatusCode.Created, response.StatusCode);
                Assert.Equal(789632145, newAmex.AcctNumber);
                Assert.Equal("American Express", newAmex.Name);
                Assert.Equal(1, newAmex.CustomerId);


                var deleteResponse = await client.DeleteAsync($"/PaymentType/{newAmex.Id}");
                deleteResponse.EnsureSuccessStatusCode();
                Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
            }
        }
        [Fact]
        public async Task Test_Put_For_PaymentType()
        {
            // New last name to change to and test
            string newName = "Discover";

            using (var client = new APIClientProvider().Client)
            {
                /*
                    PUT section
                 */
                PaymentType changeAMEX = new PaymentType
                {
                    AcctNumber = 3504,
                    Name = newName,
                    CustomerId = 1,
                 };
                var newNameAsJSON = JsonConvert.SerializeObject(changeAMEX);

                var response = await client.PutAsync(
                    "/PaymentType/1",
                    new StringContent(newNameAsJSON, Encoding.UTF8, "application/json")
                );
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();

                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

                /*
                    GET section
                 */
                var getNewName = await client.GetAsync("/PaymentType/1");
                getNewName.EnsureSuccessStatusCode();

                string getNewNameBody = await getNewName.Content.ReadAsStringAsync();
                PaymentType newNameCard = JsonConvert.DeserializeObject<PaymentType>(getNewNameBody);

                Assert.Equal(HttpStatusCode.OK, getNewName.StatusCode);
                Assert.Equal(newName, newNameCard.Name);
            }
        }

    }
}
