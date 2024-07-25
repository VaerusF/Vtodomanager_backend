using MediatR;
using Vtodo.UseCases.Handlers.Projects.Dto;

namespace Vtodo.UseCases.Handlers.Projects.Queries.GetProject
{
    public class GetProjectRequest : IRequest<ProjectDto>
    {
        public long Id { get; set; }
    }
}