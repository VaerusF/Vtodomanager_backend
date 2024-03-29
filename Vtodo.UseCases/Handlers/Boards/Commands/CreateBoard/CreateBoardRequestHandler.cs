using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Vtodo.Entities.Models;
using Vtodo.Infrastructure.Interfaces.DataAccess;
using MediatR;
using Vtodo.Entities.Enums;
using Vtodo.Entities.Exceptions;
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
        
        public CreateBoardRequestHandler(
            IDbContext dbContext, 
            IProjectSecurityService projectSecurityService,
            IMapper mapper,
            IMediator mediator)
        {
            _dbContext = dbContext;
            _projectSecurityService = projectSecurityService;
            _mapper = mapper;
            _mediator = mediator;
        }
        
        public async Task Handle(CreateBoardRequest request, CancellationToken cancellationToken)
        {
            var createDto = request.CreateBoardDto;
            var board = _mapper.Map<Board>(createDto);

            var project = await _dbContext.Projects.FindAsync(createDto.ProjectId, cancellationToken);
            if (project == null)
            {
                await _mediator.Send(new SendErrorToClientRequest() { Error = new ProjectNotFoundError() }, cancellationToken); 
                return;
            }
            
            _projectSecurityService.CheckAccess(project, ProjectRoles.ProjectUpdate);
            
            board.Project = project;
            
            _dbContext.Boards.Add(board);

            await _dbContext.SaveChangesAsync(cancellationToken);

        }
    }
}