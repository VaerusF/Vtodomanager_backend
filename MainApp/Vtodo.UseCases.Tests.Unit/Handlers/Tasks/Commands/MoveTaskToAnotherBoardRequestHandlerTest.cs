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
using Vtodo.UseCases.Handlers.Errors.Dto.InvalidOperation;
using Vtodo.UseCases.Handlers.Errors.Dto.NotFound;
using Vtodo.UseCases.Handlers.Tasks.Commands.MoveTaskToAnotherBoard;
using Vtodo.UseCases.Handlers.Tasks.Dto;
using Xunit;

namespace Vtodo.UseCases.Tests.Unit.Handlers.Tasks.Commands
{
    public class MoveTaskToAnotherBoardRequestHandlerTest
    {
        private AppDbContext _dbContext = null!;
        private IDistributedCache? _distributedCache = null!;

        [Fact]
        public async void Handle_SuccessfulMoveTaskToAnotherBoard_ReturnsSystemTask()
        {
            SetupDbContext();
            SetupDistributedCache();
            
            var request = new MoveTaskToAnotherBoardRequest() { ProjectId = 1, BoardId = 1, TaskId = 1, NewBoardId = 2};
            var newBoard = _dbContext.Boards.First(x => x.Id == request.NewBoardId); 
            
            var projectSecurityServiceMock = SetupProjectSecurityServiceMock();
            
            var mockTaskService = SetupMockTaskService();
            mockTaskService
                .Setup(x => x.MoveAllTaskFromListToAnotherBoard(It.IsAny<List<TaskM>>(), 
                    It.IsAny<Board>())
            )
                .Callback((List<TaskM> tasksList, Board newBoard ) =>
                {
                    foreach (var task in tasksList)
                    {
                        task.Board = newBoard;
                    }
                }
            );
            
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

            var listDto2 = new List<TaskDto>()
            {
                new TaskDto
                {
                    Id = _dbContext.Tasks.First(x => x.Id == 3).Id,
                    Title = _dbContext.Tasks.First(x => x.Id == 3).Title,
                    Description = _dbContext.Tasks.First(x => x.Id == 3).Description,
                    EndDate = _dbContext.Tasks.First(x => x.Id == 3).EndDate == null
                        ? -1
                        : new DateTimeOffset((DateTime)_dbContext.Tasks.First(x => x.Id == 3).EndDate!)
                            .ToUnixTimeMilliseconds(),
                    IsCompleted = _dbContext.Tasks.First(x => x.Id == 3).IsCompleted,
                    BoardId = _dbContext.Tasks.Include(taskM => taskM.Board).First(x => x.Id == 3).Board.Id,
                    ParentId = _dbContext.Tasks.Include(taskM => taskM.ParentTask).First(x => x.Id == 3).ParentTask?.Id,
                    PrioritySort = _dbContext.Tasks.First(x => x.Id == 3).PrioritySort,
                    Priority = (int)_dbContext.Tasks.First(x => x.Id == 3).Priority,
                    ImageHeaderPath = _dbContext.Tasks.First(x => x.Id == 3).ImageHeaderPath
                },
            };
            
            await _distributedCache!.SetStringAsync($"task_{request.TaskId}", JsonSerializer.Serialize(listDto[0]));
            await _distributedCache!.SetStringAsync($"tasks_by_board_{request.BoardId}", JsonSerializer.Serialize(listDto));
            await _distributedCache!.SetStringAsync($"tasks_by_board_{request.NewBoardId}", JsonSerializer.Serialize(listDto2));
            
            var moveTaskToAnotherBoardRequestHandler = new MoveTaskToAnotherBoardRequestHandler(
                _dbContext, 
                projectSecurityServiceMock.Object,
                mockTaskService.Object,
                SetupMockMediatorService().Object,
                _distributedCache!
            );

            await moveTaskToAnotherBoardRequestHandler.Handle(request, CancellationToken.None);
            
            projectSecurityServiceMock.Verify(x => x.CheckAccess(It.IsIn(request.ProjectId), ProjectRoles.ProjectUpdate), Times.Exactly(2));

            mockTaskService.Verify(x => x.MoveAllTaskFromListToAnotherBoard(
                It.IsAny<List<TaskM>>(), 
                It.IsAny<Board>()), Times.Once
            );
            
            Assert.Null(_dbContext.Tasks.FirstOrDefault(x => x.Id == request.TaskId && x.Board.Id == 1));
            Assert.NotNull(_dbContext.Tasks.FirstOrDefault(x => x.Id == request.TaskId && x.Board.Id == 2));
            
            Assert.Null(_dbContext.Tasks.FirstOrDefault(x => x.Id == 2 && x.Board.Id == 1));
            Assert.NotNull(_dbContext.Tasks.FirstOrDefault(x => x.Id == 2 && x.Board.Id == 2));
            
            Assert.Null(await _distributedCache!.GetStringAsync($"task_{request.TaskId}"));
            Assert.Null(await _distributedCache!.GetStringAsync($"tasks_by_board_{request.BoardId}"));
            Assert.Null(await _distributedCache!.GetStringAsync($"tasks_by_board_{request.NewBoardId}"));
            
            CleanUp();
        }
        
