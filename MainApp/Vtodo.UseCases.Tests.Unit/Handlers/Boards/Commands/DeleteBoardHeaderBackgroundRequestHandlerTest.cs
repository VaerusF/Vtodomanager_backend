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
using Vtodo.UseCases.Handlers.Boards.Commands.DeleteBoardHeaderBackground;
using Vtodo.UseCases.Handlers.Errors.Commands;
using Vtodo.UseCases.Handlers.Errors.Dto.NotFound;
using Xunit;

namespace Vtodo.UseCases.Tests.Unit.Handlers.Boards.Commands
{
    public class DeleteBoardHeaderBackgroundRequestHandlerTest
    {
        private AppDbContext _dbContext = null!;
        
        [Fact]
        public async void Handle_SuccessfulDeleteBoardHeaderBackground_ReturnsTask()
        {
            SetupDbContext();

            var request = new DeleteBoardHeaderBackgroundRequest() { Id = 1};

            var oldPath = "";
            
            var mockProjectFileService = SetupProjectFilesServiceMock();
            mockProjectFileService.Setup(x =>
                x.DeleteProjectFile(It.IsAny<Project>(), It.IsAny<Board>(), It.IsAny<string>()))
                .Callback(
                (Project project, Board board, string path) =>
                {
                    oldPath = path;
                    var removeBoardFile = _dbContext.ProjectBoardsFiles.First(x =>
                        x.ProjectId == project.Id && x.BoardId == board.Id && x.FileName == path);
                    _dbContext.ProjectBoardsFiles.Remove(removeBoardFile);

                    _dbContext.SaveChanges();
                });
            
            var deleteBoardHeaderBackgroundRequestHandler = new DeleteBoardHeaderBackgroundRequestHandler(
                _dbContext, 
                SetupProjectSecurityServiceMock().Object, 
                mockProjectFileService.Object,
                SetupMockMediatorService().Object);
            
            await deleteBoardHeaderBackgroundRequestHandler.Handle(request, CancellationToken.None);
            
            Assert.Null(_dbContext.ProjectBoardsFiles.FirstOrDefault(x => x.BoardId == request.Id && x.FileName == oldPath));
            Assert.Null(_dbContext.Boards.FirstOrDefault(x => x.Id == request.Id && x.ImageHeaderPath != null));
            
            CleanUp();
        }
        
        [Fact]
        public async void Handle_BoardNotFound_SendBoardNotFoundError()
        {
            SetupDbContext();

            var request = new DeleteBoardHeaderBackgroundRequest() { Id = 2};

            var mockProjectFileService = SetupProjectFilesServiceMock();
            mockProjectFileService.Setup(x =>
                x.DeleteProjectFile(It.IsAny<Project>(), It.IsAny<Board>(), It.IsAny<string>())).Verifiable();
            
            var mediatorMock = SetupMockMediatorService();
            var error = new BoardNotFoundError();
            
            var deleteBoardHeaderBackgroundRequestHandler = new DeleteBoardHeaderBackgroundRequestHandler(
                _dbContext, 
                SetupProjectSecurityServiceMock().Object, 
                mockProjectFileService.Object,
                mediatorMock.Object);
            
            await deleteBoardHeaderBackgroundRequestHandler.Handle(request, CancellationToken.None);
            
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
            
            return mock;;
        }
        
        private static Mock<IProjectsFilesService> SetupProjectFilesServiceMock()
        {
            var mock = new Mock<IProjectsFilesService>();

            return mock;
        }
        
        private void SetupDbContext()
        {
            _dbContext = TestDbUtils.SetupTestDbContextInMemory();
            
            _dbContext.Projects.Add(new Project() {Title = "Test Project"});
            _dbContext.Projects.Add(new Project() {Title = "Test Project2"});
            _dbContext.SaveChanges();
            
            _dbContext.Boards.Add(new Board() {Title = "Test Board", PrioritySort = 0, Project = _dbContext.Projects.First(), ImageHeaderPath = "Test file name"});
            _dbContext.SaveChanges();
            
            _dbContext.ProjectBoardsFiles.Add(new ProjectBoardFile()
                { ProjectId = 1, BoardId = 1, FileName = "Test file name"} );
            _dbContext.SaveChanges();
        }
        
        private void CleanUp()
        {
            _dbContext?.Database.EnsureDeleted();
            _dbContext?.Dispose();
        }
    }
}