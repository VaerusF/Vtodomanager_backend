using MediatR;
using Microsoft.AspNetCore.Mvc;
using Vtodo.UseCases.Handlers.Tasks.Commands.CreateTask;
using Vtodo.UseCases.Handlers.Tasks.Commands.DeleteTask;
using Vtodo.UseCases.Handlers.Tasks.Commands.MoveTaskToAnotherBoard;
using Vtodo.UseCases.Handlers.Tasks.Commands.MoveTaskToAnotherTask;
using Vtodo.UseCases.Handlers.Tasks.Commands.MoveTaskToRoot;
using Vtodo.UseCases.Handlers.Tasks.Commands.UpdateTask;
using Vtodo.UseCases.Handlers.Tasks.Commands.UpdateTaskComplete;
using Vtodo.UseCases.Handlers.Tasks.Dto;
using Vtodo.UseCases.Handlers.Tasks.Queries.GetTask;
using Vtodo.UseCases.Handlers.Tasks.Queries.GetTasksByBoard;

namespace Vtodo.Controllers
{
    [Route("api/v1/projects/{projectId:long}/boards/{boardId:long}/tasks")]
    [ApiController]
    public class TasksController : ControllerBase
    {
        private readonly IMediator _mediator;

        public TasksController(IMediator mediator)
        {
            _mediator = mediator;
        }
        
        /// <summary>
        /// Get task by id
        /// </summary>
        /// <param name="projectId">Project id</param>
        /// <param name="boardId">Board id</param>
        /// <param name="taskId">Task id</param>
        /// <response code="200">Returns task dto</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="403">Access denied</response>
        /// <response code="404">task not found</response>
        [HttpGet("{taskId:long}")]
        public async Task<ActionResult<TaskDto>> Get(long projectId, long boardId, long taskId)
        {
            return await _mediator.Send(new GetTaskRequest() { ProjectId = projectId, TaskId = taskId});
        }
        
        /// <summary>
        /// Get list of tasks by board
        /// </summary>
        /// <param name="projectId">Project id</param>
        /// <param name="boardId">Board id</param>
        /// <response code="200">Returns list of task dto</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="403">Access denied</response>
        /// <response code="404">Board not found</response>
        /// <response code="404">Task not found</response>
        [HttpGet("by_board/")]
        public async Task<ActionResult<List<TaskDto>>> GetByTask(long projectId, long boardId)
        {
            return await _mediator.Send(new GetTasksByBoardRequest() {ProjectId = projectId, BoardId = boardId});
        }
        
        /// <summary>
        /// Create a task
        /// </summary>
        /// <param name="projectId">Project id</param>
        /// <param name="boardId">Board id</param>
        /// <param name="createTaskDto">Task dto</param>
        /// <response code="200"></response>
        /// <response code="401">Unauthorized</response>
        /// <response code="403">Access denied</response>
        /// <response code="404">Board not found</response>
        /// <response code="404">Task not found</response>
        [HttpPost()]
        public async Task Create(long projectId, long boardId, [FromBody] CreateTaskDto createTaskDto)
        {
            await _mediator.Send(new CreateTaskRequest()
            {
                ProjectId = projectId, 
                BoardId = boardId, 
                CreateTaskDto = createTaskDto
            });
        }
        
        /// <summary>
        /// Update task
        /// </summary>
        /// <param name="projectId">Project id</param>
        /// <param name="boardId">Board id</param>
        /// <param name="taskId">Task id</param>
        /// <param name="updateTaskDto">Task dto</param>
        /// <response code="200"></response>
        /// <response code="401">Unauthorized</response>
        /// <response code="403">Access denied</response>
        /// <response code="404">Task not found</response>
        [HttpPut("{taskId:long}")]
        public async Task Update(long projectId, long boardId, long taskId, [FromBody] UpdateTaskDto updateTaskDto)
        {
            await _mediator.Send(new UpdateTaskRequest()
            {
                ProjectId = projectId, 
                BoardId = boardId,
                TaskId = taskId, 
                UpdateTaskDto = updateTaskDto
            });
        }
        
