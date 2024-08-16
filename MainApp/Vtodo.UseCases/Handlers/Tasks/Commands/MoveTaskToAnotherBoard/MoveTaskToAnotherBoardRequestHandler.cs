using Vtodo.Infrastructure.Interfaces.DataAccess;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Vtodo.DomainServices.Interfaces;
using Vtodo.Entities.Enums;
using Vtodo.Entities.Models;
using Vtodo.Infrastructure.Interfaces.Services;
using Vtodo.UseCases.Handlers.Errors.Commands;
using Vtodo.UseCases.Handlers.Errors.Dto.InvalidOperation;
using Vtodo.UseCases.Handlers.Errors.Dto.NotFound;

namespace Vtodo.UseCases.Handlers.Tasks.Commands.MoveTaskToAnotherBoard
{
    internal class MoveTaskToAnotherBoardRequestHandler : IRequestHandler<MoveTaskToAnotherBoardRequest>
    {
        private readonly IDbContext _dbContext;
        private readonly IProjectSecurityService _projectSecurityService;
        private readonly ITaskService _taskService;
        private readonly IMediator _mediator;
        private readonly IDistributedCache _distributedCache;
        
        public MoveTaskToAnotherBoardRequestHandler(
            IDbContext dbContext, 
            IProjectSecurityService projectSecurityService,
            ITaskService taskService,
            IMediator mediator,
            IDistributedCache distributedCache)
        {
            _dbContext = dbContext;
            _projectSecurityService = projectSecurityService;
            _taskService = taskService;
            _mediator = mediator;
            _distributedCache = distributedCache;
        }
        
        public async Task Handle(MoveTaskToAnotherBoardRequest request, CancellationToken cancellationToken)
        {
            _projectSecurityService.CheckAccess(request.ProjectId, ProjectRoles.ProjectUpdate);
            
            var newBoard = await _dbContext.Boards
                .Include(x => x.Project)
                .FirstOrDefaultAsync(x => x.Id == request.NewBoardId, cancellationToken: cancellationToken);
            if (newBoard == null)
            {
                await _mediator.Send(new SendErrorToClientRequest() { Error = new BoardNotFoundError() }, cancellationToken); 
                return;
            }

            _projectSecurityService.CheckAccess(newBoard.Project.Id, ProjectRoles.ProjectUpdate);
            
            var rootTask = _dbContext.Tasks
                .Include(t => t.Board)
                .Include(t => t.Board.Project)
                .Include(t=> t.ParentTask)
                .Include(t => t.ChildrenTasks)
                .FirstOrDefault(x => x.Id == request.TaskId);
            if (rootTask == null)
            {
                await _mediator.Send(new SendErrorToClientRequest() { Error = new TaskNotFoundError() }, cancellationToken); 
                return;
            }

            if (rootTask.Board.Id == newBoard.Id)
            {
                await _mediator.Send(new SendErrorToClientRequest() { Error = new NewBoardIdEqualOldIdError() }, cancellationToken); 
                return;
            }
            
            _taskService.MoveTaskToRoot(rootTask);

            var tasksList = new List<TaskM> {rootTask};

            var childrenIterationList = await _dbContext.Tasks
                .Where(x => x.ParentTask == rootTask)
                .ToListAsync(cancellationToken: cancellationToken);
            
            while (childrenIterationList.Count > 0 && !cancellationToken.IsCancellationRequested)
            {
                tasksList.AddRange(childrenIterationList);
                
                var childrenIterationIds = childrenIterationList.Select(x => x.Id).ToArray();
                childrenIterationList = await _dbContext.Tasks
                    .Where(x => x.ParentTask != null && childrenIterationIds.Contains(x.ParentTask.Id))
                    .ToListAsync(cancellationToken: cancellationToken);
            }

            _taskService.MoveAllTaskFromListToAnotherBoard(tasksList, newBoard);
            
            await _dbContext.SaveChangesAsync(cancellationToken);
            
            await _distributedCache.RemoveAsync($"task_{request.TaskId}", cancellationToken);
            await _distributedCache.RemoveAsync($"tasks_by_board_{request.BoardId}", cancellationToken);
            await _distributedCache.RemoveAsync($"tasks_by_board_{request.NewBoardId}", cancellationToken);
        }
        
    }
}