using System.ComponentModel.DataAnnotations;
using MediatR;
using Vtodo.UseCases.Handlers.Tasks.Dto;

namespace Vtodo.UseCases.Handlers.Tasks.Commands.CreateTask
{
    public class CreateTaskRequest : IRequest
    {
        [Required]
        public long ProjectId { get; set; }
        
        [Required]
        public long BoardId { get; set; }
        
        public CreateTaskDto CreateTaskDto { get; set; } = null!;
    }
}