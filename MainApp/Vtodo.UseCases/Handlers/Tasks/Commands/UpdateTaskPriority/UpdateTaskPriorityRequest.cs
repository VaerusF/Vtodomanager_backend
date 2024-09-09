using System.ComponentModel.DataAnnotations;
using MediatR;
using Vtodo.UseCases.Handlers.Tasks.Dto;

namespace Vtodo.UseCases.Handlers.Tasks.Commands.UpdateTaskPriority
{
    public class UpdateTaskPriorityRequest : IRequest
    {
        [Required]
        public long ProjectId { get; set; }
        
        [Required]
        public long BoardId { get; set; }
        
        [Required]
        public long TaskId { get; set; }
        public UpdateTaskPriorityDto UpdateTaskPriorityDto { get; set; } = null!;
    }
}