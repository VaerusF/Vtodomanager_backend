using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Vtodo.DomainServices.Interfaces;
using Vtodo.Entities.Enums;
using Vtodo.Infrastructure.Interfaces.DataAccess;
using Vtodo.Infrastructure.Interfaces.Services;
using Vtodo.UseCases.Handlers.Errors.Commands;
using Vtodo.UseCases.Handlers.Errors.Dto.NotFound;

namespace Vtodo.UseCases.Handlers.Boards.Commands.DeleteBoardHeaderBackground
{
    internal class DeleteBoardHeaderBackgroundRequestHandler : IRequestHandler<DeleteBoardHeaderBackgroundRequest>
    {
        private readonly IDbContext _dbContext;
        private readonly IProjectSecurityService _projectSecurityService;
        private readonly IProjectsFilesService _projectFilesService;
        private readonly IBoardService _boardService;
        private readonly IMediator _mediator;
        private readonly IDistributedCache _distributedCache;
        
        public DeleteBoardHeaderBackgroundRequestHandler(
            IDbContext dbContext, 
            IProjectSecurityService projectSecurityService,
            IProjectsFilesService projectFilesService,
            IBoardService boardService,
            IMediator mediator,
            IDistributedCache distributedCache)
        {
            _dbContext = dbContext;
            _projectSecurityService = projectSecurityService;
            _projectFilesService = projectFilesService;
            _boardService = boardService;
            _mediator = mediator;
            
            _distributedCache = distributedCache;
        }
        
        public async Task Handle(DeleteBoardHeaderBackgroundRequest request, CancellationToken cancellationToken)
        {
            _projectSecurityService.CheckAccess(request.ProjectId, ProjectRoles.ProjectUpdate);
            
            var board = await _dbContext.Boards
                .Include(t => t.Project)
                .FirstOrDefaultAsync(x => x.Id == request.BoardId, cancellationToken);
            
            if (board == null)
            {
                await _mediator.Send(new SendErrorToClientRequest() { Error = new BoardNotFoundError() }, cancellationToken); 
                return;
            }
            
            if (string.IsNullOrWhiteSpace(board.ImageHeaderPath)) return;
            _projectFilesService.DeleteProjectFile(board.Project, board, board.ImageHeaderPath);

            _boardService.UpdateImageHeaderPath(board, null);
            
            await _dbContext.SaveChangesAsync(cancellationToken);
            
            await _distributedCache.RemoveAsync($"board_{request.BoardId}", cancellationToken);
            await _distributedCache.RemoveAsync($"boards_by_project_{request.ProjectId}", cancellationToken);
        }
    }
}