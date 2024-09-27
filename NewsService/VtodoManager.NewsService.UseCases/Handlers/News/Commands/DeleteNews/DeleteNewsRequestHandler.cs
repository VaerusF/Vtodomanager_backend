using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using VtodoManager.NewsService.Infrastructure.Interfaces.DataAccess;
using VtodoManager.NewsService.Infrastructure.Interfaces.Services;
using VtodoManager.NewsService.UseCases.Handlers.Errors.Commands;
using VtodoManager.NewsService.UseCases.Handlers.Errors.Dto.NotFound;

namespace VtodoManager.NewsService.UseCases.Handlers.News.Commands.DeleteNews;

internal class DeleteNewsRequestHandler: IRequestHandler<DeleteNewsRequest>
{
    private readonly IDbContext _dbContext;
    private readonly IMediator _mediator;
    private readonly IDistributedCache _distributedCache;
    private readonly IRedisKeysUtilsService _redisKeysUtilsService;
    
    public DeleteNewsRequestHandler(
        IDbContext dbContext, 
        IMediator mediator, 
        IDistributedCache distributedCache,
        IRedisKeysUtilsService redisKeysUtilsService)
    {
        _dbContext = dbContext;
        _mediator = mediator;
        _distributedCache = distributedCache;
        _redisKeysUtilsService = redisKeysUtilsService;
    }

    public async Task Handle(DeleteNewsRequest request, CancellationToken cancellationToken)
    {
        var news = _dbContext.News.FirstOrDefault(x => x.Id == request.NewsId);
            
        if (news == null)
        {
            await _mediator.Send(new SendErrorToClientRequest() { Error = new NewsNotFoundError() }, cancellationToken); 
            return;
        }

        _dbContext.News.Remove(news);
        await _dbContext.SaveChangesAsync(cancellationToken);
        
        await _distributedCache.RemoveAsync($"news_{request.NewsId}", cancellationToken);
        await _redisKeysUtilsService.RemoveKeysByKeyroot("news_paged_");
    }
}