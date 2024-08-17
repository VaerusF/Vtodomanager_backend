using MediatR;
using Microsoft.AspNetCore.Mvc;
using Vtodo.UseCases.Handlers.Boards.Commands.CreateBoard;
using Vtodo.UseCases.Handlers.Boards.Commands.DeleteBoard;
using Vtodo.UseCases.Handlers.Boards.Commands.MoveBoardToAnotherProject;
using Vtodo.UseCases.Handlers.Boards.Commands.SwapBoardsPrioritySort;
using Vtodo.UseCases.Handlers.Boards.Commands.UpdateBoard;
using Vtodo.UseCases.Handlers.Boards.Dto;
using Vtodo.UseCases.Handlers.Boards.Queries.GetBoard;
using Vtodo.UseCases.Handlers.Boards.Queries.GetBoardsByProject;

namespace Vtodo.Controllers
{
    [Route("api/v1/projects/{projectId:long}/boards")]
    [ApiController]
    public class BoardsController : ControllerBase
    {
        private readonly IMediator _mediator;
        
        public BoardsController(IMediator mediator)
        {
            _mediator = mediator;
        }
        
        /// <summary>
        /// Get board by id
        /// </summary>
        /// <param name="projectId">Project id</param>
        /// <param name="boardId">Board id</param>
        /// <response code="200">Returns board dto</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="403">Access denied</response>
        /// <response code="404">Board not found</response>
        [HttpGet("{boardId:long}")]
        public async Task<ActionResult<BoardDto>> Get(long projectId, long boardId)
        {
            return await _mediator.Send(new GetBoardRequest() { ProjectId = projectId, BoardId = boardId});
        }
        
        /// <summary>
        /// Get all boards by project id
        /// </summary>
        /// <param name="projectId">Project id</param>
        /// <response code="200">Returns list board dto</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="403">Access denied</response>
        /// <response code="404">Project not found</response>
        [HttpGet("by_project")]
        public async Task<ActionResult<List<BoardDto>>> GetByProject(long projectId)
        {
            return await _mediator.Send(new GetBoardsByProjectRequest() { ProjectId = projectId});
        }
        
        /// <summary>
        /// Create a board
        /// </summary>
        /// <param name="projectId">Project id</param>
        /// <param name="createBoardDto">Create board Dto</param>
        /// <response code="200"></response>
        /// <response code="401">Unauthorized</response>
        /// <response code="403">Access denied</response>
        /// <response code="404">Project not found</response>
        [HttpPost()]
        public async Task Create(long projectId, [FromBody] CreateBoardDto createBoardDto)
        {
            await _mediator.Send(new CreateBoardRequest() { ProjectId = projectId, CreateBoardDto = createBoardDto});
        }
        
        /// <summary>
        /// Update board
        /// </summary>
        /// <param name="projectId">Project id</param>
        /// <param name="boardId">Board id</param>
        /// <param name="updateBoardDto">Update board Dto</param>
        /// <response code="200"></response>
        /// <response code="401">Unauthorized</response>
        /// <response code="403">Access denied</response>
        /// <response code="404">Board not found</response>
        [HttpPut("{boardId:long}")]
        public async Task Update(long projectId, long boardId, [FromBody] UpdateBoardDto updateBoardDto)
        {
            await _mediator.Send(new UpdateBoardRequest() { ProjectId = projectId, BoardId = boardId, UpdateBoardDto = updateBoardDto});
        }
        
        /// <summary>
        /// Swap board priority
        /// </summary>
        /// <param name="projectId">Project id</param>
        /// <param name="boardId1">Board id</param>
        /// <param name="boardId2">Board id</param>
        /// <response code="200"></response>
        /// <response code="400">Board ids should not be equals</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="403">Access denied</response>
        /// <response code="404">Board not found</response>
        [HttpPut("{boardId1:long}/swap_with/{boardId2:long}")]
        public async Task SwapBoardsPrioritySort(long projectId, long boardId1, long boardId2)
        {
            await _mediator.Send(new SwapBoardsPrioritySortRequest() { ProjectId = projectId, BoardId1 = boardId1, BoardId2 = boardId2});
        }
        
        /// <summary>
        /// Move board to another project
        /// </summary>
        /// <param name="projectId">Project id</param>
        /// <param name="boardId">Board id</param>
        /// <param name="newProjectId">New project id</param>
        /// <response code="200"></response>
        /// <response code="400">New project id should not be equal to old project id</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="403">Access denied</response>
        /// <response code="404">Board not found</response>
        /// <response code="404">Project not found</response>
        [HttpPut("{boardId:long}/move_to_project/{newProjectId:long}")]
        public async Task MoveToProject(long projectId, long boardId, long newProjectId)
        {
            await _mediator.Send(new MoveBoardToAnotherProjectRequest() { ProjectId = projectId, BoardId = boardId, NewProjectId = newProjectId});
        }
        
        /// <summary>
        /// Delete board
        /// </summary>
        /// <param name="projectId">Project id</param>
        /// <param name="boardId">Board id</param>
        /// <response code="200"></response>
        /// <response code="401">Unauthorized</response>
        /// <response code="403">Access denied</response>
        /// <response code="404">Board not found</response>
        [HttpDelete("{boardId:long}")]
        public async Task Delete(long projectId, long boardId)
        {
            await _mediator.Send(new DeleteBoardRequest() { ProjectId = projectId, BoardId = boardId});
        }
    }
}