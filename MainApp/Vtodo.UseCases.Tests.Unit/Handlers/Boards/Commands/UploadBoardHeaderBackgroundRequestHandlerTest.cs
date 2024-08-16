using System.Text.Json;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Moq;
using Vtodo.DataAccess.Postgres;
using Vtodo.DomainServices.Interfaces;
using Vtodo.Entities.Enums;
using Vtodo.Entities.Models;
using Vtodo.Infrastructure.Interfaces.Services;
using Vtodo.Tests.Utils;
using Vtodo.UseCases.Handlers.Boards.Commands.UploadBoardHeaderBackground;
using Vtodo.UseCases.Handlers.Boards.Dto;
using Vtodo.UseCases.Handlers.Errors.Commands;
using Vtodo.UseCases.Handlers.Errors.Dto.NotFound;
using Xunit;

namespace Vtodo.UseCases.Tests.Unit.Handlers.Boards.Commands
{
    public class UploadBoardHeaderBackgroundRequestHandlerTest
    {
        private AppDbContext _dbContext = null!;
        private IDistributedCache? _distributedCache = null!;

        [Fact]
        public async void Handle_SuccessfulUploadProjectFile_ReturnsTask()
        {
            SetupDbContext();
            SetupDistributedCache();
            
            var projectSecurityServiceMock = SetupProjectSecurityServiceMock();
            
            var mockBoardService = SetupMockBoardService();
            mockBoardService.Setup(x => x.UpdateImageHeaderPath(It.IsAny<Board>(), It.IsAny<string?>()))
                .Callback((Board board, string? savedFileName) =>
                    {
                        board.ImageHeaderPath = savedFileName;
                    }
            );
            
            var request = new UploadBoardHeaderBackgroundRequest() {ProjectId = 1, BoardId = 1, BackgroundImage = new MemoryStream(), FileName = "test.png"};
            
            var board1 = _dbContext.Boards.First(x => x.Id == 1);
            var board2 = _dbContext.Boards.First(x => x.Id == 2);
            
            var listDto = new List<BoardDto>()
            {
                new BoardDto() { 
                    Id = _dbContext.Boards.First(x => x.Id == 1).Id,
                    Title = _dbContext.Boards.First(x => x.Id == 1).Title,
                    PrioritySort = _dbContext.Boards.First(x => x.Id == 1).PrioritySort,
                    ImageHeaderPath = _dbContext.Boards.First(x => x.Id == 1).ImageHeaderPath
                },
                new BoardDto() {
                    Id = _dbContext.Boards.First(x => x.Id == 2).Id, 
                    Title = _dbContext.Boards.First(x => x.Id == 2).Title,
                    PrioritySort = _dbContext.Boards.First(x => x.Id == 2).PrioritySort,
                    ImageHeaderPath = _dbContext.Boards.First(x => x.Id == 2).ImageHeaderPath
                }
            };
        
            await _distributedCache!.SetStringAsync($"boards_by_project_{request.ProjectId}", JsonSerializer.Serialize(listDto));
            await _distributedCache!.SetStringAsync($"board_{request.BoardId}", JsonSerializer.Serialize(listDto.First()));
            
            var uploadBoardHeaderBackgroundRequestHandler = new UploadBoardHeaderBackgroundRequestHandler(
                _dbContext, 
                projectSecurityServiceMock.Object, 
                SetupProjectFilesServiceMock().Object,
                mockBoardService.Object,
                SetupMockMediatorService().Object,
                _distributedCache!
            );

            await uploadBoardHeaderBackgroundRequestHandler.Handle(request, CancellationToken.None);

            projectSecurityServiceMock.Verify(x => x.CheckAccess(It.IsIn(request.ProjectId), ProjectRoles.ProjectUpdate), Times.Once);
            
            mockBoardService.Verify(x => x.UpdateImageHeaderPath(It.IsAny<Board>(), 
                It.IsAny<string?>()), Times.Once
            );
            
            Assert.NotNull(_dbContext.ProjectBoardsFiles.FirstOrDefault(x =>
                x.BoardId == request.BoardId && x.FileName == request.FileName));

            Assert.Null(await _distributedCache!.GetStringAsync($"board_{request.BoardId}"));
            Assert.Null(await _distributedCache!.GetStringAsync($"boards_by_project_{request.ProjectId}"));
            
            CleanUp();
        }
        
        [Fact]
        public async void Handle_BoardNotFound_SendBoardNotFoundError()
        {
            SetupDbContext();

            var request = new UploadBoardHeaderBackgroundRequest() {ProjectId = 1, BoardId = 3, BackgroundImage = new MemoryStream(), FileName = "test.png"};
            
            var projectSecurityServiceMock = SetupProjectSecurityServiceMock();
            
            var mediatorMock = SetupMockMediatorService();
            var error = new BoardNotFoundError();
            
            var uploadBoardHeaderBackgroundRequestHandler = new UploadBoardHeaderBackgroundRequestHandler(
                _dbContext, 
                projectSecurityServiceMock.Object, 
                SetupProjectFilesServiceMock().Object,
                SetupMockBoardService().Object,
                mediatorMock.Object,
                _distributedCache!
            );
            
            await uploadBoardHeaderBackgroundRequestHandler.Handle(request, CancellationToken.None);
            
            projectSecurityServiceMock.Verify(x => x.CheckAccess(It.IsIn(request.ProjectId), ProjectRoles.ProjectUpdate), Times.Once);
            
            mediatorMock.Verify(x => x.Send(It.Is<SendErrorToClientRequest>(y => 
                        y.Error.GetType() == error.GetType()), 
                    It.IsAny<CancellationToken>()), Times.Once, $"Error request type is not a { error.GetType() }");

            CleanUp();
        }
        
        private static Mock<IProjectSecurityService> SetupProjectSecurityServiceMock()
        {
            var mock = new Mock<IProjectSecurityService>();
            mock.Setup(x => x.CheckAccess(It.IsAny<Project>(), It.IsAny<ProjectRoles>())).Verifiable();
            
            return mock;
        }
        
        private static Mock<IBoardService> SetupMockBoardService()
        {
            var mock = new Mock<IBoardService>();
            
            return mock;
        }
        
        private static Mock<IMediator> SetupMockMediatorService()
        {
            var mock = new Mock<IMediator>();
            
            return mock;
        }
        
        private Mock<IProjectsFilesService> SetupProjectFilesServiceMock()
        {
            var mock = new Mock<IProjectsFilesService>();
            mock.Setup(x => x.CheckFile(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string[]>())).Returns(".png");
            mock.Setup(x => x.UploadProjectFile(It.IsAny<Project>(), It.IsAny<Board>(), It.IsAny<Stream>() , It.IsAny<string>()))
                .Callback(
                (Project project, Board board, Stream steam, string extension) =>
                {
                    _dbContext.ProjectBoardsFiles.Add(new ProjectBoardFile() { BoardId = board.Id, ProjectId = project.Id, FileName = "test.png"});
                    _dbContext.SaveChanges();
                }
                )
                .Returns("test.png");
            
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
            _dbContext.Boards.Add(new Board() {Title = "Test Board2", PrioritySort = 0, Project = _dbContext.Projects.First()});
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