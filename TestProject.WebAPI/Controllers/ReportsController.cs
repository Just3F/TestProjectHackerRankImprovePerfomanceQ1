using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TestProject.WebAPI.Data;
using TestProject.WebAPI.Services;

namespace TestProject.WebAPI.Controllers
{
    [ApiController]
    [Route("/api/documents/{documentId}/[controller]")]
    public class ReportsController : ControllerBase
    {
        private readonly IReportsService _reportsService;
        private readonly IDocumentsService _documentsService;

        public ReportsController(IReportsService reportsService, IDocumentsService documentsService)
        {
            _reportsService = reportsService;
            _documentsService = documentsService;
        }

        [HttpGet("{reportId}")]
        public async Task<IActionResult> Get(int documentId, int reportId)
        {
            var document = (await _documentsService.Get(new[] { documentId })).FirstOrDefault();
            if (document == null)
                return NotFound("Document");

            var report = (await _reportsService.Get(documentId, new[] { reportId })).FirstOrDefault();
            if (report == null)
                return NotFound("Report");

            return Ok(report);
        }

        [HttpGet("")]
        public async Task<IActionResult> GetAll(int documentId)
        {
            var document = (await _documentsService.Get(new[] { documentId })).FirstOrDefault();
            if (document == null)
                return NotFound("Document");

            var newsItems = await _reportsService.Get(documentId, null);
            return Ok(newsItems);
        }

        [HttpPost]
        public async Task<IActionResult> Add(int documentId, Report report)
        {
            var document = (await _documentsService.Get(new[] { documentId })).FirstOrDefault();
            if (document == null)
                return NotFound("Document");

            await _reportsService.Add(report);
            return Ok(report);
        }

        [HttpPut]
        public async Task<IActionResult> Update(int documentId, Report report)
        {
            var document = (await _documentsService.Get(new[] { documentId })).FirstOrDefault();
            if (document == null)
                return NotFound("Document");

            await _reportsService.Update(report);
            return NoContent();
        }

        [HttpDelete("{reportId}")]
        public async Task<IActionResult> Delete(int documentId, int reportId)
        {
            var report = (await _reportsService.Get(documentId, new[] { reportId })).FirstOrDefault();
            if (report == null)
                return NotFound();

            await _reportsService.Delete(report);
            return NoContent();
        }
    }
}
