using System.IO;
using System.Linq;
using System.Threading;
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
using Vtodo.UseCases.Handlers.Tasks.Commands.DeleteTaskHeaderBackground;
using Xunit;

namespace Vtodo.UseCases.Tests.Unit.Handlers.Tasks.Commands
{
    public class DeleteTaskHeaderBackgroundRequestHandlerTest
    {
        private AppDbContext _dbContext = null!;

        [Fact]
        public async void Handle_SuccessfulDeleteTaskHeaderBackground_ReturnsSystemTask()
        {
            SetupDbContext();
            
            var request = new DeleteTaskHeaderBackgroundRequest() { Id = 1 };

            var oldPath = "";
            
            var mockProjectFileService = SetupProjectFilesServiceMock();
            mockProjectFileService.Setup(x =>
                x.DeleteProjectFile(It.IsAny<Project>(), It.IsAny<TaskM>(), It.IsAny<string>()))
                .Callback(
                (Project project, TaskM task, string path) =>
                {
                    oldPath = path;
                    var removeTaskFile = _dbContext.ProjectTasksFiles.First(x =>
                        x.ProjectId == project.Id && x.TaskId == task.Id && x.FileName == path);
                    _dbContext.ProjectTasksFiles.Remove(removeTaskFile);

                    _dbContext.SaveChanges();
                });

            var deleteTaskHeaderBackgroundRequestHandler = new DeleteTaskHeaderBackgroundRequestHandler(_dbContext, 
                SetupProjectSecurityServiceMock().Object, 
                mockProjectFileService.Object, 
                SetupMockMediatorService().Object);

            await deleteTaskHeaderBackgroundRequestHandler.Handle(request, CancellationToken.None);
            
            Assert.Null(_dbContext.ProjectTasksFiles.FirstOrDefault(x => x.TaskId == request.Id && x.FileName == oldPath));
            Assert.Null(_dbContext.Tasks.FirstOrDefault(x => x.Id == request.Id && x.ImageHeaderPath != null));
            
            CleanUp();
        }

        [Fact]
        public async void Handle_TaskNotFound_SendTaskNotFoundError()
        {
            SetupDbContext();
            
            var request = new DeleteTaskHeaderBackgroundRequest() { Id = 2};

            var mediatorMock = SetupMockMediatorService();
            var error = new TaskNotFoundError();
            
            var mockProjectFileService = SetupProjectFilesServiceMock();
            mockProjectFileService.Setup(x =>
                x.DeleteProjectFile(It.IsAny<Project>(), It.IsAny<TaskM>(), It.IsAny<string>())).Verifiable();
            
            var deleteTaskHeaderBackgroundRequestHandler = new DeleteTaskHeaderBackgroundRequestHandler(
                _dbContext, 
                SetupProjectSecurityServiceMock().Object, 
                mockProjectFileService.Object,
                mediatorMock.Object);
            
            await deleteTaskHeaderBackgroundRequestHandler.Handle(request, CancellationToken.None);
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
        
        private Mock<IProjectsFilesService> SetupProjectFilesServiceMock()
        {
            var mock = new Mock<IProjectsFilesService>();
            mock.Setup(x => x.CheckFile(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string[]>())).Returns(".png");

            return mock;
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
                Board = _dbContext.Boards.First(x => x.Id == 1), 
                ImageHeaderPath = "Test file name"
            });
            _dbContext.SaveChanges();
            
            _dbContext.ProjectTasksFiles.Add(new ProjectTaskFile()
                { ProjectId = 1, TaskId = 1, FileName = "Test file name"} );
            _dbContext.SaveChanges();
        }
        
        private void CleanUp()
        {
            _dbContext?.Database.EnsureDeleted();
            _dbContext?.Dispose();
        }
    }
}