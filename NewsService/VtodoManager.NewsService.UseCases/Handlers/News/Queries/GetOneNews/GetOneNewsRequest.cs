using MediatR;
using VtodoManager.NewsService.UseCases.Handlers.News.Dto;

namespace VtodoManager.NewsService.UseCases.Handlers.News.Queries.GetOneNews;

public class GetOneNewsRequest: IRequest<NewsDto?>
{
    public long NewsId { get; set; }
}