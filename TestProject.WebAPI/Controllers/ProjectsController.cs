using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TestProject.WebAPI.Data;
using TestProject.WebAPI.Services;

namespace TestProject.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProjectsController : ControllerBase
    {
        private readonly IProjectsService _projectsService;

        public ProjectsController(IProjectsService projectsService)
        {
            _projectsService = projectsService;
        }

        [HttpGet("{projectId}")]
        public async Task<IActionResult> Get(int projectId)
        {
            var user = (await _projectsService.Get(new[] { projectId })).FirstOrDefault();
            if (user == null)
                return NotFound();

            return Ok(user);
        }

        [HttpGet("")]
        public async Task<IActionResult> GetAll()
        {
            var newsItems = await _projectsService.Get(null);
            return Ok(newsItems);
        }

        [HttpPost]
        public async Task<IActionResult> Add(Project project)
        {
            await _projectsService.Add(project);
            return Ok(project);
        }

        [HttpPut("{projectId}")]
        public async Task<IActionResult> Update(int projectId, Project project)
        {
            await _projectsService.Update(project);
            return NoContent();
        }

        [HttpDelete("{projectId}")]
        public async Task<IActionResult> Delete(int projectId)
        {
            var user = (await _projectsService.Get(new[] { projectId })).FirstOrDefault();
            if (user == null)
                return NotFound();

            await _projectsService.Delete(user);
            return NoContent();
        }
    }
}
