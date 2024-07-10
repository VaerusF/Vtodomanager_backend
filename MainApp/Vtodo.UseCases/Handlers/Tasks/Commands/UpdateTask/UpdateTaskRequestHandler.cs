using Vtodo.Infrastructure.Interfaces.DataAccess;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vtodo.DomainServices.Interfaces;
using Vtodo.Entities.Enums;
using Vtodo.Infrastructure.Interfaces.Services;
using Vtodo.UseCases.Handlers.Errors.Commands;
using Vtodo.UseCases.Handlers.Errors.Dto.NotFound;

namespace Vtodo.UseCases.Handlers.Tasks.Commands.UpdateTask
{
    internal class UpdateTaskRequestHandler : IRequestHandler<UpdateTaskRequest>
    {
        private readonly IDbContext _dbContext;
        private readonly IProjectSecurityService _projectSecurityService;
        private readonly ITaskService _taskService;
        private readonly IMediator _mediator;
        
        public UpdateTaskRequestHandler(
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
        
        public async Task Handle(UpdateTaskRequest request, CancellationToken cancellationToken)
        {
            var updateBoardDto = request.UpdateTaskDto;

            var task = await _dbContext.Tasks
                .Include(x => x.Board)
                .Include(x => x.Board.Project)
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken: cancellationToken);
            if (task == null)
            {
                await _mediator.Send(new SendErrorToClientRequest() { Error = new TaskNotFoundError() }, cancellationToken); 
                return;
            }

            _projectSecurityService.CheckAccess(task.Board.Project, ProjectRoles.ProjectUpdate);
            
            _taskService.UpdateTask(task, updateBoardDto.Title, updateBoardDto.Description, 
                updateBoardDto.IsCompleted, updateBoardDto.EndDateTimeStamp, updateBoardDto.Priority);
            
            await _dbContext.SaveChangesAsync(cancellationToken);

        }
    }
}