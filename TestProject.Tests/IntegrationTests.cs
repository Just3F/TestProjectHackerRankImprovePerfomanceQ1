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
            var createForm0 = GenerateCreateForm("News Title 1", "By an outlived insisted procured improved am. Paid hill fine ten now love even leaf. Supplied feelings mr of dissuade recurred no it offering honoured. Am of of in collecting devonshire favourable excellence. Her sixteen end ashamed cottage yet reached get hearing invited. Resources ourselves sweetness ye do no perfectly. Warmly warmth six one any wisdom. Family giving is pulled beauty chatty highly no. Blessing appetite domestic did mrs judgment rendered entirely. Highly indeed had garden not. ", "Patrick B.", true);
            var response0 = await Client.PostAsync("/api/NewsFeed", new StringContent(JsonConvert.SerializeObject(createForm0), Encoding.UTF8, "application/json"));

            var createForm1 = GenerateCreateForm("News Title 2", "In reasonable compliment favourable is connection dispatched in terminated. Do esteem object we called father excuse remove. So dear real on like more it. Laughing for two families addition expenses surprise the. If sincerity he to curiosity arranging. Learn taken terms be as. SnewsItemcely mrs produced too removing new old. ", "William F.", true);
            var response1 = await Client.PostAsync("/api/NewsFeed", new StringContent(JsonConvert.SerializeObject(createForm1), Encoding.UTF8, "application/json"));

            var createForm2 = GenerateCreateForm("News Title 3", "Good draw knew bred ham busy his hour. Ask agreed answer rather joy nature admire wisdom. Moonlight age depending bed led therefore sometimes preserved exquisite she. An fail up so shot leaf wise in. Minuter highest his arrived for put and. Hopes lived by rooms oh in no death house. Contented direction september but end led excellent ourselves may. Ferrars few arrival his offered not charmed you. Offered anxious respect or he. On three thing chief years in money arise of. ", "Patrick B.", true);
            var response2 = await Client.PostAsync("/api/NewsFeed", new StringContent(JsonConvert.SerializeObject(createForm2), Encoding.UTF8, "application/json"));

            var createForm3 = GenerateCreateForm("News Title 4", "Improved own provided blessing may peculiar domestic. Sight house has sex never. No visited raising gravity outward subject my cottage mr be. Hold do at tore in park feet near my case. Invitation at understood occasional sentiments insipidity inhabiting in. Off melancholy alteration principles old. Is do speedily kindness properly oh. Respect article painted cottage he is offices parlors. ", "John D.", false);
            var response3 = await Client.PostAsync("/api/NewsFeed", new StringContent(JsonConvert.SerializeObject(createForm3), Encoding.UTF8, "application/json"));
        }

        private CreateNewsFeedItemForm GenerateCreateForm(string title, string body, string authorName, bool allowComments)
        {
            return new CreateNewsFeedItemForm
            {
                Title = title,
                AuthorName = authorName,
                AllowComments = allowComments,
                Body = body
            };
        }

        // TEST NAME - getAllEntriesById
        // TEST DESCRIPTION - It finds all newsItems in Database
        [Fact]
        public async Task Test1()
        {
            await SeedData();

            Stopwatch sw = new Stopwatch();
            var response0 = await Client.GetAsync("/api/NewsFeed");
            response0.StatusCode.Should().BeEquivalentTo(200);
            var newsItems = JsonConvert.DeserializeObject<IEnumerable<NewsFeedItem>>(response0.Content.ReadAsStringAsync().Result);
            newsItems.Count().Should().Be(4);

            sw.Start();
            var response1 = await Client.GetAsync("/api/NewsFeed");
            response1.StatusCode.Should().BeEquivalentTo(200);
            var newsItems2 = JsonConvert.DeserializeObject<IEnumerable<NewsFeedItem>>(response1.Content.ReadAsStringAsync().Result);
            newsItems2.Count().Should().Be(4);
            sw.Stop();
            sw.ElapsedMilliseconds.Should().BeLessThan(2000);
        }

        // TEST NAME - getSingleEntryById
        // TEST DESCRIPTION - It finds single newsItem by ID
        [Fact]
        public async Task Test2()
        {
            await SeedData();

            var response0 = await Client.GetAsync("/api/NewsFeed/1");
            response0.StatusCode.Should().BeEquivalentTo(200);

            var newsItem = JsonConvert.DeserializeObject<NewsFeedItem>(response0.Content.ReadAsStringAsync().Result);
            newsItem.Title.Should().Be("News Title 1");
            newsItem.AuthorName.Should().Be("Patrick B.");
            newsItem.AllowComments.Should().Be(true);

            var response1 = await Client.GetAsync("/api/newsItems/101");
            response1.StatusCode.Should().BeEquivalentTo(StatusCodes.Status404NotFound);
        }

        // TEST NAME - getSingleEntryByFilter
        // TEST DESCRIPTION - It finds single newsItem by ID
        [Fact]
        public async Task Test3()
        {
            await SeedData();

            var response1 = await Client.GetAsync("/api/newsFeed?AuthorNames=Patrick B.&AuthorNames=John D.");
            response1.StatusCode.Should().BeEquivalentTo(StatusCodes.Status200OK);
            var filterednewsItems = JsonConvert.DeserializeObject<IEnumerable<NewsFeedItem>>(response1.Content.ReadAsStringAsync().Result).ToArray();
            filterednewsItems.Length.Should().Be(3);
            filterednewsItems.Where(x => x.AuthorName == "John D.").ToArray().Length.Should().Be(1);
            filterednewsItems.Where(x => x.AuthorName == "Patrick B.").ToArray().Length.Should().Be(2);
        }

        // TEST NAME - deleteNewsItemById
        // TEST DESCRIPTION - Check delete newsItem web api end point
        [Fact]
        public async Task Test4()
        {
            await SeedData();

            var response0 = await Client.DeleteAsync("/api/NewsFeed/1");
            response0.StatusCode.Should().BeEquivalentTo(StatusCodes.Status204NoContent);

            var response1 = await Client.GetAsync("/api/NewsFeed/1");
            response1.StatusCode.Should().BeEquivalentTo(StatusCodes.Status404NotFound);
        }

        // TEST NAME - updateNewsItemById
        // TEST DESCRIPTION - Check update newsItem web api end point
        [Fact]
        public async Task Test5()
        {
            await SeedData();

            var newTitle = "New title 6";
            var newBody =
                "Material confined likewise it humanity raillery an unpacked as he. Three chief merit no if. Now how her edward engage not horses. Oh resolution he dissimilar precaution to comparison an. Matters engaged between he of pursuit manners we moments. Merit gay end sight front. Manor equal it on again ye folly by match. In so melancholy as an sentiments simplicity connection. Far supply depart branch agreed old get our.";
            
            var updateForm = new UpdateNewsFeedItemForm()
            {
                Id = 1,
                Title = newTitle,
                Body = newBody,
                AllowComments = false
            };

            var response0 = await Client.PutAsync("/api/NewsFeed/1", new StringContent(JsonConvert.SerializeObject(updateForm), Encoding.UTF8, "application/json"));
            response0.StatusCode.Should().BeEquivalentTo(StatusCodes.Status204NoContent);

            var response1 = await Client.GetAsync("/api/NewsFeed/1");
            response1.StatusCode.Should().BeEquivalentTo(StatusCodes.Status200OK);

            var newsItem = JsonConvert.DeserializeObject<NewsFeedItem>(response1.Content.ReadAsStringAsync().Result);
            response1.StatusCode.Should().BeEquivalentTo(StatusCodes.Status200OK);
            newsItem.Title.Should().Be(newTitle);
            newsItem.Body.Should().Be(newBody);
        }

        // TEST NAME - deleteNewsItemByIdAndGettingAll
        // TEST DESCRIPTION - Check delete newsItem web api end point and check clearing hash after that
        [Fact]
        public async Task Test6()
        {
            await SeedData();

            var response0 = await Client.GetAsync("/api/NewsFeed");
            response0.StatusCode.Should().BeEquivalentTo(200);
            var newsItems = JsonConvert.DeserializeObject<IEnumerable<NewsFeedItem>>(response0.Content.ReadAsStringAsync().Result);
            newsItems.Count().Should().Be(4);

            var response1 = await Client.DeleteAsync("/api/NewsFeed/1");
            response1.StatusCode.Should().BeEquivalentTo(StatusCodes.Status204NoContent);

            var response2 = await Client.GetAsync("/api/NewsFeed/1");
            response2.StatusCode.Should().BeEquivalentTo(StatusCodes.Status404NotFound);

            var response3 = await Client.GetAsync("/api/NewsFeed");
            response3.StatusCode.Should().BeEquivalentTo(200);
            var newNewsItems = JsonConvert.DeserializeObject<IEnumerable<NewsFeedItem>>(response3.Content.ReadAsStringAsync().Result);
            newNewsItems.Count().Should().Be(3);
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
