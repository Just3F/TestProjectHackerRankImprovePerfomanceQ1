using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using TestProject.WebAPI.Data;
using TestProject.WebAPI.Services;

namespace TestProject.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NewsFeedController : ControllerBase
    {
        private readonly INewsFeedService _newsFeedService;
        private readonly IMemoryCache _memoryCache;

        public NewsFeedController(INewsFeedService newsFeedService, IMemoryCache memoryCache)
        {
            _newsFeedService = newsFeedService;
            _memoryCache = memoryCache;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var user = (await _newsFeedService.Get(new[] { id }, null)).FirstOrDefault();
            if (user == null)
                return NotFound();

            return Ok(user);
        }

        [HttpGet("")]
        public async Task<IActionResult> GetAll([FromQuery]Filters filters)
        {
            if (_memoryCache.TryGetValue<IEnumerable<NewsFeedItem>>("newsItems", out var newsItems))
            {
                return Ok(newsItems);
            }

            newsItems = await _newsFeedService.Get(null, filters);
            _memoryCache.Set("newsItems", newsItems);

            return Ok(newsItems);
        }

        [HttpPost]
        public async Task<IActionResult> Add(NewsFeedItem newsFeedItem)
        {
            await _newsFeedService.Add(newsFeedItem);
            _memoryCache.Remove("newsItems");
            return Ok(newsFeedItem);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, NewsFeedItem newsFeedItem)
        {
            if (id != newsFeedItem.Id)
            {
                return BadRequest();
            }
            _memoryCache.Remove("newsItems");

            await _newsFeedService.Update(newsFeedItem);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var user = (await _newsFeedService.Get(new[] { id }, null)).FirstOrDefault();
            if (user == null)
                return NotFound();

            _memoryCache.Remove("newsItems");

            await _newsFeedService.Delete(user);
            return NoContent();
        }
    }
}
