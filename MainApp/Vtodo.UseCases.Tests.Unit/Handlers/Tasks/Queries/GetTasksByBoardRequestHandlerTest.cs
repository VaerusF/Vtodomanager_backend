using System.Text.Json;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Moq;
using Vtodo.DataAccess.Postgres;
using Vtodo.Entities.Enums;
using Vtodo.Entities.Models;
using Vtodo.Infrastructure.Interfaces.Services;
using Vtodo.Tests.Utils;
using Vtodo.UseCases.Handlers.Errors.Commands;
using Vtodo.UseCases.Handlers.Errors.Dto.NotFound;
using Vtodo.UseCases.Handlers.Tasks.Dto;
using Vtodo.UseCases.Handlers.Tasks.Queries.GetTasksByBoard;
using Xunit;

namespace Vtodo.UseCases.Tests.Unit.Handlers.Tasks.Queries
{
    public class GetTasksByBoardRequestHandlerTest
    {
        private AppDbContext _dbContext = null!;
        private IDistributedCache? _distributedCache = null!;
        
        [Fact]
        public async void Handle_SuccessfulGetTasksByBoardFromCache_ReturnsSystemTaskListTaskDto()
        {
            SetupDbContext();
            SetupDistributedCache();
            
            var projectSecurityServiceMock = SetupProjectSecurityServiceMock();
            
            var request = new GetTasksByBoardRequest() { BoardId = 1};
            
            
            var testTask1 = _dbContext.Tasks.First(x => x.Id == 1);
            var testTask2 = _dbContext.Tasks.First(x => x.Id == 2);
            var testTask3 = _dbContext.Tasks.First(x => x.Id == 3);

            var listDto = new List<TaskDto>()
            {
                new TaskDto()
                {
                    Id = testTask1.Id,
                    Title = testTask1.Title,
                    PrioritySort = testTask1.PrioritySort,
                    IsCompleted = testTask1.IsCompleted,
                    ImageHeaderPath = testTask1.ImageHeaderPath
                },
                new TaskDto()
                {
                    Id = testTask2.Id,
                    Title = testTask2.Title,
                    PrioritySort = testTask2.PrioritySort,
                    IsCompleted = testTask2.IsCompleted,
                    ImageHeaderPath = testTask2.ImageHeaderPath
                },
                new TaskDto()
                {
                    Id = testTask3.Id,
                    Title = testTask3.Title,
                    PrioritySort = testTask3.PrioritySort,
                    IsCompleted = testTask3.IsCompleted,
                    ImageHeaderPath = testTask3.ImageHeaderPath
                },
            };
            
            await _distributedCache!.SetStringAsync($"tasks_by_board_{request.BoardId}", JsonSerializer.Serialize(listDto));
            
            var getTasksByBoardRequestHandler = new GetTasksByBoardRequestHandler(
                _dbContext, 
                projectSecurityServiceMock.Object, 
                SetupMockMediatorService().Object,
                _distributedCache!
            );

            var result = await getTasksByBoardRequestHandler.Handle(request, CancellationToken.None);
            
            projectSecurityServiceMock.Verify(x => x.CheckAccess(It.IsIn(request.ProjectId), ProjectRoles.ProjectMember), Times.Once);
            
            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
            
            Assert.NotNull(await _distributedCache!.GetStringAsync($"tasks_by_board_{request.BoardId}"));
            
            CleanUp();
        }
        
        [Fact]
        public async void Handle_SuccessfulGetTasksByBoardFromDb_ReturnsSystemTaskListTaskDto()
        {
            SetupDbContext();
            SetupDistributedCache();
            
            var projectSecurityServiceMock = SetupProjectSecurityServiceMock();
            
            var request = new GetTasksByBoardRequest() { BoardId = 1};
            
            var getTasksByBoardRequestHandler = new GetTasksByBoardRequestHandler(
                _dbContext, 
                projectSecurityServiceMock.Object, 
                SetupMockMediatorService().Object,
                _distributedCache!
            );

            var result = await getTasksByBoardRequestHandler.Handle(request, CancellationToken.None);
            
            projectSecurityServiceMock.Verify(x => x.CheckAccess(It.IsIn(request.ProjectId), ProjectRoles.ProjectMember), Times.Once);
            
            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
            
            Assert.NotNull(await _distributedCache!.GetStringAsync($"tasks_by_board_{request.BoardId}"));
            
            CleanUp();
        }
        
        [Fact]
        public async void Handle_BoardNotFound_SendBoardNotFoundError()
        {
            SetupDbContext();
            SetupDistributedCache();
            
            var projectSecurityServiceMock = SetupProjectSecurityServiceMock();

            var request = new GetTasksByBoardRequest() { BoardId = 3};
            
            var mediatorMock = SetupMockMediatorService();
            var error = new BoardNotFoundError();
            
            var getTasksByBoardRequestHandler = new GetTasksByBoardRequestHandler(
                _dbContext, 
                projectSecurityServiceMock.Object,
                mediatorMock.Object,
                _distributedCache!
            );
            
            var result = await getTasksByBoardRequestHandler.Handle(request, CancellationToken.None);
            
            projectSecurityServiceMock.Verify(x => x.CheckAccess(It.IsIn(request.ProjectId), ProjectRoles.ProjectMember), Times.Once);
            
            mediatorMock.Verify(x => x.Send(It.Is<SendErrorToClientRequest>(y => 
                        y.Error.GetType() == error.GetType()), 
                    It.IsAny<CancellationToken>()), Times.Once, $"Error request type is not a { error.GetType() }");
            Assert.Null(result);
            
            CleanUp();
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

        private void SetupDistributedCache()
        {
            _distributedCache = TestDbUtils.SetupTestCacheInMemory();
        }
        
        private void SetupDbContext()
        {
            _dbContext = TestDbUtils.SetupTestDbContextInMemory();
            
            _dbContext.Projects.Add(new Project() {Title = "Test Project"});
            _dbContext.Projects.Add(new Project() {Title = "Test Project2"});
            _dbContext.SaveChanges();
            
            _dbContext.Boards.Add(new Board() {Title = "Test Board", PrioritySort = 0, Project = _dbContext.Projects.First()});
            _dbContext.Boards.Add(new Board() {Title = "Test Board 2", PrioritySort = 0, Project = _dbContext.Projects.First()});
            _dbContext.SaveChanges();
            
            _dbContext.Tasks.Add(new TaskM() { 
                Title = "Test get task by board",
                Description = "Test task 1",
                Priority = TaskPriority.None,
                PrioritySort = 0,
                Board = _dbContext.Boards.First(x => x.Id == 1)
            });
            _dbContext.Tasks.Add(new TaskM() { 
                Title = "Test get task by board",
                Description = "Test task 2",
                Priority = TaskPriority.None,
                PrioritySort = 0,
                Board = _dbContext.Boards.First(x => x.Id == 1)
            });
            _dbContext.SaveChanges();
            
            _dbContext.Tasks.Add(new TaskM() { 
                Title = "Test children Task",
                Description = "Test update task",
                Priority = TaskPriority.None,
                PrioritySort = 0,
                Board = _dbContext.Boards.First(x => x.Id == 1),
                ParentTask = _dbContext.Tasks.First(x => x.Id == 2)
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