using MediatR;
using VtodoManager.NewsService.UseCases.Handlers.News.Dto;

namespace VtodoManager.NewsService.UseCases.Handlers.News.Commands.CreateNews;

public class CreateNewsRequest : IRequest
{
    public CreateNewsDto CreateNewsDto { get; set; } = null!;
}