using System.ComponentModel.DataAnnotations;
using MediatR;

namespace Vtodo.UseCases.Handlers.Boards.Commands.DeleteBoard
{
    public class DeleteBoardRequest : IRequest
    {
        [Required]
        public long ProjectId { get; set; }
        [Required]
        public long BoardId { get; set; }
    }
}