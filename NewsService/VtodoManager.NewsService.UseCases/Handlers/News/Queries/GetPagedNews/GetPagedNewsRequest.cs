using MediatR;
using VtodoManager.NewsService.UseCases.Handlers.News.Dto;

namespace VtodoManager.NewsService.UseCases.Handlers.News.Queries.GetPagedNews;

public class GetPagedNewsRequest: IRequest<List<NewsDto>>
{
    public GetPagedNewsDto GetPagedNewsDto { get; set; } = null!;
}