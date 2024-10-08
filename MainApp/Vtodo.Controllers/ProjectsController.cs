using MediatR;
using Microsoft.AspNetCore.Mvc;
using Vtodo.UseCases.Handlers.Projects.Commands.CreateProject;
using Vtodo.UseCases.Handlers.Projects.Commands.DeleteProject;
using Vtodo.UseCases.Handlers.Projects.Commands.UpdateProject;
using Vtodo.UseCases.Handlers.Projects.Dto;
using Vtodo.UseCases.Handlers.Projects.Queries.GetProject;
using Vtodo.UseCases.Handlers.Projects.Queries.GetUserProjectList;

namespace Vtodo.Controllers
{
    [Route("api/v1/projects")]
    [ApiController]
    public class ProjectsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ProjectsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get project by id
        /// </summary>
        /// <param name="projectId">Project id</param>
        /// <response code="200">Returns project dto</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="403">Access denied</response>
        /// <response code="404">Project not found</response>
        [HttpGet("{projectId:long}")]
        public async Task<ActionResult<ProjectDto>> Get(long projectId)
        {
           return await _mediator.Send(new GetProjectRequest() { Id = projectId});
        }
        
        /// <summary>
        /// Get projects list by account
        /// </summary>
        /// <response code="200">Returns list of project dto</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="403">Access denied</response>
        /// <response code="404">Account not found</response>
        /// <response code="404">Project not found</response>
        [HttpGet("by_account/")]
        public async Task<ActionResult<List<ProjectDto>>> GetByAccount()
        {
            return await _mediator.Send(new GetAccountProjectsListRequest() {});
        }
        
        /// <summary>
        /// Create a project
        /// </summary>
        /// <param name="createProjectDto">Create project dto</param>
        /// <response code="200"></response>
        /// <response code="401">Unauthorized</response>
        /// <response code="403">Access denied</response>
        [HttpPost()]
        public async Task Create([FromBody] CreateProjectDto createProjectDto)
        {
            await _mediator.Send(new CreateProjectRequest() { CreateProjectDto = createProjectDto});
        }
        
        /// <summary>
        /// Update project
        /// </summary>
        /// <param name="projectId">Project id</param>
        /// <param name="updateProjectDto">Update board Dto</param>
        /// <response code="200"></response>
        /// <response code="401">Unauthorized</response>
        /// <response code="403">Access denied</response>
        /// <response code="404">Project not found</response>
        [HttpPut("{projectId:long}")]
        public async Task Update(long projectId, [FromBody] UpdateProjectDto updateProjectDto)
        {
            await _mediator.Send(new UpdateProjectRequest() { Id = projectId, UpdateProjectDto = updateProjectDto});
        }
        
        /// <summary>
        /// Delete project
        /// </summary>
        /// <param name="projectId">Project id</param>
        /// <response code="200"></response>
        /// <response code="401">Unauthorized</response>
        /// <response code="403">Access denied</response>
        /// <response code="404">Project not found</response>
        [HttpDelete("{projectId:long}")]
        public async Task Delete(long projectId)
        {
            await _mediator.Send(new DeleteProjectRequest { Id = projectId});
        }
    }
}