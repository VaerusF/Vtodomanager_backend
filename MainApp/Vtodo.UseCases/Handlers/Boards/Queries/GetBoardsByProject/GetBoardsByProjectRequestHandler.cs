using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Vtodo.Entities.Enums;
using Vtodo.Infrastructure.Interfaces.DataAccess;
using Vtodo.Infrastructure.Interfaces.Services;
using Vtodo.UseCases.Handlers.Boards.Dto;

namespace Vtodo.UseCases.Handlers.Boards.Queries.GetBoardsByProject
{
    internal class GetBoardsByProjectRequestHandler : IRequestHandler<GetBoardsByProjectRequest, List<BoardDto>?>
    {
        private readonly IDbContext _dbContext;
        private readonly IProjectSecurityService _projectSecurityService;
        private readonly IMediator _mediator;
        private readonly IDistributedCache _distributedCache;
        
        public GetBoardsByProjectRequestHandler(
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
        
        public async Task<List<BoardDto>?> Handle(GetBoardsByProjectRequest request, CancellationToken cancellationToken)
        {
            _projectSecurityService.CheckAccess(request.ProjectId, ProjectRoles.ProjectMember);
            
            var boardsListByProjectStringFromCache = await _distributedCache.GetStringAsync($"boards_by_project_{request.ProjectId}", cancellationToken);
            
            if (boardsListByProjectStringFromCache != null) return JsonSerializer.Deserialize<List<BoardDto>>(boardsListByProjectStringFromCache);
            
            var boards = await _dbContext.Boards
                .Include(x => x.Project)
                .AsNoTracking()
                .Where(x => x.Project.Id == request.ProjectId)
                .ToListAsync(cancellationToken: cancellationToken);

            var result = boards.Select(board => new BoardDto()
                {
                    Id = board.Id,
                    Title = board.Title,
                    PrioritySort = board.PrioritySort,
                    ProjectId = board.Project.Id,
                    ImageHeaderPath = board.ImageHeaderPath
                })
            .ToList();
            
            await _distributedCache.SetStringAsync($"boards_by_project_{request.ProjectId}", JsonSerializer.Serialize(result), cancellationToken);
            
            return result;
        }
    }
}