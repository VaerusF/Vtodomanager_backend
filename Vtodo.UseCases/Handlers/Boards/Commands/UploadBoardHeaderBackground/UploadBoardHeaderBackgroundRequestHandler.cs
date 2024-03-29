using System;
using System.IO;
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
using Vtodo.UseCases.Handlers.Projects.Dto;

namespace Vtodo.UseCases.Handlers.Boards.Commands.UploadBoardHeaderBackground
{
    internal class UploadBoardHeaderBackgroundRequestHandler : IRequestHandler<UploadBoardHeaderBackgroundRequest>
    {
        private readonly IDbContext _dbContext;
        private readonly IProjectSecurityService _projectSecurityService;
        private readonly IProjectsFilesService _projectFilesService;
        private readonly IMediator _mediator;
        
        public UploadBoardHeaderBackgroundRequestHandler(
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
        
        public async Task Handle(UploadBoardHeaderBackgroundRequest request, CancellationToken cancellationToken)
        {
            var streamFile = request.BackgroundImage;
            var fileName = request.FileName;
            
            var board = await _dbContext.Boards
                .Include(t => t.Project)
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
            if (board == null)
            {
                await _mediator.Send(new SendErrorToClientRequest() { Error = new BoardNotFoundError() }, cancellationToken); 
                return;
            }

            _projectSecurityService.CheckAccess(board.Project, ProjectRoles.ProjectUpdate);

            if (!string.IsNullOrWhiteSpace(board.ImageHeaderPath)) _projectFilesService.DeleteProjectFile(board.Project, board, board.ImageHeaderPath);
            
            var extension = _projectFilesService.CheckFile(streamFile, fileName, new string[] {".jpg", ".png"});

            var savedFileName = _projectFilesService.UploadProjectFile(board.Project, board, streamFile, extension);
            board.ImageHeaderPath = savedFileName;
            
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}