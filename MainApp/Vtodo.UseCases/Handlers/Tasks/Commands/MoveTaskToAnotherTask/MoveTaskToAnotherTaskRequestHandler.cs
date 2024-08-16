using Vtodo.Infrastructure.Interfaces.DataAccess;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Vtodo.DomainServices.Interfaces;
using Vtodo.Entities.Enums;
using Vtodo.Infrastructure.Interfaces.Services;
using Vtodo.UseCases.Handlers.Errors.Commands;
using Vtodo.UseCases.Handlers.Errors.Dto.InvalidOperation;
using Vtodo.UseCases.Handlers.Errors.Dto.NotFound;

namespace Vtodo.UseCases.Handlers.Tasks.Commands.MoveTaskToAnotherTask
{
    internal class MoveTaskToAnotherTaskRequestHandler : IRequestHandler<MoveTaskToAnotherTaskRequest>
    {
        private readonly IDbContext _dbContext;
        private readonly IProjectSecurityService _projectSecurityService;
        private readonly ITaskService _taskService;
        private readonly IMediator _mediator;
        private readonly IDistributedCache _distributedCache;
        
        public MoveTaskToAnotherTaskRequestHandler(
            IDbContext dbContext, 
            IProjectSecurityService projectSecurityService,
            ITaskService taskService,
            IMediator mediator,
            IDistributedCache distributedCache
        )
        {
            _dbContext = dbContext;
            _projectSecurityService = projectSecurityService;
            _taskService = taskService;
            _mediator = mediator;
            _distributedCache = distributedCache;
        }
        
        public async Task Handle(MoveTaskToAnotherTaskRequest request, CancellationToken cancellationToken)
        {
            _projectSecurityService.CheckAccess(request.ProjectId, ProjectRoles.ProjectUpdate);
            
            var task = await _dbContext.Tasks
                .Include(t => t.Board)
                .Include(t => t.Board.Project)
                .Include(t => t.ParentTask)
                .FirstOrDefaultAsync(x => x.Id == request.TaskId, cancellationToken);
            if (task == null)
            {
                await _mediator.Send(new SendErrorToClientRequest() { Error = new TaskNotFoundError() }, cancellationToken); 
                return;
            }
            
            if (task.Id == request.NewParentTaskId)
            {
                await _mediator.Send(new SendErrorToClientRequest() { Error = new TaskIdEqualNewParentTaskIdError() }, cancellationToken); 
                return;
            }

            if (task.ParentTask != null)
            {
                if (task.ParentTask.Id == request.NewParentTaskId)
                {
                    await _mediator.Send(new SendErrorToClientRequest() { Error = new NewParentTaskIdEqualOldIdError() }, cancellationToken); 
                    return;
                }
            }

            var newParentTask = await _dbContext.Tasks
                .Include(t => t.Board)
                .FirstOrDefaultAsync(x => x.Id == request.NewParentTaskId, cancellationToken);

            if (newParentTask == null)
            {
                await _mediator.Send(new SendErrorToClientRequest() { Error = new TaskNotFoundError() }, cancellationToken); 
                return;
            }

            if (task.Board.Id != newParentTask.Board.Id)
            {
                await _mediator.Send(new SendErrorToClientRequest() { Error = new AnotherBoardError() }, cancellationToken); 
                return;
            }

            _taskService.MoveTaskToAnotherTask(task, newParentTask);

            await _dbContext.SaveChangesAsync(cancellationToken);
            
            await _distributedCache.RemoveAsync($"task_{request.TaskId}", cancellationToken);
            await _distributedCache.RemoveAsync($"task_{request.NewParentTaskId}", cancellationToken);
            await _distributedCache.RemoveAsync($"tasks_by_board_{request.BoardId}", cancellationToken);
        }
    }
}