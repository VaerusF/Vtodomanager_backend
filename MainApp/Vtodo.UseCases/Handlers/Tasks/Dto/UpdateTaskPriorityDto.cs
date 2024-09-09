using System.ComponentModel.DataAnnotations;
using Vtodo.Entities.Enums;

namespace Vtodo.UseCases.Handlers.Tasks.Dto
{
    public class UpdateTaskPriorityDto
    {
        [Required]
        [EnumDataType(typeof(TaskPriority))]
        public TaskPriority Priority { get; set; }
    }
}