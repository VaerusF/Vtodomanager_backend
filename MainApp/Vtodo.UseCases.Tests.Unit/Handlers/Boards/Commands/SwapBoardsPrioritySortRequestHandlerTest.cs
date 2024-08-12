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
using Vtodo.UseCases.Handlers.Boards.Commands.SwapBoardsPrioritySort;
using Vtodo.UseCases.Handlers.Boards.Dto;
using Vtodo.UseCases.Handlers.Errors.Commands;
using Vtodo.UseCases.Handlers.Errors.Dto.InvalidOperation;
using Vtodo.UseCases.Handlers.Errors.Dto.NotFound;
using Xunit;

namespace Vtodo.UseCases.Tests.Unit.Handlers.Boards.Commands;

public class SwapBoardsPrioritySortRequestHandlerTest
{
    private AppDbContext _dbContext = null!;
    private IDistributedCache? _distributedCache = null!;
    
    [Fact]
    public async void Handle_SuccessfulSwapBoardsPrioritySort_ReturnsTask()
    {
        SetupDbContext();
        SetupDistributedCache();
        
        var request = new SwapBoardsPrioritySortRequest() { ProjectId = 1, BoardId1 = 1, BoardId2 = 2};
        
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
        await _distributedCache!.SetStringAsync($"board_{request.BoardId1}", JsonSerializer.Serialize(listDto.First()));
        await _distributedCache!.SetStringAsync($"board_{request.BoardId2}", JsonSerializer.Serialize(listDto[1]));
        
        var projectSecurityServiceMock = SetupProjectSecurityServiceMock();
        
        var mockBoardService = SetupMockBoardService();
        mockBoardService.Setup(x => x.SwapBoardsPrioritySort(It.IsAny<Board>(), 
            It.IsAny<Board>())
        ).Callback((Board board1, Board board2) =>
        {
            (board1.PrioritySort, board2.PrioritySort) = (board2.PrioritySort, board1.PrioritySort);
        });
            
        var swapBoardsPrioritySortRequestHandler = new  SwapBoardsPrioritySortRequestHandler(
            _dbContext, 
            projectSecurityServiceMock.Object,
            mockBoardService.Object,
            SetupMockMediatorService().Object,
            _distributedCache!
        );

        await swapBoardsPrioritySortRequestHandler.Handle(request, CancellationToken.None);
            
        projectSecurityServiceMock.Verify(x => x.CheckAccess(request.ProjectId, ProjectRoles.ProjectUpdate), Times.Once);
        
        mockBoardService.Verify(x => x.SwapBoardsPrioritySort(It.IsAny<Board>(), It.IsAny<Board>()), Times.Once);
            
        Assert.NotNull(_dbContext.Boards.FirstOrDefault(x => x.Id == 1 && x.PrioritySort == 2));
        Assert.NotNull(_dbContext.Boards.FirstOrDefault(x => x.Id == 2 && x.PrioritySort == 1));
            
        Assert.Null(await _distributedCache!.GetStringAsync($"board_{request.BoardId1}"));
        Assert.Null(await _distributedCache!.GetStringAsync($"board_{request.BoardId2}"));
        Assert.Null(await _distributedCache!.GetStringAsync($"boards_by_project_{request.ProjectId}"));
        
        CleanUp();
    }
    
    [Fact]
    public async void Handle_Board1EqualBoard2_SendBoardIdsEqualsIdError()
    {
        SetupDbContext();
    
        var request = new SwapBoardsPrioritySortRequest() { ProjectId = 1, BoardId1 = 10, BoardId2 = 10 };
        
        var mediatorMock = SetupMockMediatorService();
        var error = new BoardIdsEqualsIdError();
            
        var projectSecurityServiceMock = SetupProjectSecurityServiceMock();
        
        var swapBoardsPrioritySortRequestHandler = new  SwapBoardsPrioritySortRequestHandler(
            _dbContext, 
            projectSecurityServiceMock.Object,
            SetupMockBoardService().Object,
            mediatorMock.Object,
            _distributedCache!
        );
            
        await swapBoardsPrioritySortRequestHandler.Handle(request, CancellationToken.None);
        
        projectSecurityServiceMock.Verify(x => x.CheckAccess(request.ProjectId, ProjectRoles.ProjectUpdate), Times.Once);
        
        mediatorMock.Verify(x => x.Send(It.Is<SendErrorToClientRequest>(y => 
                    y.Error.GetType() == error.GetType()), 
                It.IsAny<CancellationToken>()), Times.Once, $"Error request type is not a { error.GetType() }");

        CleanUp();
    }
    
    [Fact]
    public async void Handle_Board1NotFound_SendBoardNotFoundError()
    {
        SetupDbContext();
    
        var request = new SwapBoardsPrioritySortRequest() { ProjectId = 1, BoardId1 = 10, BoardId2 = 2 };
        
        var mediatorMock = SetupMockMediatorService();
        var error = new BoardNotFoundError();
            
        var projectSecurityServiceMock = SetupProjectSecurityServiceMock();
        
        var swapBoardsPrioritySortRequestHandler = new  SwapBoardsPrioritySortRequestHandler(
            _dbContext, 
            projectSecurityServiceMock.Object,
            SetupMockBoardService().Object,
            mediatorMock.Object,
            _distributedCache!
        );
            
        await swapBoardsPrioritySortRequestHandler.Handle(request, CancellationToken.None);
        
        projectSecurityServiceMock.Verify(x => x.CheckAccess(request.ProjectId, ProjectRoles.ProjectUpdate), Times.Once);
        
        mediatorMock.Verify(x => x.Send(It.Is<SendErrorToClientRequest>(y => 
                    y.Error.GetType() == error.GetType()), 
                It.IsAny<CancellationToken>()), Times.Once, $"Error request type is not a { error.GetType() }");

        CleanUp();
    }
    
    [Fact]
    public async void Handle_Board2NotFound_SendBoardNotFoundError()
    {
        SetupDbContext();
    
        var request = new SwapBoardsPrioritySortRequest() { ProjectId = 1, BoardId1 = 1, BoardId2 = 20 };
        
        var mediatorMock = SetupMockMediatorService();
        var error = new BoardNotFoundError();
            
        var projectSecurityServiceMock = SetupProjectSecurityServiceMock();
        
        var swapBoardsPrioritySortRequestHandler = new  SwapBoardsPrioritySortRequestHandler(
            _dbContext, 
            projectSecurityServiceMock.Object,
            SetupMockBoardService().Object,
            mediatorMock.Object,
            _distributedCache!
        );
            
        await swapBoardsPrioritySortRequestHandler.Handle(request, CancellationToken.None);
        
        projectSecurityServiceMock.Verify(x => x.CheckAccess(request.ProjectId, ProjectRoles.ProjectUpdate), Times.Once);
        
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
        _dbContext.SaveChanges();
            
        _dbContext.Boards.Add(new Board() {Title = "Test Board1", PrioritySort = 1, Project = _dbContext.Projects.First()});
        _dbContext.Boards.Add(new Board() {Title = "Test Board2", PrioritySort = 2, Project = _dbContext.Projects.First()});
        _dbContext.SaveChanges();
    }
    
    private void CleanUp()
    {
        _dbContext?.Database.EnsureDeleted();
        _dbContext?.Dispose();
        
        _distributedCache = null!;
    }
}