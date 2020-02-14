using System;
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
            var createForm0 = GenerateCreateForm("Room Category 1", 523, 5, DateTime.Parse("02.01.2019"));
            var response0 = await Client.PostAsync("/api/rooms", new StringContent(JsonConvert.SerializeObject(createForm0), Encoding.UTF8, "application/json"));

            var createForm1 = GenerateCreateForm("Room Category 2", 512, 5, DateTime.Parse("03.05.2020"));
            var response1 = await Client.PostAsync("/api/rooms", new StringContent(JsonConvert.SerializeObject(createForm1), Encoding.UTF8, "application/json"));

            var createForm2 = GenerateCreateForm("Room Category 3", 332, 3, DateTime.Parse("12.04.2018"));
            var response2 = await Client.PostAsync("/api/rooms", new StringContent(JsonConvert.SerializeObject(createForm2), Encoding.UTF8, "application/json"));

            var createForm3 = GenerateCreateForm("Room Category 4", 123, 1, DateTime.Parse("06.11.2019"));
            var response3 = await Client.PostAsync("/api/rooms", new StringContent(JsonConvert.SerializeObject(createForm3), Encoding.UTF8, "application/json"));
       
            var createForm4 = GenerateCreateForm("Room Category 5", 573, 5, DateTime.Parse("03.01.2020"));
            var response4 = await Client.PostAsync("/api/rooms", new StringContent(JsonConvert.SerializeObject(createForm4), Encoding.UTF8, "application/json"));
       
            var createForm5 = GenerateCreateForm("Room Category 6", 632, 6, DateTime.Parse("06.12.2018"));
            var response5 = await Client.PostAsync("/api/rooms", new StringContent(JsonConvert.SerializeObject(createForm5), Encoding.UTF8, "application/json"));
        }

        private RoomForm GenerateCreateForm(string category, int number, int floor, DateTime publishedDate)
        {
            return new RoomForm
            {
                Category = category,
                Floor = floor,
                Number = number,
                AddedDate = publishedDate
            };
        }

        // TEST NAME - getAllEntriesById
        // TEST DESCRIPTION - It finds all rooms in Database
        [Fact]
        public async Task Test1()
        {
            await SeedData();

            var response0 = await Client.GetAsync("/api/rooms");
            response0.StatusCode.Should().BeEquivalentTo(200);
            var rooms = JsonConvert.DeserializeObject<IEnumerable<Room>>(response0.Content.ReadAsStringAsync().Result);
            rooms.Count().Should().Be(6);

            //var value = response0.Headers.GetValues("requestCounter").FirstOrDefault();
            //value.Should().Be("5");
        }

        // TEST NAME - getSingleEntryById
        // TEST DESCRIPTION - It finds single room by ID
        [Fact]
        public async Task Test2()
        {
            await SeedData();

            var response0 = await Client.GetAsync("/api/rooms/1");
            response0.StatusCode.Should().BeEquivalentTo(200);

            var room = JsonConvert.DeserializeObject<Room>(response0.Content.ReadAsStringAsync().Result);
            room.Category.Should().Be("Room Category 1");
            room.Number.Should().Be(523);

            var response1 = await Client.GetAsync("/api/rooms/101");
            response1.StatusCode.Should().BeEquivalentTo(StatusCodes.Status404NotFound);

            //var value = response1.Headers.GetValues("requestCounter").FirstOrDefault();
            //value.Should().Be("6");
        }

        // TEST NAME - getSingleEntryByFilter
        // TEST DESCRIPTION - It finds single room by ID
        [Fact]
        public async Task Test3()
        {
            await SeedData();

            var response1 = await Client.GetAsync("/api/rooms?Floors=5&Floors=6");
            response1.StatusCode.Should().BeEquivalentTo(StatusCodes.Status200OK);
            var filteredRooms = JsonConvert.DeserializeObject<IEnumerable<Room>>(response1.Content.ReadAsStringAsync().Result).ToArray();
            filteredRooms.Length.Should().Be(4);
            filteredRooms.Where(x => x.Floor == 5).ToArray().Length.Should().Be(3);
            filteredRooms.Where(x => x.Floor == 6).ToArray().Length.Should().Be(1);

            //var value = response1.Headers.GetValues("requestCounter").FirstOrDefault();
            //value.Should().Be("5");
        }

        // TEST NAME - deleteRoomById
        // TEST DESCRIPTION - Check delete room web api end point
        [Fact]
        public async Task Test4()
        {
            await SeedData();

            var response0 = await Client.DeleteAsync("/api/rooms/1");
            response0.StatusCode.Should().BeEquivalentTo(StatusCodes.Status204NoContent);

            var response1 = await Client.GetAsync("/api/rooms/1");
            response1.StatusCode.Should().BeEquivalentTo(StatusCodes.Status404NotFound);

            //var value = response1.Headers.GetValues("requestCounter").FirstOrDefault();
            //value.Should().Be("6");
        }

        // TEST NAME - updateRoomById
        // TEST DESCRIPTION - Check update room web api end point
        [Fact]
        public async Task Test5()
        {
            await SeedData();

            var newFloor = 5;
            var updatedCategory = "Updated category";

            var updateForm = new RoomForm()
            {
                Id = 1,
                Floor = newFloor,
                Category = updatedCategory,
                IsAvailable = false
            };

            var response0 = await Client.PutAsync("/api/rooms/1", new StringContent(JsonConvert.SerializeObject(updateForm), Encoding.UTF8, "application/json"));
            response0.StatusCode.Should().BeEquivalentTo(StatusCodes.Status204NoContent);

            var response1 = await Client.GetAsync("/api/rooms/1");
            response1.StatusCode.Should().BeEquivalentTo(StatusCodes.Status200OK);

            var room = JsonConvert.DeserializeObject<Room>(response1.Content.ReadAsStringAsync().Result);
            response1.StatusCode.Should().BeEquivalentTo(StatusCodes.Status200OK);
            room.Category.Should().Be(updatedCategory);
            room.IsAvailable.Should().Be(false);
            room.Floor.Should().Be(newFloor);

            //var value = response1.Headers.GetValues("requestCounter").FirstOrDefault();
            //value.Should().Be("6");
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
