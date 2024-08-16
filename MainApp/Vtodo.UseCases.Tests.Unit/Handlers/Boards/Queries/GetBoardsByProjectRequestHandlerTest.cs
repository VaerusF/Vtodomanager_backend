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
using Vtodo.UseCases.Handlers.Boards.Queries.GetBoardsByProject;
using Xunit;

namespace Vtodo.UseCases.Tests.Unit.Handlers.Boards.Queries
{
    public class GetBoardsByProjectRequestHandlerTest
    {
        private AppDbContext _dbContext = null!;
        private IDistributedCache? _distributedCache = null!;

        [Fact]
        public async void Handle_SuccessfulGetBoardsByProjectFromCache_ReturnsTaskListBoardDto()
        {
            SetupDbContext();
            SetupDistributedCache();
            
            var request = new GetBoardsByProjectRequest() { ProjectId = 1};

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
            
            await _distributedCache!.SetStringAsync($"boards_by_project_{request.ProjectId}", JsonSerializer.Serialize(listDto));
            
            var mapperMock = SetupMapperMock();
            mapperMock.Setup(x => x.Map<List<BoardDto>>(It.IsAny<List<Board>>())).Returns(listDto);
            
            var getBoardsByProjectRequestHandler = new GetBoardsByProjectRequestHandler(
                _dbContext, 
                projectSecurityServiceMock.Object, 
                mapperMock.Object,
                SetupMockMediatorService().Object,
                _distributedCache!
            );

            var result = await getBoardsByProjectRequestHandler.Handle(request, CancellationToken.None);
            
            projectSecurityServiceMock.Verify(x => x.CheckAccess(It.IsIn(request.ProjectId), ProjectRoles.ProjectMember), Times.Once);
            
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            
            Assert.NotNull(await _distributedCache!.GetStringAsync($"boards_by_project_{request.ProjectId}"));
            
            CleanUp();
        }
        
        [Fact]
        public async void Handle_SuccessfulGetBoardsByProjectFromDb_ReturnsTaskListBoardDto()
        {
            SetupDbContext();
            SetupDistributedCache();
            
            var request = new GetBoardsByProjectRequest() { ProjectId = 1};

            var projectSecurityServiceMock = SetupProjectSecurityServiceMock();
            
            var mapperMock = SetupMapperMock();
            mapperMock.Setup(x => x.Map<List<BoardDto>>(It.IsAny<List<Board>>())).Returns(new List<BoardDto>()
            {
                new BoardDto() {Id = _dbContext.Boards.First(x => x.Id == 1).Id,
                    Title = _dbContext.Boards.First(x => x.Id == 1).Title,
                    PrioritySort = _dbContext.Boards.First(x => x.Id == 1).PrioritySort,
                    ImageHeaderPath = _dbContext.Boards.First(x => x.Id == 1).ImageHeaderPath
                },
                new BoardDto() {Id = _dbContext.Boards.First(x => x.Id == 2).Id, 
                    Title = _dbContext.Boards.First(x => x.Id == 2).Title,
                    PrioritySort = _dbContext.Boards.First(x => x.Id == 2).PrioritySort,
                    ImageHeaderPath = _dbContext.Boards.First(x => x.Id == 2).ImageHeaderPath
                }
            });
            
            var getBoardsByProjectRequestHandler = new GetBoardsByProjectRequestHandler(
                _dbContext, 
                projectSecurityServiceMock.Object, 
                mapperMock.Object,
                SetupMockMediatorService().Object,
                _distributedCache!
            );

            var result = await getBoardsByProjectRequestHandler.Handle(request, CancellationToken.None);
            
            projectSecurityServiceMock.Verify(x => x.CheckAccess(It.IsIn(request.ProjectId), ProjectRoles.ProjectMember), Times.Once);
            
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            
            Assert.NotNull(await _distributedCache!.GetStringAsync($"boards_by_project_{request.ProjectId}"));
            
            CleanUp();
        }

        private static Mock<IMediator> SetupMockMediatorService()
        {
            var mock = new Mock<IMediator>();
            
            return mock;
        }
        
        private static Mock<IMapper> SetupMapperMock()
        {
            return new Mock<IMapper>();
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