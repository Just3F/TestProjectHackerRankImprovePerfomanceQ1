using Newtonsoft.Json;

namespace TestProject.WebAPI.SeedData
{
    public class CompanyForm
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("location")]
        public string Location { get; set; }
    }
}
