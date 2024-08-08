using System.Text.Json;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
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
        private readonly IMapper _mapper;
        private readonly IDistributedCache _distributedCache;
        
        public GetAccountProjectsListRequestHandler(
            IDbContext dbContext, 
            IProjectSecurityService projectSecurityService,
            ICurrentAccountService currentAccountService,
            IMapper mapper,
            IDistributedCache distributedCache)
        {
            _dbContext = dbContext;
            _projectSecurityService = projectSecurityService;
            _currentAccountService = currentAccountService;
            _mapper = mapper;
            _distributedCache = distributedCache;
        }
        
        public async Task<List<ProjectDto>> Handle(GetAccountProjectsListRequest request, CancellationToken cancellationToken)
        {
            var account = _currentAccountService.GetAccount();
            
            var projectsStringFromCache = await _distributedCache.GetStringAsync($"projects_by_account_{account.Id}", cancellationToken);
            
            if (projectsStringFromCache != null) return JsonSerializer.Deserialize<List<ProjectDto>>(projectsStringFromCache) ?? [];
            
            var projects = await _dbContext.ProjectAccountsRoles
                .AsNoTracking()
                .Where(x => x.Account.Id == account.Id)
                .Select(x => x.Project)
                .ToListAsync(cancellationToken);

            var result = _mapper.Map<List<ProjectDto>>(projects);

            for (var i = 0; i < result.Count; i++)
            {
                var project = projects.FirstOrDefault(x=> x.Id == result.ElementAt(i).Id);
                if (project == null) continue;

                result[i].CreationDate = new DateTimeOffset(project.CreationDate).ToUnixTimeMilliseconds();
            }

            await _distributedCache.SetStringAsync($"projects_by_account_{account.Id}", JsonSerializer.Serialize(result), cancellationToken);
            
            return result;
        }
    }
}