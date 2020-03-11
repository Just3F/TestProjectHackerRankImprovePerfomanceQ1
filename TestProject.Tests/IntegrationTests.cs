using System;
using System.Collections.Generic;
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
            var createForm0 = GenerateLibraryCreateForm("Library Name 1");
            var response0 = await Client.PostAsync("/api/libraries", new StringContent(JsonConvert.SerializeObject(createForm0), Encoding.UTF8, "application/json"));

            var createForm1 = GenerateLibraryCreateForm("Library Name 2");
            var response1 = await Client.PostAsync("/api/libraries", new StringContent(JsonConvert.SerializeObject(createForm1), Encoding.UTF8, "application/json"));

            var createForm2 = GenerateLibraryCreateForm("Library Name 3");
            var response2 = await Client.PostAsync("/api/libraries", new StringContent(JsonConvert.SerializeObject(createForm2), Encoding.UTF8, "application/json"));

            var createForm3 = GenerateLibraryCreateForm("Library Name 4");
            var response3 = await Client.PostAsync("/api/libraries", new StringContent(JsonConvert.SerializeObject(createForm3), Encoding.UTF8, "application/json"));
        }

        public async Task SeedBook(string bookName, int libraryId)
        {
            var bookForm = new BookForm
            {
                Name = bookName,
                LibraryId = libraryId
            };
            var response1 = await Client.PostAsync($"/api/libraries/{libraryId}/books",
                new StringContent(JsonConvert.SerializeObject(bookForm), Encoding.UTF8, "application/json"));
        }

        private LibraryForm GenerateLibraryCreateForm(string libraryName)
        {
            return new LibraryForm
            {
                Name = libraryName,
            };
        }

        // TEST NAME - getAllEntriesById
        // TEST DESCRIPTION - It finds all libraries in Database and book for the created library
        [Fact]
        public async Task Test1()
        {
            await SeedData();

            var response0 = await Client.GetAsync("/api/libraries");
            response0.StatusCode.Should().BeEquivalentTo(StatusCodes.Status200OK);
            var libraries = JsonConvert.DeserializeObject<IEnumerable<Library>>(response0.Content.ReadAsStringAsync().Result).ToList();
            libraries.Count.Should().Be(4);

            var library = libraries.FirstOrDefault(x => x.Name == "Library Name 1");
            library.Should().NotBeNull();

            await SeedBook("test book 1", library.Id);
            await SeedBook("test book 2", library.Id);
            var response1 = await Client.GetAsync($"/api/libraries/{library.Id}/books");
            response1.StatusCode.Should().BeEquivalentTo(StatusCodes.Status200OK);
            var books = JsonConvert.DeserializeObject<IEnumerable<Book>>(response1.Content.ReadAsStringAsync().Result).ToList();
            books.Count.Should().Be(2);

        }

        // TEST NAME - getSingleEntryById
        // TEST DESCRIPTION - It finds single library by ID
        [Fact]
        public async Task Test2()
        {
            await SeedData();

            var response0 = await Client.GetAsync("/api/libraries/1");
            response0.StatusCode.Should().BeEquivalentTo(200);

            var library = JsonConvert.DeserializeObject<Library>(response0.Content.ReadAsStringAsync().Result);
            library.Name.Should().Be("Library Name 1");

            var response1 = await Client.GetAsync("/api/libraries/101");
            response1.StatusCode.Should().BeEquivalentTo(StatusCodes.Status404NotFound);

            await SeedBook("test book", library.Id);
            var response2 = await Client.GetAsync($"/api/libraries/21312/books/1");
            response2.StatusCode.Should().BeEquivalentTo(StatusCodes.Status404NotFound);

            await SeedBook("test book", library.Id);
            var response3 = await Client.GetAsync($"/api/libraries/{library.Id}/books/1");
            response3.StatusCode.Should().BeEquivalentTo(StatusCodes.Status200OK);
            var book = JsonConvert.DeserializeObject<Book>(response3.Content.ReadAsStringAsync().Result);
            book.Name.Should().Be("test book");
            book.LibraryId.Should().Be(library.Id);
        }

        // TEST NAME - getSingleEntryByFilter
        // TEST DESCRIPTION - It finds single book for library by ID
        [Fact]
        public async Task Test3()
        {
            await SeedData();

            var response1 = await Client.GetAsync("/api/libraries");
            response1.StatusCode.Should().BeEquivalentTo(StatusCodes.Status200OK);
            var filteredLibraries = JsonConvert.DeserializeObject<IEnumerable<Library>>(response1.Content.ReadAsStringAsync().Result).ToArray();
            filteredLibraries.Length.Should().Be(4);

            await SeedBook("test book 1", 1);
            await SeedBook("test book 2", 1);
            var response2 = await Client.GetAsync($"/api/libraries/2/books");
            response2.StatusCode.Should().BeEquivalentTo(StatusCodes.Status200OK);
            var books = JsonConvert.DeserializeObject<IEnumerable<Book>>(response2.Content.ReadAsStringAsync().Result).ToList();
            books.Count.Should().Be(0);
            
            var response3 = await Client.GetAsync($"/api/libraries/1/books");
            response3.StatusCode.Should().BeEquivalentTo(StatusCodes.Status200OK);
            var books2 = JsonConvert.DeserializeObject<IEnumerable<Book>>(response3.Content.ReadAsStringAsync().Result).ToList();
            books2.Count.Should().Be(2);

            var response4 = await Client.GetAsync($"/api/libraries/31232/books");
            response4.StatusCode.Should().BeEquivalentTo(StatusCodes.Status404NotFound);
        }

        // TEST NAME - deleteLibraryById
        // TEST DESCRIPTION - Check delete library web api end point
        [Fact]
        public async Task Test4()
        {
            await SeedData();

            var response0 = await Client.DeleteAsync("/api/libraries/1");
            response0.StatusCode.Should().BeEquivalentTo(StatusCodes.Status204NoContent);

            var response1 = await Client.GetAsync("/api/libraries/1");
            response1.StatusCode.Should().BeEquivalentTo(StatusCodes.Status404NotFound);
        }

        // TEST NAME - updateLibraryById
        // TEST DESCRIPTION - Check update library web api end point
        [Fact]
        public async Task Test5()
        {
            await SeedData();

            var updatedLibraryName = "Updated libraryName";
            var newLibraryBody = "Updated library body";

            var updateForm = new LibraryForm()
            {
                Id = 1,
                Name = updatedLibraryName,
                Location = newLibraryBody
            };

            var response0 = await Client.PutAsync("/api/libraries/1", new StringContent(JsonConvert.SerializeObject(updateForm), Encoding.UTF8, "application/json"));
            response0.StatusCode.Should().BeEquivalentTo(StatusCodes.Status204NoContent);

            var response1 = await Client.GetAsync("/api/libraries/1");
            response1.StatusCode.Should().BeEquivalentTo(StatusCodes.Status200OK);

            var library = JsonConvert.DeserializeObject<Library>(response1.Content.ReadAsStringAsync().Result);
            response1.StatusCode.Should().BeEquivalentTo(StatusCodes.Status200OK);
            library.Name.Should().Be(updatedLibraryName);
            library.Location.Should().Be(newLibraryBody);
        }

        // TEST NAME - updateBookById
        // TEST DESCRIPTION - Check update book web api end point
        [Fact]
        public async Task Test6()
        {
            await SeedData();
            await SeedBook("test book 1", 1);

            var updatedBookname = "Updated bookname";

            var updateForm = new BookForm
            {
                Id = 1,
                Name = updatedBookname,
            };

            var response0 = await Client.PutAsync("/api/libraries/1/books", new StringContent(JsonConvert.SerializeObject(updateForm), Encoding.UTF8, "application/json"));
            response0.StatusCode.Should().BeEquivalentTo(StatusCodes.Status204NoContent);

            var response1 = await Client.GetAsync("/api/libraries/1/books/1");
            response1.StatusCode.Should().BeEquivalentTo(StatusCodes.Status200OK);

            var book = JsonConvert.DeserializeObject<Book>(response1.Content.ReadAsStringAsync().Result);
            response1.StatusCode.Should().BeEquivalentTo(StatusCodes.Status200OK);
            book.Name.Should().Be(updatedBookname);
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
