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
using Vtodo.UseCases.Handlers.Errors.Commands;
using Vtodo.UseCases.Handlers.Errors.Dto.InvalidOperation;
using Vtodo.UseCases.Handlers.Errors.Dto.NotFound;
using Vtodo.UseCases.Handlers.Tasks.Commands.SwapTasksPrioritySort;
using Vtodo.UseCases.Handlers.Tasks.Dto;
using Xunit;

namespace Vtodo.UseCases.Tests.Unit.Handlers.Tasks.Commands;

public class SwapTasksPrioritySortRequestHandlerTest
{
    private AppDbContext _dbContext = null!;
    private IDistributedCache? _distributedCache = null!;
    
    [Fact]
    public async void Handle_SuccessfulSwapTasksPrioritySort_ReturnsTask()
    {
        SetupDbContext();
        SetupDistributedCache();
        
        var request = new SwapTasksPrioritySortRequest() { ProjectId = 1, BoardId = 1, TaskId1 = 1, TaskId2 = 2};
        
        var taskId1 = _dbContext.Tasks.First(x => x.Id == 1);
        var taskId2 = _dbContext.Tasks.First(x => x.Id == 2);
            
        var listDto = new List<TaskDto>()
        {
            new TaskDto() { 
                Id = _dbContext.Tasks.First(x => x.Id == 1).Id,
                Title = _dbContext.Tasks.First(x => x.Id == 1).Title,
                PrioritySort = _dbContext.Tasks.First(x => x.Id == 1).PrioritySort,
            },
            new TaskDto() {
                Id = _dbContext.Tasks.First(x => x.Id == 2).Id, 
                Title = _dbContext.Tasks.First(x => x.Id == 2).Title,
                PrioritySort = _dbContext.Tasks.First(x => x.Id == 2).PrioritySort,
            }
        };
        
        await _distributedCache!.SetStringAsync($"tasks_by_board_{request.ProjectId}", JsonSerializer.Serialize(listDto));
        await _distributedCache!.SetStringAsync($"task_{request.TaskId1}", JsonSerializer.Serialize(listDto.First()));
        await _distributedCache!.SetStringAsync($"task_{request.TaskId2}", JsonSerializer.Serialize(listDto[1]));
        
        var projectSecurityServiceMock = SetupProjectSecurityServiceMock();
        
        var mockTaskService = SetupMockTaskService();
        mockTaskService.Setup(x => x.SwapTasksPrioritySort(It.IsAny<TaskM>(), 
            It.IsAny<TaskM>())
        ).Callback((TaskM task1, TaskM task2) =>
        {
            (task1.PrioritySort, task2.PrioritySort) = (task2.PrioritySort, task1.PrioritySort);
        });
            
        var swapTasksPrioritySortRequestHandler = new  SwapTasksPrioritySortRequestHandler(
            _dbContext, 
            projectSecurityServiceMock.Object,
            mockTaskService.Object,
            SetupMockMediatorService().Object,
            _distributedCache!
        );

        await swapTasksPrioritySortRequestHandler.Handle(request, CancellationToken.None);
            
        projectSecurityServiceMock.Verify(x => x.CheckAccess(It.IsIn(request.ProjectId), ProjectRoles.ProjectUpdate), Times.Once);
        
        mockTaskService.Verify(x => x.SwapTasksPrioritySort(It.IsAny<TaskM>(), It.IsAny<TaskM>()), Times.Once);
            
        Assert.NotNull(_dbContext.Tasks.FirstOrDefault(x => x.Id == 1 && x.PrioritySort == 2));
        Assert.NotNull(_dbContext.Tasks.FirstOrDefault(x => x.Id == 2 && x.PrioritySort == 1));
            
        Assert.Null(await _distributedCache!.GetStringAsync($"task_{request.TaskId1}"));
        Assert.Null(await _distributedCache!.GetStringAsync($"task_{request.TaskId2}"));
        Assert.Null(await _distributedCache!.GetStringAsync($"tasks_by_board_{request.BoardId}"));
        
        CleanUp();
    }
    
    [Fact]
    public async void Handle_Task1EqualTask2_SendTaskIdsEqualsIdError()
    {
        SetupDbContext();
    
        var request = new SwapTasksPrioritySortRequest() { ProjectId = 1, BoardId = 1, TaskId1 = 10, TaskId2 = 10 };
        
        var mediatorMock = SetupMockMediatorService();
        var error = new TaskIdsEqualsIdError();
            
        var projectSecurityServiceMock = SetupProjectSecurityServiceMock();
        
        var swapTasksPrioritySortRequestHandler = new  SwapTasksPrioritySortRequestHandler(
            _dbContext, 
            projectSecurityServiceMock.Object,
            SetupMockTaskService().Object,
            mediatorMock.Object,
            _distributedCache!
        );
            
        await swapTasksPrioritySortRequestHandler.Handle(request, CancellationToken.None);
        
        projectSecurityServiceMock.Verify(x => x.CheckAccess(It.IsIn(request.ProjectId), ProjectRoles.ProjectUpdate), Times.Once);
        
        mediatorMock.Verify(x => x.Send(It.Is<SendErrorToClientRequest>(y => 
                    y.Error.GetType() == error.GetType()), 
                It.IsAny<CancellationToken>()), Times.Once, $"Error request type is not a { error.GetType() }");

        CleanUp();
    }
    
