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
            var createForm0 = GenerateDocumentCreateForm("Document Name 1");
            var response0 = await Client.PostAsync("/api/documents", new StringContent(JsonConvert.SerializeObject(createForm0), Encoding.UTF8, "application/json"));

            var createForm1 = GenerateDocumentCreateForm("Document Name 2");
            var response1 = await Client.PostAsync("/api/documents", new StringContent(JsonConvert.SerializeObject(createForm1), Encoding.UTF8, "application/json"));

            var createForm2 = GenerateDocumentCreateForm("Document Name 3");
            var response2 = await Client.PostAsync("/api/documents", new StringContent(JsonConvert.SerializeObject(createForm2), Encoding.UTF8, "application/json"));

            var createForm3 = GenerateDocumentCreateForm("Document Name 4");
            var response3 = await Client.PostAsync("/api/documents", new StringContent(JsonConvert.SerializeObject(createForm3), Encoding.UTF8, "application/json"));
        }

        public async Task SeedReport(string reportName, int projectId)
        {
            var reportForm = new ReportForm
            {
                Name = reportName,
                DocumentId = projectId
            };
            var response1 = await Client.PostAsync($"/api/documents/{projectId}/reports",
                new StringContent(JsonConvert.SerializeObject(reportForm), Encoding.UTF8, "application/json"));
        }

        private DocumentForm GenerateDocumentCreateForm(string projectName)
        {
            return new DocumentForm
            {
                Name = projectName,
            };
        }

        // TEST NAME - getAllEntriesById
        // TEST DESCRIPTION - It finds all documents in Database and report for the created document
        [Fact]
        public async Task Test1()
        {
            await SeedData();

            var response0 = await Client.GetAsync("/api/documents");
            response0.StatusCode.Should().BeEquivalentTo(StatusCodes.Status200OK);
            var documents = JsonConvert.DeserializeObject<IEnumerable<Document>>(response0.Content.ReadAsStringAsync().Result).ToList();
            documents.Count.Should().Be(4);

            var project = documents.FirstOrDefault(x => x.Name == "Document Name 1");
            project.Should().NotBeNull();

            await SeedReport("test report 1", project.Id);
            await SeedReport("test report 2", project.Id);
            var response1 = await Client.GetAsync($"/api/documents/{project.Id}/reports");
            response1.StatusCode.Should().BeEquivalentTo(StatusCodes.Status200OK);
            var reports = JsonConvert.DeserializeObject<IEnumerable<Report>>(response1.Content.ReadAsStringAsync().Result).ToList();
            reports.Count.Should().Be(2);

        }

        // TEST NAME - getSingleEntryById
        // TEST DESCRIPTION - It finds single document by ID
        [Fact]
        public async Task Test2()
        {
            await SeedData();

            var response0 = await Client.GetAsync("/api/documents/1");
            response0.StatusCode.Should().BeEquivalentTo(200);

            var project = JsonConvert.DeserializeObject<Document>(response0.Content.ReadAsStringAsync().Result);
            project.Name.Should().Be("Document Name 1");

            var response1 = await Client.GetAsync("/api/documents/101");
            response1.StatusCode.Should().BeEquivalentTo(StatusCodes.Status404NotFound);

            await SeedReport("test report", project.Id);
            var response2 = await Client.GetAsync($"/api/documents/21312/reports/1");
            response2.StatusCode.Should().BeEquivalentTo(StatusCodes.Status404NotFound);

            await SeedReport("test report", project.Id);
            var response3 = await Client.GetAsync($"/api/documents/{project.Id}/reports/1");
            response3.StatusCode.Should().BeEquivalentTo(StatusCodes.Status200OK);
            var report = JsonConvert.DeserializeObject<Report>(response3.Content.ReadAsStringAsync().Result);
            report.Name.Should().Be("test report");
            report.DocumentId.Should().Be(project.Id);
        }

        // TEST NAME - getSingleEntryByFilter
        // TEST DESCRIPTION - It finds single report for document by ID
        [Fact]
        public async Task Test3()
        {
            await SeedData();

            var response1 = await Client.GetAsync("/api/documents");
            response1.StatusCode.Should().BeEquivalentTo(StatusCodes.Status200OK);
            var filteredDocuments = JsonConvert.DeserializeObject<IEnumerable<Document>>(response1.Content.ReadAsStringAsync().Result).ToArray();
            filteredDocuments.Length.Should().Be(4);

            await SeedReport("test report 1", 1);
            await SeedReport("test report 2", 1);
            var response2 = await Client.GetAsync($"/api/documents/2/reports");
            response2.StatusCode.Should().BeEquivalentTo(StatusCodes.Status200OK);
            var reports = JsonConvert.DeserializeObject<IEnumerable<Report>>(response2.Content.ReadAsStringAsync().Result).ToList();
            reports.Count.Should().Be(0);
            
            var response3 = await Client.GetAsync($"/api/documents/1/reports");
            response3.StatusCode.Should().BeEquivalentTo(StatusCodes.Status200OK);
            var reports2 = JsonConvert.DeserializeObject<IEnumerable<Report>>(response3.Content.ReadAsStringAsync().Result).ToList();
            reports2.Count.Should().Be(2);

            var response4 = await Client.GetAsync($"/api/documents/31232/reports");
            response4.StatusCode.Should().BeEquivalentTo(StatusCodes.Status404NotFound);
        }

        // TEST NAME - deleteDocumentById
        // TEST DESCRIPTION - Check delete document web api end point
        [Fact]
        public async Task Test4()
        {
            await SeedData();

            var response0 = await Client.DeleteAsync("/api/documents/1");
            response0.StatusCode.Should().BeEquivalentTo(StatusCodes.Status204NoContent);

            var response1 = await Client.GetAsync("/api/documents/1");
            response1.StatusCode.Should().BeEquivalentTo(StatusCodes.Status404NotFound);
        }

        // TEST NAME - updateDocumentById
        // TEST DESCRIPTION - Check update document web api end point
        [Fact]
        public async Task Test5()
        {
            await SeedData();

            var updatedDocumentName = "Updated projectName";
            var newDocumentBody = "Updated document body";

            var updateForm = new DocumentForm()
            {
                Id = 1,
                Name = updatedDocumentName,
                Body = newDocumentBody
            };

            var response0 = await Client.PutAsync("/api/documents/1", new StringContent(JsonConvert.SerializeObject(updateForm), Encoding.UTF8, "application/json"));
            response0.StatusCode.Should().BeEquivalentTo(StatusCodes.Status204NoContent);

            var response1 = await Client.GetAsync("/api/documents/1");
            response1.StatusCode.Should().BeEquivalentTo(StatusCodes.Status200OK);

            var project = JsonConvert.DeserializeObject<Document>(response1.Content.ReadAsStringAsync().Result);
            response1.StatusCode.Should().BeEquivalentTo(StatusCodes.Status200OK);
            project.Name.Should().Be(updatedDocumentName);
            project.Body.Should().Be(newDocumentBody);
        }

        // TEST NAME - updateReportById
        // TEST DESCRIPTION - Check update report web api end point
        [Fact]
        public async Task Test6()
        {
            await SeedData();
            await SeedReport("test report 1", 1);

            var updatedReportname = "Updated reportname";

            var updateForm = new ReportForm
            {
                Id = 1,
                Name = updatedReportname,
            };

            var response0 = await Client.PutAsync("/api/documents/1/reports", new StringContent(JsonConvert.SerializeObject(updateForm), Encoding.UTF8, "application/json"));
            response0.StatusCode.Should().BeEquivalentTo(StatusCodes.Status204NoContent);

            var response1 = await Client.GetAsync("/api/documents/1/reports/1");
            response1.StatusCode.Should().BeEquivalentTo(StatusCodes.Status200OK);

            var report = JsonConvert.DeserializeObject<Report>(response1.Content.ReadAsStringAsync().Result);
            response1.StatusCode.Should().BeEquivalentTo(StatusCodes.Status200OK);
            report.Name.Should().Be(updatedReportname);
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
