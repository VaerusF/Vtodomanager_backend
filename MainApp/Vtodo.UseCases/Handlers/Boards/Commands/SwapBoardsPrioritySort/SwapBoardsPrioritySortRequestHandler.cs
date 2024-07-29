using MediatR;
using Microsoft.EntityFrameworkCore;
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
    
    public SwapBoardsPrioritySortRequestHandler(
        IDbContext dbContext, 
        IProjectSecurityService projectSecurityService,   
        IBoardService boardService,
        IMediator mediator)
    {
        _dbContext = dbContext;
        _projectSecurityService = projectSecurityService;
        _boardService = boardService;
        _mediator = mediator;
    }
    
    public async Task Handle(SwapBoardsPrioritySortRequest request, CancellationToken cancellationToken)
    {
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
        
        _projectSecurityService.CheckAccess(board1.Project, ProjectRoles.ProjectUpdate);
        _projectSecurityService.CheckAccess(board2.Project, ProjectRoles.ProjectUpdate);
        
        _boardService.SwapBoardsPrioritySort(board1, board2);
        
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}