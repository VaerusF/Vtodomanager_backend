using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Vtodo.DomainServices.Interfaces;
using Vtodo.Entities.Enums;
using Vtodo.Infrastructure.Interfaces.DataAccess;
using Vtodo.Infrastructure.Interfaces.Services;
using Vtodo.UseCases.Handlers.Errors.Commands;
using Vtodo.UseCases.Handlers.Errors.Dto.NotFound;

namespace Vtodo.UseCases.Handlers.Boards.Commands.UploadBoardHeaderBackground
{
    internal class UploadBoardHeaderBackgroundRequestHandler : IRequestHandler<UploadBoardHeaderBackgroundRequest>
    {
        private readonly IDbContext _dbContext;
        private readonly IProjectSecurityService _projectSecurityService;
        private readonly IProjectsFilesService _projectFilesService;
        private readonly IBoardService _boardService;
        private readonly IMediator _mediator;
        private readonly IDistributedCache _distributedCache;
        
        public UploadBoardHeaderBackgroundRequestHandler(
            IDbContext dbContext, 
            IProjectSecurityService projectSecurityService,
            IProjectsFilesService projectFilesService,
            IBoardService boardService,
            IMediator mediator,
            IDistributedCache distributedCache)
        {
            _dbContext = dbContext;
            _projectSecurityService = projectSecurityService;
            _projectFilesService = projectFilesService;
            _boardService = boardService;
            _mediator = mediator;
            _distributedCache = distributedCache;
        }
        
        public async Task Handle(UploadBoardHeaderBackgroundRequest request, CancellationToken cancellationToken)
        {
            _projectSecurityService.CheckAccess(request.ProjectId, ProjectRoles.ProjectUpdate);
            
            var streamFile = request.BackgroundImage;
            var fileName = request.FileName;
            
            var board = await _dbContext.Boards
                .Include(t => t.Project)
                .FirstOrDefaultAsync(x => x.Id == request.BoardId, cancellationToken);
            
            if (board == null)
            {
                await _mediator.Send(new SendErrorToClientRequest() { Error = new BoardNotFoundError() }, cancellationToken); 
                return;
            }

            if (!string.IsNullOrWhiteSpace(board.ImageHeaderPath)) _projectFilesService.DeleteProjectFile(board.Project, board, board.ImageHeaderPath);
            
            var extension = _projectFilesService.CheckFile(streamFile, fileName, new string[] {".jpg", ".png"});

            var savedFileName = _projectFilesService.UploadProjectFile(board.Project, board, streamFile, extension);
            
            _boardService.UpdateImageHeaderPath(board, savedFileName);
            
            await _dbContext.SaveChangesAsync(cancellationToken);
            
            await _distributedCache.RemoveAsync($"board_{request.BoardId}", cancellationToken);
            await _distributedCache.RemoveAsync($"boards_by_project_{request.ProjectId}", cancellationToken);
        }
    }
}