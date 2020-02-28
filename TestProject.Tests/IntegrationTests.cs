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
            var createForm0 = GenerateProjectCreateForm("Project Name 1");
            var response0 = await Client.PostAsync("/api/projects", new StringContent(JsonConvert.SerializeObject(createForm0), Encoding.UTF8, "application/json"));

            var createForm1 = GenerateProjectCreateForm("Project Name 2");
            var response1 = await Client.PostAsync("/api/projects", new StringContent(JsonConvert.SerializeObject(createForm1), Encoding.UTF8, "application/json"));

            var createForm2 = GenerateProjectCreateForm("Project Name 3");
            var response2 = await Client.PostAsync("/api/projects", new StringContent(JsonConvert.SerializeObject(createForm2), Encoding.UTF8, "application/json"));

            var createForm3 = GenerateProjectCreateForm("Project Name 4");
            var response3 = await Client.PostAsync("/api/projects", new StringContent(JsonConvert.SerializeObject(createForm3), Encoding.UTF8, "application/json"));
        }

        public async Task SeedUser(string userName, int projectId)
        {
            var userForm = new UserForm
            {
                Name = userName,
                ProjectId = projectId
            };
            var response1 = await Client.PostAsync($"/api/projects/{projectId}/users",
                new StringContent(JsonConvert.SerializeObject(userForm), Encoding.UTF8, "application/json"));
        }

        private ProjectForm GenerateProjectCreateForm(string projectName)
        {
            return new ProjectForm
            {
                Name = projectName,
            };
        }

        // TEST NAME - getAllEntriesById
        // TEST DESCRIPTION - It finds all projects in Database
        [Fact]
        public async Task Test1()
        {
            await SeedData();

            var response0 = await Client.GetAsync("/api/projects");
            response0.StatusCode.Should().BeEquivalentTo(200);
            var projects = JsonConvert.DeserializeObject<IEnumerable<Project>>(response0.Content.ReadAsStringAsync().Result).ToList();
            projects.Count().Should().Be(4);

            var project = projects.FirstOrDefault(x => x.Name == "Project Name 1");
            project.Should().NotBeNull();

            await SeedUser("test user", project.Id);
        }

        // TEST NAME - getSingleEntryById
        // TEST DESCRIPTION - It finds single room by ID
        [Fact]
        public async Task Test2()
        {
            await SeedData();

            var response0 = await Client.GetAsync("/api/projects/1");
            response0.StatusCode.Should().BeEquivalentTo(200);

            var room = JsonConvert.DeserializeObject<Project>(response0.Content.ReadAsStringAsync().Result);
            room.Name.Should().Be("Project Name 1");

            var response1 = await Client.GetAsync("/api/projects/101");
            response1.StatusCode.Should().BeEquivalentTo(StatusCodes.Status404NotFound);
        }

        // TEST NAME - getSingleEntryByFilter
        // TEST DESCRIPTION - It finds single room by ID
        [Fact]
        public async Task Test3()
        {
            await SeedData();

            var response1 = await Client.GetAsync("/api/projects?Floors=5&Floors=6");
            response1.StatusCode.Should().BeEquivalentTo(StatusCodes.Status200OK);
            var filteredProjects = JsonConvert.DeserializeObject<IEnumerable<Project>>(response1.Content.ReadAsStringAsync().Result).ToArray();
            filteredProjects.Length.Should().Be(4);
            //filteredProjects.Where(x => x.Floor == 5).ToArray().Length.Should().Be(3);
            //filteredProjects.Where(x => x.Floor == 6).ToArray().Length.Should().Be(1);
        }

        // TEST NAME - deleteRoomById
        // TEST DESCRIPTION - Check delete room web api end point
        [Fact]
        public async Task Test4()
        {
            await SeedData();

            var response0 = await Client.DeleteAsync("/api/projects/1");
            response0.StatusCode.Should().BeEquivalentTo(StatusCodes.Status204NoContent);

            var response1 = await Client.GetAsync("/api/projects/1");
            response1.StatusCode.Should().BeEquivalentTo(StatusCodes.Status404NotFound);
        }

        // TEST NAME - updateRoomById
        // TEST DESCRIPTION - Check update room web api end point
        [Fact]
        public async Task Test5()
        {
            await SeedData();

            var newFloor = 5;
            var updatedCategory = "Updated projectName";

            var updateForm = new ProjectForm()
            {
                Id = 1,
                Name = updatedCategory,
                IsAvailable = false
            };

            var response0 = await Client.PutAsync("/api/projects/1", new StringContent(JsonConvert.SerializeObject(updateForm), Encoding.UTF8, "application/json"));
            response0.StatusCode.Should().BeEquivalentTo(StatusCodes.Status204NoContent);

            var response1 = await Client.GetAsync("/api/projects/1");
            response1.StatusCode.Should().BeEquivalentTo(StatusCodes.Status200OK);

            var room = JsonConvert.DeserializeObject<Project>(response1.Content.ReadAsStringAsync().Result);
            response1.StatusCode.Should().BeEquivalentTo(StatusCodes.Status200OK);
            room.Name.Should().Be(updatedCategory);
            room.IsAvailable.Should().Be(false);

            Client.DefaultRequestHeaders.Clear();
            var response2 = await Client.GetAsync("/api/projects/1");
            response2.StatusCode.Should().BeEquivalentTo(403);
            var room2 = JsonConvert.DeserializeObject<Project>(response2.Content.ReadAsStringAsync().Result);
            room2.Should().BeNull();
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
