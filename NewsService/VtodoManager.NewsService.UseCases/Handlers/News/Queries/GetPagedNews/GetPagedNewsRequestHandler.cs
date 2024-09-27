using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using VtodoManager.NewsService.Infrastructure.Interfaces.DataAccess;
using VtodoManager.NewsService.UseCases.Handlers.Errors.Commands;
using VtodoManager.NewsService.UseCases.Handlers.News.Dto;

namespace VtodoManager.NewsService.UseCases.Handlers.News.Queries.GetPagedNews;

internal class GetPagedNewsRequestHandler: IRequestHandler<GetPagedNewsRequest, List<NewsDto>>
{
    private readonly IDbContext _dbContext;
    private readonly IMediator _mediator;
    private readonly IDistributedCache _distributedCache;

    public GetPagedNewsRequestHandler(
        IDbContext dbContext, 
        IMediator mediator, 
        IDistributedCache distributedCache)
    {
        _dbContext = dbContext;
        _mediator = mediator;
        _distributedCache = distributedCache;
    }
    
    public async Task<List<NewsDto>> Handle(GetPagedNewsRequest request, CancellationToken cancellationToken)
    {
        var pagedDto = request.GetPagedNewsDto;
        
        var newsStringFromCache = await _distributedCache
            .GetStringAsync($"news_paged_{pagedDto.PageCount}_count_{pagedDto.CountOnPage}", cancellationToken);
        
        if (newsStringFromCache != null) return JsonSerializer.Deserialize<List<NewsDto>>(newsStringFromCache) ?? [];
        
        var news = _dbContext.News
            .AsNoTracking()
            .Skip((pagedDto.PageCount - 1) * pagedDto.CountOnPage)
            .Take(pagedDto.CountOnPage)
            .ToList();

        var listDto = news
            .Select(oneNews => new NewsDto()
            {
                Id = oneNews.Id, 
                Title = oneNews.Title, 
                Content = oneNews.Content,
                CreatedAt = new DateTimeOffset(oneNews.CreatedAt).ToUnixTimeMilliseconds()
            })
            .ToList();

        await _distributedCache
            .SetStringAsync($"news_paged_{pagedDto.PageCount}_count_{pagedDto.CountOnPage}", 
                JsonSerializer.Serialize(listDto), cancellationToken);
        
        return listDto;
    }
}