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
using Vtodo.UseCases.Handlers.Tasks.Commands.UploadTaskHeaderBackground;
using Xunit;

namespace Vtodo.UseCases.Tests.Unit.Handlers.Tasks.Commands
{
    public class UploadTaskHeaderBackgroundRequestHandlerTest
    {
        private AppDbContext _dbContext = null!;
        
        [Fact]
        public async void Handle_SuccessfulUploadTaskFile_ReturnsSystemTask()
        {
            SetupDbContext();
            
            var request = new UploadTaskHeaderBackgroundRequest() {Id = 1, BackgroundImage = new MemoryStream(), FileName = "test.png"};
            
            var uploadTaskHeaderBackgroundRequestHandler = new UploadTaskHeaderBackgroundRequestHandler(
                _dbContext, 
                SetupProjectSecurityServiceMock().Object, 
                SetupProjectFilesServiceMock().Object,
                SetupMockMediatorService().Object
            );

            await uploadTaskHeaderBackgroundRequestHandler.Handle(request, CancellationToken.None);

            Assert.NotNull(_dbContext.Tasks.FirstOrDefault(x =>
                x.Id == request.Id && x.ImageHeaderPath != null));
            
            Assert.NotNull(_dbContext.ProjectTasksFiles.FirstOrDefault(x =>
                x.TaskId == request.Id && x.FileName == request.FileName));
            
            
            CleanUp();
        }

        [Fact]
        public async void Handle_TaskNotFound_SendTaskNotFoundError()
        {
            SetupDbContext();
            
            var request = new UploadTaskHeaderBackgroundRequest() {Id = 100, BackgroundImage = new MemoryStream(), FileName = "test.png"};
            
            var mediatorMock = SetupMockMediatorService();
            var error = new TaskNotFoundError();
            
            var uploadTaskHeaderBackgroundRequestHandler = new UploadTaskHeaderBackgroundRequestHandler(
                _dbContext, 
                SetupProjectSecurityServiceMock().Object, 
                SetupProjectFilesServiceMock().Object, 
                mediatorMock.Object
            );
            
            await uploadTaskHeaderBackgroundRequestHandler.Handle(request, CancellationToken.None);
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
            mock.Setup(x => x.UploadProjectFile(It.IsAny<Project>(), It.IsAny<TaskM>(), It.IsAny<Stream>() , It.IsAny<string>()))
                .Callback(
                    (Project project, TaskM task, Stream steam, string extension) =>
                    {
                        _dbContext.ProjectTasksFiles.Add(new ProjectTaskFile() { TaskId = task.Id, ProjectId = project.Id, FileName = "test.png"});
                        _dbContext.SaveChanges();
                    }
                )
                .Returns("test.png");
            
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
                Board = _dbContext.Boards.First(x => x.Id == 1)
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