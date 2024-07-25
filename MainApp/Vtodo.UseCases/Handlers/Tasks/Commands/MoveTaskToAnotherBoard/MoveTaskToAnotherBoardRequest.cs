using MediatR;

namespace Vtodo.UseCases.Handlers.Tasks.Commands.MoveTaskToAnotherBoard
{
    public class MoveTaskToAnotherBoardRequest : IRequest
    {
        public long TaskId { get; set; }
        public long NewBoardId { get; set; }
    }
}