    [Fact]
    public async void Handle_Task1NotFound_SendTaskNotFoundError()
    {
        SetupDbContext();
    
        var request = new SwapTasksPrioritySortRequest() { ProjectId = 1, BoardId = 1, TaskId1 = 10, TaskId2 = 2 };
        
        var mediatorMock = SetupMockMediatorService();
        var error = new TaskNotFoundError();
            
        var projectSecurityServiceMock = SetupProjectSecurityServiceMock();
        
        var swapTasksPrioritySortRequestHandler = new SwapTasksPrioritySortRequestHandler(
            _dbContext, 
            projectSecurityServiceMock.Object,
            SetupMockTaskService().Object,
            mediatorMock.Object,
            _distributedCache!
        );
            
        await swapTasksPrioritySortRequestHandler.Handle(request, CancellationToken.None);
        
        projectSecurityServiceMock.Verify(x => x.CheckAccess(It.IsIn(request.ProjectId), ProjectRoles.ProjectUpdate), Times.Once);
        
        mediatorMock.Verify(x => x.Send(It.Is<SendErrorToClientRequest>(y => 
                    y.Error.GetType() == error.GetType()), 
                It.IsAny<CancellationToken>()), Times.Once, $"Error request type is not a { error.GetType() }");

        CleanUp();
    }
    
    [Fact]
    public async void Handle_Task2NotFound_SendTaskNotFoundError()
    {
        SetupDbContext();
    
        var request = new SwapTasksPrioritySortRequest() { ProjectId = 1, BoardId = 1, TaskId1 = 1, TaskId2 = 10 };
        
        var mediatorMock = SetupMockMediatorService();
        var error = new TaskNotFoundError();
            
        var projectSecurityServiceMock = SetupProjectSecurityServiceMock();
        
        var swapTasksPrioritySortRequestHandler = new SwapTasksPrioritySortRequestHandler(
            _dbContext, 
            projectSecurityServiceMock.Object,
            SetupMockTaskService().Object,
            mediatorMock.Object,
            _distributedCache!
        );
            
        await swapTasksPrioritySortRequestHandler.Handle(request, CancellationToken.None);
        
        projectSecurityServiceMock.Verify(x => x.CheckAccess(It.IsIn(request.ProjectId), ProjectRoles.ProjectUpdate), Times.Once);
        
        mediatorMock.Verify(x => x.Send(It.Is<SendErrorToClientRequest>(y => 
                    y.Error.GetType() == error.GetType()), 
                It.IsAny<CancellationToken>()), Times.Once, $"Error request type is not a { error.GetType() }");

        CleanUp();
    }
    
    [Fact]
    public async void Handle_Task1BoardNotEqualTask2Board_SendDifferentBoardError()
    {
        SetupDbContext();
    
        var request = new SwapTasksPrioritySortRequest() { ProjectId = 1, BoardId = 1, TaskId1 = 1, TaskId2 = 100 };
        
        var mediatorMock = SetupMockMediatorService();
        var error = new TaskNotFoundError();
            
        var projectSecurityServiceMock = SetupProjectSecurityServiceMock();
        
        var swapTasksPrioritySortRequestHandler = new SwapTasksPrioritySortRequestHandler(
            _dbContext, 
            projectSecurityServiceMock.Object,
            SetupMockTaskService().Object,
            mediatorMock.Object,
            _distributedCache!
        );
            
        await swapTasksPrioritySortRequestHandler.Handle(request, CancellationToken.None);
        
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
        
    private static Mock<IProjectSecurityService> SetupProjectSecurityServiceMock()
    {
        var mock = new Mock<IProjectSecurityService>();
        mock.Setup(x => x.CheckAccess(It.IsAny<Project>(), It.IsAny<ProjectRoles>())).Verifiable();
            
        return new Mock<IProjectSecurityService>();
    }
        
    private static Mock<ITaskService> SetupMockTaskService()
    {
        var mock = new Mock<ITaskService>();
            
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
        _dbContext.Boards.Add(new Board() {Title = "Tes Boards not equal", PrioritySort = 2, Project = _dbContext.Projects.First()});
        _dbContext.SaveChanges();

        _dbContext.Tasks.Add(new TaskM() { Title = "Test Task1", PrioritySort = 1, Board = _dbContext.Boards.First() });
        _dbContext.Tasks.Add(new TaskM() { Title = "Test Task2", PrioritySort = 2, Board = _dbContext.Boards.First() });
        _dbContext.Tasks.Add(new TaskM() { Title = "Test Boards not equal", PrioritySort = 2, Board = _dbContext.Boards.First(x => x.Id == 3) });
        _dbContext.SaveChanges();
    }
    
    private void CleanUp()
    {
        _dbContext?.Database.EnsureDeleted();
        _dbContext?.Dispose();
        
        _distributedCache = null!;
    }
}