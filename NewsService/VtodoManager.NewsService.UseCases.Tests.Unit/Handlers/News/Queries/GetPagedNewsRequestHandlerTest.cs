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
using VtodoManager.NewsService.UseCases.Handlers.News.Queries.GetPagedNews;
using Xunit;

namespace VtodoManager.NewsService.UseCases.Tests.Unit.Handlers.News.Queries;

public class GetPagedNewsRequestHandlerTest
{
    private AppDbContext _dbContext = null!;
    private IDistributedCache? _distributedCache = null!;

    [Fact]
    public async void Handle_SuccessfulGetListNewsFromCache_ReturnTaskListNewsDto()
    {
        SetupDbContext();
        SetupDistributedCache();

        var dto = new GetPagedNewsDto() { PageCount = 1, CountOnPage = 6 };
        var request = new GetPagedNewsRequest()
        {
            GetPagedNewsDto = dto
        };

        var listCachedDto = new List<NewsDto>();
        
        for (var i = 1; i < dto.CountOnPage; i++)
        {
            listCachedDto.Add(new NewsDto()
            {
                Id = i,
                Title = $"Test get {i} title news",
                Content = $"Test get {i} content news"
            });
        }
        
        await _distributedCache!
            .SetStringAsync($"news_paged_{dto.CountOnPage}_count_{dto.CountOnPage}", 
                JsonSerializer.Serialize(listCachedDto));

        var getPagedNewsRequestHandler = new GetPagedNewsRequestHandler(
            _dbContext, 
            SetupMockMediatorService().Object, 
            _distributedCache!
        );

        var result = await getPagedNewsRequestHandler.Handle(request, CancellationToken.None);
        
        Assert.NotNull(result);
        Assert.NotNull(await _distributedCache!.GetStringAsync($"news_paged_{dto.PageCount}_count_{dto.CountOnPage}"));
        
        Assert.Equal(dto.CountOnPage, result.Count);
        CleanUp();
    }
    
    [Fact]
    public async void Handle_SuccessfulGetListNewsFromDb_ReturnTaskListNewsDto()
    {
        SetupDbContext();
        SetupDistributedCache();

        var dto = new GetPagedNewsDto() { PageCount = 11, CountOnPage = 10 };
        var request = new GetPagedNewsRequest()
        {
            GetPagedNewsDto = dto
        };
        
        var getPagedNewsRequestHandler = new GetPagedNewsRequestHandler(
            _dbContext, 
            SetupMockMediatorService().Object, 
            _distributedCache!
        );

        var result = await getPagedNewsRequestHandler.Handle(request, CancellationToken.None);
        
        Assert.NotNull(result);
        Assert.NotNull(await _distributedCache!.GetStringAsync($"news_paged_{dto.PageCount}_count_{dto.CountOnPage}"));
        
        Assert.Equal(5, result.Count);
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

        for (var i = 1; i <= 105; i++)
        {
            _dbContext.News.Add(new Entities.Models.News()
            {
                Title = $"Test get {i} title news",
                Content = $"Test get {i} content news"
            });
        }

        _dbContext.SaveChangesAsync();
    }
        
    private void CleanUp()
    {
        _dbContext?.Database.EnsureDeleted();
        _dbContext?.Dispose();
            
        _distributedCache = null!;
    }
}