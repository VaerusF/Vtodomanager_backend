using System.ComponentModel.DataAnnotations;
using MediatR;
using Vtodo.UseCases.Handlers.Tasks.Dto;

namespace Vtodo.UseCases.Handlers.Tasks.Commands.UpdateTask
{
    public class UpdateTaskRequest : IRequest
    {
        [Required]
        public long ProjectId { get; set; }
        
        [Required]
        public long BoardId { get; set; }
        
        [Required]
        public long TaskId { get; set; }
        public UpdateTaskDto UpdateTaskDto { get; set; } = null!;
    }
}