using Newtonsoft.Json;

namespace TestProject.WebAPI.SeedData
{
    public class DocumentForm
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("body")]
        public string Body { get; set; }
    }
}
