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
            var createForm0 = GenerateCompanyCreateForm("Company Name 1");
            var response0 = await Client.PostAsync("/api/companies", new StringContent(JsonConvert.SerializeObject(createForm0), Encoding.UTF8, "application/json"));

            var createForm1 = GenerateCompanyCreateForm("Company Name 2");
            var response1 = await Client.PostAsync("/api/companies", new StringContent(JsonConvert.SerializeObject(createForm1), Encoding.UTF8, "application/json"));

            var createForm2 = GenerateCompanyCreateForm("Company Name 3");
            var response2 = await Client.PostAsync("/api/companies", new StringContent(JsonConvert.SerializeObject(createForm2), Encoding.UTF8, "application/json"));

            var createForm3 = GenerateCompanyCreateForm("Company Name 4");
            var response3 = await Client.PostAsync("/api/companies", new StringContent(JsonConvert.SerializeObject(createForm3), Encoding.UTF8, "application/json"));
        }

        public async Task SeedProduct(string productName, int companyId)
        {
            var productForm = new ProductForm
            {
                Name = productName,
                CompanyId = companyId
            };
            var response1 = await Client.PostAsync($"/api/companies/{companyId}/products",
                new StringContent(JsonConvert.SerializeObject(productForm), Encoding.UTF8, "application/json"));
        }

        private CompanyForm GenerateCompanyCreateForm(string companyName)
        {
            return new CompanyForm
            {
                Name = companyName,
            };
        }

        // TEST NAME - getAllEntriesById
        // TEST DESCRIPTION - It finds all companies in Database and product for the created company
        [Fact]
        public async Task Test1()
        {
            await SeedData();

            var response0 = await Client.GetAsync("/api/companies");
            response0.StatusCode.Should().BeEquivalentTo(StatusCodes.Status200OK);
            var companies = JsonConvert.DeserializeObject<IEnumerable<Company>>(response0.Content.ReadAsStringAsync().Result).ToList();
            companies.Count.Should().Be(4);

            var company = companies.FirstOrDefault(x => x.Name == "Company Name 1");
            company.Should().NotBeNull();

            await SeedProduct("test product 1", company.Id);
            await SeedProduct("test product 2", company.Id);
            var response1 = await Client.GetAsync($"/api/companies/{company.Id}/products");
            response1.StatusCode.Should().BeEquivalentTo(StatusCodes.Status200OK);
            var products = JsonConvert.DeserializeObject<IEnumerable<Product>>(response1.Content.ReadAsStringAsync().Result).ToList();
            products.Count.Should().Be(2);

        }

        // TEST NAME - getSingleEntryById
        // TEST DESCRIPTION - It finds single company by ID
        [Fact]
        public async Task Test2()
        {
            await SeedData();

            var response0 = await Client.GetAsync("/api/companies/1");
            response0.StatusCode.Should().BeEquivalentTo(200);

            var company = JsonConvert.DeserializeObject<Company>(response0.Content.ReadAsStringAsync().Result);
            company.Name.Should().Be("Company Name 1");

            var response1 = await Client.GetAsync("/api/companies/101");
            response1.StatusCode.Should().BeEquivalentTo(StatusCodes.Status404NotFound);

            await SeedProduct("test product", company.Id);
            var response2 = await Client.GetAsync($"/api/companies/21312/products/1");
            response2.StatusCode.Should().BeEquivalentTo(StatusCodes.Status404NotFound);

            await SeedProduct("test product", company.Id);
            var response3 = await Client.GetAsync($"/api/companies/{company.Id}/products/1");
            response3.StatusCode.Should().BeEquivalentTo(StatusCodes.Status200OK);
            var product = JsonConvert.DeserializeObject<Product>(response3.Content.ReadAsStringAsync().Result);
            product.Name.Should().Be("test product");
            product.CompanyId.Should().Be(company.Id);
        }

        // TEST NAME - getSingleEntryByFilter
        // TEST DESCRIPTION - It finds single product for company by ID
        [Fact]
        public async Task Test3()
        {
            await SeedData();

            var response1 = await Client.GetAsync("/api/companies");
            response1.StatusCode.Should().BeEquivalentTo(StatusCodes.Status200OK);
            var filteredCompanies = JsonConvert.DeserializeObject<IEnumerable<Company>>(response1.Content.ReadAsStringAsync().Result).ToArray();
            filteredCompanies.Length.Should().Be(4);

            await SeedProduct("test product 1", 1);
            await SeedProduct("test product 2", 1);
            var response2 = await Client.GetAsync($"/api/companies/2/products");
            response2.StatusCode.Should().BeEquivalentTo(StatusCodes.Status200OK);
            var products = JsonConvert.DeserializeObject<IEnumerable<Product>>(response2.Content.ReadAsStringAsync().Result).ToList();
            products.Count.Should().Be(0);
            
            var response3 = await Client.GetAsync($"/api/companies/1/products");
            response3.StatusCode.Should().BeEquivalentTo(StatusCodes.Status200OK);
            var products2 = JsonConvert.DeserializeObject<IEnumerable<Product>>(response3.Content.ReadAsStringAsync().Result).ToList();
            products2.Count.Should().Be(2);

            var response4 = await Client.GetAsync($"/api/companies/31232/products");
            response4.StatusCode.Should().BeEquivalentTo(StatusCodes.Status404NotFound);
        }

        // TEST NAME - deleteCompanyById
        // TEST DESCRIPTION - Check delete company web api end point
        [Fact]
        public async Task Test4()
        {
            await SeedData();

            var response0 = await Client.DeleteAsync("/api/companies/1");
            response0.StatusCode.Should().BeEquivalentTo(StatusCodes.Status204NoContent);

            var response1 = await Client.GetAsync("/api/companies/1");
            response1.StatusCode.Should().BeEquivalentTo(StatusCodes.Status404NotFound);
        }

        // TEST NAME - updateCompanyById
        // TEST DESCRIPTION - Check update company web api end point
        [Fact]
        public async Task Test5()
        {
            await SeedData();

            var updatedCompanyName = "Updated companyName";
            var newCompanyBody = "Updated company body";

            var updateForm = new CompanyForm()
            {
                Id = 1,
                Name = updatedCompanyName,
                Location = newCompanyBody
            };

            var response0 = await Client.PutAsync("/api/companies/1", new StringContent(JsonConvert.SerializeObject(updateForm), Encoding.UTF8, "application/json"));
            response0.StatusCode.Should().BeEquivalentTo(StatusCodes.Status204NoContent);

            var response1 = await Client.GetAsync("/api/companies/1");
            response1.StatusCode.Should().BeEquivalentTo(StatusCodes.Status200OK);

            var company = JsonConvert.DeserializeObject<Company>(response1.Content.ReadAsStringAsync().Result);
            response1.StatusCode.Should().BeEquivalentTo(StatusCodes.Status200OK);
            company.Name.Should().Be(updatedCompanyName);
            company.Location.Should().Be(newCompanyBody);
        }

        // TEST NAME - updateProductById
        // TEST DESCRIPTION - Check update product web api end point
        [Fact]
        public async Task Test6()
        {
            await SeedData();
            await SeedProduct("test product 1", 1);

            var updatedProductname = "Updated productname";

            var updateForm = new ProductForm
            {
                Id = 1,
                Name = updatedProductname,
            };

            var response0 = await Client.PutAsync("/api/companies/1/products", new StringContent(JsonConvert.SerializeObject(updateForm), Encoding.UTF8, "application/json"));
            response0.StatusCode.Should().BeEquivalentTo(StatusCodes.Status204NoContent);

            var response1 = await Client.GetAsync("/api/companies/1/products/1");
            response1.StatusCode.Should().BeEquivalentTo(StatusCodes.Status200OK);

            var product = JsonConvert.DeserializeObject<Product>(response1.Content.ReadAsStringAsync().Result);
            response1.StatusCode.Should().BeEquivalentTo(StatusCodes.Status200OK);
            product.Name.Should().Be(updatedProductname);
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
