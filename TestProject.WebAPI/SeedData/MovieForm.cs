using System;
using Newtonsoft.Json;

namespace TestProject.WebAPI.SeedData
{
    public class MovieForm
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("category")]
        public string Category { get; set; }

        [JsonProperty("releaseDate")]
        public DateTime ReleaseDate { get; set; }

    }
}
