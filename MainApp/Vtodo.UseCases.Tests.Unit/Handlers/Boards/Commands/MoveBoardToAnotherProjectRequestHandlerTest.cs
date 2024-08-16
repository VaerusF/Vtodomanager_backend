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
using Vtodo.UseCases.Handlers.Boards.Commands.MoveBoardToAnotherProject;
using Vtodo.UseCases.Handlers.Boards.Dto;
using Vtodo.UseCases.Handlers.Errors.Commands;
using Vtodo.UseCases.Handlers.Errors.Dto.InvalidOperation;
using Vtodo.UseCases.Handlers.Errors.Dto.NotFound;
using Xunit;

namespace Vtodo.UseCases.Tests.Unit.Handlers.Boards.Commands
{
    public class MoveBoardToAnotherProjectRequestHandlerTest
    {
        private AppDbContext _dbContext = null!;
        private IDistributedCache? _distributedCache = null!;
        
        [Fact]
        public async void Handle_SuccessfulMoveBoardToAnotherProject_ReturnsTask()
        {
            SetupDbContext();
            SetupDistributedCache();
            
            var request = new MoveBoardToAnotherProjectRequest() { ProjectId = 1, BoardId = 1, NewProjectId = 2};
            
            var projectSecurityServiceMock = SetupProjectSecurityServiceMock();
            
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
            
            var board3 = _dbContext.Boards.First(x => x.Id == 3);
            var board4 = _dbContext.Boards.First(x => x.Id == 4);
            
            var listDto2 = new List<BoardDto>()
            {
                new BoardDto() { 
                    Id = _dbContext.Boards.First(x => x.Id == 3).Id,
                    Title = _dbContext.Boards.First(x => x.Id == 3).Title,
                    PrioritySort = _dbContext.Boards.First(x => x.Id == 3).PrioritySort,
                    ImageHeaderPath = _dbContext.Boards.First(x => x.Id == 3).ImageHeaderPath
                },
                new BoardDto() {
                    Id = _dbContext.Boards.First(x => x.Id == 4).Id, 
                    Title = _dbContext.Boards.First(x => x.Id == 4).Title,
                    PrioritySort = _dbContext.Boards.First(x => x.Id == 4).PrioritySort,
                    ImageHeaderPath = _dbContext.Boards.First(x => x.Id == 4).ImageHeaderPath
                }
            };
            
            await _distributedCache!.SetStringAsync($"boards_by_project_{request.ProjectId}", JsonSerializer.Serialize(listDto));
            await _distributedCache!.SetStringAsync($"boards_by_project_{request.NewProjectId}", JsonSerializer.Serialize(listDto2));
            await _distributedCache!.SetStringAsync($"board_{request.BoardId}", JsonSerializer.Serialize(listDto.First()));
            
            var mockBoardService = SetupMockBoardService();
            mockBoardService.Setup(x => x.MoveBoardToAnotherProject(It.IsAny<Board>(), 
                It.IsAny<Project>())
            ).Callback((Board board, Project newProject) =>
            {
                board.Project = newProject;
            });
            
            var moveBoardToAnotherProjectRequestHandler = new MoveBoardToAnotherProjectRequestHandler(
                _dbContext, 
                projectSecurityServiceMock.Object,
                mockBoardService.Object,
                SetupMockMediatorService().Object,
                _distributedCache!
            );

            await moveBoardToAnotherProjectRequestHandler.Handle(request, CancellationToken.None);
            
            projectSecurityServiceMock.Verify(x => x.CheckAccess(It.IsIn(request.ProjectId), ProjectRoles.ProjectUpdate), Times.Once);
            projectSecurityServiceMock.Verify(x => x.CheckAccess(It.IsIn(request.NewProjectId), ProjectRoles.ProjectUpdate), Times.Once);
            
            mockBoardService.Verify(x => x.MoveBoardToAnotherProject(It.IsAny<Board>(), It.IsAny<Project>()), Times.Once);
            
            Assert.Null(_dbContext.Boards.FirstOrDefault(x => x.Project == _dbContext.Projects.First(d => d.Id == 1) && x.Id == request.BoardId));
            Assert.NotNull(_dbContext.Boards.FirstOrDefault(x => x.Project == _dbContext.Projects.First(d => d.Id == 2) && x.Id == 1));
            
            Assert.Null(await _distributedCache!.GetStringAsync($"board_{request.BoardId}"));
            Assert.Null(await _distributedCache!.GetStringAsync($"boards_by_project_{request.ProjectId}"));
            Assert.Null(await _distributedCache!.GetStringAsync($"boards_by_project_{request.NewProjectId}"));
            
            CleanUp();
        }

