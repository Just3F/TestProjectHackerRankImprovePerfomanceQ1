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
            var createForm0 = GenerateReportCreateForm("Report Name 1");
            var response0 = await Client.PostAsync("/api/reports", new StringContent(JsonConvert.SerializeObject(createForm0), Encoding.UTF8, "application/json"));

            var createForm1 = GenerateReportCreateForm("Report Name 2");
            var response1 = await Client.PostAsync("/api/reports", new StringContent(JsonConvert.SerializeObject(createForm1), Encoding.UTF8, "application/json"));

            var createForm2 = GenerateReportCreateForm("Report Name 3");
            var response2 = await Client.PostAsync("/api/reports", new StringContent(JsonConvert.SerializeObject(createForm2), Encoding.UTF8, "application/json"));

            var createForm3 = GenerateReportCreateForm("Report Name 4");
            var response3 = await Client.PostAsync("/api/reports", new StringContent(JsonConvert.SerializeObject(createForm3), Encoding.UTF8, "application/json"));
        }

        private ReportForm GenerateReportCreateForm(string reportName)
        {
            return new ReportForm
            {
                Name = reportName,
            };
        }

        // TEST NAME - getAllEntriesById
        // TEST DESCRIPTION - It finds all documents in Database and report for the created document
        [Fact]
        public async Task Test1()
        {
            await SeedData();

            var response0 = await Client.GetAsync("/api/reports");
            response0.StatusCode.Should().BeEquivalentTo(StatusCodes.Status200OK);
            var documents = JsonConvert.DeserializeObject<IEnumerable<Report>>(response0.Content.ReadAsStringAsync().Result).ToList();
            documents.Count.Should().Be(4);
        }

        // TEST NAME - getSingleEntryById
        // TEST DESCRIPTION - It finds single document by ID
        [Fact]
        public async Task Test2()
        {
            await SeedData();

            var response0 = await Client.GetAsync("/api/reports/1");
            response0.StatusCode.Should().BeEquivalentTo(200);

            var project = JsonConvert.DeserializeObject<Report>(response0.Content.ReadAsStringAsync().Result);
            project.Name.Should().Be("Report Name 1");

            var response1 = await Client.GetAsync("/api/reports/101");
            response1.StatusCode.Should().BeEquivalentTo(StatusCodes.Status404NotFound);
        }


        // TEST NAME - getSingleEntryById
        // TEST DESCRIPTION - It finds single document by ID
        [Fact]
        public async Task Test3()
        {
            var createForm3 = GenerateReportCreateForm("Report Name 4");
            var response3 = await Client.PostAsync("/api/reports", new StringContent(JsonConvert.SerializeObject(createForm3), Encoding.UTF8, "application/json"));

            var response0 = await Client.GetAsync("/api/reports/1");
            response0.StatusCode.Should().BeEquivalentTo(200);

            var project = JsonConvert.DeserializeObject<Report>(response0.Content.ReadAsStringAsync().Result);
            project.Name.Should().Be("Report Name 4");
        }

        // TEST NAME - deleteDocumentById
        // TEST DESCRIPTION - Check delete document web api end point
        [Fact]
        public async Task Test4()
        {
            await SeedData();

            var response0 = await Client.DeleteAsync("/api/reports/1");
            response0.StatusCode.Should().BeEquivalentTo(StatusCodes.Status204NoContent);

            var response1 = await Client.GetAsync("/api/reports/1");
            response1.StatusCode.Should().BeEquivalentTo(StatusCodes.Status404NotFound);
        }

        // TEST NAME - updateDocumentById
        // TEST DESCRIPTION - Check update document web api end point
        [Fact]
        public async Task Test5()
        {
            await SeedData();

            var updatedReportName = "Updated reportName";

            var updateForm = new ReportForm()
            {
                Id = 1,
                Name = updatedReportName,
            };

            var response0 = await Client.PutAsync("/api/reports", new StringContent(JsonConvert.SerializeObject(updateForm), Encoding.UTF8, "application/json"));
            response0.StatusCode.Should().BeEquivalentTo(StatusCodes.Status204NoContent);

            var response1 = await Client.GetAsync("/api/reports/1");
            response1.StatusCode.Should().BeEquivalentTo(StatusCodes.Status200OK);

            var project = JsonConvert.DeserializeObject<Report>(response1.Content.ReadAsStringAsync().Result);
            response1.StatusCode.Should().BeEquivalentTo(StatusCodes.Status200OK);
            project.Name.Should().Be(updatedReportName);
        }

        // TEST NAME - checkTranslations
        // TEST DESCRIPTION - Check update document web api end point
        [Fact]
        public async Task Test6()
        {
            await SeedData();

            Client.DefaultRequestHeaders.Clear();
            var response = await Client.GetAsync("/api/reports/1");
            response.StatusCode.Should().BeEquivalentTo(200);

            var reportDefault = JsonConvert.DeserializeObject<Report>(response.Content.ReadAsStringAsync().Result);
            reportDefault.Name.Should().Be("Report Name 1");

            reportDefault.Rows.Should().HaveCount(3);
            reportDefault.Rows[0].Should().Be("Header 1");

            Client.DefaultRequestHeaders.Clear();
            Client.DefaultRequestHeaders.AcceptLanguage.Add(new StringWithQualityHeaderValue("ru"));
            var response0 = await Client.GetAsync("/api/reports/1");
            response0.StatusCode.Should().BeEquivalentTo(200);

            var report = JsonConvert.DeserializeObject<Report>(response0.Content.ReadAsStringAsync().Result);
            report.Name.Should().Be("Report Name 1");

            report.Rows.Should().HaveCount(3);
            report.Rows[0].Should().Be("Заголовок 1");

            Client.DefaultRequestHeaders.Clear();
            Client.DefaultRequestHeaders.AcceptLanguage.Add(new StringWithQualityHeaderValue("it"));
            var response1 = await Client.GetAsync("/api/reports/1");
            response1.StatusCode.Should().BeEquivalentTo(200);

            var reportIt = JsonConvert.DeserializeObject<Report>(response1.Content.ReadAsStringAsync().Result);
            reportIt.Name.Should().Be("Report Name 1");

            reportIt.Rows.Should().HaveCount(3);
            reportIt.Rows[0].Should().Be("Intestazione 1");
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
