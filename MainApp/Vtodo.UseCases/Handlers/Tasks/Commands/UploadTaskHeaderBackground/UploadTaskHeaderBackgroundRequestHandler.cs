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

namespace Vtodo.UseCases.Handlers.Tasks.Commands.UploadTaskHeaderBackground
{
    internal class UploadTaskHeaderBackgroundRequestHandler : IRequestHandler<UploadTaskHeaderBackgroundRequest>
    {
        private readonly IDbContext _dbContext;
        private readonly IProjectSecurityService _projectSecurityService;
        private readonly IProjectsFilesService _projectFilesService;
        private readonly IMediator _mediator;
        
        public UploadTaskHeaderBackgroundRequestHandler(
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
        
        public async Task Handle(UploadTaskHeaderBackgroundRequest request, CancellationToken cancellationToken)
        {
            var streamFile = request.BackgroundImage;
            var fileName = request.FileName;
            
            var task = await _dbContext.Tasks
                .Include(t => t.Board)
                .Include(t => t.Board.Project)
                .Include(t => t.ParentTask)
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
            if (task == null)
            {
                await _mediator.Send(new SendErrorToClientRequest() { Error = new TaskNotFoundError() }, cancellationToken); 
                return;
            }

            _projectSecurityService.CheckAccess(task.Board.Project, ProjectRoles.ProjectUpdate);

            if (!string.IsNullOrWhiteSpace(task.ImageHeaderPath)) _projectFilesService.DeleteProjectFile(task.Board.Project, task, task.ImageHeaderPath);
            
            var extension = _projectFilesService.CheckFile(streamFile, fileName, new string[] {".jpg", ".png"});

            var savedFileName = _projectFilesService.UploadProjectFile(task.Board.Project, task, streamFile, extension);
            task.ImageHeaderPath = savedFileName;
            
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}