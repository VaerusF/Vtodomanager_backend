using System.Text.Json;
using Vtodo.Infrastructure.Interfaces.DataAccess;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Vtodo.Entities.Enums;
using Vtodo.Infrastructure.Interfaces.Services;
using Vtodo.UseCases.Handlers.Errors.Commands;
using Vtodo.UseCases.Handlers.Errors.Dto.NotFound;
using Vtodo.UseCases.Handlers.Tasks.Dto;

namespace Vtodo.UseCases.Handlers.Tasks.Queries.GetTasksByBoard
{
    internal class GetTasksByBoardRequestHandler : IRequestHandler<GetTasksByBoardRequest, List<TaskDto>?>
    {
        private readonly IDbContext _dbContext;
        private readonly IProjectSecurityService _projectSecurityService;
        private readonly IMediator _mediator;
        private readonly IDistributedCache _distributedCache;
        
        public GetTasksByBoardRequestHandler(
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
        
        public async Task<List<TaskDto>?> Handle(GetTasksByBoardRequest request, CancellationToken cancellationToken)
        {
            _projectSecurityService.CheckAccess(request.ProjectId, ProjectRoles.ProjectMember);
            
            var tasksByBoardStringFromCache = await _distributedCache.GetStringAsync($"tasks_by_board_{request.BoardId}", cancellationToken);
            
            if (tasksByBoardStringFromCache != null) return JsonSerializer.Deserialize<List<TaskDto>>(tasksByBoardStringFromCache);
            
            var board = await _dbContext.Boards
                .Include(x => x.Project)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.BoardId, cancellationToken: cancellationToken);
            
            if (board == null)
            {
                await _mediator.Send(new SendErrorToClientRequest() { Error = new BoardNotFoundError() }, cancellationToken); 
                return null;
            }

            var tasks = await _dbContext.Tasks
                .Include(x => x.Board)
                .Include(x => x.ParentTask)
                .AsNoTracking()
                .Where(x => x.Board.Id == board.Id)
                .ToListAsync(cancellationToken: cancellationToken);

            var result = tasks.Select(task => new TaskDto()
                {
                    Id = task.Id,
                    Title = task.Title,
                    Description = task.Description,
                    EndDate = task.EndDate == null
                        ? -1
                        : new DateTimeOffset((DateTime)task.EndDate).ToUnixTimeMilliseconds(),
                    IsCompleted = task.IsCompleted,
                    BoardId = task.Board.Id,
                    ParentId = task.ParentTask?.Id,
                    PrioritySort = task.PrioritySort,
                    Priority = (int)task.Priority,
                    ImageHeaderPath = task.ImageHeaderPath
                })
                .ToList();
            
            await _distributedCache.SetStringAsync($"tasks_by_board_{request.BoardId}", JsonSerializer.Serialize(result), cancellationToken);
            
            return result;
        }
    }
}