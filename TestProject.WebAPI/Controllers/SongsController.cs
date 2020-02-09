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
    public class SongsController : ControllerBase
    {
        private readonly IBooksService _booksService;
        private readonly IMemoryCache _memoryCache;

        public SongsController(IBooksService booksService, IMemoryCache memoryCache)
        {
            _booksService = booksService;
            _memoryCache = memoryCache;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var user = (await _booksService.Get(new[] { id }, null)).FirstOrDefault();
            if (user == null)
                return NotFound();

            return Ok(user);
        }

        [HttpGet("")]
        public async Task<IActionResult> GetAll([FromQuery]Filters filters)
        {
            if (_memoryCache.TryGetValue<IEnumerable<Song>>("newsItems", out var newsItems))
            {
                return Ok(newsItems);
            }

            newsItems = await _booksService.Get(null, filters);
            _memoryCache.Set("newsItems", newsItems);

            return Ok(newsItems);
        }

        [HttpPost]
        public async Task<IActionResult> Add(Song song)
        {
            await _booksService.Add(song);
            _memoryCache.Remove("newsItems");
            return Ok(song);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Song song)
        {
            if (id != song.Id)
            {
                return BadRequest();
            }
            _memoryCache.Remove("newsItems");

            await _booksService.Update(song);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var user = (await _booksService.Get(new[] { id }, null)).FirstOrDefault();
            if (user == null)
                return NotFound();

            _memoryCache.Remove("newsItems");

            await _booksService.Delete(user);
            return NoContent();
        }
    }
}
