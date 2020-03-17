using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TestProject.WebAPI.Data;
using TestProject.WebAPI.Services;

namespace TestProject.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CompaniesController : ControllerBase
    {
        private readonly ICompaniesService _companiesService;

        public CompaniesController(ICompaniesService companiesService)
        {
            _companiesService = companiesService;
        }

        [HttpGet("{companyId}")]
        public async Task<IActionResult> Get(int companyId)
        {
            var user = (await _companiesService.Get(new[] { companyId })).FirstOrDefault();
            if (user == null)
                return NotFound();

            return Ok(user);
        }

        [HttpGet("")]
        public async Task<IActionResult> GetAll()
        {
            var newsItems = await _companiesService.Get(null);
            return Ok(newsItems);
        }

        [HttpPost]
        public async Task<IActionResult> Add(Company company)
        {
            await _companiesService.Add(company);
            return Ok(company);
        }

        [HttpPut("{companyId}")]
        public async Task<IActionResult> Update(int companyId, Company company)
        {
            await _companiesService.Update(company);
            return NoContent();
        }

        [HttpDelete("{companyId}")]
        public async Task<IActionResult> Delete(int companyId)
        {
            var user = (await _companiesService.Get(new[] { companyId })).FirstOrDefault();
            if (user == null)
                return NotFound();

            await _companiesService.Delete(user);
            return NoContent();
        }
    }
}
