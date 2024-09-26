using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using VtodoManager.NewsService.DomainServices.Interfaces;
using VtodoManager.NewsService.Infrastructure.Interfaces.DataAccess;

namespace VtodoManager.NewsService.UseCases.Handlers.News.Commands.CreateNews;

internal class CreateNewsRequestHandler: IRequestHandler<CreateNewsRequest>
{
    private readonly IDbContext _dbContext;
    private readonly IMediator _mediator;
    private readonly IDistributedCache _distributedCache;
    private readonly INewsService _newsService;

    public CreateNewsRequestHandler(
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
    
    public async Task Handle(CreateNewsRequest request, CancellationToken cancellationToken)
    {
        var createDto = request.CreateNewsDto;

        var newNews = _newsService.CreateNews(createDto.Title, createDto.Content);

        _dbContext.News.Add(newNews);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}