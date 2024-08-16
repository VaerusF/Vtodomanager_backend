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
using Vtodo.UseCases.Handlers.Tasks.Commands.CreateTask;
using Vtodo.UseCases.Handlers.Tasks.Dto;
using Xunit;

namespace Vtodo.UseCases.Tests.Unit.Handlers.Tasks.Commands
{
    public class CreateTaskRequestHandlerTest
    {
        private AppDbContext _dbContext = null!;
        private IDistributedCache? _distributedCache = null!;
        
        [Fact]
        public async void Handle_SuccessfulCreateTask_ReturnsSystemTask()
        {
            SetupDbContext();
            SetupDistributedCache();
            
            var createTaskDto = new CreateTaskDto() { Title = "test", Description = "test", Priority = TaskPriority.Priority1, PrioritySort = 0};
            
            var request = new CreateTaskRequest() { ProjectId = 1, BoardId = 1, CreateTaskDto = createTaskDto};

            var projectSecurityServiceMock = SetupProjectSecurityServiceMock();

            var taskServiceMock = SetupTaskService();
            taskServiceMock.Setup(x => x.CreateTask(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<Board>(),
                It.IsAny<bool>(),
                It.IsAny<int?>(),
                It.IsAny<TaskPriority>(),
                It.IsAny<TaskM?>()
            )).Returns(new TaskM()
            {
                Id = 3,
                Title = createTaskDto.Title,
                Description = createTaskDto.Description,
                IsCompleted = false,
                Board = _dbContext.Boards.First(x => x.Id == request.BoardId),
                ParentTask = _dbContext.Tasks.FirstOrDefault(x => x.Id == createTaskDto.ParentTaskId),
                PrioritySort = createTaskDto.PrioritySort,
                Priority = createTaskDto.Priority,
                ImageHeaderPath = null
            });
            
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
                
            await _distributedCache!.SetStringAsync($"tasks_by_board_{request.BoardId}", JsonSerializer.Serialize(listDto));
            
            var createTaskRequestHandler = new CreateTaskRequestHandler(
                _dbContext, 
                projectSecurityServiceMock.Object,
                taskServiceMock.Object,
                SetupMockMediatorService().Object,
                _distributedCache!
            );
            
            await createTaskRequestHandler.Handle(request, CancellationToken.None);

            projectSecurityServiceMock.Verify(x => x.CheckAccess(It.IsIn(request.ProjectId), ProjectRoles.ProjectUpdate), Times.Once);
            
            Assert.NotNull(_dbContext.Tasks.FirstOrDefault(x => x.Title == createTaskDto.Title && 
                x.Description == createTaskDto.Description && 
                x.Priority == createTaskDto.Priority && 
                x.PrioritySort == createTaskDto.PrioritySort && 
                x.Board.Id == request.BoardId
            ));
            
            Assert.Null(await _distributedCache!.GetStringAsync($"tasks_by_board_{request.BoardId}"));
            
            CleanUp();
        }
        
        [Fact]
        public async void Handle_BoardNotFound_ThrowsBoardNotFoundException()
        {
            SetupDbContext();
            SetupDistributedCache();
            
            var createTaskDto = new CreateTaskDto() { Title = "test", Description = "test", Priority = TaskPriority.Priority1, PrioritySort = 0};
            
            var request = new CreateTaskRequest() { ProjectId = 1, BoardId = 10, CreateTaskDto = createTaskDto};

            var projectSecurityServiceMock = SetupProjectSecurityServiceMock();
            
            var mediatorMock = SetupMockMediatorService();
            var error = new BoardNotFoundError();
            
            var createTaskRequestHandler = new CreateTaskRequestHandler(
                _dbContext, 
                projectSecurityServiceMock.Object,
                SetupTaskService().Object,
                mediatorMock.Object,
                _distributedCache!
            );
            
            await createTaskRequestHandler.Handle(request, CancellationToken.None);
            
            projectSecurityServiceMock.Verify(x => x.CheckAccess(It.IsIn(request.ProjectId), ProjectRoles.ProjectUpdate), Times.Once);
            
            mediatorMock.Verify(x => x.Send(It.Is<SendErrorToClientRequest>(y => 
                        y.Error.GetType() == error.GetType()), 
                    It.IsAny<CancellationToken>()), Times.Once, $"Error request type is not a { error.GetType() }");
            
            CleanUp();
        }
        
        [Fact]
        public async void Handle_ParentTaskNotFound_SendTaskNotFoundError()
        {
            SetupDbContext();
            SetupDistributedCache();
            
            var createTaskDto = new CreateTaskDto() { Title = "test", Description = "test", Priority = TaskPriority.Priority1, PrioritySort = 0, ParentTaskId = 100};
            
            var request = new CreateTaskRequest() { ProjectId = 1, BoardId = 1, CreateTaskDto = createTaskDto};

            var projectSecurityServiceMock = SetupProjectSecurityServiceMock();
            
            var mediatorMock = SetupMockMediatorService();
            var error = new TaskNotFoundError();
            
            var createTaskRequestHandler = new CreateTaskRequestHandler(
                _dbContext, 
                projectSecurityServiceMock.Object,
                SetupTaskService().Object,
                mediatorMock.Object,
                _distributedCache!
            );
            
            await createTaskRequestHandler.Handle(request, CancellationToken.None);
            
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
        
        private static Mock<ITaskService> SetupTaskService()
        {
            var mock = new Mock<ITaskService>();
            
            return mock;
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
            
            _dbContext.Boards.Add(new Board() {Title = "Test board", Project = _dbContext.Projects.First(), PrioritySort = 0});
            _dbContext.SaveChanges();

            _dbContext.Tasks.Add(new TaskM()
            {
                Title = "Test task1", 
                Description = "Test1", 
                ImageHeaderPath = "", 
                IsCompleted = false, 
                PrioritySort = 0,
                ParentTask = null,
                Board = _dbContext.Boards.First()
            });
            _dbContext.SaveChanges();
            
            _dbContext.Tasks.Add(new TaskM()
            {
                Title = "Test task12", 
                Description = "Test2", 
                ImageHeaderPath = "", 
                IsCompleted = false, 
                PrioritySort = 0,
                ParentTask = _dbContext.Tasks.First(),
                Board = _dbContext.Boards.First()
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