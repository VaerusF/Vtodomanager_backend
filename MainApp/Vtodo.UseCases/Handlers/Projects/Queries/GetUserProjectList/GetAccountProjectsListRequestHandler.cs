using MediatR;
using Microsoft.EntityFrameworkCore;
using Vtodo.Infrastructure.Interfaces.DataAccess;
using Vtodo.Infrastructure.Interfaces.Services;
using Vtodo.UseCases.Handlers.Projects.Dto;

namespace Vtodo.UseCases.Handlers.Projects.Queries.GetUserProjectList
{
    internal class GetAccountProjectsListRequestHandler : IRequestHandler<GetAccountProjectsListRequest, List<ProjectDto>>
    {
        private readonly IDbContext _dbContext;
        private readonly IProjectSecurityService _projectSecurityService;
        private readonly ICurrentAccountService _currentAccountService;
        
        public GetAccountProjectsListRequestHandler(
            IDbContext dbContext, 
            IProjectSecurityService projectSecurityService,
            ICurrentAccountService currentAccountService)
        {
            _dbContext = dbContext;
            _projectSecurityService = projectSecurityService;
            _currentAccountService = currentAccountService;
        }
        
        public async Task<List<ProjectDto>> Handle(GetAccountProjectsListRequest request, CancellationToken cancellationToken)
        {
            var account = _currentAccountService.GetAccount();
            
            var projects = await _dbContext.ProjectAccountsRoles
                .AsNoTracking()
                .Where(x => x.Account.Id == account.Id)
                .Select(x => x.Project)
                .ToListAsync(cancellationToken);

            var result = projects.Select(project => new ProjectDto()
                {
                    Id = project.Id,
                    Title = project.Title,
                    CreationDate = new DateTimeOffset(project.CreationDate).ToUnixTimeMilliseconds()
                })
            .ToList();
            
            return result;
        }
    }
}