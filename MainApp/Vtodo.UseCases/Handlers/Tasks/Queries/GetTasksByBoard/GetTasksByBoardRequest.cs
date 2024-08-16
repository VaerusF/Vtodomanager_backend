using System.ComponentModel.DataAnnotations;
using MediatR;
using Vtodo.UseCases.Handlers.Tasks.Dto;

namespace Vtodo.UseCases.Handlers.Tasks.Queries.GetTasksByBoard
{
    public class GetTasksByBoardRequest : IRequest<List<TaskDto>>
    {
        [Required]
        public long ProjectId { get; set; }
        
        [Required]
        public long BoardId { get; set; }
    }
}