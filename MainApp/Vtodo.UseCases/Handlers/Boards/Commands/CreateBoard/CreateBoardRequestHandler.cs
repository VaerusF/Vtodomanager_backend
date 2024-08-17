using Vtodo.Infrastructure.Interfaces.DataAccess;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Vtodo.DomainServices.Interfaces;
using Vtodo.Entities.Enums;
using Vtodo.Infrastructure.Interfaces.Services;
using Vtodo.UseCases.Handlers.Errors.Commands;
using Vtodo.UseCases.Handlers.Errors.Dto.NotFound;

namespace Vtodo.UseCases.Handlers.Boards.Commands.CreateBoard
{
    internal class CreateBoardRequestHandler : IRequestHandler<CreateBoardRequest>
    {
        private readonly IDbContext _dbContext;
        private readonly IProjectSecurityService _projectSecurityService;
        private readonly IMediator _mediator;
        private readonly IDistributedCache _distributedCache;
        private readonly IBoardService _boardService;
        
        public CreateBoardRequestHandler(
            IDbContext dbContext, 
            IProjectSecurityService projectSecurityService,
            IMediator mediator,
            IDistributedCache distributedCache,
            IBoardService boardService)
        {
            _dbContext = dbContext;
            _projectSecurityService = projectSecurityService;
            _mediator = mediator;
            _distributedCache = distributedCache;
            _boardService = boardService;
        }
        
        public async Task Handle(CreateBoardRequest request, CancellationToken cancellationToken)
        {
            _projectSecurityService.CheckAccess(request.ProjectId, ProjectRoles.ProjectUpdate);
            
            var createDto = request.CreateBoardDto;

            var project = await _dbContext.Projects.FindAsync(request.ProjectId, cancellationToken);
            if (project == null)
            {
                await _mediator.Send(new SendErrorToClientRequest() { Error = new ProjectNotFoundError() }, cancellationToken); 
                return;
            }
            
            var board = _boardService.CreateBoard(createDto.Title, project);
            
            _dbContext.Boards.Add(board);

            await _dbContext.SaveChangesAsync(cancellationToken);
            
            await _distributedCache.RemoveAsync($"boards_by_project_{request.ProjectId}", cancellationToken);
        }
    }
}