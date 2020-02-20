using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TestProject.WebAPI.Data;
using TestProject.WebAPI.Services;

namespace TestProject.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TicketsController : ControllerBase
    {
        private readonly ITicketsService _ticketsService;

        public TicketsController(ITicketsService ticketsService)
        {
            _ticketsService = ticketsService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var user = (await _ticketsService.Get(new[] { id }, null)).FirstOrDefault();
            if (user == null)
                return NotFound();

            return Ok(user);
        }

        [HttpGet("")]
        public async Task<IActionResult> GetAll([FromQuery]Filters filters)
        {
            var newsItems = await _ticketsService.Get(null, filters);

            return Ok(newsItems);
        }

        [HttpPost]
        public async Task<IActionResult> Add(Ticket ticket)
        {
            await _ticketsService.Add(ticket);
            return Ok(ticket);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Ticket ticket)
        {
            if (id != ticket.Id)
            {
                return BadRequest();
            }

            await _ticketsService.Update(ticket);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var user = (await _ticketsService.Get(new[] { id }, null)).FirstOrDefault();
            if (user == null)
                return NotFound();

            await _ticketsService.Delete(user);
            return NoContent();
        }
    }
}
