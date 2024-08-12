using AutoMapper;
using Vtodo.Entities.Models;
using Vtodo.Infrastructure.Interfaces.DataAccess;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Vtodo.Entities.Enums;
using Vtodo.Infrastructure.Interfaces.Services;
using Vtodo.UseCases.Handlers.Errors.Commands;
using Vtodo.UseCases.Handlers.Errors.Dto.NotFound;

namespace Vtodo.UseCases.Handlers.Boards.Commands.CreateBoard
{
    internal class CreateBoardRequestHandler : IRequestHandler<CreateBoardRequest>
    {
        private readonly IDbContext _dbContext;
        private readonly IProjectSecurityService _projectSecurityService;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly IDistributedCache _distributedCache;
        
        public CreateBoardRequestHandler(
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
        
        public async Task Handle(CreateBoardRequest request, CancellationToken cancellationToken)
        {
            _projectSecurityService.CheckAccess(request.ProjectId, ProjectRoles.ProjectUpdate);
            
            var createDto = request.CreateBoardDto;
            var board = _mapper.Map<Board>(createDto);

            var project = await _dbContext.Projects.FindAsync(request.ProjectId, cancellationToken);
            if (project == null)
            {
                await _mediator.Send(new SendErrorToClientRequest() { Error = new ProjectNotFoundError() }, cancellationToken); 
                return;
            }
            
            board.Project = project;
            
            _dbContext.Boards.Add(board);

            await _dbContext.SaveChangesAsync(cancellationToken);
            
            await _distributedCache.RemoveAsync($"boards_by_project_{request.ProjectId}", cancellationToken);
        }
    }
}