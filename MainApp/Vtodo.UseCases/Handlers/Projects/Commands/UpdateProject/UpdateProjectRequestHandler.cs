using Vtodo.Infrastructure.Interfaces.DataAccess;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Vtodo.DomainServices.Interfaces;
using Vtodo.Entities.Enums;
using Vtodo.Infrastructure.Interfaces.Services;
using Vtodo.UseCases.Handlers.Errors.Commands;
using Vtodo.UseCases.Handlers.Errors.Dto.NotFound;

namespace Vtodo.UseCases.Handlers.Projects.Commands.UpdateProject
{
    internal class UpdateProjectRequestHandler : IRequestHandler<UpdateProjectRequest>
    {
        private readonly IDbContext _dbContext;
        private readonly IProjectSecurityService _projectSecurityService;
        private readonly IProjectService _projectService;
        private readonly IMediator _mediator;
        
        public UpdateProjectRequestHandler(
            IDbContext dbContext, 
            IProjectSecurityService projectSecurityService,
            IProjectService projectService,
            IMediator mediator)
        {
            _dbContext = dbContext;
            _projectSecurityService = projectSecurityService;
            _projectService = projectService;
            _mediator = mediator;
        }
        
        public async Task Handle(UpdateProjectRequest request, CancellationToken cancellationToken)
        {
            _projectSecurityService.CheckAccess(request.Id, ProjectRoles.ProjectUpdate);
            
            var updateDto = request.UpdateProjectDto;
            
            var project = await _dbContext.Projects.FindAsync(request.Id, cancellationToken);

            if (project == null)
            {
                await _mediator.Send(new SendErrorToClientRequest() { Error = new ProjectNotFoundError() }, cancellationToken); 
                return;
            }
            
            _projectService.UpdateProject(project, updateDto.Title);
            
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}