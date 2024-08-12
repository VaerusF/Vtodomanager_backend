using Vtodo.Infrastructure.Interfaces.DataAccess;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Vtodo.Entities.Enums;
using Vtodo.Entities.Models;
using Vtodo.Infrastructure.Interfaces.Services;
using Vtodo.UseCases.Handlers.Errors.Commands;
using Vtodo.UseCases.Handlers.Errors.Dto.NotFound;
using Vtodo.UseCases.Handlers.Logs.Commands.SendLogToLogger;

namespace Vtodo.UseCases.Handlers.Boards.Commands.DeleteBoard
{
    internal class DeleteBoardRequestHandler : IRequestHandler<DeleteBoardRequest>
    {
        private readonly IDbContext _dbContext;
        private readonly IProjectSecurityService _projectSecurityService;
        private readonly IMediator _mediator;
        private readonly IDistributedCache _distributedCache;
        
        public DeleteBoardRequestHandler(
            IDbContext dbContext, 
            IProjectSecurityService projectSecurityService,
            IMediator mediator,
            IDistributedCache distributedCache)
        {
            _dbContext = dbContext;
            _projectSecurityService = projectSecurityService;
            _mediator = mediator;
            _distributedCache = distributedCache;
        }
        
        public async Task Handle(DeleteBoardRequest request, CancellationToken cancellationToken)
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
            
            _dbContext.Boards.Remove(board);
            
            await _dbContext.SaveChangesAsync(cancellationToken);
            
            await _distributedCache.RemoveAsync($"board_{request.BoardId}", cancellationToken);
            await _distributedCache.RemoveAsync($"boards_by_project_{request.ProjectId}", cancellationToken);
            
            await _mediator.Send(new SendLogToLoggerRequest() { Log = new Log()
                    {
                        ServiceName = "MainApp",
                        LogLevel = CustomLogLevels.Information, 
                        Message = $"Board {board.Id} \"{board.Title}\" has been deleted",
                        DateTime = DateTime.UtcNow
                    } 
                }, cancellationToken
            );
        }
    }
}