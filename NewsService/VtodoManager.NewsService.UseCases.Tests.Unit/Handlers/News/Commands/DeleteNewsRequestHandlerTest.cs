using System.Text.Json;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Moq;
using VtodoManager.NewsService.DataAccess.Postgres;
using VtodoManager.NewsService.Infrastructure.Interfaces.Services;
using VtodoManager.NewsService.Tests.Utils;
using VtodoManager.NewsService.UseCases.Handlers.Errors.Commands;
using VtodoManager.NewsService.UseCases.Handlers.Errors.Dto.NotFound;
using VtodoManager.NewsService.UseCases.Handlers.News.Commands.DeleteNews;
using VtodoManager.NewsService.UseCases.Handlers.News.Dto;
using Xunit;

namespace VtodoManager.NewsService.UseCases.Tests.Unit.Handlers.News.Commands;

public class DeleteNewsRequestHandlerTest
{
    private AppDbContext _dbContext = null!;
    private IDistributedCache? _distributedCache = null!;

    [Fact]
    public async void Handle_SuccessfulDeleteNews_ReturnsTask()
    {
        SetupDbContext();
        SetupDistributedCache();
        
        var request = new DeleteNewsRequest() { NewsId = 1};

        var cachedDto = new NewsDto()
        {
            Id = _dbContext.News.FirstOrDefault()!.Id,
            Title = _dbContext.News.FirstOrDefault()!.Title,
            Content = _dbContext.News.FirstOrDefault()!.Content
        };
        
        await _distributedCache!.SetStringAsync($"news_{request.NewsId}", JsonSerializer.Serialize(cachedDto));
        
        var redisCacheUtilsService = SetupRedisKeysUtilsService();
        redisCacheUtilsService
            .Setup(x => x.RemoveKeysByKeyroot(It.IsAny<string>()))
            .Verifiable();

        var deleteNewsRequestHandler = new DeleteNewsRequestHandler(
            _dbContext,
            SetupMockMediatorService().Object,
            _distributedCache!,
            redisCacheUtilsService.Object
        );

        await deleteNewsRequestHandler.Handle(request, CancellationToken.None);
        
        redisCacheUtilsService
            .Verify(x => x.RemoveKeysByKeyroot(
                It.Is<string>(y => y == "news_paged_")), 
                Times.Once
            );

        Assert.Null(_dbContext.News.FirstOrDefault(x => x.Id == 1));
        
        Assert.Null(await _distributedCache!.GetStringAsync($"news_{request.NewsId}"));
        
        CleanUp();
    }
    
    [Fact]
    public async void Handle_NewsNotFound_SendNewsNotFoundError()
    {
        SetupDbContext();
        SetupDistributedCache();
        
        var request = new DeleteNewsRequest() { NewsId = 2};
        
        var error = new NewsNotFoundError();
        var mediatorMock = SetupMockMediatorService();
        
        var redisCacheUtilsService = SetupRedisKeysUtilsService();
        redisCacheUtilsService
            .Setup(x => x.RemoveKeysByKeyroot(It.IsAny<string>()))
            .Verifiable();

        var deleteNewsRequestHandler = new DeleteNewsRequestHandler(
            _dbContext,
            mediatorMock.Object,
            _distributedCache!,
            redisCacheUtilsService.Object
        );

        await deleteNewsRequestHandler.Handle(request, CancellationToken.None);
        
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
    
    private static Mock<IRedisKeysUtilsService> SetupRedisKeysUtilsService()
    {
        var mock = new Mock<IRedisKeysUtilsService>();
            
        return new Mock<IRedisKeysUtilsService>();
    }
    
    private void SetupDistributedCache()
    {
        _distributedCache = TestDbUtils.SetupTestCacheInMemory();
    }
        
    private void SetupDbContext()
    {
        _dbContext = TestDbUtils.SetupTestDbContextInMemory();

        _dbContext.News.Add(new Entities.Models.News()
        {
            Title = "Test delete news",
            Content = "Test delete news content"
        });

        _dbContext.SaveChangesAsync();
    }
        
    private void CleanUp()
    {
        _dbContext?.Database.EnsureDeleted();
        _dbContext?.Dispose();
            
        _distributedCache = null!;
    }
}