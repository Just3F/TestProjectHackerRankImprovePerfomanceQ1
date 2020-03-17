using System;
using Newtonsoft.Json;

namespace TestProject.WebAPI.SeedData
{
    public class ProductForm
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("category")]
        public string Category { get; set; }

        [JsonProperty("companyId")]
        public int CompanyId { get; set; }
    }
}
