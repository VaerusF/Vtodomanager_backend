using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Vtodo.Entities.Enums;
using Vtodo.Infrastructure.Interfaces.DataAccess;
using Vtodo.Infrastructure.Interfaces.Services;
using Vtodo.UseCases.Handlers.Errors.Commands;
using Vtodo.UseCases.Handlers.Errors.Dto.NotFound;

namespace Vtodo.UseCases.Handlers.Boards.Queries.GetBoardHeaderBackground
{
    internal class GetBoardHeaderBackgroundRequestHandler : IRequestHandler<GetBoardHeaderBackgroundRequest, FileStream?>
    {
        private readonly IDbContext _dbContext;
        private readonly IProjectsFilesService _projectFilesService;
        private readonly IMediator _mediator;
        private readonly IDistributedCache _distributedCache;
        private readonly IProjectSecurityService _projectSecurityService;
        
        public GetBoardHeaderBackgroundRequestHandler(
            IDbContext dbContext,
            IProjectsFilesService projectFilesService,
            IMediator mediator,
            IDistributedCache distributedCache, 
            IProjectSecurityService projectSecurityService)
        {
            _dbContext = dbContext;
            _projectFilesService = projectFilesService;
            _mediator = mediator;
            _distributedCache = distributedCache;
            _projectSecurityService = projectSecurityService;
        }
        
        public async Task<FileStream?> Handle(GetBoardHeaderBackgroundRequest request, CancellationToken cancellationToken)
        {
            _projectSecurityService.CheckAccess(request.ProjectId, ProjectRoles.ProjectMember);
            
            var board = await _dbContext.Boards
                .Include(x => x.Project)
                .AsNoTracking()
                .SingleOrDefaultAsync(p => p.Id == request.BoardId, cancellationToken: cancellationToken);

            if (board == null)
            {
                await _mediator.Send(new SendErrorToClientRequest() { Error = new BoardNotFoundError() }, cancellationToken); 
                return null;
            }

            if (board.ImageHeaderPath == null) return null;

            var result = _projectFilesService.GetProjectFile(board.Project, board, board.ImageHeaderPath);

            return result;
        }
    }
}