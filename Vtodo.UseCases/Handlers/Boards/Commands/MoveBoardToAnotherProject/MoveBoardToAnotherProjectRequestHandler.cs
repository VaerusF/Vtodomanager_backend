using System.Threading;
using System.Threading.Tasks;
using Vtodo.Infrastructure.Interfaces.DataAccess;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vtodo.Entities.Enums;
using Vtodo.Entities.Exceptions;
using Vtodo.Infrastructure.Interfaces.Services;
using Vtodo.UseCases.Handlers.Errors.Commands;
using Vtodo.UseCases.Handlers.Errors.Dto.InvalidOperation;
using Vtodo.UseCases.Handlers.Errors.Dto.NotFound;

namespace Vtodo.UseCases.Handlers.Boards.Commands.MoveBoardToAnotherProject
{
    internal class MoveBoardToAnotherProjectRequestHandler : IRequestHandler<MoveBoardToAnotherProjectRequest>
    {
        private readonly IDbContext _dbContext;
        private readonly IProjectSecurityService _projectSecurityService;
        private readonly IMediator _mediator;

        public MoveBoardToAnotherProjectRequestHandler(
            IDbContext dbContext, 
            IProjectSecurityService projectSecurityService,
            IMediator mediator)
        {
            _dbContext = dbContext;
            _projectSecurityService = projectSecurityService;
            _mediator = mediator;
        }
        
        public async Task Handle(MoveBoardToAnotherProjectRequest request, CancellationToken cancellationToken)
        {
            var board = await _dbContext.Boards.Include(b => b.Project).FirstOrDefaultAsync(x => x.Id == request.BoardId, cancellationToken);
            if (board == null)
            {
                await _mediator.Send(new SendErrorToClientRequest() { Error = new BoardNotFoundError() }, cancellationToken); 
                return;
            }
            
            _projectSecurityService.CheckAccess(board.Project, ProjectRoles.ProjectUpdate);
            
            var newProject = await _dbContext.Projects.FindAsync(request.ProjectId, cancellationToken);

            if (newProject == null)
            {
                await _mediator.Send(new SendErrorToClientRequest() { Error = new ProjectNotFoundError() }, cancellationToken); 
                return;
            }

            if (newProject.Id == board.Project.Id)
            {
                await _mediator.Send(new SendErrorToClientRequest() { Error = new NewProjectIdEqualOldIdError() }, cancellationToken); 
                return;
            }
            
            _projectSecurityService.CheckAccess(newProject, ProjectRoles.ProjectUpdate);
            
            board.Project = newProject;

            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}