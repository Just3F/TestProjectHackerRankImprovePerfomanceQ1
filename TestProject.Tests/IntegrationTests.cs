using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Newtonsoft.Json;
using TestProject.WebAPI;
using TestProject.WebAPI.Data;
using TestProject.WebAPI.Models;
using TestProject.WebAPI.SeedData;
using Xunit;

namespace TestProject.Tests
{
    public class IntegrationTests
    {
        private TestServer _server;

        public HttpClient Client { get; private set; }

        public IntegrationTests()
        {
            SetUpClient();
        }

        private async Task SeedData()
        {
            var createForm0 = GenerateCreateForm("Audi", 2019, "A6", 50000);
            var response0 = await Client.PostAsync("/api/cars", new StringContent(JsonConvert.SerializeObject(createForm0), Encoding.UTF8, "application/json"));

            var createForm1 = GenerateCreateForm("BMW", 2020, "5", 55000);
            var response1 = await Client.PostAsync("/api/cars", new StringContent(JsonConvert.SerializeObject(createForm1), Encoding.UTF8, "application/json"));

            var createForm2 = GenerateCreateForm("Toyota", 2019, "Camry", 45000);
            var response2 = await Client.PostAsync("/api/cars", new StringContent(JsonConvert.SerializeObject(createForm2), Encoding.UTF8, "application/json"));

            var createForm3 = GenerateCreateForm("Toyota", 2018, "Supra", 35000);
            var response3 = await Client.PostAsync("/api/cars", new StringContent(JsonConvert.SerializeObject(createForm3), Encoding.UTF8, "application/json"));
        }

        private CreateCarForm GenerateCreateForm(string make, uint year, string model, uint price)
        {
            return new CreateCarForm()
            {
                Make = make,
                Model = model,
                Price = price,
                Year = year
            };
        }

        // TEST NAME - getAllEntriesById
        // TEST DESCRIPTION - It finds all cars in Database
        [Fact]
        public async Task Test1()
        {
            await SeedData();

            Stopwatch sw = new Stopwatch();
            var response0 = await Client.GetAsync("/api/cars");
            response0.StatusCode.Should().BeEquivalentTo(200);
            var cars = JsonConvert.DeserializeObject<IEnumerable<Car>>(response0.Content.ReadAsStringAsync().Result);
            cars.Count().Should().Be(4);

            sw.Start();
            var response1 = await Client.GetAsync("/api/cars");
            response1.StatusCode.Should().BeEquivalentTo(200);
            var cars2 = JsonConvert.DeserializeObject<IEnumerable<Car>>(response1.Content.ReadAsStringAsync().Result);
            cars2.Count().Should().Be(4);
            sw.Stop();
            sw.ElapsedMilliseconds.Should().BeLessThan(4000);
        }

        // TEST NAME - getSingleEntryById
        // TEST DESCRIPTION - It finds single user by ID
        [Fact]
        public async Task Test2()
        {
            await SeedData();

            var response0 = await Client.GetAsync("/api/cars/1");
            response0.StatusCode.Should().BeEquivalentTo(200);

            var user = JsonConvert.DeserializeObject<Car>(response0.Content.ReadAsStringAsync().Result);
            user.Price.Should().Be(24);

            var response1 = await Client.GetAsync("/api/cars/101");
            response1.StatusCode.Should().BeEquivalentTo(StatusCodes.Status404NotFound);
        }

        // TEST NAME - getSingleEntryById
        // TEST DESCRIPTION - It finds single user by ID
        [Fact]
        public async Task Test3()
        {
            await SeedData();

            var response1 = await Client.GetAsync("/api/cars");
            response1.StatusCode.Should().BeEquivalentTo(StatusCodes.Status200OK);
            var cars = JsonConvert.DeserializeObject<IEnumerable<Car>>(response1.Content.ReadAsStringAsync().Result);
            cars.Count().Should().Be(4);

            var response2 = await Client.GetAsync("/api/cars?firstNames=Mike&firstNames=Daniel");
            response2.StatusCode.Should().BeEquivalentTo(StatusCodes.Status200OK);
            var filteredcars = JsonConvert.DeserializeObject<IEnumerable<Car>>(response2.Content.ReadAsStringAsync().Result).ToArray();
            filteredcars.Length.Should().Be(3);
            filteredcars.Where(x => x.Make == "Mike").ToArray().Length.Should().Be(1);
            filteredcars.Where(x => x.Make == "Daniel").ToArray().Length.Should().Be(2);
        }

        // TEST NAME - deleteUserById
        // TEST DESCRIPTION - Check delete user web api end point
        [Fact]
        public async Task Test4()
        {
            await SeedData();

            var response0 = await Client.DeleteAsync("/api/cars/1");
            response0.StatusCode.Should().BeEquivalentTo(StatusCodes.Status204NoContent);

            var response1 = await Client.GetAsync("/api/cars/1");
            response1.StatusCode.Should().BeEquivalentTo(StatusCodes.Status404NotFound);
        }

