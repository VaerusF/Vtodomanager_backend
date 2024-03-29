using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vtodo.Entities.Enums;
using Vtodo.Entities.Exceptions;
using Vtodo.Infrastructure.Interfaces.DataAccess;
using Vtodo.Infrastructure.Interfaces.Services;
using Vtodo.UseCases.Handlers.Boards.Dto;
using Vtodo.UseCases.Handlers.Errors.Commands;
using Vtodo.UseCases.Handlers.Errors.Dto.NotFound;

namespace Vtodo.UseCases.Handlers.Boards.Queries.GetBoard
{
    internal class GetBoardRequestHandler : IRequestHandler<GetBoardRequest, BoardDto?>
    {
        private readonly IDbContext _dbContext;
        private readonly IProjectSecurityService _projectSecurityService;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        
        public GetBoardRequestHandler(
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
        
        public async Task<BoardDto?> Handle(GetBoardRequest request, CancellationToken cancellationToken)
        {
            var board = await _dbContext.Boards
                .Include(x => x.Project)
                .AsNoTracking()
                .SingleOrDefaultAsync(p => p.Id == request.Id, cancellationToken: cancellationToken);

            if (board == null)
            {
                await _mediator.Send(new SendErrorToClientRequest() { Error = new BoardNotFoundError() }, cancellationToken); 
                return null;
            }


            _projectSecurityService.CheckAccess(board.Project, ProjectRoles.ProjectMember);
            
            var result = _mapper.Map<BoardDto>(board);

            result.ProjectId = board.Project.Id;
            return result;
        }
    }
}