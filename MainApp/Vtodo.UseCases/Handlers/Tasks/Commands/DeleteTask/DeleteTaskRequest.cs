using MediatR;

namespace Vtodo.UseCases.Handlers.Tasks.Commands.DeleteTask
{
    public class DeleteTaskRequest : IRequest
    {
        public long Id { get; set; }
    }
}