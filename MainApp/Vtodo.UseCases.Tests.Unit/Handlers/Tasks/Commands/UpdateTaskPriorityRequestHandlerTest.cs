using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Moq;
using Vtodo.DataAccess.Postgres;
using Vtodo.DomainServices.Interfaces;
using Vtodo.Entities.Enums;
using Vtodo.Entities.Models;
using Vtodo.Infrastructure.Interfaces.Services;
using Vtodo.Tests.Utils;
using Vtodo.UseCases.Handlers.Errors.Commands;
using Vtodo.UseCases.Handlers.Errors.Dto.NotFound;
using Vtodo.UseCases.Handlers.Tasks.Commands.UpdateTask;
using Vtodo.UseCases.Handlers.Tasks.Commands.UpdateTaskComplete;
using Vtodo.UseCases.Handlers.Tasks.Commands.UpdateTaskPriority;
using Vtodo.UseCases.Handlers.Tasks.Dto;
using Xunit;

namespace Vtodo.UseCases.Tests.Unit.Handlers.Tasks.Commands
{
    public class UpdateTaskPriorityRequestHandlerTest
    {
        private AppDbContext _dbContext = null!;
        private IDistributedCache? _distributedCache = null!;

        [Fact]
        public async void Handle_SuccessfulUpdateTaskPriority_ReturnsSystemTask()
        {
            SetupDbContext();
            SetupDistributedCache();
            
            var projectSecurityServiceMock = SetupProjectSecurityServiceMock();
            
            var updateTaskPriorityDto = new UpdateTaskPriorityDto()
            {
                Priority = TaskPriority.Priority2
            };
            
            var mockTaskService = SetupMockTaskService();
            mockTaskService.Setup(x => x.UpdateTaskPriority(
                It.IsAny<TaskM>(),  
                It.IsAny<TaskPriority>()
            )
            ).Callback((TaskM task, TaskPriority priority) =>
            {
                task.Priority = priority;
            });
            
            var request = new UpdateTaskPriorityRequest() { ProjectId = 1, BoardId = 1, TaskId = 1, UpdateTaskPriorityDto = updateTaskPriorityDto };
            
            var listDto = new List<TaskDto>()
            {
                new TaskDto
                {
                    Id = _dbContext.Tasks.First(x => x.Id == 1).Id,
                    Title = _dbContext.Tasks.First(x => x.Id == 1).Title,
                    Description = _dbContext.Tasks.First(x => x.Id == 1).Description,
                    EndDate = _dbContext.Tasks.First(x => x.Id == 1).EndDate == null
                        ? null
                        : new DateTimeOffset((DateTime)_dbContext.Tasks.First(x => x.Id == 1).EndDate!).ToUnixTimeMilliseconds(),
                    IsCompleted = _dbContext.Tasks.First(x => x.Id == 1).IsCompleted,
                    BoardId = _dbContext.Tasks.Include(taskM => taskM.Board).First(x => x.Id == 1).Board.Id,
                    ParentId = _dbContext.Tasks.Include(taskM => taskM.ParentTask).First(x => x.Id == 1).ParentTask?.Id,
                    PrioritySort = _dbContext.Tasks.First(x => x.Id == 1).PrioritySort,
                    Priority = (int)_dbContext.Tasks.First(x => x.Id == 1).Priority,
                    ImageHeaderPath = _dbContext.Tasks.First(x => x.Id == 1).ImageHeaderPath
                }
            };
            
            await _distributedCache!.SetStringAsync($"task_{request.TaskId}", JsonSerializer.Serialize(listDto[0]));
            await _distributedCache!.SetStringAsync($"tasks_by_board_{request.BoardId}", JsonSerializer.Serialize(listDto));
            
            var updateTaskPriorityRequestHandler = new UpdateTaskPriorityRequestHandler(
                _dbContext, 
                projectSecurityServiceMock.Object,
                mockTaskService.Object,
                SetupMockMediatorService().Object,
                _distributedCache!
            );

            await updateTaskPriorityRequestHandler.Handle(request, CancellationToken.None);

            projectSecurityServiceMock.Verify(x => x.CheckAccess(It.IsIn(request.ProjectId), ProjectRoles.ProjectUpdate), Times.Once);
            
            mockTaskService.Verify(x => x.UpdateTaskPriority(
                It.IsAny<TaskM>(),  
                It.IsAny<TaskPriority>()
            ));
            
            Assert.Null(_dbContext.Tasks.FirstOrDefault(x => x.Id == 1 && x.Priority == TaskPriority.None ));
            
            Assert.NotNull(_dbContext.Tasks.FirstOrDefault(x => x.Id == 1 && x.Priority == updateTaskPriorityDto.Priority));
            
            Assert.Null(await _distributedCache!.GetStringAsync($"task_{request.TaskId}"));
            Assert.Null(await _distributedCache!.GetStringAsync($"tasks_by_board_{request.BoardId}"));
            
            CleanUp();
        }
        
