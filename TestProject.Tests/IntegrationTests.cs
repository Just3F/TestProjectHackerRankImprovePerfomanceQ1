using System;
using System.Collections.Generic;
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
            var createForm0 = GenerateMovieCreateForm("Movie Title 1", "DramaKey", DateTime.Parse("05.03.2019"));
            var response0 = await Client.PostAsync("/api/movies", new StringContent(JsonConvert.SerializeObject(createForm0), Encoding.UTF8, "application/json"));

            var createForm1 = GenerateMovieCreateForm("Movie Title 2", "ComedyKey", DateTime.Parse("15.08.2019"));
            var response1 = await Client.PostAsync("/api/movies", new StringContent(JsonConvert.SerializeObject(createForm1), Encoding.UTF8, "application/json"));

            var createForm2 = GenerateMovieCreateForm("Movie Title 3", "HorrorKey", DateTime.Parse("05.10.2019"));
            var response2 = await Client.PostAsync("/api/movies", new StringContent(JsonConvert.SerializeObject(createForm2), Encoding.UTF8, "application/json"));

            var createForm3 = GenerateMovieCreateForm("Movie Title 4", "ComedyKey", DateTime.Parse("22.03.2020"));
            var response3 = await Client.PostAsync("/api/movies", new StringContent(JsonConvert.SerializeObject(createForm3), Encoding.UTF8, "application/json"));
        }

        private MovieForm GenerateMovieCreateForm(string movieName, string category, DateTime releaseDate)
        {
            return new MovieForm
            {
                Title = movieName,
                Category = category,
                ReleaseDate = releaseDate
            };
        }

        // TEST NAME - getAllEntriesById
        // TEST DESCRIPTION - It finds all documents in Database and movie for the created document
        [Fact]
        public async Task Test1()
        {
            await SeedData();

            var response0 = await Client.GetAsync("/api/movies");
            response0.StatusCode.Should().BeEquivalentTo(StatusCodes.Status200OK);
            var documents = JsonConvert.DeserializeObject<IEnumerable<Movie>>(response0.Content.ReadAsStringAsync().Result).ToList();
            documents.Count.Should().Be(4);
        }

        // TEST NAME - getSingleEntryById
        // TEST DESCRIPTION - It finds single document by ID
        [Fact]
        public async Task Test2()
        {
            await SeedData();

            var response0 = await Client.GetAsync("/api/movies/1");
            response0.StatusCode.Should().BeEquivalentTo(200);

            var project = JsonConvert.DeserializeObject<Movie>(response0.Content.ReadAsStringAsync().Result);
            project.Title.Should().Be("Movie Title 1");

            var response1 = await Client.GetAsync("/api/movies/101");
            response1.StatusCode.Should().BeEquivalentTo(StatusCodes.Status404NotFound);
        }


        // TEST NAME - getSingleEntryById
        // TEST DESCRIPTION - It finds single document by ID
        [Fact]
        public async Task Test3()
        {
            var createForm3 = GenerateMovieCreateForm("Movie Title 4", "DramaKey", DateTime.Parse("05.03.2019"));
            var response3 = await Client.PostAsync("/api/movies", new StringContent(JsonConvert.SerializeObject(createForm3), Encoding.UTF8, "application/json"));

            var response0 = await Client.GetAsync("/api/movies/1");
            response0.StatusCode.Should().BeEquivalentTo(200);

            var project = JsonConvert.DeserializeObject<Movie>(response0.Content.ReadAsStringAsync().Result);
            project.Title.Should().Be("Movie Title 4");
        }

        // TEST NAME - deleteDocumentById
        // TEST DESCRIPTION - Check delete document web api end point
        [Fact]
        public async Task Test4()
        {
            await SeedData();

            var response0 = await Client.DeleteAsync("/api/movies/1");
            response0.StatusCode.Should().BeEquivalentTo(StatusCodes.Status204NoContent);

            var response1 = await Client.GetAsync("/api/movies/1");
            response1.StatusCode.Should().BeEquivalentTo(StatusCodes.Status404NotFound);
        }

        // TEST NAME - updateDocumentById
        // TEST DESCRIPTION - Check update document web api end point
        [Fact]
        public async Task Test5()
        {
            await SeedData();

            var updatedMovieName = "Updated movieName";

            var updateForm = new MovieForm()
            {
                Id = 1,
                Title = updatedMovieName,
            };

            var response0 = await Client.PutAsync("/api/movies", new StringContent(JsonConvert.SerializeObject(updateForm), Encoding.UTF8, "application/json"));
            response0.StatusCode.Should().BeEquivalentTo(StatusCodes.Status204NoContent);

            var response1 = await Client.GetAsync("/api/movies/1");
            response1.StatusCode.Should().BeEquivalentTo(StatusCodes.Status200OK);

            var project = JsonConvert.DeserializeObject<Movie>(response1.Content.ReadAsStringAsync().Result);
            response1.StatusCode.Should().BeEquivalentTo(StatusCodes.Status200OK);
            project.Title.Should().Be(updatedMovieName);
        }

        // TEST NAME - checkTranslations
        // TEST DESCRIPTION - Check update document web api end point
        [Fact]
        public async Task Test6()
        {
            await SeedData();

            Client.DefaultRequestHeaders.Clear();
            var response = await Client.GetAsync("/api/movies/1");
            response.StatusCode.Should().BeEquivalentTo(200);

            var movieDefault = JsonConvert.DeserializeObject<Movie>(response.Content.ReadAsStringAsync().Result);
            movieDefault.Title.Should().Be("Movie Title 1");

            movieDefault.Category.Should().Be("Drama");

            Client.DefaultRequestHeaders.Clear();
            Client.DefaultRequestHeaders.AcceptLanguage.Add(new StringWithQualityHeaderValue("ru"));
            var response0 = await Client.GetAsync("/api/movies/1");
            response0.StatusCode.Should().BeEquivalentTo(200);

            var movie = JsonConvert.DeserializeObject<Movie>(response0.Content.ReadAsStringAsync().Result);
            movie.Title.Should().Be("Movie Title 1");

            movie.Category.Should().Be("Драма");

            Client.DefaultRequestHeaders.Clear();
            Client.DefaultRequestHeaders.AcceptLanguage.Add(new StringWithQualityHeaderValue("it"));
            var response1 = await Client.GetAsync("/api/movies/1");
            response1.StatusCode.Should().BeEquivalentTo(200);

            var movieIt = JsonConvert.DeserializeObject<Movie>(response1.Content.ReadAsStringAsync().Result);
            movieIt.Title.Should().Be("Movie Title 1");

            movieIt.Category.Should().Be("Dramma");
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