        [Fact]
        public async void Handle_BoardNotFound_SendBoardNotFoundError()
        {
            SetupDbContext();

            var request = new MoveBoardToAnotherProjectRequest() { ProjectId = 1, BoardId = 5, NewProjectId = 2};
            
            var projectSecurityServiceMock = SetupProjectSecurityServiceMock();
            
            var mediatorMock = SetupMockMediatorService();
            var error = new BoardNotFoundError();
            
            var moveBoardToAnotherProjectRequestHandler = new MoveBoardToAnotherProjectRequestHandler(
                _dbContext, 
                projectSecurityServiceMock.Object, 
                SetupMockBoardService().Object,
                mediatorMock.Object,
                _distributedCache!
            );

            await moveBoardToAnotherProjectRequestHandler.Handle(request, CancellationToken.None);
            
            projectSecurityServiceMock.Verify(x => x.CheckAccess(It.IsIn(request.ProjectId), ProjectRoles.ProjectUpdate), Times.Once);
            projectSecurityServiceMock.Verify(x => x.CheckAccess(It.IsIn(request.NewProjectId), ProjectRoles.ProjectUpdate), Times.Once);
            
            mediatorMock.Verify(x => x.Send(It.Is<SendErrorToClientRequest>(y => 
                        y.Error.GetType() == error.GetType()), 
                    It.IsAny<CancellationToken>()), Times.Once, $"Error request type is not a { error.GetType() }");

            CleanUp();
        }
        
        [Fact]
        public async void Handle_NewProjectIdEqualOldIdException_SendNewProjectIdEqualOldIdError()
        {
            SetupDbContext();

            var request = new MoveBoardToAnotherProjectRequest() { BoardId = 1, NewProjectId = 1};
            
            var projectSecurityServiceMock = SetupProjectSecurityServiceMock();
            
            var mediatorMock = SetupMockMediatorService();
            var error = new NewProjectIdEqualOldIdError();
            
            var moveBoardToAnotherProjectRequestHandler = new MoveBoardToAnotherProjectRequestHandler(
                _dbContext, 
                projectSecurityServiceMock.Object,
                SetupMockBoardService().Object,
                mediatorMock.Object,
                _distributedCache!
            );
            
            await moveBoardToAnotherProjectRequestHandler.Handle(request, CancellationToken.None);
            
            mediatorMock.Verify(x => x.Send(It.Is<SendErrorToClientRequest>(y => 
                        y.Error.GetType() == error.GetType()), 
                    It.IsAny<CancellationToken>()), Times.Once, $"Error request type is not a { error.GetType() }");

            projectSecurityServiceMock.Verify(x => x.CheckAccess(It.IsIn(request.ProjectId), ProjectRoles.ProjectUpdate), Times.Once);
            projectSecurityServiceMock.Verify(x => x.CheckAccess(It.IsIn(request.NewProjectId), ProjectRoles.ProjectUpdate), Times.Once);
            
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
            
            return new Mock<IProjectSecurityService>();
        }
        
        private static Mock<IBoardService> SetupMockBoardService()
        {
            var mock = new Mock<IBoardService>();
            
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
            _dbContext.Boards.Add(new Board() {Title = "Test Board3", PrioritySort = 0, Project = _dbContext.Projects.First(x => x.Id == 2)});
            _dbContext.Boards.Add(new Board() {Title = "Test Board4", PrioritySort = 0, Project = _dbContext.Projects.First(x => x.Id == 2)});
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