        // TEST NAME - updateUserById
        // TEST DESCRIPTION - Check update user web api end point
        [Fact]
        public async Task Test5()
        {
            await SeedData();

            var updateForm = new UpdateUserForm()
            {
                Id = 1,
                //Age = 40,
                //Email = "testemail1@mail.com",
                //FirstName = "Mike",
                //LastName = "Emil",
                //Password = "0000000"
            };

            var response0 = await Client.PutAsync("/api/cars/1", new StringContent(JsonConvert.SerializeObject(updateForm), Encoding.UTF8, "application/json"));
            response0.StatusCode.Should().BeEquivalentTo(StatusCodes.Status204NoContent);

            var response1 = await Client.GetAsync("/api/cars/1");
            response1.StatusCode.Should().BeEquivalentTo(StatusCodes.Status200OK);

            var user = JsonConvert.DeserializeObject<Car>(response1.Content.ReadAsStringAsync().Result);
            response1.StatusCode.Should().BeEquivalentTo(StatusCodes.Status200OK);
            user.Price.Should().Be(40);
            //user.Count.Should().Be(5);
        }

        // TEST NAME - exportcars
        // TEST DESCRIPTION - In this test user should send byte array to the web api and put all cars(count is 1000) into the database
        [Fact]
        public async Task Test6()
        {
            //Here data is exporting to the end point
            var myJsonString = File.ReadAllBytes("MOCK_DATA.json");
            var content = new ByteArrayContent(myJsonString);
            var response0 = await Client.PostAsync("/api/cars/export", content);
            response0.StatusCode.Should().BeEquivalentTo(StatusCodes.Status200OK);

            //Here expect to see all cars from web api end point (1000).
            var response1 = await Client.GetAsync("/api/cars");
            response1.StatusCode.Should().BeEquivalentTo(StatusCodes.Status200OK);
            var cars = JsonConvert.DeserializeObject<IEnumerable<Car>>(response1.Content.ReadAsStringAsync().Result);
            cars.Count().Should().Be(1000);

            //Here check that the data is exported in the correct way
            var response2 = await Client.GetAsync("/api/cars?firstNames=Veronika&firstNames=Frances");
            response2.StatusCode.Should().BeEquivalentTo(StatusCodes.Status200OK);
            var filteredcars = JsonConvert.DeserializeObject<IEnumerable<Car>>(response2.Content.ReadAsStringAsync().Result).ToArray();
            filteredcars.Length.Should().Be(3);
            filteredcars.Where(x => x.Make == "Frances").ToArray().Length.Should().Be(1);
            filteredcars.Where(x => x.Make == "Veronika").ToArray().Length.Should().Be(2);
        }

        // TEST NAME - checkAuthorization
        // TEST DESCRIPTION - Here need to implement authorization by JWT tokens
        [Fact]
        public async Task Test7()
        {
            await SeedData();
            //var userLoginForm = new LoginUserForm { Email = "testemail2@mail.com", Password = "12345678" };

            ////Getting token by email and password
            //var response0 = await Client.PostAsync("/token",
            //    new StringContent(JsonConvert.SerializeObject(userLoginForm), Encoding.UTF8, "application/json"));
            //var jwtData = JsonConvert.DeserializeObject<LoginResponseModel>(response0.Content.ReadAsStringAsync().Result);

            ////Check that user Unauthorized
            //var response1 = await Client.GetAsync("/currentuser");
            //response1.StatusCode.Should().BeEquivalentTo(StatusCodes.Status401Unauthorized);

            ////adding token to request and check this end-point again
            //Client.DefaultRequestHeaders.Add("Authorization", "Bearer " + jwtData.AccessToken);
            //var response2 = await Client.GetAsync("/currentuser");
            //var user = JsonConvert.DeserializeObject<Car>(response2.Content.ReadAsStringAsync().Result);
            //user.Year.Should().BeEquivalentTo("testemail2@mail.com");
        }

        private void SetUpClient()
        {
            var builder = new WebHostBuilder()
                .UseStartup<Startup>()
                .ConfigureServices(services =>
                {
                    var context = new TestProjectContext(new DbContextOptionsBuilder<TestProjectContext>()
                        .UseSqlite("DataSource=:memory:")
                        .EnableSensitiveDataLogging()
                        .Options);

                    services.RemoveAll(typeof(TestProjectContext));
                    services.AddSingleton(context);

                    context.Database.OpenConnection();
                    context.Database.EnsureCreated();

                    context.SaveChanges();

                    // Clear local context cache
                    foreach (var entity in context.ChangeTracker.Entries().ToList())
                    {
                        entity.State = EntityState.Detached;
                    }
                });

            _server = new TestServer(builder);

            Client = _server.CreateClient();
        }
    }
}
