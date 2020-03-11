using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TestProject.WebAPI.Data;
using TestProject.WebAPI.Services;

namespace TestProject.WebAPI.Controllers
{
    [ApiController]
    [Route("/api/libraries/{libraryId}/[controller]")]
    public class BooksController : ControllerBase
    {
        private readonly IBooksService _booksService;
        private readonly ILibrariesService _librariesService;

        public BooksController(IBooksService booksService, ILibrariesService librariesService)
        {
            _booksService = booksService;
            _librariesService = librariesService;
        }

        [HttpGet("{bookId}")]
        public async Task<IActionResult> Get(int libraryId, int bookId)
        {
            var library = (await _librariesService.Get(new[] { libraryId })).FirstOrDefault();
            if (library == null)
                return NotFound("Library");

            var book = (await _booksService.Get(libraryId, new[] { bookId })).FirstOrDefault();
            if (book == null)
                return NotFound("Book");

            return Ok(book);
        }

        [HttpGet("")]
        public async Task<IActionResult> GetAll(int libraryId)
        {
            var library = (await _librariesService.Get(new[] { libraryId })).FirstOrDefault();
            if (library == null)
                return NotFound("Library");

            var newsItems = await _booksService.Get(libraryId, null);
            return Ok(newsItems);
        }

        [HttpPost]
        public async Task<IActionResult> Add(int libraryId, Book book)
        {
            var library = (await _librariesService.Get(new[] { libraryId })).FirstOrDefault();
            if (library == null)
                return NotFound("Library");

            await _booksService.Add(book);
            return Ok(book);
        }

        [HttpPut]
        public async Task<IActionResult> Update(int libraryId, Book book)
        {
            var library = (await _librariesService.Get(new[] { libraryId })).FirstOrDefault();
            if (library == null)
                return NotFound("Library");

            await _booksService.Update(book);
            return NoContent();
        }

        [HttpDelete("{bookId}")]
        public async Task<IActionResult> Delete(int libraryId, int bookId)
        {
            var book = (await _booksService.Get(libraryId, new[] { bookId })).FirstOrDefault();
            if (book == null)
                return NotFound();

            await _booksService.Delete(book);
            return NoContent();
        }
    }
}
