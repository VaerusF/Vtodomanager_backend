using System.ComponentModel.DataAnnotations;

namespace Vtodo.UseCases.Handlers.Tasks.Dto
{
    public class TaskDto
    {
        public long Id { get; set; }
        
        [Required]
        [StringLength(255, MinimumLength = 3)]
        public string Title { get; set; } = string.Empty;
        
        [Required]
        public string Description { get; set; } = string.Empty;
        
        [Required]
        public long? EndDate { get; set; }
        
        [Required]
        public bool IsCompleted { get; set; }
        
        [Required]
        public long BoardId { get; set; }
        
        public long? ParentId { get; set; }
        
        [Required]
        public int PrioritySort { get; set; }
        
        [Required]
        public int Priority { get; set; }
        
        public string? ImageHeaderPath { get; set; }
    }
}