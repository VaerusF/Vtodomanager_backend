using Vtodo.Infrastructure.Interfaces.DataAccess;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Vtodo.Entities.Enums;
using Vtodo.Entities.Models;
using Vtodo.Infrastructure.Interfaces.Services;
using Vtodo.UseCases.Handlers.Errors.Commands;
using Vtodo.UseCases.Handlers.Errors.Dto.NotFound;
using Vtodo.UseCases.Handlers.Logs.Commands.SendLogToLogger;

namespace Vtodo.UseCases.Handlers.Projects.Commands.DeleteProject
{
    internal class DeleteProjectRequestHandler : IRequestHandler<DeleteProjectRequest>
    {
        private readonly IDbContext _dbContext;
        private readonly IProjectSecurityService _projectSecurityService;
        private readonly IMediator _mediator;
        private readonly IDistributedCache _distributedCache;
        private readonly ICurrentAccountService _currentAccountService;
        
        public DeleteProjectRequestHandler(
            IDbContext dbContext, 
            IProjectSecurityService projectSecurityService,
            IMediator mediator,
            IDistributedCache distributedCache,
            ICurrentAccountService currentAccountService)
        {
            _dbContext = dbContext;
            _projectSecurityService = projectSecurityService;
            _mediator = mediator;
            _distributedCache = distributedCache;
            _currentAccountService = currentAccountService;
        }
        
        public async Task Handle(DeleteProjectRequest request, CancellationToken cancellationToken)
        {
            _projectSecurityService.CheckAccess(request.Id, ProjectRoles.ProjectOwner);
            
            var project = await _dbContext.Projects.FindAsync(request.Id,cancellationToken);

            if (project == null)
            {
                await _mediator.Send(new SendErrorToClientRequest() { Error = new ProjectNotFoundError() }, cancellationToken); 
                return;
            }
            
            _dbContext.Projects.Remove(project);

            await _dbContext.SaveChangesAsync(cancellationToken);

            var account = _currentAccountService.GetAccount();
            
            await _distributedCache.RemoveAsync($"projects_by_account_{account.Id}", cancellationToken);
            await _distributedCache.RemoveAsync($"project_{request.Id}", cancellationToken);
            
            await _mediator.Send(new SendLogToLoggerRequest() { Log = new Log()
                    {
                        ServiceName = "MainApp",
                        LogLevel = CustomLogLevels.Information, 
                        Message = $"Project {project.Id} \"{project.Title}\" has been deleted",
                        DateTime = DateTime.UtcNow
                    }
                }, cancellationToken
            );
        }
    }
}