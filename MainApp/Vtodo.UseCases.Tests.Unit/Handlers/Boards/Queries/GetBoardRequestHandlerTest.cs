using System.Text.Json;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Moq;
using Vtodo.DataAccess.Postgres;
using Vtodo.Entities.Enums;
using Vtodo.Entities.Models;
using Vtodo.Infrastructure.Interfaces.Services;
using Vtodo.Tests.Utils;
using Vtodo.UseCases.Handlers.Boards.Dto;
using Vtodo.UseCases.Handlers.Boards.Queries.GetBoard;
using Vtodo.UseCases.Handlers.Errors.Commands;
using Vtodo.UseCases.Handlers.Errors.Dto.NotFound;
using Xunit;

namespace Vtodo.UseCases.Tests.Unit.Handlers.Boards.Queries
{
    public class GetBoardRequestHandlerTest
    {
        private AppDbContext _dbContext = null!;
        private IDistributedCache? _distributedCache = null!;
        
        [Fact]
        public async void Handle_SuccessfulGetBoardFromCache_ReturnsTaskBoardDto()
        {
            SetupDbContext();
            SetupDistributedCache();
            
            var request = new GetBoardRequest() { ProjectId = 1, BoardId = 1 };

            var projectSecurityServiceMock = SetupProjectSecurityServiceMock();

            var boardDto = new BoardDto()
            {
                Id = request.BoardId,
                ImageHeaderPath = _dbContext.Boards.First(x => x.Id == request.BoardId).ImageHeaderPath,
                Title = _dbContext.Boards.First(x => x.Id == request.BoardId).Title,
                PrioritySort = _dbContext.Boards.First(x => x.Id == request.BoardId).PrioritySort,
                ProjectId = request.ProjectId
            };
            
            var mapperMock = SetupMapperMock();
            mapperMock.Setup(x => x.Map<BoardDto>(It.IsAny<Board>())).Returns(boardDto);
            
            await _distributedCache!.SetStringAsync($"board_{request.BoardId}", JsonSerializer.Serialize(boardDto));
            
            var getBoardRequestHandler = new GetBoardRequestHandler(
                _dbContext, 
                projectSecurityServiceMock.Object, 
                mapperMock.Object,
                SetupMockMediatorService().Object,
                _distributedCache!
            );

            var result = await getBoardRequestHandler.Handle(request, CancellationToken.None);
            
            projectSecurityServiceMock.Verify(x => x.CheckAccess(request.ProjectId, ProjectRoles.ProjectMember), Times.Once);
            
            Assert.NotNull(result);
            Assert.NotNull(await _distributedCache!.GetStringAsync($"board_{request.BoardId}"));
            
            CleanUp();
        }
        
        [Fact]
        public async void Handle_SuccessfulGetBoardFromDb_ReturnsTaskBoardDto()
        {
            SetupDbContext();
            SetupDistributedCache();
            
            var request = new GetBoardRequest() { ProjectId = 1, BoardId = 1 };

            var projectSecurityServiceMock = SetupProjectSecurityServiceMock();
            
            var mapperMock = SetupMapperMock();
            mapperMock.Setup(x => x.Map<BoardDto>(It.IsAny<Board>())).Returns(new BoardDto()
            {
                Id = request.BoardId,
                ImageHeaderPath = _dbContext.Boards.First(x => x.Id == request.BoardId).ImageHeaderPath,
                Title = _dbContext.Boards.First(x => x.Id == request.BoardId).Title,
                PrioritySort = _dbContext.Boards.First(x => x.Id == request.BoardId).PrioritySort,
                ProjectId = request.ProjectId
            });
            
            var getBoardRequestHandler = new GetBoardRequestHandler(
                _dbContext, 
                projectSecurityServiceMock.Object, 
                mapperMock.Object,
                SetupMockMediatorService().Object,
                _distributedCache!
            );

            var result = await getBoardRequestHandler.Handle(request, CancellationToken.None);
            
            projectSecurityServiceMock.Verify(x => x.CheckAccess(request.ProjectId, ProjectRoles.ProjectMember), Times.Once);
            
            Assert.NotNull(result);
            Assert.NotNull(await _distributedCache!.GetStringAsync($"board_{request.BoardId}"));
            
            CleanUp();
        }
        
        [Fact]
        public async void Handle_BoardNotFound_SendBoardNotFoundError()
        {
            SetupDbContext();
            SetupDistributedCache();
            
            var request = new GetBoardRequest() { ProjectId = 1, BoardId = 2 };
            
            var projectSecurityServiceMock = SetupProjectSecurityServiceMock();
            
            var mediatorMock = SetupMockMediatorService();
            var error = new BoardNotFoundError();
            
            var getBoardRequestHandler = new GetBoardRequestHandler(
                _dbContext, 
                projectSecurityServiceMock.Object, 
                SetupMapperMock().Object,
                mediatorMock.Object,
                _distributedCache!
            );
            
            var result = await getBoardRequestHandler.Handle(request, CancellationToken.None);
            
            projectSecurityServiceMock.Verify(x => x.CheckAccess(request.ProjectId, ProjectRoles.ProjectMember), Times.Once);
            
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
        
        private static Mock<IMapper> SetupMapperMock()
        {
            return new Mock<IMapper>();
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
        }
        
        private void CleanUp()
        {
            _dbContext?.Database.EnsureDeleted();
            _dbContext?.Dispose();
            
            _distributedCache = null!;
        }
    }
}