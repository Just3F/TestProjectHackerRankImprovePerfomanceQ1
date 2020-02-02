using System;
using Newtonsoft.Json;

namespace TestProject.WebAPI.SeedData
{
    public class CreateBookForm
    {
        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("authorName")]
        public string AuthorName { get; set; }

        [JsonProperty("body")]
        public string Body { get; set; }

        [JsonProperty("publishedDate")]
        public DateTime PublishedDate { get; set; }
    }
}
