using System;
using Newtonsoft.Json;

namespace TestProject.WebAPI.SeedData
{
    public class CreateBookForm
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("singer")]
        public SingerForm Singer { get; set; }

        [JsonProperty("releaseDate")]
        public DateTime ReleaseDate { get; set; }
    }

    public class SingerForm
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }
}
