using MediatR;

namespace Vtodo.UseCases.Handlers.Tasks.Commands.SwapTasksPrioritySort;

public class SwapTasksPrioritySortRequest : IRequest
{
    public long ProjectId { get; set; }
    public long BoardId { get; set; }
    public long TaskId1 { get; set; }
    public long TaskId2 { get; set; }
}