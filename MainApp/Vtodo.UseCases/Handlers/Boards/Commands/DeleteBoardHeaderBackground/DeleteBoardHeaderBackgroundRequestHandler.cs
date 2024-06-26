using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vtodo.Entities.Enums;
using Vtodo.Entities.Exceptions;
using Vtodo.Infrastructure.Interfaces.DataAccess;
using Vtodo.Infrastructure.Interfaces.Services;
using Vtodo.UseCases.Handlers.Errors.Commands;
using Vtodo.UseCases.Handlers.Errors.Dto.NotFound;

namespace Vtodo.UseCases.Handlers.Boards.Commands.DeleteBoardHeaderBackground
{
    internal class DeleteBoardHeaderBackgroundRequestHandler : IRequestHandler<DeleteBoardHeaderBackgroundRequest>
    {
        private readonly IDbContext _dbContext;
        private readonly IProjectSecurityService _projectSecurityService;
        private readonly IProjectsFilesService _projectFilesService;
        private readonly IMediator _mediator;
        
        public DeleteBoardHeaderBackgroundRequestHandler(
            IDbContext dbContext, 
            IProjectSecurityService projectSecurityService,
            IProjectsFilesService projectFilesService,
            IMediator mediator)
        {
            _dbContext = dbContext;
            _projectSecurityService = projectSecurityService;
            _projectFilesService = projectFilesService;
            _mediator = mediator;
        }
        
        public async Task Handle(DeleteBoardHeaderBackgroundRequest request, CancellationToken cancellationToken)
        {
            var board = await _dbContext.Boards
                .Include(t => t.Project)
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
            if (board == null)
            {
                await _mediator.Send(new SendErrorToClientRequest() { Error = new BoardNotFoundError() }, cancellationToken); 
                return;
            }

            _projectSecurityService.CheckAccess(board.Project, ProjectRoles.ProjectUpdate);

            if (string.IsNullOrWhiteSpace(board.ImageHeaderPath)) return;
            _projectFilesService.DeleteProjectFile(board.Project, board, board.ImageHeaderPath);

            board.ImageHeaderPath = null;
            
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}