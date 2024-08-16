using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Moq;
using Vtodo.DataAccess.Postgres;
using Vtodo.Entities.Enums;
using Vtodo.Entities.Models;
using Vtodo.Infrastructure.Interfaces.Services;
using Vtodo.Tests.Utils;
using Vtodo.UseCases.Handlers.Errors.Commands;
using Vtodo.UseCases.Handlers.Errors.Dto.NotFound;
using Vtodo.UseCases.Handlers.Tasks.Commands.DeleteTask;
using Vtodo.UseCases.Handlers.Tasks.Dto;
using Xunit;

namespace Vtodo.UseCases.Tests.Unit.Handlers.Tasks.Commands
{
    public class DeleteTaskRequestHandlerTest
    {
        private AppDbContext _dbContext = null!;
        private IDistributedCache? _distributedCache = null!;

        [Fact]
        private async void Handle_SuccessfulDeleteTask_ReturnsSystemTask()
        {
            SetupDbContext();
            SetupDistributedCache();
            
            var request = new DeleteTaskRequest() {ProjectId = 1, BoardId = 1, TaskId = 1};

            var projectSecurityServiceMock = SetupProjectSecurityServiceMock();
            
            var listDto = new List<TaskDto>()
            {
                new TaskDto
                {
                    Id = _dbContext.Tasks.First(x => x.Id == 1).Id,
                    Title = _dbContext.Tasks.First(x => x.Id == 1).Title,
                    Description = _dbContext.Tasks.First(x => x.Id == 1).Description,
                    EndDate = _dbContext.Tasks.First(x => x.Id == 1).EndDate == null
                        ? -1
                        : new DateTimeOffset((DateTime)_dbContext.Tasks.First(x => x.Id == 1).EndDate!).ToUnixTimeMilliseconds(),
                    IsCompleted = _dbContext.Tasks.First(x => x.Id == 1).IsCompleted,
                    BoardId = _dbContext.Tasks.Include(taskM => taskM.Board).First(x => x.Id == 1).Board.Id,
                    ParentId = _dbContext.Tasks.Include(taskM => taskM.ParentTask).First(x => x.Id == 1).ParentTask?.Id,
                    PrioritySort = _dbContext.Tasks.First(x => x.Id == 1).PrioritySort,
                    Priority = (int)_dbContext.Tasks.First(x => x.Id == 1).Priority,
                    ImageHeaderPath = _dbContext.Tasks.First(x => x.Id == 1).ImageHeaderPath
                },
                new TaskDto
                {
                    Id = _dbContext.Tasks.First(x => x.Id == 2).Id,
                    Title = _dbContext.Tasks.First(x => x.Id == 2).Title,
                    Description = _dbContext.Tasks.First(x => x.Id == 2).Description,
                    EndDate = _dbContext.Tasks.First(x => x.Id == 2).EndDate == null
                        ? -1
                        : new DateTimeOffset((DateTime)_dbContext.Tasks.First(x => x.Id == 2).EndDate!).ToUnixTimeMilliseconds(),
                    IsCompleted = _dbContext.Tasks.First(x => x.Id == 2).IsCompleted,
                    BoardId = _dbContext.Tasks.Include(taskM => taskM.Board).First(x => x.Id == 2).Board.Id,
                    ParentId = _dbContext.Tasks.Include(taskM => taskM.ParentTask).First(x => x.Id == 2).ParentTask?.Id,
                    PrioritySort = _dbContext.Tasks.First(x => x.Id == 2).PrioritySort,
                    Priority = (int)_dbContext.Tasks.First(x => x.Id == 2).Priority,
                    ImageHeaderPath = _dbContext.Tasks.First(x => x.Id == 2).ImageHeaderPath
                },
            };
                
            await _distributedCache!.SetStringAsync($"task_{request.TaskId}", JsonSerializer.Serialize(listDto[0]));
            await _distributedCache!.SetStringAsync($"tasks_by_board_{request.BoardId}", JsonSerializer.Serialize(listDto));
            
            var deleteTaskRequestHandler = new DeleteTaskRequestHandler(
                _dbContext, 
                projectSecurityServiceMock.Object,
                SetupMockMediatorService().Object,
                _distributedCache!
            );
            await deleteTaskRequestHandler.Handle(request, CancellationToken.None);
            
            projectSecurityServiceMock.Verify(x => x.CheckAccess(It.IsIn(request.ProjectId), ProjectRoles.ProjectUpdate), Times.Once);
            
            Assert.Null(_dbContext.Tasks.FirstOrDefault(x => x.Id == 1));
            
            Assert.Null(await _distributedCache!.GetStringAsync($"task_{request.TaskId}"));
            Assert.Null(await _distributedCache!.GetStringAsync($"tasks_by_board_{request.BoardId}"));
            
            CleanUp();
        }
        
        [Fact]
        private async void Handle_TaskNotFound_SendTaskNotFoundError()
        {
            SetupDbContext();
            SetupDistributedCache();

            var request = new DeleteTaskRequest() {ProjectId = 1, BoardId = 1, TaskId = 50};

            var projectSecurityServiceMock = SetupProjectSecurityServiceMock();
            
            var mediatorMock = SetupMockMediatorService();
            var error = new TaskNotFoundError();
            
            var deleteTaskRequestHandler = new DeleteTaskRequestHandler(
                _dbContext, 
                projectSecurityServiceMock.Object,
                mediatorMock.Object,
                _distributedCache!
            );
            
            await deleteTaskRequestHandler.Handle(request, CancellationToken.None);
            
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
            
            _dbContext.Boards.Add(new Board() {Title = "Test Project", Project = _dbContext.Projects.First(), PrioritySort = 0});
            _dbContext.SaveChanges();

            _dbContext.Tasks.Add(new TaskM() { 
                Title = "Test delete task",
                Description = "Test delete task",
                Priority = TaskPriority.None,
                PrioritySort = 0,
                Board = _dbContext.Boards.First(x => x.Id == 1)
            });
            _dbContext.SaveChanges();
            
            _dbContext.Tasks.Add(new TaskM() { 
                Title = "Test child task",
                Description = "Test delete task",
                Priority = TaskPriority.None,
                PrioritySort = 0,
                Board = _dbContext.Boards.First(x => x.Id == 1),
                ParentTask = _dbContext.Tasks.First(x => x.Id == 1)
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