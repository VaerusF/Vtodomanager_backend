using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using VtodoManager.NewsService.Infrastructure.Interfaces.DataAccess;
using VtodoManager.NewsService.UseCases.Handlers.Errors.Commands;
using VtodoManager.NewsService.UseCases.Handlers.Errors.Dto.NotFound;

namespace VtodoManager.NewsService.UseCases.Handlers.News.Commands.DeleteNews;

internal class DeleteNewRequestHandler: IRequestHandler<DeleteNewsRequest>
{
    private readonly IDbContext _dbContext;
    private readonly IMediator _mediator;
    private readonly IDistributedCache _distributedCache;
    
    public DeleteNewRequestHandler(
        IDbContext dbContext, 
        IMediator mediator, 
        IDistributedCache distributedCache)
    {
        _dbContext = dbContext;
        _mediator = mediator;
        _distributedCache = distributedCache;
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
    }
}