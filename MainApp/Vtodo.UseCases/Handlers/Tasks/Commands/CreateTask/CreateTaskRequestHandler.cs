using Vtodo.Entities.Models;
using Vtodo.Infrastructure.Interfaces.DataAccess;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Vtodo.DomainServices.Interfaces;
using Vtodo.Entities.Enums;
using Vtodo.Infrastructure.Interfaces.Services;
using Vtodo.UseCases.Handlers.Errors.Commands;
using Vtodo.UseCases.Handlers.Errors.Dto.NotFound;

namespace Vtodo.UseCases.Handlers.Tasks.Commands.CreateTask
{
    internal class CreateTaskRequestHandler : IRequestHandler<CreateTaskRequest>
    {
        private readonly IDbContext _dbContext;
        private readonly IProjectSecurityService _projectSecurityService;
        private readonly ITaskService _taskService;
        private readonly IMediator _mediator;
        private readonly IDistributedCache _distributedCache;
        
        public CreateTaskRequestHandler(
            IDbContext dbContext, 
            IProjectSecurityService projectSecurityService,
            ITaskService taskService,
            IMediator mediator,
            IDistributedCache distributedCache)
        {
            _dbContext = dbContext;
            _projectSecurityService = projectSecurityService;
            _taskService = taskService;
            _mediator = mediator;
            _distributedCache = distributedCache;
        }
        
        public async Task Handle(CreateTaskRequest request, CancellationToken cancellationToken)
        {
            _projectSecurityService.CheckAccess(request.ProjectId, ProjectRoles.ProjectUpdate);
            
            var createDto = request.CreateTaskDto;
            
            var board = await _dbContext.Boards
                .Include(x => x.Project)
                .FirstOrDefaultAsync(x => x.Id == request.BoardId, cancellationToken);

            if (board == null)
            {
                await _mediator.Send(new SendErrorToClientRequest() { Error = new BoardNotFoundError() }, cancellationToken); 
                return;
            }

            TaskM? parentTask = null;
            
            if (createDto.ParentTaskId != null)
            {
                var parentTaskFromDb = await _dbContext.Tasks.FindAsync(createDto.ParentTaskId, cancellationToken);
                if (parentTaskFromDb == null)
                {
                    await _mediator.Send(new SendErrorToClientRequest() { Error = new TaskNotFoundError() }, cancellationToken); 
                    return;
                }

                parentTask = parentTaskFromDb;
            }
            
            var task = _taskService.CreateTask(
                createDto.Title, 
                createDto.Description,
                board, 
                newParentTaskM: parentTask
            );
            
            _dbContext.Tasks.Add(task);

            await _dbContext.SaveChangesAsync(cancellationToken);
            
            await _distributedCache.RemoveAsync($"tasks_by_board_{request.BoardId}", cancellationToken);
        }
    }
}