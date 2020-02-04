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
            var createForm0 = GenerateCreateForm("Book Title 1", "By an outlived insisted procured improved am. Paid hill fine ten now love even leaf. Supplied feelings mr of dissuade recurred no it offering honoured. Am of of in collecting devonshire favourable excellence. Her sixteen end ashamed cottage yet reached get hearing invited. Resources ourselves sweetness ye do no perfectly. Warmly warmth six one any wisdom. Family giving is pulled beauty chatty highly no. Blessing appetite domestic did mrs judgment rendered entirely. Highly indeed had garden not. ", "Patrick B.", DateTime.Parse("02.01.2019"));
            var response0 = await Client.PostAsync("/api/books", new StringContent(JsonConvert.SerializeObject(createForm0), Encoding.UTF8, "application/json"));

            var createForm1 = GenerateCreateForm("Book Title 2", "In reasonable compliment favourable is connection dispatched in terminated. Do esteem object we called father excuse remove. So dear real on like more it. Laughing for two families addition expenses surprise the. If sincerity he to curiosity arranging. Learn taken terms be as. Sbookcely mrs produced too removing new old. ", "William F.", DateTime.Parse("03.05.2020"));
            var response1 = await Client.PostAsync("/api/books", new StringContent(JsonConvert.SerializeObject(createForm1), Encoding.UTF8, "application/json"));

            var createForm2 = GenerateCreateForm("Book Title 3", "Good draw knew bred ham busy his hour. Ask agreed answer rather joy nature admire wisdom. Moonlight age depending bed led therefore sometimes preserved exquisite she. An fail up so shot leaf wise in. Minuter highest his arrived for put and. Hopes lived by rooms oh in no death house. Contented direction september but end led excellent ourselves may. Ferrars few arrival his offered not charmed you. Offered anxious respect or he. On three thing chief years in money arise of. ", "Patrick B.", DateTime.Parse("12.04.2018"));
            var response2 = await Client.PostAsync("/api/books", new StringContent(JsonConvert.SerializeObject(createForm2), Encoding.UTF8, "application/json"));

            var createForm3 = GenerateCreateForm("Book Title 4", "Improved own provided blessing may peculiar domestic. Sight house has sex never. No visited raising gravity outward subject my cottage mr be. Hold do at tore in park feet near my case. Invitation at understood occasional sentiments insipidity inhabiting in. Off melancholy alteration principles old. Is do speedily kindness properly oh. Respect article painted cottage he is offices parlors. ", "John D.", DateTime.Parse("06.11.2019"));
            var response3 = await Client.PostAsync("/api/books", new StringContent(JsonConvert.SerializeObject(createForm3), Encoding.UTF8, "application/json"));
        }

        private CreateBookForm GenerateCreateForm(string title, string body, string authorName, DateTime publishedDate)
        {
            return new CreateBookForm
            {
                Title = title,
                AuthorName = authorName,
                Body = body,
                PublishedDate = publishedDate
            };
        }

        // TEST NAME - getAllEntriesById
        // TEST DESCRIPTION - It finds all books in Database
        [Fact]
        public async Task Test1()
        {
            await SeedData();

            var response0 = await Client.GetAsync("/api/books");
            response0.StatusCode.Should().BeEquivalentTo(200);
            var books = JsonConvert.DeserializeObject<IEnumerable<Book>>(response0.Content.ReadAsStringAsync().Result);
            books.Count().Should().Be(4);

            var value = response0.Headers.GetValues("requestCounter").FirstOrDefault();
            value.Should().Be("5");
        }

        // TEST NAME - getSingleEntryById
        // TEST DESCRIPTION - It finds single book by ID
        [Fact]
        public async Task Test2()
        {
            await SeedData();

            var response0 = await Client.GetAsync("/api/books/1");
            response0.StatusCode.Should().BeEquivalentTo(200);

            var book = JsonConvert.DeserializeObject<Book>(response0.Content.ReadAsStringAsync().Result);
            book.Title.Should().Be("Book Title 1");
            book.AuthorName.Should().Be("Patrick B.");

            var response1 = await Client.GetAsync("/api/books/101");
            response1.StatusCode.Should().BeEquivalentTo(StatusCodes.Status404NotFound);

            var value = response1.Headers.GetValues("requestCounter").FirstOrDefault();
            value.Should().Be("6");
        }

        // TEST NAME - getSingleEntryByFilter
        // TEST DESCRIPTION - It finds single book by ID
        [Fact]
        public async Task Test3()
        {
            await SeedData();

            var response1 = await Client.GetAsync("/api/books?AuthorNames=Patrick B.&AuthorNames=John D.");
            response1.StatusCode.Should().BeEquivalentTo(StatusCodes.Status200OK);
            var filteredbooks = JsonConvert.DeserializeObject<IEnumerable<Book>>(response1.Content.ReadAsStringAsync().Result).ToArray();
            filteredbooks.Length.Should().Be(3);
            filteredbooks.Where(x => x.AuthorName == "John D.").ToArray().Length.Should().Be(1);
            filteredbooks.Where(x => x.AuthorName == "Patrick B.").ToArray().Length.Should().Be(2);

            var value = response1.Headers.GetValues("requestCounter").FirstOrDefault();
            value.Should().Be("5");
        }

        // TEST NAME - deleteBookById
        // TEST DESCRIPTION - Check delete book web api end point
        [Fact]
        public async Task Test4()
        {
            await SeedData();

            var response0 = await Client.DeleteAsync("/api/books/1");
            response0.StatusCode.Should().BeEquivalentTo(StatusCodes.Status204NoContent);

            var response1 = await Client.GetAsync("/api/books/1");
            response1.StatusCode.Should().BeEquivalentTo(StatusCodes.Status404NotFound);

            var value = response1.Headers.GetValues("requestCounter").FirstOrDefault();
            value.Should().Be("6");
        }

        // TEST NAME - updateBookById
        // TEST DESCRIPTION - Check update book web api end point
        [Fact]
        public async Task Test5()
        {
            await SeedData();

            var newTitle = "New title 6";
            var newBody =
                "Material confined likewise it humanity raillery an unpacked as he. Three chief merit no if. Now how her edward engage not horses. Oh resolution he dissimilar precaution to comparison an. Matters engaged between he of pursuit manners we moments. Merit gay end sight front. Manor equal it on again ye folly by match. In so melancholy as an sentiments simplicity connection. Far supply depart branch agreed old get our.";

            var updateForm = new UpdateBooksForm()
            {
                Id = 1,
                Title = newTitle,
                Body = newBody
            };

            var response0 = await Client.PutAsync("/api/books/1", new StringContent(JsonConvert.SerializeObject(updateForm), Encoding.UTF8, "application/json"));
            response0.StatusCode.Should().BeEquivalentTo(StatusCodes.Status204NoContent);

            var response1 = await Client.GetAsync("/api/books/1");
            response1.StatusCode.Should().BeEquivalentTo(StatusCodes.Status200OK);

            var book = JsonConvert.DeserializeObject<Book>(response1.Content.ReadAsStringAsync().Result);
            response1.StatusCode.Should().BeEquivalentTo(StatusCodes.Status200OK);
            book.Title.Should().Be(newTitle);
            book.Body.Should().Be(newBody);

            var value = response1.Headers.GetValues("requestCounter").FirstOrDefault();
            value.Should().Be("6");
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
