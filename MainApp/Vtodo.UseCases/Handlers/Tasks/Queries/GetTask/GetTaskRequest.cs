using MediatR;
using Vtodo.UseCases.Handlers.Tasks.Dto;

namespace Vtodo.UseCases.Handlers.Tasks.Queries.GetTask
{
    public class GetTaskRequest : IRequest<TaskDto>
    {
        public long Id { get; set; }
    }
}