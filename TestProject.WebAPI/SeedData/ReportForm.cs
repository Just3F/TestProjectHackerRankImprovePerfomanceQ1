using System;
using Newtonsoft.Json;

namespace TestProject.WebAPI.SeedData
{
    public class ReportForm
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("category")]
        public string Category { get; set; }

        [JsonProperty("documentId")]
        public int DocumentId { get; set; }
    }
}
