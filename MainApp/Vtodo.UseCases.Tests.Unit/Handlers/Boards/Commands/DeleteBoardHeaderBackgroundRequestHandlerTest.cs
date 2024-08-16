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
using Vtodo.UseCases.Handlers.Boards.Commands.DeleteBoardHeaderBackground;
using Vtodo.UseCases.Handlers.Boards.Dto;
using Vtodo.UseCases.Handlers.Errors.Commands;
using Vtodo.UseCases.Handlers.Errors.Dto.NotFound;
using Xunit;

namespace Vtodo.UseCases.Tests.Unit.Handlers.Boards.Commands
{
    public class DeleteBoardHeaderBackgroundRequestHandlerTest
    {
        private AppDbContext _dbContext = null!;
        private IDistributedCache? _distributedCache = null!;
        
        [Fact]
        public async void Handle_SuccessfulDeleteBoardHeaderBackground_ReturnsTask()
        {
            SetupDbContext();
            SetupDistributedCache();
            
            var mockBoardService = SetupMockBoardService();
            mockBoardService.Setup(x => x.UpdateImageHeaderPath(It.IsAny<Board>(), It.IsAny<string?>()))
                .Callback((Board board, string? savedFileName) =>
                    {
                        board.ImageHeaderPath = savedFileName;
                    }
                );
            
            var request = new DeleteBoardHeaderBackgroundRequest() { ProjectId = 1, BoardId = 1};

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
            
            var oldPath = "";
            
            var projectSecurityServiceMock = SetupProjectSecurityServiceMock();
            
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
                projectSecurityServiceMock.Object, 
                mockProjectFileService.Object,
                mockBoardService.Object,
                SetupMockMediatorService().Object,
                _distributedCache!
            );
            
            await deleteBoardHeaderBackgroundRequestHandler.Handle(request, CancellationToken.None);
            
            projectSecurityServiceMock.Verify(x => x.CheckAccess(It.IsIn(request.ProjectId), ProjectRoles.ProjectUpdate), Times.Once);
            
            mockBoardService.Verify(x => x.UpdateImageHeaderPath(It.IsAny<Board>(), 
                    It.IsAny<string?>()), Times.Once
            );
            
            Assert.Null(_dbContext.ProjectBoardsFiles.FirstOrDefault(x => x.BoardId == request.BoardId && x.FileName == oldPath));
            Assert.Null(_dbContext.Boards.FirstOrDefault(x => x.Id == request.BoardId && x.ImageHeaderPath != null));
            
            Assert.Null(await _distributedCache!.GetStringAsync($"board_{request.BoardId}"));
            Assert.Null(await _distributedCache!.GetStringAsync($"boards_by_project_{request.ProjectId}"));
            
            CleanUp();
        }
        
        [Fact]
        public async void Handle_BoardNotFound_SendBoardNotFoundError()
        {
            SetupDbContext();

            var request = new DeleteBoardHeaderBackgroundRequest() { ProjectId = 1, BoardId = 3};

            var mockProjectFileService = SetupProjectFilesServiceMock();
            mockProjectFileService.Setup(x =>
                x.DeleteProjectFile(It.IsAny<Project>(), It.IsAny<Board>(), It.IsAny<string>())).Verifiable();
            
            var projectSecurityServiceMock = SetupProjectSecurityServiceMock();
            
            var mediatorMock = SetupMockMediatorService();
            var error = new BoardNotFoundError();
            
            var deleteBoardHeaderBackgroundRequestHandler = new DeleteBoardHeaderBackgroundRequestHandler(
                _dbContext, 
                projectSecurityServiceMock.Object, 
                mockProjectFileService.Object,
                SetupMockBoardService().Object, 
                mediatorMock.Object,
                _distributedCache!
            );
            
            await deleteBoardHeaderBackgroundRequestHandler.Handle(request, CancellationToken.None);
            
            projectSecurityServiceMock.Verify(x => x.CheckAccess(It.IsIn(request.ProjectId), ProjectRoles.ProjectUpdate), Times.Once);
            
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
        
        private static Mock<IBoardService> SetupMockBoardService()
        {
            var mock = new Mock<IBoardService>();
            
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
            
            _dbContext.Boards.Add(new Board() {Title = "Test Board", PrioritySort = 0, Project = _dbContext.Projects.First(), ImageHeaderPath = "Test file name"});
            _dbContext.Boards.Add(new Board() {Title = "Test Board2", PrioritySort = 0, Project = _dbContext.Projects.First()});
            _dbContext.SaveChanges();
            
            _dbContext.ProjectBoardsFiles.Add(new ProjectBoardFile()
                { ProjectId = 1, BoardId = 1, FileName = "Test file name"} );
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