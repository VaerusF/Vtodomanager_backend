using Vtodo.Infrastructure.Interfaces.DataAccess;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vtodo.DomainServices.Interfaces;
using Vtodo.Entities.Enums;
using Vtodo.Infrastructure.Interfaces.Services;
using Vtodo.UseCases.Handlers.Errors.Commands;
using Vtodo.UseCases.Handlers.Errors.Dto.NotFound;

namespace Vtodo.UseCases.Handlers.Boards.Commands.UpdateBoard
{
    internal class UpdateBoardRequestHandler : IRequestHandler<UpdateBoardRequest>
    {
        private readonly IDbContext _dbContext;
        private readonly IProjectSecurityService _projectSecurityService;
        private readonly IBoardService _boardService;
        private readonly IMediator _mediator;
        
        public UpdateBoardRequestHandler(
            IDbContext dbContext, 
            IProjectSecurityService projectSecurityService,
            IBoardService boardService,
            IMediator mediator)
        {
            _dbContext = dbContext;
            _projectSecurityService = projectSecurityService;
            _boardService = boardService;
            _mediator = mediator;
        }
        
        public async Task Handle(UpdateBoardRequest request, CancellationToken cancellationToken)
        {
            var updateBoardDto = request.UpdateBoardDto;

            var board = await _dbContext.Boards
                .Include(t => t.Project)
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
            if (board == null)
            {
                await _mediator.Send(new SendErrorToClientRequest() { Error = new BoardNotFoundError() }, cancellationToken); 
                return;
            }

            _projectSecurityService.CheckAccess(board.Project, ProjectRoles.ProjectUpdate);
            
            _boardService.UpdateBoard(board, updateBoardDto.Title);
            _boardService.UpdateBoardPrioritySort(board, updateBoardDto.PrioritySort);
            
            await _dbContext.SaveChangesAsync(cancellationToken);

        }
    }
}