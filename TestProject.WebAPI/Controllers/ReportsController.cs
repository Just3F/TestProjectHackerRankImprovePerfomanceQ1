using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TestProject.WebAPI.Data;
using TestProject.WebAPI.Services;

namespace TestProject.WebAPI.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class ReportsController : ControllerBase
    {
        private readonly IReportsService _reportsService;

        public ReportsController(IReportsService reportsService)
        {
            _reportsService = reportsService;
        }

        [HttpGet("{reportId}")]
        public async Task<IActionResult> Get(int reportId)
        {
            var report = (await _reportsService.Get(new[] { reportId })).FirstOrDefault();
            if (report == null)
                return NotFound("Report");

            return Ok(report);
        }

        [HttpGet("")]
        public async Task<IActionResult> GetAll()
        {
            var newsItems = await _reportsService.Get(null);
            return Ok(newsItems);
        }

        [HttpPost]
        public async Task<IActionResult> Add(Report report)
        {
            await _reportsService.Add(report);
            return Ok(report);
        }

        [HttpPut]
        public async Task<IActionResult> Update(Report report)
        {
            await _reportsService.Update(report);
            return NoContent();
        }

        [HttpDelete("{reportId}")]
        public async Task<IActionResult> Delete(int reportId)
        {
            var report = (await _reportsService.Get(new[] { reportId })).FirstOrDefault();
            if (report == null)
                return NotFound();

            await _reportsService.Delete(report);
            return NoContent();
        }
    }
}
