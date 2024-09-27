using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using VtodoManager.NewsService.DomainServices.Interfaces;
using VtodoManager.NewsService.Infrastructure.Interfaces.DataAccess;
using VtodoManager.NewsService.Infrastructure.Interfaces.Services;

namespace VtodoManager.NewsService.UseCases.Handlers.News.Commands.CreateNews;

internal class CreateNewsRequestHandler: IRequestHandler<CreateNewsRequest>
{
    private readonly IDbContext _dbContext;
    private readonly IMediator _mediator;
    private readonly IDistributedCache _distributedCache;
    private readonly INewsService _newsService;
    private readonly IRedisKeysUtilsService _redisKeysUtilsService;

    public CreateNewsRequestHandler(
        IDbContext dbContext, 
        IMediator mediator, 
        IDistributedCache distributedCache,
        INewsService newsService,
        IRedisKeysUtilsService redisKeysUtilsService)
    {
        _dbContext = dbContext;
        _mediator = mediator;
        _distributedCache = distributedCache;
        _newsService = newsService;
        _redisKeysUtilsService = redisKeysUtilsService;
    }
    
    public async Task Handle(CreateNewsRequest request, CancellationToken cancellationToken)
    {
        var createDto = request.CreateNewsDto;

        var newNews = _newsService.CreateNews(createDto.Title, createDto.Content);

        _dbContext.News.Add(newNews);
        await _dbContext.SaveChangesAsync(cancellationToken);
        
        await _redisKeysUtilsService.RemoveKeysByKeyroot("news_paged_");
    }
}