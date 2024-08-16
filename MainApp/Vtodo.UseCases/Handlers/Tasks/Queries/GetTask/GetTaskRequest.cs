using System.ComponentModel.DataAnnotations;
using MediatR;
using Vtodo.UseCases.Handlers.Tasks.Dto;

namespace Vtodo.UseCases.Handlers.Tasks.Queries.GetTask
{
    public class GetTaskRequest : IRequest<TaskDto>
    {
        [Required]
        public long ProjectId { get; set; }
        
        [Required]
        public long TaskId { get; set; }
    }
}