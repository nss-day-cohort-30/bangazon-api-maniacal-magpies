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
    public class TestTrainingPrograms
    {
        [Fact]
        public async Task Test_Get_All_TrainingPrograms()
        {
            using (var client = new APIClientProvider().Client)
            {
                var response = await client.GetAsync("/trainingprogram");

                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();
                var trainingPrograms = JsonConvert.DeserializeObject<List<TrainingProgram>>(responseBody);

                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.True(trainingPrograms.Count > 0);
            }
        }
        [Fact]
        public async Task Test_Get_Single_TrainingProgram()
        {

            using (var client = new APIClientProvider().Client)
            {
                var response = await client.GetAsync("/trainingprogram/2");

                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();
                var trainingProgram = JsonConvert.DeserializeObject<TrainingProgram>(responseBody);

                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Equal("Windows for Mac Users", trainingProgram.Name);
                Assert.NotNull(trainingProgram);
            }
        }

        [Fact]
        public async Task Test_Get_NonExistant_TrainingProgram_Fails()
        {

            using (var client = new APIClientProvider().Client)
            {
                var response = await client.GetAsync("/trainingprogram/999999999");
                Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            }
        }


        [Fact]
        public async Task Test_Create_And_Delete_TrainingProgram()
        {
            using (var client = new APIClientProvider().Client)
            {
                TrainingProgram tp = new TrainingProgram
                {
                    Name = "Test Program",
                    StartDate = System.DateTime.Today,
                    EndDate = System.DateTime.Today,
                    MaxAttendees = 10
                };
                var tpAsJSON = JsonConvert.SerializeObject(tp);


                var response = await client.PostAsync(
                    "/TrainingProgram",
                    new StringContent(tpAsJSON, Encoding.UTF8, "application/json")
                );

                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();
                var newTrainingProgram = JsonConvert.DeserializeObject<TrainingProgram>(responseBody);

                Assert.Equal(HttpStatusCode.Created, response.StatusCode);
                Assert.Equal("Test Program", newTrainingProgram.Name);


                var deleteResponse = await client.DeleteAsync($"/trainingProgram/{newTrainingProgram.Id}");
                deleteResponse.EnsureSuccessStatusCode();
                Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
            }
        }

        [Fact]
        public async Task Test_Delete_NonExistent_TrainingProgram_Fails()
        {
            using (var client = new APIClientProvider().Client)
            {
                var deleteResponse = await client.DeleteAsync("/trainingprogram/600000");

                Assert.False(deleteResponse.IsSuccessStatusCode);
                Assert.Equal(HttpStatusCode.NotFound, deleteResponse.StatusCode);
            }
        }

        [Fact]
        public async Task Test_Modify_TrainingProgram()
        {
            // New name to change to and test
            int id = 1;
            string newName = "Cyber Awareness for the Completely Unaware";

            using (var client = new APIClientProvider().Client)
            {
                /*
                    PUT section
                 */
                TrainingProgram modifiedTrainingProgram = new TrainingProgram
                {
                    Id = id,
                    Name = newName,
                    StartDate = System.DateTime.Today,
                    EndDate = System.DateTime.Today,
                    MaxAttendees = 22
                };
                var modifiedTrainingProgramAsJSON = JsonConvert.SerializeObject(modifiedTrainingProgram);

                var response = await client.PutAsync(
                    "/trainingprogram/1",
                    new StringContent(modifiedTrainingProgramAsJSON, Encoding.UTF8, "application/json")
                );
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();

                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

                /*
                    GET section
                 */
                var getTrainingProgram = await client.GetAsync("/trainingprogram/1");
                getTrainingProgram.EnsureSuccessStatusCode();

                string getTrainingProgramBody = await getTrainingProgram.Content.ReadAsStringAsync();
                TrainingProgram newTrainingProgram = JsonConvert.DeserializeObject<TrainingProgram>(getTrainingProgramBody);

                Assert.Equal(HttpStatusCode.OK, getTrainingProgram.StatusCode);
                Assert.Equal(newName, newTrainingProgram.Name);
            }
        }
    }
}
