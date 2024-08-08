using System.Text.Json;
using AutoMapper;
using Vtodo.Infrastructure.Interfaces.DataAccess;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Vtodo.Entities.Enums;
using Vtodo.Infrastructure.Interfaces.Services;
using Vtodo.UseCases.Handlers.Errors.Commands;
using Vtodo.UseCases.Handlers.Errors.Dto.NotFound;
using Vtodo.UseCases.Handlers.Projects.Dto;

namespace Vtodo.UseCases.Handlers.Projects.Queries.GetProject
{
    internal class GetProjectRequestHandler : IRequestHandler<GetProjectRequest, ProjectDto?>
    {
        private readonly IDbContext _dbContext;
        private readonly IProjectSecurityService _projectSecurityService;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly IDistributedCache _distributedCache;
        
        public GetProjectRequestHandler(
            IDbContext dbContext, 
            IProjectSecurityService projectSecurityService,
            IMapper mapper,
            IMediator mediator,
            IDistributedCache distributedCache)
        {
            _dbContext = dbContext;
            _projectSecurityService = projectSecurityService;
            _mapper = mapper;
            _mediator = mediator;
            _distributedCache = distributedCache;
        }
        
        public async Task<ProjectDto?> Handle(GetProjectRequest request, CancellationToken cancellationToken)
        {
            _projectSecurityService.CheckAccess(request.Id, ProjectRoles.ProjectMember);
            
            var projectStringFromCache = await _distributedCache.GetStringAsync($"project_{request.Id}", cancellationToken);

            if (projectStringFromCache != null) return JsonSerializer.Deserialize<ProjectDto>(projectStringFromCache);
            
            var project = await _dbContext.Projects.AsNoTracking()
                .SingleOrDefaultAsync(p => p.Id == request.Id, cancellationToken: cancellationToken);

            if (project == null)
            {
                await _mediator.Send(new SendErrorToClientRequest() { Error = new ProjectNotFoundError() }, cancellationToken); 
                return null;
            }
            
            var result = _mapper.Map<ProjectDto>(project);
            result.CreationDate = new DateTimeOffset(project.CreationDate).ToUnixTimeMilliseconds();

            await _distributedCache.SetStringAsync($"project_{request.Id}", JsonSerializer.Serialize(result), cancellationToken);

            return result;
        }
    }
}