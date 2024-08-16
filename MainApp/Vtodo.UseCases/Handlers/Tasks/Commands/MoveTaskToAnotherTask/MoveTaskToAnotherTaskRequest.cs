using System.ComponentModel.DataAnnotations;
using MediatR;

namespace Vtodo.UseCases.Handlers.Tasks.Commands.MoveTaskToAnotherTask
{
    public class MoveTaskToAnotherTaskRequest : IRequest
    {
        [Required]
        public long ProjectId { get; set; }
        
        [Required]
        public long BoardId { get; set; }
        
        [Required]
        public long TaskId { get; set; }
        
        [Required]
        public long NewParentTaskId { get; set; }
    }
}