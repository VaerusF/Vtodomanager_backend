using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Vtodo.Entities.Models;
using Vtodo.Infrastructure.Interfaces.DataAccess;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vtodo.Entities.Enums;
using Vtodo.Entities.Exceptions;
using Vtodo.Infrastructure.Interfaces.Services;
using Vtodo.UseCases.Handlers.Errors.Commands;
using Vtodo.UseCases.Handlers.Errors.Dto.NotFound;

namespace Vtodo.UseCases.Handlers.Tasks.Commands.UpdateTask
{
    internal class UpdateTaskRequestHandler : IRequestHandler<UpdateTaskRequest>
    {
        private readonly IDbContext _dbContext;
        private readonly IProjectSecurityService _projectSecurityService;
        private readonly IMediator _mediator;
        
        public UpdateTaskRequestHandler(
            IDbContext dbContext, 
            IProjectSecurityService projectSecurityService,
            IMediator mediator)
        {
            _dbContext = dbContext;
            _projectSecurityService = projectSecurityService;
            _mediator = mediator;
        }
        
        public async Task Handle(UpdateTaskRequest request, CancellationToken cancellationToken)
        {
            var updateBoardDto = request.UpdateTaskDto;

            var task = await _dbContext.Tasks
                .Include(x => x.Board)
                .Include(x => x.Board.Project)
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken: cancellationToken);
            if (task == null)
            {
                await _mediator.Send(new SendErrorToClientRequest() { Error = new TaskNotFoundError() }, cancellationToken); 
                return;
            }

            _projectSecurityService.CheckAccess(task.Board.Project, ProjectRoles.ProjectUpdate);
            
            task.Title = updateBoardDto.Title;
            task.Description = updateBoardDto.Description;
            task.IsCompleted = updateBoardDto.IsCompleted;
            if (updateBoardDto.EndDateTimeStamp == null)
            {
                task.EndDate = null;
            }
            else
            {
                task.EndDate = DateTimeOffset.FromUnixTimeSeconds((int) updateBoardDto.EndDateTimeStamp).DateTime;
            }

            task.Priority = updateBoardDto.Priority;
            task.PrioritySort = updateBoardDto.PrioritySort;
            
            await _dbContext.SaveChangesAsync(cancellationToken);

        }
    }
}