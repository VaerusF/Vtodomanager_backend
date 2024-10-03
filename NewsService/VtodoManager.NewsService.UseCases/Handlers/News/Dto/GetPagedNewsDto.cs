using System.ComponentModel.DataAnnotations;

namespace VtodoManager.NewsService.UseCases.Handlers.News.Dto;

public class GetPagedNewsDto
{
    [Range(1, int.MaxValue)]
    public uint PageCount { get; set; }
    
    [Range(1, int.MaxValue)]
    public uint CountOnPage { get; set; }

}