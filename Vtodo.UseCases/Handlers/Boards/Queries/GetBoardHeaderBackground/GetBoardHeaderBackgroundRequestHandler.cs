using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vtodo.Entities.Exceptions;
using Vtodo.Infrastructure.Interfaces.DataAccess;
using Vtodo.Infrastructure.Interfaces.Services;
using Vtodo.UseCases.Handlers.Errors.Commands;
using Vtodo.UseCases.Handlers.Errors.Dto.NotFound;

namespace Vtodo.UseCases.Handlers.Boards.Queries.GetBoardHeaderBackground
{
    internal class GetBoardHeaderBackgroundRequestHandler : IRequestHandler<GetBoardHeaderBackgroundRequest, FileStream?>
    {
        private readonly IDbContext _dbContext;
        private readonly IProjectsFilesService _projectFilesService;
        private readonly IMediator _mediator;
        
        public GetBoardHeaderBackgroundRequestHandler(
            IDbContext dbContext,
            IProjectsFilesService projectFilesService,
            IMediator mediator)
        {
            _dbContext = dbContext;
            _projectFilesService = projectFilesService;
            _mediator = mediator;
        }
        
        public async Task<FileStream?> Handle(GetBoardHeaderBackgroundRequest request, CancellationToken cancellationToken)
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

            if (board.ImageHeaderPath == null) return null;

            var result = _projectFilesService.GetProjectFile(board.Project, board, board.ImageHeaderPath);

            return result;
        }
    }
}