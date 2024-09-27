using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using VtodoManager.NewsService.Infrastructure.Interfaces.DataAccess;
using VtodoManager.NewsService.UseCases.Handlers.Errors.Commands;
using VtodoManager.NewsService.UseCases.Handlers.Errors.Dto.NotFound;
using VtodoManager.NewsService.UseCases.Handlers.News.Dto;

namespace VtodoManager.NewsService.UseCases.Handlers.News.Queries.GetOneNews;

internal class GetOneNewsRequestHandler : IRequestHandler<GetOneNewsRequest, NewsDto?>
{
    private readonly IDbContext _dbContext;
    private readonly IMediator _mediator;
    private readonly IDistributedCache _distributedCache;
    
    public GetOneNewsRequestHandler(
        IDbContext dbContext, 
        IMediator mediator, 
        IDistributedCache distributedCache)
    {
        _dbContext = dbContext;
        _mediator = mediator;
        _distributedCache = distributedCache;
    }
    
    public async Task<NewsDto?> Handle(GetOneNewsRequest request, CancellationToken cancellationToken)
    {
        var newsStringFromCache = await _distributedCache.GetStringAsync($"news_{request.NewsId}", cancellationToken);
        
        if (newsStringFromCache != null) return JsonSerializer.Deserialize<NewsDto>(newsStringFromCache);
        
        var news = _dbContext.News
            .AsNoTracking()
            .FirstOrDefault(x => x.Id == request.NewsId);
        
        if (news == null)
        {
            await _mediator.Send(new SendErrorToClientRequest() { Error = new NewsNotFoundError() }, cancellationToken); 
            return null;
        }

        var result = new NewsDto()
        {
            Id = news.Id,
            Title = news.Title,
            Content = news.Content,
            CreatedAt = new DateTimeOffset(news.CreatedAt).ToUnixTimeMilliseconds()
        };
        
        await _distributedCache.SetStringAsync($"news_{request.NewsId}", JsonSerializer.Serialize(result), cancellationToken);

        return result;
    }
}