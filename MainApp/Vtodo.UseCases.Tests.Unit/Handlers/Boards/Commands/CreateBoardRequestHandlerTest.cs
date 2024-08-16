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
using Vtodo.UseCases.Handlers.Boards.Commands.CreateBoard;
using Vtodo.UseCases.Handlers.Boards.Dto;
using Vtodo.UseCases.Handlers.Errors.Commands;
using Vtodo.UseCases.Handlers.Errors.Dto.NotFound;
using Xunit;

namespace Vtodo.UseCases.Tests.Unit.Handlers.Boards.Commands
{
    public class CreateBoardRequestHandlerTest
    {
        private AppDbContext _dbContext = null!;
        private IDistributedCache? _distributedCache = null!;
        
        [Fact]
        public async void Handle_SuccessfulCreateBoard_ReturnsTask()
        {
            SetupDbContext();
            SetupDistributedCache();
            
            var createBoardDto = new CreateBoardDto() { Title ="Create Test Board", PrioritySort = 0};
            var request = new CreateBoardRequest() { ProjectId = 1, CreateBoardDto = createBoardDto};
            
            var projectSecurityServiceMock = SetupProjectSecurityService();
            
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
            
            var mockMapper = SetupMockMapper();
            mockMapper.Setup(x => x.Map<Board>(createBoardDto)).Returns(new Board() {Title = createBoardDto.Title, PrioritySort = 0});
            
            var createBoardRequestHandler = new CreateBoardRequestHandler(
                _dbContext, 
                projectSecurityServiceMock.Object, 
                mockMapper.Object,
                SetupMockMediatorService().Object,
                _distributedCache!
            );
            
            await createBoardRequestHandler.Handle(request, CancellationToken.None);
            
            projectSecurityServiceMock.Verify(x => x.CheckAccess(It.IsAny<long>(), ProjectRoles.ProjectUpdate), Times.Once);
            
            Assert.NotNull(_dbContext.Boards.FirstOrDefault(x => x.Title == createBoardDto.Title));
            Assert.Null(await _distributedCache!.GetStringAsync($"boards_by_project_{request.ProjectId}"));
            
            CleanUp();
        }
        
        [Fact]
        public async void Handle_ProjectNotFound_SendProjectNotFoundError()
        {
            SetupDbContext();
            SetupDistributedCache();
            
            var createBoardDto = new CreateBoardDto() { Title ="Test Board", PrioritySort = 0};
            var request = new CreateBoardRequest() { ProjectId = 4, CreateBoardDto = createBoardDto };

            var projectSecurityServiceMock = SetupProjectSecurityService();
            
            var mockMapper = SetupMockMapper();
            mockMapper.Setup(x => x.Map<Board>(createBoardDto)).Returns(new Board() {Title = createBoardDto.Title, PrioritySort = 0});
            
            var mediatorMock = SetupMockMediatorService();
            var error = new ProjectNotFoundError();
            
            var createBoardRequestHandler = new CreateBoardRequestHandler(
                _dbContext, 
                projectSecurityServiceMock.Object, 
                mockMapper.Object,
                mediatorMock.Object,
                _distributedCache!
            );
            
            await createBoardRequestHandler.Handle(request, CancellationToken.None);
            
            projectSecurityServiceMock.Verify(x => x.CheckAccess(It.IsAny<long>(), ProjectRoles.ProjectUpdate), Times.Once);
            
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
        
        private static Mock<IProjectSecurityService> SetupProjectSecurityService()
        {
            var mock = new Mock<IProjectSecurityService>();
            mock.Setup(x => x.CheckAccess(It.IsAny<Project>(), It.IsAny<ProjectRoles>())).Verifiable();
            
            return new Mock<IProjectSecurityService>();
        }
        
        private static Mock<IMapper> SetupMockMapper()
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