using Vtodo.Entities.Models;
using Vtodo.Infrastructure.Interfaces.DataAccess;
using MediatR;
using Vtodo.DomainServices.Interfaces;
using Vtodo.Infrastructure.Interfaces.Services;

namespace Vtodo.UseCases.Handlers.Projects.Commands.CreateProject
{
    internal class CreateProjectRequestHandler : IRequestHandler<CreateProjectRequest>
    {
        private readonly IDbContext _dbContext;
        private readonly ICurrentAccountService _currentAccountService;
        private readonly IProjectSecurityService _projectSecurityService;
        private readonly IProjectService _projectService;
        
        public CreateProjectRequestHandler(
            IDbContext dbContext, 
            ICurrentAccountService currentAccountService,
            IProjectSecurityService projectSecurityService,
            IProjectService projectService)
        {
            _dbContext = dbContext;
            _currentAccountService = currentAccountService;
            _projectSecurityService = projectSecurityService;
            _projectService = projectService;
        }
        
        public async Task Handle(CreateProjectRequest request, CancellationToken cancellationToken)
        {
            var createDto = request.CreateProjectDto;
            
            var project = _projectService.CreateProject(createDto.Title);
            
            var account = _currentAccountService.GetAccount();
            
            _dbContext.Projects.Add(project);
            await _dbContext.SaveChangesAsync(cancellationToken);
            
            _projectSecurityService.ChangeOwner(project, account);
        }
    }
}