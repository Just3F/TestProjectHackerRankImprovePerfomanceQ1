using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TestProject.WebAPI.Data;
using TestProject.WebAPI.Services;

namespace TestProject.WebAPI.Controllers
{
    [ApiController]
    [Route("/api/projects/{projectId}/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUsersService _usersService;
        private readonly IProjectsService _projectsService;

        public UsersController(IUsersService usersService, IProjectsService projectsService)
        {
            _usersService = usersService;
            _projectsService = projectsService;
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> Get(int projectId, int userId)
        {
            var project = (await _projectsService.Get(new[] { projectId })).FirstOrDefault();
            if (project == null)
                return NotFound("Project");

            var user = (await _usersService.Get(projectId, new[] { userId })).FirstOrDefault();
            if (user == null)
                return NotFound("User");

            return Ok(user);
        }

        [HttpGet("")]
        public async Task<IActionResult> GetAll(int projectId)
        {
            var newsItems = await _usersService.Get(projectId, null);
            return Ok(newsItems);
        }

        [HttpPost]
        public async Task<IActionResult> Add(int projectId, User user)
        {
            var project = (await _projectsService.Get(new[] { projectId })).FirstOrDefault();
            if (project == null)
                return NotFound("Project");

            await _usersService.Add(user);
            return Ok(user);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int projectId, int id, User user)
        {
            var project = (await _projectsService.Get(new[] { projectId })).FirstOrDefault();
            if (project == null)
                return NotFound("Project");

            await _usersService.Update(user);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int projectId, int id)
        {
            var user = (await _usersService.Get(projectId, new[] { id })).FirstOrDefault();
            if (user == null)
                return NotFound();

            await _usersService.Delete(user);
            return NoContent();
        }
    }
}
