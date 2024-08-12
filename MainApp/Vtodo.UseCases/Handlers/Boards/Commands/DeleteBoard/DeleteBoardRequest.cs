using MediatR;
using Vtodo.UseCases.Handlers.Boards.Dto;

namespace Vtodo.UseCases.Handlers.Boards.Commands.DeleteBoard
{
    public class DeleteBoardRequest : IRequest
    {
        public long ProjectId { get; set; }
        public long BoardId { get; set; }
    }
}