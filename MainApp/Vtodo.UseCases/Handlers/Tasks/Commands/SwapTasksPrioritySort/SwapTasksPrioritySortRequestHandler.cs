using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Vtodo.DomainServices.Interfaces;
using Vtodo.Entities.Enums;
using Vtodo.Infrastructure.Interfaces.DataAccess;
using Vtodo.Infrastructure.Interfaces.Services;
using Vtodo.UseCases.Handlers.Errors.Commands;
using Vtodo.UseCases.Handlers.Errors.Dto.InvalidOperation;
using Vtodo.UseCases.Handlers.Errors.Dto.NotFound;

namespace Vtodo.UseCases.Handlers.Tasks.Commands.SwapTasksPrioritySort;

internal class SwapTasksPrioritySortRequestHandler : IRequestHandler<SwapTasksPrioritySortRequest>
{
    private readonly IDbContext _dbContext;
    private readonly IProjectSecurityService _projectSecurityService;
    private readonly ITaskService _taskService;
    private readonly IMediator _mediator;
    private readonly IDistributedCache _distributedCache;
    
    public SwapTasksPrioritySortRequestHandler(
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
    
    public async Task Handle(SwapTasksPrioritySortRequest request, CancellationToken cancellationToken)
    {
        _projectSecurityService.CheckAccess(request.ProjectId, ProjectRoles.ProjectUpdate);
        
        if (request.TaskId1 == request.TaskId2)
        {
            await _mediator.Send(new SendErrorToClientRequest() { Error = new TaskIdsEqualsIdError() }, cancellationToken); 
            return;
        }
        
        var task1 = await _dbContext.Tasks
            .Include(t => t.Board)
            .Include(t => t.Board.Project)
            .FirstOrDefaultAsync(x => x.Id == request.TaskId1, cancellationToken);
        if (task1 == null)
        {
            await _mediator.Send(new SendErrorToClientRequest() { Error = new TaskNotFoundError() }, cancellationToken); 
            return;
        }
        
        var task2 = await _dbContext.Tasks
            .Include(t => t.Board)
            .Include(t => t.Board.Project)
            .FirstOrDefaultAsync(x => x.Id == request.TaskId2, cancellationToken);
        if (task2 == null)
        {
            await _mediator.Send(new SendErrorToClientRequest() { Error = new TaskNotFoundError() }, cancellationToken); 
            return;
        }
        
        if (task1.Board.Id != task2.Board.Id)
        {
            await _mediator.Send(new SendErrorToClientRequest() { Error = new DifferentBoardsError() }, cancellationToken); 
            return;
        }
        
        _taskService.SwapTasksPrioritySort(task1, task2);
        
        await _dbContext.SaveChangesAsync(cancellationToken);
        
        await _distributedCache.RemoveAsync($"task_{request.TaskId1}", cancellationToken);
        await _distributedCache.RemoveAsync($"task_{request.TaskId2}", cancellationToken);
        await _distributedCache.RemoveAsync($"tasks_by_board_{request.BoardId}", cancellationToken);
    }
}