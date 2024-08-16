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
using Vtodo.UseCases.Handlers.Tasks.Dto;
using Vtodo.UseCases.Handlers.Tasks.Queries.GetTask;
using Xunit;

namespace Vtodo.UseCases.Tests.Unit.Handlers.Tasks.Queries
{
    public class GetTaskRequestHandlerTest
    {
        private AppDbContext _dbContext = null!;
        private IDistributedCache? _distributedCache = null!;

        [Fact]
        public async void Handle_SuccessfulGetTaskFromCache_ReturnSystemTaskTaskDto()
        {
            SetupDbContext();
            SetupDistributedCache();
            
            var projectSecurityServiceMock = SetupProjectSecurityServiceMock();
            
            var request = new GetTaskRequest() { ProjectId = 1, TaskId = 1 };
            
            var testTask = _dbContext.Tasks.Include(x => x.Board).First(x => x.Id == request.TaskId);

            var dto = new TaskDto()
            {
                Id = testTask.Id,
                Title = testTask.Title,
                PrioritySort = testTask.PrioritySort,
                IsCompleted = testTask.IsCompleted,
                ImageHeaderPath = testTask.ImageHeaderPath
            };
            
            await _distributedCache!.SetStringAsync($"task_{request.TaskId}", JsonSerializer.Serialize(dto));
            
            var getTaskRequestHandler = new GetTaskRequestHandler(
                _dbContext, 
                projectSecurityServiceMock.Object, 
                SetupMockMediatorService().Object,
                _distributedCache!
            );

            var result = await getTaskRequestHandler.Handle(request, CancellationToken.None);
            
            projectSecurityServiceMock.Verify(x => x.CheckAccess(It.IsIn(request.ProjectId), ProjectRoles.ProjectMember), Times.Once);
            
            Assert.NotNull(result);
            Assert.NotNull(await _distributedCache!.GetStringAsync($"task_{request.TaskId}"));
            
            CleanUp();
        }
        
        [Fact]
        public async void Handle_SuccessfulGetTaskFromDb_ReturnSystemTaskTaskDto()
        {
            SetupDbContext();
            SetupDistributedCache();
            
            var projectSecurityServiceMock = SetupProjectSecurityServiceMock();
            
            var request = new GetTaskRequest() { ProjectId = 1, TaskId = 1 };
            
            var testTask = _dbContext.Tasks.Include(x => x.Board).First(x => x.Id == request.TaskId);
            
            var getTaskRequestHandler = new GetTaskRequestHandler(
                _dbContext, 
                projectSecurityServiceMock.Object, 
                SetupMockMediatorService().Object,
                _distributedCache!
            );

            var result = await getTaskRequestHandler.Handle(request, CancellationToken.None);
            
            projectSecurityServiceMock.Verify(x => x.CheckAccess(It.IsIn(request.ProjectId), ProjectRoles.ProjectMember), Times.Once);
            
            Assert.NotNull(result);
            Assert.NotNull(await _distributedCache!.GetStringAsync($"task_{request.TaskId}"));
            
            CleanUp();
        }
        
        [Fact]
        public async void Handle_TaskNotFound_SendTaskNotFoundError()
        {
            SetupDbContext();
            SetupDistributedCache();
            
            var projectSecurityServiceMock = SetupProjectSecurityServiceMock();
            
            var request = new GetTaskRequest() { ProjectId = 1, TaskId = 100 };
            
            var mediatorMock = SetupMockMediatorService();
            var error = new TaskNotFoundError();
            
            var getTaskRequestHandler = new GetTaskRequestHandler(
                _dbContext, 
                projectSecurityServiceMock.Object,
                mediatorMock.Object,
                _distributedCache!
            );

            var result = await getTaskRequestHandler.Handle(request, CancellationToken.None);
            
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
            _dbContext.SaveChanges();
            
            _dbContext.Tasks.Add(new TaskM() { 
                Title = "Test upload background image",
                Description = "Test update task",
                Priority = TaskPriority.None,
                PrioritySort = 0,
                Board = _dbContext.Boards.First(x => x.Id == 1)
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