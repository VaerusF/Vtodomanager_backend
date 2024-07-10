using System.Threading;
using System.Threading.Tasks;
using Vtodo.Infrastructure.Interfaces.DataAccess;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vtodo.DomainServices.Interfaces;
using Vtodo.Entities.Enums;
using Vtodo.Entities.Exceptions;
using Vtodo.Infrastructure.Interfaces.Services;
using Vtodo.UseCases.Handlers.Errors.Commands;
using Vtodo.UseCases.Handlers.Errors.Dto.InvalidOperation;
using Vtodo.UseCases.Handlers.Errors.Dto.NotFound;

namespace Vtodo.UseCases.Handlers.Tasks.Commands.MoveTaskToAnotherTask
{
    internal class MoveTaskToAnotherTaskRequestHandler : IRequestHandler<MoveTaskToAnotherTaskRequest>
    {
        private readonly IDbContext _dbContext;
        private readonly IProjectSecurityService _projectSecurityService;
        private readonly ITaskService _taskService;
        private readonly IMediator _mediator;

        public MoveTaskToAnotherTaskRequestHandler(
            IDbContext dbContext, 
            IProjectSecurityService projectSecurityService,
            ITaskService taskService,
            IMediator mediator)
        {
            _dbContext = dbContext;
            _projectSecurityService = projectSecurityService;
            _taskService = taskService;
            _mediator = mediator;
        }
        
        public async Task Handle(MoveTaskToAnotherTaskRequest request, CancellationToken cancellationToken)
        {
            var task = await _dbContext.Tasks
                .Include(t => t.Board)
                .Include(t => t.Board.Project)
                .Include(t => t.ParentTask)
                .FirstOrDefaultAsync(x => x.Id == request.TaskId, cancellationToken);
            if (task == null)
            {
                await _mediator.Send(new SendErrorToClientRequest() { Error = new TaskNotFoundError() }, cancellationToken); 
                return;
            }
            
            _projectSecurityService.CheckAccess(task.Board.Project, ProjectRoles.ProjectUpdate);

            if (task.Id == request.NewParentTaskId)
            {
                await _mediator.Send(new SendErrorToClientRequest() { Error = new TaskIdEqualNewParentTaskIdError() }, cancellationToken); 
                return;
            }

            if (task.ParentTask != null)
            {
                if (task.ParentTask.Id == request.NewParentTaskId)
                {
                    await _mediator.Send(new SendErrorToClientRequest() { Error = new NewParentTaskIdEqualOldIdError() }, cancellationToken); 
                    return;
                }
            }

            var newParentTask = await _dbContext.Tasks
                .Include(t => t.Board)
                .FirstOrDefaultAsync(x => x.Id == request.NewParentTaskId, cancellationToken);

            if (newParentTask == null)
            {
                await _mediator.Send(new SendErrorToClientRequest() { Error = new TaskNotFoundError() }, cancellationToken); 
                return;
            }

            if (task.Board.Id != newParentTask.Board.Id)
            {
                await _mediator.Send(new SendErrorToClientRequest() { Error = new AnotherBoardError() }, cancellationToken); 
                return;
            }

            _taskService.MoveTaskToAnotherTask(task, newParentTask);

            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}