using MediatR;
using Vtodo.UseCases.Handlers.ProjectsRoles.Dto;

namespace Vtodo.UseCases.Handlers.ProjectsRoles.Commands.ChangeOwner
{
    public class ChangeOwnerRequest : IRequest
    {
        public long ProjectId { get; set; }
        
        public ChangeOwnerDto ChangeOwnerDto { get; set; } = null!;
    }
}