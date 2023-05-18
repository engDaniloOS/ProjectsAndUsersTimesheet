using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TimeSheet.Domain.Dtos;
using TimeSheet.Domain.UseCases;

namespace TimeSheet.Connectors.Controllers
{
    [Route("api/v1/projects")]
    [ApiController]
    [Authorize]
    public class ProjectsController : ControllerBase
    {
        private readonly IProjectService _service;

        public ProjectsController(IProjectService service) => _service = service;

        [HttpGet]
        public async Task<IActionResult> GetProjects()
        {
            var projectsDto = await _service.GetProjects();

            if (!string.IsNullOrWhiteSpace(projectsDto.Error))
                return BadRequest(new ErrorDto { Message = projectsDto.Error });

            if (projectsDto.Projects.Count == 0)
                return NotFound();

            return Ok(projectsDto.Projects);
        }

        [HttpGet]
        [Route("{project_id}")]
        public async Task<IActionResult> GetProjectById([FromRoute(Name = "project_id")] int id)
        {
            ProjectWithUserOutDto project = await _service.GetProjectById(id);

            if (!string.IsNullOrWhiteSpace(project.Error))
                return BadRequest(new ErrorDto { Message = project.Error });

            if (project.Id == 0)
                return NotFound();

            return Ok(project);
        }

        [HttpPost]
        public async Task<IActionResult> SaveNewProject([FromBody] ProjectDto projectDto)
        {
            var project = await _service.SaveNewProject(projectDto);

            if (!string.IsNullOrWhiteSpace(project.Error))
                return BadRequest(new ErrorDto { Message = project.Error });

            return Ok(project);
        }

        [HttpPut]
        [Route("{project_id}")]
        public async Task<IActionResult> EditProject([FromRoute(Name = "project_id")] int projectId, [FromBody] ProjectDto projectDto)
        {
            var project = await _service.EditProject(projectId, projectDto);

            if (!string.IsNullOrWhiteSpace(project.Error))
                return BadRequest(new ErrorDto { Message = project.Error });

            if (project.Id == 0)
                return NotFound();

            return Ok(project);
        }
    }
}
