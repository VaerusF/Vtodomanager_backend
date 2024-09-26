using System.ComponentModel.DataAnnotations;

namespace VtodoManager.NewsService.UseCases.Handlers.News.Dto;

public class CreateNewsDto
{
    [Required]
    [StringLength(255, MinimumLength = 3)]
    public string Title { get; set; } = null!;
    
    [Required]
    [StringLength(255, MinimumLength = 3)]
    public string Content { get; set; } = null!;
}