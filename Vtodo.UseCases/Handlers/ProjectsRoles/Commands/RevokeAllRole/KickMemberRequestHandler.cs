using System.Threading;
using System.Threading.Tasks;
using Vtodo.Infrastructure.Interfaces.DataAccess;
using MediatR;
using Vtodo.Entities.Enums;
using Vtodo.Entities.Exceptions;
using Vtodo.Infrastructure.Interfaces.Services;
using Vtodo.UseCases.Handlers.Errors.Commands;
using Vtodo.UseCases.Handlers.Errors.Dto.NotFound;

namespace Vtodo.UseCases.Handlers.ProjectsRoles.Commands.RevokeAllRole
{
    internal class KickMemberRequestHandler : IRequestHandler<KickMemberRequest>
    {
        private readonly IDbContext _dbContext;
        private readonly IProjectSecurityService _projectSecurityService;
        private readonly IMediator _mediator;

        public KickMemberRequestHandler(
            IDbContext dbContext,
            IProjectSecurityService projectSecurityService,
            IMediator mediator)
        {
            _dbContext = dbContext;
            _projectSecurityService = projectSecurityService;
            _mediator = mediator;
        }
        
        public async Task Handle(KickMemberRequest request, CancellationToken cancellationToken)
        {
            var kickMemberDto = request.KickMemberDto;

            var project = await _dbContext.Projects.FindAsync(request.ProjectId, cancellationToken);
            if (project == null)
            {
                await _mediator.Send(new SendErrorToClientRequest() { Error = new ProjectNotFoundError() }, cancellationToken);
                return;
            }
            
            _projectSecurityService.CheckAccess(project, ProjectRoles.ProjectAdmin);
            
            var account = await _dbContext.Accounts.FindAsync(kickMemberDto.AccountId);
            if (account == null)
            {
                await _mediator.Send(new SendErrorToClientRequest() { Error = new AccountNotFoundError() }, cancellationToken);
                return;
            }

            _projectSecurityService.RevokeAllRoles(project, account);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}