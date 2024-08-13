using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Vtodo.UseCases.Handlers.Boards.Commands.DeleteBoardHeaderBackground;
using Vtodo.UseCases.Handlers.Boards.Commands.UploadBoardHeaderBackground;
using Vtodo.UseCases.Handlers.Boards.Queries.GetBoardHeaderBackground;

namespace Vtodo.Controllers;

[Route("api/v1/projects/{projectId:long}/boards/{boardId:long}/header_background")]
[ApiController]
public class BoardHeaderBackgroundController : ControllerBase
{
    private readonly IMediator _mediator;
        
    public BoardHeaderBackgroundController(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    /// <summary>
        /// Get board header background image 
        /// </summary>
        /// <param name="projectId">Project id</param>
        /// <param name="boardId">Board id</param>
        /// <response code="200"></response>
        /// <response code="400">Background image file not exists</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="403">Access denied</response>
        /// <response code="404">Board not found</response>
        /// <response code="500">File not found</response>
        [HttpGet("")]
        public async Task<ActionResult<FileStream?>> GetBoardHeaderBackground(long projectId, long boardId)
        {
            var file = await _mediator.Send(new GetBoardHeaderBackgroundRequest() { ProjectId = projectId, BoardId = boardId});
            
            return file == null ? Ok() : File(file, "application/octet-stream", Path.GetFileName(file.Name));
        }
        
        /// <summary>
        /// Upload board header background image 
        /// </summary>
        /// <param name="projectId">Project id</param>
        /// <param name="boardId">Board id</param>
        /// <param name="uploadBoardHeaderBackground">Image, one of the types: ".jpg", ".png"</param>
        /// <response code="200"></response>
        /// <response code="400">Background image file not exists</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="403">Access denied</response>
        /// <response code="404">Board not found</response>
        [HttpPut("")]
        public async Task UploadBoardHeaderBackground(long boardId, IFormFile uploadBoardHeaderBackground, long projectId)
        {
            await _mediator.Send(new UploadBoardHeaderBackgroundRequest ()
            {
                ProjectId = projectId, 
                BoardId = boardId, 
                BackgroundImage = uploadBoardHeaderBackground.OpenReadStream(), 
                FileName = uploadBoardHeaderBackground.FileName
            });
        }
        
        /// <summary>
        /// Delete board header background image 
        /// </summary>
        /// <param name="projectId">Project id</param>
        /// <param name="boardId">Board id</param>
        /// <response code="200"></response>
        /// <response code="400">Background image file not exists</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="403">Access denied</response>
        /// <response code="404">Board not found</response>
        [HttpDelete("")]
        public async Task DeleteBoardHeaderBackground(long boardId, long projectId)
        {
            await _mediator.Send(new DeleteBoardHeaderBackgroundRequest () { ProjectId = projectId, BoardId = boardId});
        }
}