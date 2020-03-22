using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TestProject.WebAPI.Data;
using TestProject.WebAPI.Services;

namespace TestProject.WebAPI.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class MoviesController : ControllerBase
    {
        private readonly IMoviesService _moviesService;

        public MoviesController(IMoviesService moviesService)
        {
            _moviesService = moviesService;
        }

        [HttpGet("{movieId}")]
        public async Task<IActionResult> Get(int movieId)
        {
            var movie = (await _moviesService.Get(new[] { movieId })).FirstOrDefault();
            if (movie == null)
                return NotFound("Movie");

            return Ok(movie);
        }

        [HttpGet("")]
        public async Task<IActionResult> GetAll()
        {
            var newsItems = await _moviesService.Get(null);
            return Ok(newsItems);
        }

        [HttpPost]
        public async Task<IActionResult> Add(Movie movie)
        {
            await _moviesService.Add(movie);
            return Ok(movie);
        }

        [HttpPut]
        public async Task<IActionResult> Update(Movie movie)
        {
            await _moviesService.Update(movie);
            return NoContent();
        }

        [HttpDelete("{movieId}")]
        public async Task<IActionResult> Delete(int movieId)
        {
            var movie = (await _moviesService.Get(new[] { movieId })).FirstOrDefault();
            if (movie == null)
                return NotFound();

            await _moviesService.Delete(movie);
            return NoContent();
        }
    }
}
