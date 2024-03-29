using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vtodo.Infrastructure.Interfaces.DataAccess;
using MediatR;
using Vtodo.Entities.Enums;
using Vtodo.Entities.Exceptions;
using Vtodo.Infrastructure.Interfaces.Services;
using Vtodo.UseCases.Handlers.Errors.Commands;
using Vtodo.UseCases.Handlers.Errors.Dto.InvalidOperation;
using Vtodo.UseCases.Handlers.Errors.Dto.NotFound;

namespace Vtodo.UseCases.Handlers.ProjectsRoles.Commands.GrantRole
{
    internal class GrantRoleRequestHandler : IRequestHandler<GrantRoleRequest>
    {
        private readonly IDbContext _dbContext;
        private readonly IProjectSecurityService _projectSecurityService;
        private readonly IMediator _mediator;

        public GrantRoleRequestHandler(
            IDbContext dbContext,
            IProjectSecurityService projectSecurityService,
            IMediator mediator)
        {
            _dbContext = dbContext;
            _projectSecurityService = projectSecurityService;
            _mediator = mediator;
        }
        
        public async Task Handle(GrantRoleRequest request, CancellationToken cancellationToken)
        {
            var grantRoleDto = request.GrantRoleDto;
            if (grantRoleDto.Role == ProjectRoles.ProjectMember)
            {
                await _mediator.Send(new SendErrorToClientRequest() { Error = new AttemptAddMemberFromGrantRoleError() }, cancellationToken);
                return;
            }
            
            var project = await _dbContext.Projects.FindAsync(request.ProjectId, cancellationToken);
            if (project == null)
            {
                await _mediator.Send(new SendErrorToClientRequest() { Error = new ProjectNotFoundError() }, cancellationToken);
                return;
            }
            
            _projectSecurityService.CheckAccess(project, ProjectRoles.ProjectAdmin);
            
            var account = await _dbContext.Accounts.FindAsync(grantRoleDto.AccountId);
            if (account == null)
            {
                await _mediator.Send(new SendErrorToClientRequest() { Error = new AccountNotFoundError() }, cancellationToken);
                return;
            }

            _projectSecurityService.GrantRole(project, account, grantRoleDto.Role);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}