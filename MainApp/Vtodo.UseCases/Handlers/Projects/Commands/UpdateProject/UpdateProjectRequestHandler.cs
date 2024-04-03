using System.Threading;
using System.Threading.Tasks;
using Vtodo.Infrastructure.Interfaces.DataAccess;
using MediatR;
using Vtodo.Entities.Enums;
using Vtodo.Entities.Exceptions;
using Vtodo.Infrastructure.Interfaces.Services;
using Vtodo.UseCases.Handlers.Errors.Commands;
using Vtodo.UseCases.Handlers.Errors.Dto.NotFound;

namespace Vtodo.UseCases.Handlers.Projects.Commands.UpdateProject
{
    internal class UpdateProjectRequestHandler : IRequestHandler<UpdateProjectRequest>
    {
        private readonly IDbContext _dbContext;
        private readonly IProjectSecurityService _projectSecurityService;
        private readonly IMediator _mediator;
        
        public UpdateProjectRequestHandler(
            IDbContext dbContext, 
            IProjectSecurityService projectSecurityService,
            IMediator mediator)
        {
            _dbContext = dbContext;
            _projectSecurityService = projectSecurityService;
            _mediator = mediator;
        }
        
        public async Task Handle(UpdateProjectRequest request, CancellationToken cancellationToken)
        {
            var project = await _dbContext.Projects.FindAsync(request.Id, cancellationToken);

            if (project == null)
            {
                await _mediator.Send(new SendErrorToClientRequest() { Error = new ProjectNotFoundError() }, cancellationToken); 
                return;
            }

            _projectSecurityService.CheckAccess(project, ProjectRoles.ProjectUpdate);
            
            var updateDto = request.UpdateProjectDto;
            project.Title = updateDto.Title;
            
            await _dbContext.SaveChangesAsync(cancellationToken);

        }
    }
}