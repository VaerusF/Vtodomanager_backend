using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vtodo.Infrastructure.Interfaces.DataAccess;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vtodo.Entities.Enums;
using Vtodo.Entities.Exceptions;
using Vtodo.Entities.Models;
using Vtodo.Infrastructure.Interfaces.Services;
using Vtodo.UseCases.Handlers.Errors.Commands;
using Vtodo.UseCases.Handlers.Errors.Dto.InvalidOperation;
using Vtodo.UseCases.Handlers.Errors.Dto.NotFound;

namespace Vtodo.UseCases.Handlers.Tasks.Commands.MoveTaskToAnotherBoard
{
    internal class MoveTaskToAnotherBoardRequestHandler : IRequestHandler<MoveTaskToAnotherBoardRequest>
    {
        private readonly IDbContext _dbContext;
        private readonly IProjectSecurityService _projectSecurityService;
        private readonly IMediator _mediator;

        public MoveTaskToAnotherBoardRequestHandler(
            IDbContext dbContext, 
            IProjectSecurityService projectSecurityService,
            IMediator mediator)
        {
            _dbContext = dbContext;
            _projectSecurityService = projectSecurityService;
            _mediator = mediator;
        }
        
        public async Task Handle(MoveTaskToAnotherBoardRequest request, CancellationToken cancellationToken)
        {
            var newBoard = await _dbContext.Boards
                .Include(x => x.Project)
                .FirstOrDefaultAsync(x => x.Id == request.NewBoardId, cancellationToken: cancellationToken);
            if (newBoard == null)
            {
                await _mediator.Send(new SendErrorToClientRequest() { Error = new BoardNotFoundError() }, cancellationToken); 
                return;
            }

            var rootTask = _dbContext.Tasks
                .Include(t => t.Board)
                .Include(t => t.Board.Project)
                .Include(t=> t.ParentTask)
                .Include(t => t.ChildrenTasks)
                .FirstOrDefault(x => x.Id == request.TaskId);
            if (rootTask == null)
            {
                await _mediator.Send(new SendErrorToClientRequest() { Error = new TaskNotFoundError() }, cancellationToken); 
                return;
            }
            
            _projectSecurityService.CheckAccess(rootTask.Board.Project, ProjectRoles.ProjectUpdate);
            _projectSecurityService.CheckAccess(newBoard.Project, ProjectRoles.ProjectUpdate);

            if (rootTask.Board.Id == newBoard.Id)
            {
                await _mediator.Send(new SendErrorToClientRequest() { Error = new NewBoardIdEqualOldIdError() }, cancellationToken); 
                return;
            }

            rootTask.ParentTask = null;

            var tasksList = new List<TaskM> {rootTask};

            var childrenIterationList = await _dbContext.Tasks
                .Where(x => x.ParentTask == rootTask)
                .ToListAsync(cancellationToken: cancellationToken);
            
            while (childrenIterationList.Count > 0 && !cancellationToken.IsCancellationRequested)
            {
                tasksList.AddRange(childrenIterationList);
                
                var childrenIterationIds = childrenIterationList.Select(x => x.Id).ToArray();
                childrenIterationList = await _dbContext.Tasks
                    .Where(x => x.ParentTask != null && childrenIterationIds.Contains(x.ParentTask.Id))
                    .ToListAsync(cancellationToken: cancellationToken);
            }

            foreach (var task in tasksList)
            {
                task.Board = newBoard;
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        
    }
}