using System.Threading;
using System.Threading.Tasks;
using Vtodo.Infrastructure.Interfaces.DataAccess;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vtodo.Entities.Enums;
using Vtodo.Entities.Exceptions;
using Vtodo.Infrastructure.Interfaces.Services;
using Vtodo.UseCases.Handlers.Errors.Commands;
using Vtodo.UseCases.Handlers.Errors.Dto.NotFound;

namespace Vtodo.UseCases.Handlers.Tasks.Commands.MoveTaskToRoot
{
    internal class MoveTaskToRootRequestHandler : IRequestHandler<MoveTaskToRootRequest>
    {
        private readonly IDbContext _dbContext;
        private readonly IProjectSecurityService _projectSecurityService;
        private readonly IMediator _mediator;

        public MoveTaskToRootRequestHandler(
            IDbContext dbContext, 
            IProjectSecurityService projectSecurityService,
            IMediator mediator)
        {
            _dbContext = dbContext;
            _projectSecurityService = projectSecurityService;
            _mediator = mediator;
        }
        
        public async Task Handle(MoveTaskToRootRequest request, CancellationToken cancellationToken)
        {
            var task = await _dbContext.Tasks
                .Include(t => t.Board)
                .Include(t => t.Board.Project)
                .Include(t => t.ParentTask)
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
            if (task == null)
            {
                await _mediator.Send(new SendErrorToClientRequest() { Error = new TaskNotFoundError() }, cancellationToken); 
                return;
            }
            
            _projectSecurityService.CheckAccess(task.Board.Project, ProjectRoles.ProjectUpdate);
            task.ParentTask = null;

            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}