using MediatR;

namespace Vtodo.UseCases.Handlers.Boards.Commands.SwapBoardsPrioritySort;

public class SwapBoardsPrioritySortRequest : IRequest
{
    public long BoardId1 { get; set; }
    public long BoardId2 { get; set; }
}