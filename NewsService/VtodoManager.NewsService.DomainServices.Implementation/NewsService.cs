using VtodoManager.NewsService.DomainServices.Interfaces;
using VtodoManager.NewsService.Entities.Exceptions;
using VtodoManager.NewsService.Entities.Models;

namespace VtodoManager.NewsService.DomainServices.Implementation;

internal class NewsService : INewsService
{
    public News CreateNews(string title, string content)
    {
        if (title.Length < 3) throw new InvalidStringLengthException("Invalid title length");
        if (content.Length < 3) throw new InvalidStringLengthException("Invalid content length");
        
        var newNews =  new News()
        {
            Title = title,
            Content = content
        };

        return newNews;
    }

    public void UpdateNews(News news, string title, string content)
    {
        if (title.Length < 3) throw new InvalidStringLengthException("Invalid title length");
        if (content.Length < 3) throw new InvalidStringLengthException("Invalid content length");
        
        news.Title = title;
        news.Content = content;
    }
}