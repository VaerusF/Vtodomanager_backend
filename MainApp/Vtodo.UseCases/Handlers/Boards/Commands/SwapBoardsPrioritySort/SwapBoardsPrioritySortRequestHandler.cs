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

namespace Vtodo.UseCases.Handlers.Boards.Commands.SwapBoardsPrioritySort;

internal class SwapBoardsPrioritySortRequestHandler : IRequestHandler<SwapBoardsPrioritySortRequest>
{
    private readonly IDbContext _dbContext;
    private readonly IProjectSecurityService _projectSecurityService;
    private readonly IBoardService _boardService;
    private readonly IMediator _mediator;
    private readonly IDistributedCache _distributedCache;
    
    public SwapBoardsPrioritySortRequestHandler(
        IDbContext dbContext, 
        IProjectSecurityService projectSecurityService,   
        IBoardService boardService,
        IMediator mediator,
        IDistributedCache distributedCache)
    {
        _dbContext = dbContext;
        _projectSecurityService = projectSecurityService;
        _boardService = boardService;
        _mediator = mediator;
        _distributedCache = distributedCache;
    }
    
    public async Task Handle(SwapBoardsPrioritySortRequest request, CancellationToken cancellationToken)
    {
        _projectSecurityService.CheckAccess(request.ProjectId, ProjectRoles.ProjectUpdate);
        
        if (request.BoardId1 == request.BoardId2)
        {
            await _mediator.Send(new SendErrorToClientRequest() { Error = new BoardIdsEqualsIdError() }, cancellationToken); 
            return;
        }
        
        var board1 = await _dbContext.Boards
            .Include(t => t.Project)
            .FirstOrDefaultAsync(x => x.Id == request.BoardId1, cancellationToken);
        if (board1 == null)
        {
            await _mediator.Send(new SendErrorToClientRequest() { Error = new BoardNotFoundError() }, cancellationToken); 
            return;
        }
        
        var board2 = await _dbContext.Boards
            .Include(t => t.Project)
            .FirstOrDefaultAsync(x => x.Id == request.BoardId2, cancellationToken);
        if (board2 == null)
        {
            await _mediator.Send(new SendErrorToClientRequest() { Error = new BoardNotFoundError() }, cancellationToken); 
            return;
        }
        
        if (board1.Project.Id != board2.Project.Id)
        {
            await _mediator.Send(new SendErrorToClientRequest() { Error = new DifferentProjectsError() }, cancellationToken); 
            return;
        }
        
        _boardService.SwapBoardsPrioritySort(board1, board2);
        
        await _dbContext.SaveChangesAsync(cancellationToken);
        
        await _distributedCache.RemoveAsync($"board_{request.BoardId1}", cancellationToken);
        await _distributedCache.RemoveAsync($"board_{request.BoardId2}", cancellationToken);
        await _distributedCache.RemoveAsync($"boards_by_project_{request.ProjectId}", cancellationToken);
    }
}