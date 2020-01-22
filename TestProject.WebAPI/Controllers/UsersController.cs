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
    public class UsersController : ControllerBase
    {
        private readonly IUsersService _usersService;
        private readonly IMemoryCache _memoryCache;

        public UsersController(IUsersService usersService, IMemoryCache memoryCache)
        {
            _usersService = usersService;
            _memoryCache = memoryCache;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var user = (await _usersService.Get(new[] { id }, null)).FirstOrDefault();
            if (user == null)
                return NotFound();

            return Ok(user);
        }

        [HttpGet("")]
        public async Task<IActionResult> GetAll([FromQuery]Filters filters)
        {
            if (_memoryCache.TryGetValue<IEnumerable<User>>("users", out var users))
            {
                return Ok(users);
            }

            users = await _usersService.Get(null, filters);
            _memoryCache.Set("users", users);

            return Ok(users);
        }

        [HttpPost]
        public async Task<IActionResult> Add(User user)
        {
            await _usersService.Add(user);
            _memoryCache.Remove("users");
            return Ok(user);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, User user)
        {
            if (id != user.Id)
            {
                return BadRequest();
            }
            _memoryCache.Remove("users");

            await _usersService.Update(user);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var user = (await _usersService.Get(new[] { id }, null)).FirstOrDefault();
            if (user == null)
                return NotFound();

            _memoryCache.Remove("users");

            await _usersService.Delete(user);
            return NoContent();
        }
    }
}
