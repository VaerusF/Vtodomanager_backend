using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Moq;
using VtodoManager.NewsService.DataAccess.Postgres;
using VtodoManager.NewsService.DomainServices.Interfaces;
using VtodoManager.NewsService.Infrastructure.Interfaces.Services;
using VtodoManager.NewsService.Tests.Utils;
using VtodoManager.NewsService.UseCases.Handlers.News.Commands.CreateNews;
using VtodoManager.NewsService.UseCases.Handlers.News.Dto;
using Xunit;

namespace VtodoManager.NewsService.UseCases.Tests.Unit.Handlers.News.Commands;

public class CreateNewsRequestHandlerTest
{
    private AppDbContext _dbContext = null!;
    private IDistributedCache? _distributedCache = null!;

    [Fact]
    public async void Handle_SuccessfulCreateNews_ReturnsTask()
    {
        SetupDbContext();
        SetupDistributedCache();

        var createNewsDto = new CreateNewsDto() { Title = "Test news", Content = "Test news content"};
        var request = new CreateNewsRequest() { CreateNewsDto = createNewsDto };
        
        var newsService = SetupNewsService();
        newsService.Setup(x => x.CreateNews(
            It.IsAny<string>(),
            It.IsAny<string>())
        ).Returns(new Entities.Models.News()
        {
            Title = createNewsDto.Title,
            Content = createNewsDto.Content
        });

        var redisCacheUtilsService = SetupRedisKeysUtilsService();
        redisCacheUtilsService
            .Setup(x => x.RemoveKeysByKeyroot(It.IsAny<string>()))
            .Verifiable();

        var createNewsRequestHandler = new CreateNewsRequestHandler(
            _dbContext,
            SetupMockMediatorService().Object,
            _distributedCache!,
            newsService.Object,
            redisCacheUtilsService.Object
        );

        await createNewsRequestHandler.Handle(request, CancellationToken.None);
        
        redisCacheUtilsService
            .Verify(x => x.RemoveKeysByKeyroot(
                It.Is<string>(y => y == "news_paged_")), 
                Times.Once
            );

        Assert.NotNull(_dbContext.News.FirstOrDefault(x => 
            x.Title == createNewsDto.Title && 
            x.Content == createNewsDto.Content)
        );
        
        CleanUp();
    }
    
    private static Mock<IMediator> SetupMockMediatorService()
    {
        var mock = new Mock<IMediator>();
            
        return mock;
    }
    
    private static Mock<INewsService> SetupNewsService()
    {
        var mock = new Mock<INewsService>();
            
        return new Mock<INewsService>();
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
    }
        
    private void CleanUp()
    {
        _dbContext?.Database.EnsureDeleted();
        _dbContext?.Dispose();
            
        _distributedCache = null!;
    }
}