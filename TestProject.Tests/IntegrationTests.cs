using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
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
            sw.ElapsedMilliseconds.Should().BeLessThan(2000);
        }

        // TEST NAME - getSingleEntryById
        // TEST DESCRIPTION - It finds single car by ID
        [Fact]
        public async Task Test2()
        {
            await SeedData();

            var response0 = await Client.GetAsync("/api/cars/1");
            response0.StatusCode.Should().BeEquivalentTo(200);

            var car = JsonConvert.DeserializeObject<Car>(response0.Content.ReadAsStringAsync().Result);
            car.Price.Should().Be(50000);
            car.Make.Should().Be("Audi");
            car.Year.Should().Be(2019);

            var response1 = await Client.GetAsync("/api/cars/101");
            response1.StatusCode.Should().BeEquivalentTo(StatusCodes.Status404NotFound);
        }

        // TEST NAME - getSingleEntryByFilter
        // TEST DESCRIPTION - It finds single car by ID
        [Fact]
        public async Task Test3()
        {
            await SeedData();

            var response1 = await Client.GetAsync("/api/cars?years=2019&years=2018");
            response1.StatusCode.Should().BeEquivalentTo(StatusCodes.Status200OK);
            var filteredcars = JsonConvert.DeserializeObject<IEnumerable<Car>>(response1.Content.ReadAsStringAsync().Result).ToArray();
            filteredcars.Length.Should().Be(3);
            filteredcars.Where(x => x.Make == "Audi").ToArray().Length.Should().Be(1);
            filteredcars.Where(x => x.Make == "Toyota").ToArray().Length.Should().Be(2);
        }

        // TEST NAME - deleteCarById
        // TEST DESCRIPTION - Check delete car web api end point
        [Fact]
        public async Task Test4()
        {
            await SeedData();

            var response0 = await Client.DeleteAsync("/api/cars/1");
            response0.StatusCode.Should().BeEquivalentTo(StatusCodes.Status204NoContent);

            var response1 = await Client.GetAsync("/api/cars/1");
            response1.StatusCode.Should().BeEquivalentTo(StatusCodes.Status404NotFound);
        }

        // TEST NAME - updateCarById
        // TEST DESCRIPTION - Check update car web api end point
        [Fact]
        public async Task Test5()
        {
            await SeedData();

            var updateForm = new UpdateCarForm()
            {
                Id = 1,
                Year = 2017,
                Make = "Audi",
                Model = "A6",
                Price = 71000
            };

            var response0 = await Client.PutAsync("/api/cars/1", new StringContent(JsonConvert.SerializeObject(updateForm), Encoding.UTF8, "application/json"));
            response0.StatusCode.Should().BeEquivalentTo(StatusCodes.Status204NoContent);

            var response1 = await Client.GetAsync("/api/cars/1");
            response1.StatusCode.Should().BeEquivalentTo(StatusCodes.Status200OK);

            var car = JsonConvert.DeserializeObject<Car>(response1.Content.ReadAsStringAsync().Result);
            response1.StatusCode.Should().BeEquivalentTo(StatusCodes.Status200OK);
            car.Year.Should().Be(2017);
            car.Price.Should().Be(71000);
        }

        // TEST NAME - deleteCarByIdAndGettingAll
        // TEST DESCRIPTION - Check delete car web api end point and check clearing hash after that
        [Fact]
        public async Task Test6()
        {
            await SeedData();

            var response0 = await Client.GetAsync("/api/cars");
            response0.StatusCode.Should().BeEquivalentTo(200);
            var cars = JsonConvert.DeserializeObject<IEnumerable<Car>>(response0.Content.ReadAsStringAsync().Result);
            cars.Count().Should().Be(4);

            var response1 = await Client.DeleteAsync("/api/cars/1");
            response1.StatusCode.Should().BeEquivalentTo(StatusCodes.Status204NoContent);

            var response2 = await Client.GetAsync("/api/cars/1");
            response2.StatusCode.Should().BeEquivalentTo(StatusCodes.Status404NotFound);

            var response3 = await Client.GetAsync("/api/cars");
            response3.StatusCode.Should().BeEquivalentTo(200);
            var newCars = JsonConvert.DeserializeObject<IEnumerable<Car>>(response3.Content.ReadAsStringAsync().Result);
            newCars.Count().Should().Be(3);
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
