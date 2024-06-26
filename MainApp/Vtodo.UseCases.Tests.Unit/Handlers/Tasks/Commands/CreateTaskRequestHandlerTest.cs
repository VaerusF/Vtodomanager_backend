using System.Linq;
using System.Threading;
using AutoMapper;
using MediatR;
using Moq;
using Vtodo.DataAccess.Postgres;
using Vtodo.Entities.Enums;
using Vtodo.Entities.Exceptions;
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

        [Fact]
        public async void Handle_SuccessfulCreateTask_ReturnsSystemTask()
        {
            SetupDbContext();
            
            var createTaskDto = new CreateTaskDto() { Title = "test", Description = "test", Priority = TaskPriority.Priority1, PrioritySort = 0, BoardId = 1};
            
            var request = new CreateTaskRequest() { CreateTaskDto = createTaskDto};

            var mapperMock = SetupMapperMock();
            mapperMock.Setup(x => x.Map<TaskM>(It.IsAny<CreateTaskDto>())).Returns(new TaskM()
            {
                Title = createTaskDto.Title,
                Description = createTaskDto.Description,
                Priority = createTaskDto.Priority,
                PrioritySort = createTaskDto.PrioritySort
            });
            
            var createTaskRequestHandler = new CreateTaskRequestHandler(
                _dbContext, 
                SetupProjectSecurityServiceMock().Object, 
                mapperMock.Object,
                SetupMockMediatorService().Object);
            await createTaskRequestHandler.Handle(request, CancellationToken.None);
            
            Assert.NotNull(_dbContext.Tasks.FirstOrDefault(x => x.Title == createTaskDto.Title && 
                x.Description == createTaskDto.Description && 
                x.Priority == createTaskDto.Priority && 
                x.PrioritySort == createTaskDto.PrioritySort && 
                x.Board.Id == createTaskDto.BoardId ));
            
            CleanUp();
        }
        
        [Fact]
        public async void Handle_BoardNotFound_ThrowsBoardNotFoundException()
        {
            SetupDbContext();
            
            var createTaskDto = new CreateTaskDto() { Title = "test", Description = "test", Priority = TaskPriority.Priority1, PrioritySort = 0, BoardId = 10};
            
            var request = new CreateTaskRequest() { CreateTaskDto = createTaskDto};

            var mediatorMock = SetupMockMediatorService();
            var error = new BoardNotFoundError();
            
            var mapperMock = SetupMapperMock();
            mapperMock.Setup(x => x.Map<TaskM>(It.IsAny<CreateTaskDto>())).Returns(new TaskM()
            {
                Title = createTaskDto.Title,
                Description = createTaskDto.Description,
                Priority = createTaskDto.Priority,
                PrioritySort = createTaskDto.PrioritySort
            });
            
            var createTaskRequestHandler = new CreateTaskRequestHandler(
                _dbContext, 
                SetupProjectSecurityServiceMock().Object, 
                mapperMock.Object,
                mediatorMock.Object);
            
            await createTaskRequestHandler.Handle(request, CancellationToken.None);
            mediatorMock.Verify(x => x.Send(It.Is<SendErrorToClientRequest>(y => 
                        y.Error.GetType() == error.GetType()), 
                    It.IsAny<CancellationToken>()), Times.Once, $"Error request type is not a { error.GetType() }");
            
            CleanUp();
        }
        
        [Fact]
        public async void Handle_ParentTaskNotFound_SendTaskNotFoundError()
        {
            SetupDbContext();
            
            var createTaskDto = new CreateTaskDto() { Title = "test", Description = "test", Priority = TaskPriority.Priority1, PrioritySort = 0, BoardId = 1, ParentTaskId = 100};
            
            var request = new CreateTaskRequest() { CreateTaskDto = createTaskDto};

            var mediatorMock = SetupMockMediatorService();
            var error = new TaskNotFoundError();
            
            var mapperMock = SetupMapperMock();
            mapperMock.Setup(x => x.Map<TaskM>(It.IsAny<CreateTaskDto>())).Returns(new TaskM()
            {
                Title = createTaskDto.Title,
                Description = createTaskDto.Description,
                Priority = createTaskDto.Priority,
                PrioritySort = createTaskDto.PrioritySort
            });
            
            var createTaskRequestHandler = new CreateTaskRequestHandler(
                _dbContext, 
                SetupProjectSecurityServiceMock().Object, 
                mapperMock.Object,
                mediatorMock.Object);
            
            await createTaskRequestHandler.Handle(request, CancellationToken.None);
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

        private static Mock<IMapper> SetupMapperMock()
        {
            return new Mock<IMapper>();
        }
        
        private void SetupDbContext()
        {
            _dbContext = TestDbUtils.SetupTestDbContextInMemory();

            _dbContext.Accounts.Add(new Account() { Email = "test@test.ru", Username = "test", HashedPassword = "test" , Salt = new byte[64]});
            
            _dbContext.Projects.Add(new Project() {Title = "Test Project"});
            _dbContext.SaveChanges();
            
            _dbContext.Boards.Add(new Board() {Title = "Test board", Project = _dbContext.Projects.First(), PrioritySort = 0});
            _dbContext.SaveChanges();
        }
        
        private void CleanUp()
        {
            _dbContext?.Database.EnsureDeleted();
            _dbContext?.Dispose();
        }
    }
}