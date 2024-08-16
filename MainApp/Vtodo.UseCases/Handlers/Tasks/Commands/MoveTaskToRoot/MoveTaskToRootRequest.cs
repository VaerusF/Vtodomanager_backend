using System.ComponentModel.DataAnnotations;
using MediatR;

namespace Vtodo.UseCases.Handlers.Tasks.Commands.MoveTaskToRoot
{
    public class MoveTaskToRootRequest : IRequest
    {
        [Required]
        public long ProjectId { get; set; }
        
        [Required]
        public long BoardId { get; set; }
        
        [Required]
        public long TaskId { get; set; }
    }
}