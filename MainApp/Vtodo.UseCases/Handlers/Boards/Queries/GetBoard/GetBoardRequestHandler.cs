using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Vtodo.Entities.Enums;
using Vtodo.Infrastructure.Interfaces.DataAccess;
using Vtodo.Infrastructure.Interfaces.Services;
using Vtodo.UseCases.Handlers.Boards.Dto;
using Vtodo.UseCases.Handlers.Errors.Commands;
using Vtodo.UseCases.Handlers.Errors.Dto.NotFound;

namespace Vtodo.UseCases.Handlers.Boards.Queries.GetBoard
{
    internal class GetBoardRequestHandler : IRequestHandler<GetBoardRequest, BoardDto?>
    {
        private readonly IDbContext _dbContext;
        private readonly IProjectSecurityService _projectSecurityService;
        private readonly IMediator _mediator;
        private readonly IDistributedCache _distributedCache;
        
        public GetBoardRequestHandler(
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
        
        public async Task<BoardDto?> Handle(GetBoardRequest request, CancellationToken cancellationToken)
        {
            _projectSecurityService.CheckAccess(request.ProjectId, ProjectRoles.ProjectMember);
            
            var boardStringFromCache = await _distributedCache.GetStringAsync($"board_{request.BoardId}", cancellationToken);
            
            if (boardStringFromCache != null) return JsonSerializer.Deserialize<BoardDto>(boardStringFromCache);
            
            var board = await _dbContext.Boards
                .Include(x => x.Project)
                .AsNoTracking()
                .SingleOrDefaultAsync(p => p.Id == request.BoardId, cancellationToken: cancellationToken);

            if (board == null)
            {
                await _mediator.Send(new SendErrorToClientRequest() { Error = new BoardNotFoundError() }, cancellationToken); 
                return null;
            }

            var result = new BoardDto()
            {
                Id = board.Id,
                Title = board.Title,
                ProjectId = board.Project.Id,
                PrioritySort = board.PrioritySort,
                ImageHeaderPath = board.ImageHeaderPath
            };
            
            await _distributedCache.SetStringAsync($"board_{request.BoardId}", JsonSerializer.Serialize(result), cancellationToken);
            
            return result;
        }
    }
}