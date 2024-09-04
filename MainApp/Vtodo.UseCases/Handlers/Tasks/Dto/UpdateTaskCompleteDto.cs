using System.ComponentModel.DataAnnotations;

namespace Vtodo.UseCases.Handlers.Tasks.Dto
{
    public class UpdateTaskCompleteDto
    {
        [Required]
        public bool IsCompleted { get; set; }
    }
}