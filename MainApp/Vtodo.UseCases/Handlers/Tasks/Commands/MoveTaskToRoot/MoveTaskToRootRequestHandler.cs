using Vtodo.Infrastructure.Interfaces.DataAccess;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Vtodo.DomainServices.Interfaces;
using Vtodo.Entities.Enums;
using Vtodo.Infrastructure.Interfaces.Services;
using Vtodo.UseCases.Handlers.Errors.Commands;
using Vtodo.UseCases.Handlers.Errors.Dto.NotFound;

namespace Vtodo.UseCases.Handlers.Tasks.Commands.MoveTaskToRoot
{
    internal class MoveTaskToRootRequestHandler : IRequestHandler<MoveTaskToRootRequest>
    {
        private readonly IDbContext _dbContext;
        private readonly IProjectSecurityService _projectSecurityService;
        private readonly ITaskService _taskService;
        private readonly IMediator _mediator;
        private readonly IDistributedCache _distributedCache;
        
        public MoveTaskToRootRequestHandler(
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
        
        public async Task Handle(MoveTaskToRootRequest request, CancellationToken cancellationToken)
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
            
            _taskService.MoveTaskToRoot(task);

            await _dbContext.SaveChangesAsync(cancellationToken);
            
            await _distributedCache.RemoveAsync($"task_{request.TaskId}", cancellationToken);
            await _distributedCache.RemoveAsync($"tasks_by_board_{request.BoardId}", cancellationToken);
        }
    }
}