        [Fact]
        public async void Handle_TaskNotFound_SendTaskNotFoundError()
        {
            SetupDbContext();
            SetupDistributedCache();
            
            var projectSecurityServiceMock = SetupProjectSecurityServiceMock();
            
            var updateTaskPriorityDto = new UpdateTaskPriorityDto()
            {
                Priority = TaskPriority.Priority2
            };
            
            var request = new UpdateTaskPriorityRequest() { ProjectId = 1, BoardId = 1, TaskId = 100, UpdateTaskPriorityDto = updateTaskPriorityDto };
            
            var mediatorMock = SetupMockMediatorService();
            var error = new TaskNotFoundError();
            
            var updateTaskPriorityRequestHandler = new UpdateTaskPriorityRequestHandler(
                _dbContext, 
                projectSecurityServiceMock.Object, 
                SetupMockTaskService().Object,
                mediatorMock.Object,
                _distributedCache!
            );
            
            await updateTaskPriorityRequestHandler.Handle(request, CancellationToken.None);
            
            projectSecurityServiceMock.Verify(x => x.CheckAccess(It.IsIn(request.ProjectId), ProjectRoles.ProjectUpdate), Times.Once);
            
            mediatorMock.Verify(x => x.Send(It.Is<SendErrorToClientRequest>(y => 
                        y.Error.GetType() == error.GetType()), 
                    It.IsAny<CancellationToken>()), Times.Once, $"Error request type is not a { error.GetType() }");

            CleanUp();
        }
        
        private void SetupDistributedCache()
        {
            _distributedCache = TestDbUtils.SetupTestCacheInMemory();
        }
        
        private static Mock<IMediator> SetupMockMediatorService()
        {
            var mock = new Mock<IMediator>();
            
            return mock;
        }
        
        private static Mock<ITaskService> SetupMockTaskService()
        {
            var mock = new Mock<ITaskService>();
            
            return mock;
        }
        
        private static Mock<IProjectSecurityService> SetupProjectSecurityServiceMock()
        {
            var mock = new Mock<IProjectSecurityService>();
            mock.Setup(x => x.CheckAccess(It.IsAny<Project>(), It.IsAny<ProjectRoles>())).Verifiable();
            
            return mock;
        }
        
        private void SetupDbContext()
        {
            _dbContext = TestDbUtils.SetupTestDbContextInMemory();

            _dbContext.Accounts.Add(new Account() { Email = "test@test.ru", Username = "test", HashedPassword = "test" , Salt = new byte[64]});
            
            _dbContext.Projects.Add(new Project() {Title = "Test Project"});
            _dbContext.SaveChanges();
            
            _dbContext.Boards.Add(new Board() {Title = "Test Board 1", Project = _dbContext.Projects.First(), PrioritySort = 0});
            _dbContext.Boards.Add(new Board() {Title = "Test Board 2", Project = _dbContext.Projects.First(), PrioritySort = 0});
            _dbContext.SaveChanges();
            
            _dbContext.Tasks.Add(new TaskM() { 
                Title = "Test update task",
                Description = "Test update task",
                Priority = TaskPriority.None,
                PrioritySort = 0,
                Board = _dbContext.Boards.First(x => x.Id == 1),
                IsCompleted = false
            });
            _dbContext.SaveChanges();
        }
        
        private void CleanUp()
        {
            _dbContext?.Database.EnsureDeleted();
            _dbContext?.Dispose();
            
            _distributedCache = null!;
        }
    }
}