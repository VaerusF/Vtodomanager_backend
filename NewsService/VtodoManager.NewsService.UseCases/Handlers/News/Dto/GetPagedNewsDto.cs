using System.ComponentModel.DataAnnotations;

namespace VtodoManager.NewsService.UseCases.Handlers.News.Dto;

public class GetPagedNewsDto
{
    [Range(1, int.MaxValue)]
    public int PageCount { get; set; }
    
    [Range(1, int.MaxValue)]
    public int CountOnPage { get; set; }

}