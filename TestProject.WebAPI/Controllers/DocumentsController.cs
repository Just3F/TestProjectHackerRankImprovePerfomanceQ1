using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TestProject.WebAPI.Data;
using TestProject.WebAPI.Services;

namespace TestProject.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DocumentsController : ControllerBase
    {
        private readonly IDocumentsService _documentsService;

        public DocumentsController(IDocumentsService documentsService)
        {
            _documentsService = documentsService;
        }

        [HttpGet("{documentId}")]
        public async Task<IActionResult> Get(int documentId)
        {
            var user = (await _documentsService.Get(new[] { documentId })).FirstOrDefault();
            if (user == null)
                return NotFound();

            return Ok(user);
        }

        [HttpGet("")]
        public async Task<IActionResult> GetAll()
        {
            var newsItems = await _documentsService.Get(null);
            return Ok(newsItems);
        }

        [HttpPost]
        public async Task<IActionResult> Add(Document document)
        {
            await _documentsService.Add(document);
            return Ok(document);
        }

        [HttpPut("{documentId}")]
        public async Task<IActionResult> Update(int documentId, Document document)
        {
            await _documentsService.Update(document);
            return NoContent();
        }

        [HttpDelete("{documentId}")]
        public async Task<IActionResult> Delete(int documentId)
        {
            var user = (await _documentsService.Get(new[] { documentId })).FirstOrDefault();
            if (user == null)
                return NotFound();

            await _documentsService.Delete(user);
            return NoContent();
        }
    }
}
