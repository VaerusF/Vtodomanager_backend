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

namespace Vtodo.UseCases.Handlers.Tasks.Queries.GetTask
{
    internal class GetTaskRequestHandler : IRequestHandler<GetTaskRequest, TaskDto?>
    {
        private readonly IDbContext _dbContext;
        private readonly IProjectSecurityService _projectSecurityService;
        private readonly IMediator _mediator;
        private readonly IDistributedCache _distributedCache;
        
        public GetTaskRequestHandler(
            IDbContext dbContext, 
            IProjectSecurityService projectSecurityService,
            IMediator mediator,
            IDistributedCache distributedCache
        )
        {
            _dbContext = dbContext;
            _projectSecurityService = projectSecurityService;
            _mediator = mediator;
            _distributedCache = distributedCache;
        }
        
        public async Task<TaskDto?> Handle(GetTaskRequest request, CancellationToken cancellationToken)
        {
            _projectSecurityService.CheckAccess(request.ProjectId, ProjectRoles.ProjectMember);
            
            var taskStringFromCache = await _distributedCache.GetStringAsync($"task_{request.TaskId}", cancellationToken);
            
            if (taskStringFromCache != null) return JsonSerializer.Deserialize<TaskDto>(taskStringFromCache);
            
            var task = await _dbContext.Tasks
                .Include(x => x.ParentTask)
                .Include(x => x.ChildrenTasks)
                .Include(x => x.Board)
                .Include(x => x.Board.Project)
                .AsNoTracking()
                .SingleOrDefaultAsync(p => p.Id == request.TaskId, cancellationToken: cancellationToken);
            
            if (task == null)
            {
                await _mediator.Send(new SendErrorToClientRequest() { Error = new TaskNotFoundError() }, cancellationToken); 
                return null;
            }

            TaskDto? result = null;

            result = new TaskDto()
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
            };
            
            await _distributedCache.SetStringAsync($"task_{request.TaskId}", JsonSerializer.Serialize(result), cancellationToken);
            
            return result;
        }
    }
}