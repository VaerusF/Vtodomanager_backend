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

namespace Vtodo.UseCases.Handlers.Tasks.Queries.GetTaskHeaderBackground
{
    internal class GetTaskHeaderBackgroundRequestHandler : IRequestHandler<GetTaskHeaderBackgroundRequest, FileStream?>
    {
        private readonly IDbContext _dbContext;
        private readonly IProjectsFilesService _projectFilesService;
        private readonly IMediator _mediator;
        
        public GetTaskHeaderBackgroundRequestHandler(
            IDbContext dbContext,
            IProjectsFilesService projectFilesService,
            IMediator mediator)
        {
            _dbContext = dbContext;
            _projectFilesService = projectFilesService;
            _mediator = mediator;
        }
        
        public async Task<FileStream?> Handle(GetTaskHeaderBackgroundRequest request, CancellationToken cancellationToken)
        {
            var task = await _dbContext.Tasks
                .Include(x => x.Board)
                .Include(x => x.Board.Project)
                .Include(x => x.ParentTask)
                .AsNoTracking()
                .SingleOrDefaultAsync(p => p.Id == request.Id, cancellationToken: cancellationToken);

            if (task == null)
            {
                await _mediator.Send(new SendErrorToClientRequest() { Error = new TaskNotFoundError() }, cancellationToken); 
                return null;
            }

            if (task.ImageHeaderPath == null) return null;

            var result = _projectFilesService.GetProjectFile(task.Board.Project, task, task.ImageHeaderPath);

            return result;
        }
    }
}