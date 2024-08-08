using AutoMapper;
using Vtodo.Entities.Models;
using Vtodo.Infrastructure.Interfaces.DataAccess;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Vtodo.Infrastructure.Interfaces.Services;

namespace Vtodo.UseCases.Handlers.Projects.Commands.CreateProject
{
    internal class CreateProjectRequestHandler : IRequestHandler<CreateProjectRequest>
    {
        private readonly IDbContext _dbContext;
        private readonly ICurrentAccountService _currentAccountService;
        private readonly IProjectSecurityService _projectSecurityService;
        private readonly IMapper _mapper;
        private readonly IDistributedCache _distributedCache;
        
        public CreateProjectRequestHandler(
            IDbContext dbContext, 
            ICurrentAccountService currentAccountService,
            IProjectSecurityService projectSecurityService,
            IMapper mapper,
            IDistributedCache distributedCache)
        {
            _dbContext = dbContext;
            _currentAccountService = currentAccountService;
            _projectSecurityService = projectSecurityService;
            _mapper = mapper;
            _distributedCache = distributedCache;
        }
        
        public async Task Handle(CreateProjectRequest request, CancellationToken cancellationToken)
        {
            var project = _mapper.Map<Project>(request.CreateProjectDto);
            var account = _currentAccountService.GetAccount();
            
            _dbContext.Projects.Add(project);
            await _dbContext.SaveChangesAsync(cancellationToken);

            await _distributedCache.RemoveAsync($"projects_by_account_{account.Id}", cancellationToken);
            
            _projectSecurityService.ChangeOwner(project, account);
        }
    }
}