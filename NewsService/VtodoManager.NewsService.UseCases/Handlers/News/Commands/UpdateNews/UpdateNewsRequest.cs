using MediatR;
using VtodoManager.NewsService.UseCases.Handlers.News.Dto;

namespace VtodoManager.NewsService.UseCases.Handlers.News.Commands.UpdateNews;

public class UpdateNewsRequest : IRequest
{
    public long NewsId { get; set; }
    public UpdateNewsDto UpdateNewsDto { get; set; } = null!;
}