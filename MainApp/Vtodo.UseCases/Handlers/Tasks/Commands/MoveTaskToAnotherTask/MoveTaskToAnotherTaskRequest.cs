using MediatR;

namespace Vtodo.UseCases.Handlers.Tasks.Commands.MoveTaskToAnotherTask
{
    public class MoveTaskToAnotherTaskRequest : IRequest
    {
        public long TaskId { get; set; }
        public long NewParentTaskId { get; set; }
    }
}