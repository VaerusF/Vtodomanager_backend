using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VtodoManager.NewsService.Entities.Models;

public class NewsFile
{
    [ForeignKey("News"), Column(Order = 0)]
    public long NewsId { get; set; }
        
    [Required]
    public string FileName { get; set; } = string.Empty;
}