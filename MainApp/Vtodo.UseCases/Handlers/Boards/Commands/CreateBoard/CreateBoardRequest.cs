using MediatR;
using Vtodo.UseCases.Handlers.Boards.Dto;

namespace Vtodo.UseCases.Handlers.Boards.Commands.CreateBoard
{
    public class CreateBoardRequest : IRequest
    {
        public long ProjectId { get; set; }
        public CreateBoardDto CreateBoardDto { get; set; } = null!;
    }
}