using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using VtodoManager.NewsService.DomainServices.Interfaces;
using VtodoManager.NewsService.Infrastructure.Interfaces.DataAccess;
using VtodoManager.NewsService.UseCases.Handlers.Errors.Commands;
using VtodoManager.NewsService.UseCases.Handlers.Errors.Dto.NotFound;

namespace VtodoManager.NewsService.UseCases.Handlers.News.Commands.UpdateNews;

internal class UpdateNewsRequestHandler: IRequestHandler<UpdateNewsRequest>
{
    private readonly IDbContext _dbContext;
    private readonly IMediator _mediator;
    private readonly IDistributedCache _distributedCache;
    private readonly INewsService _newsService;

    public UpdateNewsRequestHandler(
        IDbContext dbContext, 
        IMediator mediator, 
        IDistributedCache distributedCache,
        INewsService newsService)
    {
        _dbContext = dbContext;
        _mediator = mediator;
        _distributedCache = distributedCache;
        _newsService = newsService;
    }
    
    public async Task Handle(UpdateNewsRequest request, CancellationToken cancellationToken)
    {
        var updateNewsDto = request.UpdateNewsDto;

        var news = _dbContext.News.FirstOrDefault(x => x.Id == request.NewsId);
            
        if (news == null)
        {
            await _mediator.Send(new SendErrorToClientRequest() { Error = new NewsNotFoundError() }, cancellationToken); 
            return;
        }
        
        _newsService.UpdateNews(news, updateNewsDto.Title, updateNewsDto.Content);
        
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}