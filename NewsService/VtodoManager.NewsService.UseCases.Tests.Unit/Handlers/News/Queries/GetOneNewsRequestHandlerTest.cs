using System.Text.Json;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Moq;
using VtodoManager.NewsService.DataAccess.Postgres;
using VtodoManager.NewsService.Tests.Utils;
using VtodoManager.NewsService.UseCases.Handlers.Errors.Commands;
using VtodoManager.NewsService.UseCases.Handlers.Errors.Dto.NotFound;
using VtodoManager.NewsService.UseCases.Handlers.News.Dto;
using VtodoManager.NewsService.UseCases.Handlers.News.Queries.GetOneNews;
using Xunit;

namespace VtodoManager.NewsService.UseCases.Tests.Unit.Handlers.News.Queries;

public class GetOneNewsRequestHandlerTest
{
    private AppDbContext _dbContext = null!;
    private IDistributedCache? _distributedCache = null!;

    [Fact]
    public async void Handle_SuccessfulGetNewsFromCache_ReturnTaskNewsDto()
    {
        SetupDbContext();
        SetupDistributedCache();

        var request = new GetOneNewsRequest() { NewsId = 1 };

        var cachedDto = new NewsDto()
        {
            Id = request.NewsId,
            Title = _dbContext.News.FirstOrDefault(x => x.Id == request.NewsId)!.Title,
            Content = _dbContext.News.FirstOrDefault(x => x.Id == request.NewsId)!.Content,
            CreatedAt = new DateTimeOffset(
                _dbContext.News.FirstOrDefault(x => x.Id == request.NewsId)!.CreatedAt)
                .ToUnixTimeMilliseconds()
        };
        
        await _distributedCache!.SetStringAsync($"news_{request.NewsId}", JsonSerializer.Serialize(cachedDto));

        var getOneNewsRequestHandler = new GetOneNewsRequestHandler(
            _dbContext, 
            SetupMockMediatorService().Object, 
            _distributedCache!
        );

        var result = await getOneNewsRequestHandler.Handle(request, CancellationToken.None);
        
        Assert.NotNull(result);
        Assert.NotNull(await _distributedCache!.GetStringAsync($"news_{request.NewsId}"));
        
        Assert.Equal(_dbContext.News.FirstOrDefault(x => x.Id == 1)!.Content, result.Content);
        Assert.Equal(_dbContext.News.FirstOrDefault(x => x.Id == 1)!.Title, result.Title);
        Assert.Equal(new DateTimeOffset(
                _dbContext.News.FirstOrDefault(x => x.Id == 1)!.CreatedAt).ToUnixTimeMilliseconds(), 
            result.CreatedAt
        );
        
        CleanUp();
    }
    
    [Fact]
    public async void Handle_SuccessfulGetNewsFromDb_ReturnTaskNewsDto()
    {
        SetupDbContext();
        SetupDistributedCache();

        var request = new GetOneNewsRequest() { NewsId = 1 };
        
        var getOneNewsRequestHandler = new GetOneNewsRequestHandler(
            _dbContext, 
            SetupMockMediatorService().Object, 
            _distributedCache!
        );

        var result = await getOneNewsRequestHandler.Handle(request, CancellationToken.None);
        
        Assert.NotNull(result);
        Assert.NotNull(await _distributedCache!.GetStringAsync($"news_{request.NewsId}"));
        
        Assert.Equal(_dbContext.News.FirstOrDefault(x => x.Id == 1)!.Content, result.Content);
        Assert.Equal(_dbContext.News.FirstOrDefault(x => x.Id == 1)!.Title, result.Title);
        Assert.Equal(new DateTimeOffset(
            _dbContext.News.FirstOrDefault(x => x.Id == 1)!.CreatedAt).ToUnixTimeMilliseconds(), 
            result.CreatedAt
        );
        
        CleanUp();
    }
    
    [Fact]
    public async void Handle_NewsNotFound_SendNewsNotFoundError()
    {
        SetupDbContext();
        SetupDistributedCache();

        var request = new GetOneNewsRequest() { NewsId = 10 };
        
        var error = new NewsNotFoundError();
        var mediatorMock = SetupMockMediatorService();
        
        var getOneNewsRequestHandler = new GetOneNewsRequestHandler(
            _dbContext, 
            mediatorMock.Object, 
            _distributedCache!
        );

        var result = await getOneNewsRequestHandler.Handle(request, CancellationToken.None);
        
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
    
    private void SetupDistributedCache()
    {
        _distributedCache = TestDbUtils.SetupTestCacheInMemory();
    }
        
    private void SetupDbContext()
    {
        _dbContext = TestDbUtils.SetupTestDbContextInMemory();
        
        _dbContext.News.Add(new Entities.Models.News()
        {
            Title = "Test get one news",
            Content = "Test get one content",
            CreatedAt = DateTime.MinValue.ToUniversalTime()
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