        /// <summary>
        /// Update complete status
        /// </summary>
        /// <param name="projectId">Project id</param>
        /// <param name="boardId">Board id</param>
        /// <param name="taskId">Task id</param>
        /// <param name="updateTaskCompleteDto">Task dto</param>
        /// <response code="200"></response>
        /// <response code="401">Unauthorized</response>
        /// <response code="403">Access denied</response>
        /// <response code="404">Task not found</response>
        [HttpPut("{taskId:long}/complete")]
        public async Task UpdateTaskComplete(long projectId, long boardId, long taskId, [FromBody] UpdateTaskCompleteDto updateTaskCompleteDto)
        {
            await _mediator.Send(new UpdateTaskCompleteRequest()
            {
                ProjectId = projectId, 
                BoardId = boardId,
                TaskId = taskId, 
                UpdateTaskCompleteDto = updateTaskCompleteDto
            });
        }
        
        /// <summary>
        /// Move task to root in board
        /// </summary>
        /// <param name="projectId">Project id</param>
        /// <param name="boardId">Board id</param>
        /// <param name="taskId">Task id</param>
        /// <response code="200"></response>
        /// <response code="401">Unauthorized</response>
        /// <response code="403">Access denied</response>
        /// <response code="404">Task not found</response>
        [HttpPut("{taskId:long}/move_to_root")]
        public async Task MoveToRoot(long projectId, long boardId, long taskId)
        {
            await _mediator.Send(new MoveTaskToRootRequest() { ProjectId = projectId, TaskId = taskId});
        }
        
        /// <summary>
        /// Move task to another task as children
        /// </summary>
        /// <param name="projectId">Project id</param>
        /// <param name="boardId">Board id</param>
        /// <param name="taskId">Task id</param>
        /// <param name="parentTaskId">Parent task id</param>
        /// <response code="200"></response>
        /// <response code="400">New parent task id should not be equal to task id (Attempt to use self as parent)</response>
        /// <response code="400">New parent task id should not be equal to old parent task id (Attempt to change parent task to same parent)</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="403">Access denied</response>
        /// <response code="404">Task not found</response>
        [HttpPut("{taskId:long}/move_to_task/{parentTaskId:long}")]
        public async Task MoveToTask(long projectId, long boardId, long taskId, long parentTaskId)
        {
            await _mediator.Send(new MoveTaskToAnotherTaskRequest()
            {
                ProjectId = projectId, 
                BoardId = boardId,
                TaskId = taskId, 
                NewParentTaskId = parentTaskId
            });
        }
        
        /// <summary>
        /// Move task to another board
        /// </summary>
        /// <param name="projectId">Project id</param>
        /// <param name="boardId">Board id</param>
        /// <param name="taskId">Task id</param>
        /// <param name="newBoardId">Board id</param>
        /// <response code="200"></response>
        /// <response code="400">New board id should not be equal to old board id (Attempt to change board to same board)</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="403">Access denied</response>
        /// <response code="404">Task not found</response>
        /// <response code="404">Board not found</response>
        [HttpPut("{taskId:long}/move_to_board/{newBoardId:long}")]
        public async Task MoveToBoard(long projectId, long boardId, long taskId, long newBoardId)
        {
            await _mediator.Send(new MoveTaskToAnotherBoardRequest()
            {
                ProjectId = projectId, 
                BoardId = boardId,
                TaskId = taskId, 
                NewBoardId = newBoardId
            });
        }

        /// <summary>
        /// Delete task
        /// </summary>
        /// <param name="projectId">Project id</param>
        /// <param name="boardId">Board id</param>
        /// <param name="taskId">Task id</param>
        /// <response code="200"></response>
        /// <response code="401">Unauthorized</response>
        /// <response code="403">Access denied</response>
        /// <response code="404">Task not found</response>
        [HttpDelete("{taskId:long}")]
        public async Task Delete(long projectId, long boardId, long taskId)
        {
            await _mediator.Send(new DeleteTaskRequest() { ProjectId = projectId, BoardId = boardId, TaskId = taskId });
        }
    }
}