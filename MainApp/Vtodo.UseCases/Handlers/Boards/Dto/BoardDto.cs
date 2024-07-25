using System.ComponentModel.DataAnnotations;

namespace Vtodo.UseCases.Handlers.Boards.Dto
{
    public class BoardDto
    {
        public long Id { get; set; }
        
        [Required]
        public string Title { get; set; } = string.Empty;
        
        [Required]
        public int PrioritySort { get; set; }
        
        [Required]
        public long ProjectId { get; set; }

        public string? ImageHeaderPath { get; set; }
    }
}