        [Fact]
        public async void Handle_NewBoardNotFound_SendBoardNotFoundError()
        {
            SetupDbContext();

            var request = new MoveTaskToAnotherBoardRequest() { ProjectId = 1, BoardId = 1, TaskId = 1, NewBoardId = 20};
            
            var projectSecurityServiceMock = SetupProjectSecurityServiceMock();
            
            var mediatorMock = SetupMockMediatorService();
            var error = new BoardNotFoundError();
            
            var moveTaskToAnotherBoardRequestHandler = new MoveTaskToAnotherBoardRequestHandler(
                _dbContext, 
                projectSecurityServiceMock.Object,
                SetupMockTaskService().Object,
                mediatorMock.Object,
                _distributedCache!
            );

            await moveTaskToAnotherBoardRequestHandler.Handle(request, CancellationToken.None);
            
            projectSecurityServiceMock.Verify(x => x.CheckAccess(It.IsIn(request.ProjectId), ProjectRoles.ProjectUpdate), Times.Once);
            
            mediatorMock.Verify(x => x.Send(It.Is<SendErrorToClientRequest>(y => 
                        y.Error.GetType() == error.GetType()), 
                    It.IsAny<CancellationToken>()), Times.Once, $"Error request type is not a { error.GetType() }");

            CleanUp();
        }

        [Fact]
        public async void Handle_TaskNotFound_SendTaskNotFoundError()
        {
            SetupDbContext();

            var request = new MoveTaskToAnotherBoardRequest() { ProjectId = 1, BoardId = 1, TaskId = 100, NewBoardId = 2};
            
            var projectSecurityServiceMock = SetupProjectSecurityServiceMock();
            
            var mediatorMock = SetupMockMediatorService();
            var error = new TaskNotFoundError();
            
            var moveTaskToAnotherBoardRequestHandler = new MoveTaskToAnotherBoardRequestHandler(
                _dbContext, 
                projectSecurityServiceMock.Object,
                SetupMockTaskService().Object,
                mediatorMock.Object,
                _distributedCache!
            );

            await moveTaskToAnotherBoardRequestHandler.Handle(request, CancellationToken.None);
            
            projectSecurityServiceMock.Verify(x => x.CheckAccess(It.IsIn(request.ProjectId), ProjectRoles.ProjectUpdate), Times.Exactly(2));
            
            mediatorMock.Verify(x => x.Send(It.Is<SendErrorToClientRequest>(y => 
                        y.Error.GetType() == error.GetType()), 
                    It.IsAny<CancellationToken>()), Times.Once, $"Error request type is not a { error.GetType() }");

            CleanUp();
        }
        
        [Fact]
        public async void Handle_NewBoardIdEqualOldId_SendNewBoardIdEqualOldIdError()
        {
            SetupDbContext();

            var request = new MoveTaskToAnotherBoardRequest() { ProjectId = 1, BoardId = 1, TaskId = 1, NewBoardId = 1};
            
            var projectSecurityServiceMock = SetupProjectSecurityServiceMock();
            
            var mediatorMock = SetupMockMediatorService();
            var error = new NewBoardIdEqualOldIdError();
            
            var moveTaskToAnotherBoardRequestHandler = new MoveTaskToAnotherBoardRequestHandler(
                _dbContext, 
                projectSecurityServiceMock.Object,
                SetupMockTaskService().Object,
                mediatorMock.Object,
                _distributedCache!
            );
            
            await moveTaskToAnotherBoardRequestHandler.Handle(request, CancellationToken.None);
            
            projectSecurityServiceMock.Verify(x => x.CheckAccess(It.IsIn(request.ProjectId), ProjectRoles.ProjectUpdate), Times.Exactly(2));
            
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

        private static Mock<ITaskService> SetupMockTaskService()
        {
            var mock = new Mock<ITaskService>();
            
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
                Title = "Test move task",
                Description = "Test move task",
                Priority = TaskPriority.None,
                PrioritySort = 0,
                Board = _dbContext.Boards.First(x => x.Id == 1)
            });
            _dbContext.SaveChanges();
            
            _dbContext.Tasks.Add(new TaskM() { 
                Title = "Test child task",
                Description = "Test move task",
                Priority = TaskPriority.None,
                PrioritySort = 0,
                Board = _dbContext.Boards.First(x => x.Id == 1),
                ParentTask = _dbContext.Tasks.First(x => x.Id == 1)
            });
            
            _dbContext.Tasks.Add(new TaskM() { 
                Title = "Test cache task",
                Description = "Test cache task",
                Priority = TaskPriority.None,
                PrioritySort = 0,
                Board = _dbContext.Boards.First(x => x.Id == 2),
                ParentTask = null
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