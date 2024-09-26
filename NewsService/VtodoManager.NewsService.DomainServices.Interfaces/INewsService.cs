using VtodoManager.NewsService.Entities.Models;

namespace VtodoManager.NewsService.DomainServices.Interfaces;

internal interface INewsService
{
    News CreateNews(string title, string content);
    void UpdateNews(News news, string title, string content);
}