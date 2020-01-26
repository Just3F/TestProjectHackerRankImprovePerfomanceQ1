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
    public class CarsController : ControllerBase
    {
        private readonly ICarsService _carsService;
        private readonly IMemoryCache _memoryCache;

        public CarsController(ICarsService carsService, IMemoryCache memoryCache)
        {
            _carsService = carsService;
            _memoryCache = memoryCache;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var user = (await _carsService.Get(new[] { id }, null)).FirstOrDefault();
            if (user == null)
                return NotFound();

            return Ok(user);
        }

        [HttpGet("")]
        public async Task<IActionResult> GetAll([FromQuery]Filters filters)
        {
            if (_memoryCache.TryGetValue<IEnumerable<Car>>("cars", out var cars))
            {
                return Ok(cars);
            }

            cars = await _carsService.Get(null, filters);
            _memoryCache.Set("cars", cars);

            return Ok(cars);
        }

        [HttpPost]
        public async Task<IActionResult> Add(Car car)
        {
            await _carsService.Add(car);
            _memoryCache.Remove("cars");
            return Ok(car);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Car car)
        {
            if (id != car.Id)
            {
                return BadRequest();
            }
            _memoryCache.Remove("cars");

            await _carsService.Update(car);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var user = (await _carsService.Get(new[] { id }, null)).FirstOrDefault();
            if (user == null)
                return NotFound();

            _memoryCache.Remove("cars");

            await _carsService.Delete(user);
            return NoContent();
        }
    }
}
