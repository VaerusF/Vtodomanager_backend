using System.ComponentModel.DataAnnotations;
using Vtodo.Entities.Enums;

namespace Vtodo.UseCases.Handlers.Tasks.Dto
{
    public class UpdateTaskDto
    {
        [Required]
        [StringLength(255, MinimumLength = 3)]
        public string Title { get; set; } = string.Empty;
        
        [Required]
        public string Description { get; set; } = string.Empty;
        
        public long? EndDateTimeStamp { get; set; }
        
        [Required]
        public int PrioritySort { get; set; }
        
    }
}