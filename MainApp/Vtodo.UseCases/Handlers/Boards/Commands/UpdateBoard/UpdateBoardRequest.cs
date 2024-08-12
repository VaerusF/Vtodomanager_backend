using MediatR;
using Vtodo.UseCases.Handlers.Boards.Dto;

namespace Vtodo.UseCases.Handlers.Boards.Commands.UpdateBoard
{
    public class UpdateBoardRequest : IRequest
    {
        public long ProjectId { get; set; }
        public long BoardId { get; set; }
        public UpdateBoardDto UpdateBoardDto { get; set; } = null!;
    }
}