using MediatR;

namespace VtodoManager.NewsService.UseCases.Handlers.News.Commands.DeleteNews;

public class DeleteNewsRequest: IRequest
{
    public long NewsId { get; set; }
}