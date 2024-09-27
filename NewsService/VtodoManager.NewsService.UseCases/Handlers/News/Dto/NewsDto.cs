using System.ComponentModel.DataAnnotations;

namespace VtodoManager.NewsService.UseCases.Handlers.News.Dto;

public class NewsDto
{
    public long Id { get; set; }
    
    [Required]
    public string Title { get; set; } = string.Empty;
    
    [Required]
    public string Content { get; set; } = string.Empty;
    
    [Required]
    public long CreatedAt { get; set; }
}