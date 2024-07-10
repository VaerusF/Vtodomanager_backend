using System.Linq;
using System.Threading;
using MediatR;
using Moq;
using Vtodo.DataAccess.Postgres;
using Vtodo.DomainServices.Interfaces;
using Vtodo.Entities.Enums;
using Vtodo.Entities.Exceptions;
using Vtodo.Entities.Models;
using Vtodo.Infrastructure.Interfaces.Services;
using Vtodo.Tests.Utils;
using Vtodo.UseCases.Handlers.Errors.Commands;
using Vtodo.UseCases.Handlers.Errors.Dto.NotFound;
using Vtodo.UseCases.Handlers.Tasks.Commands.MoveTaskToRoot;
using Xunit;

namespace Vtodo.UseCases.Tests.Unit.Handlers.Tasks.Commands
{
    public class MoveTaskToRootRequestHandlerTest
    {
        private AppDbContext _dbContext = null!;

        [Fact]
        public async void Handle_SuccessfulMoveTaskToRoot_ReturnsSystemTask()
        {
            SetupDbContext();
            
            var mockTaskService = SetupMockTaskService();
            mockTaskService
                .Setup(x => x.MoveTaskToRoot(It.IsAny<TaskM>()))
                .Callback((TaskM task) =>
                {
                    task.ParentTask = null;
                });
            
            var request = new MoveTaskToRootRequest() { Id = 2 };

            var moveTaskToRootRequestHandler = new MoveTaskToRootRequestHandler(
                _dbContext, 
                SetupProjectSecurityServiceMock().Object,
                mockTaskService.Object,
                SetupMockMediatorService().Object);

            await moveTaskToRootRequestHandler.Handle(request, CancellationToken.None);
            
            mockTaskService.Verify(x => x.MoveTaskToRoot(It.IsAny<TaskM>()), Times.Once);
            
            Assert.Null(_dbContext.Tasks.FirstOrDefault(x => x.Id == request.Id && x.ParentTask != null));
            Assert.NotNull(_dbContext.Tasks.FirstOrDefault(x => x.Id == request.Id && x.ParentTask == null));
            
            CleanUp();
        }

        [Fact]
        public async void Handle_TaskNotFound_SendTaskNotFoundError()
        {
            SetupDbContext();

            var request = new MoveTaskToRootRequest() { Id = 20 };

            var mediatorMock = SetupMockMediatorService();
            var error = new TaskNotFoundError();
            
            var moveTaskToRootRequestHandler = new MoveTaskToRootRequestHandler(
                _dbContext, 
                SetupProjectSecurityServiceMock().Object,
                SetupMockTaskService().Object,
                mediatorMock.Object);

            await moveTaskToRootRequestHandler.Handle(request, CancellationToken.None);
            mediatorMock.Verify(x => x.Send(It.Is<SendErrorToClientRequest>(y => 
                        y.Error.GetType() == error.GetType()), 
                    It.IsAny<CancellationToken>()), Times.Once, $"Error request type is not a { error.GetType() }");

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
            _dbContext.SaveChanges();
        }
        
        private void CleanUp()
        {
            _dbContext?.Database.EnsureDeleted();
            _dbContext?.Dispose();
        }
    }
}