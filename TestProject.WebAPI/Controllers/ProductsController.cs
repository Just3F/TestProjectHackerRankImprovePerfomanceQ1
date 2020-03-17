using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TestProject.WebAPI.Data;
using TestProject.WebAPI.Services;

namespace TestProject.WebAPI.Controllers
{
    [ApiController]
    [Route("/api/companies/{companyId}/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductsService _productsService;
        private readonly ICompaniesService _companiesService;

        public ProductsController(IProductsService productsService, ICompaniesService companiesService)
        {
            _productsService = productsService;
            _companiesService = companiesService;
        }

        [HttpGet("{productId}")]
        public async Task<IActionResult> Get(int companyId, int productId)
        {
            var company = (await _companiesService.Get(new[] { companyId })).FirstOrDefault();
            if (company == null)
                return NotFound("Company");

            var product = (await _productsService.Get(companyId, new[] { productId })).FirstOrDefault();
            if (product == null)
                return NotFound("Product");

            return Ok(product);
        }

        [HttpGet("")]
        public async Task<IActionResult> GetAll(int companyId)
        {
            var company = (await _companiesService.Get(new[] { companyId })).FirstOrDefault();
            if (company == null)
                return NotFound("Company");

            var newsItems = await _productsService.Get(companyId, null);
            return Ok(newsItems);
        }

        [HttpPost]
        public async Task<IActionResult> Add(int companyId, Product product)
        {
            var company = (await _companiesService.Get(new[] { companyId })).FirstOrDefault();
            if (company == null)
                return NotFound("Company");

            await _productsService.Add(product);
            return Ok(product);
        }

        [HttpPut]
        public async Task<IActionResult> Update(int companyId, Product product)
        {
            var company = (await _companiesService.Get(new[] { companyId })).FirstOrDefault();
            if (company == null)
                return NotFound("Company");

            await _productsService.Update(product);
            return NoContent();
        }

        [HttpDelete("{productId}")]
        public async Task<IActionResult> Delete(int companyId, int productId)
        {
            var product = (await _productsService.Get(companyId, new[] { productId })).FirstOrDefault();
            if (product == null)
                return NotFound();

            await _productsService.Delete(product);
            return NoContent();
        }
    }
}
