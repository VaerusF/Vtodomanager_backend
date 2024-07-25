using MediatR;
using Vtodo.UseCases.Handlers.ProjectsRoles.Dto;

namespace Vtodo.UseCases.Handlers.ProjectsRoles.Commands.GrantRole
{
    public class GrantRoleRequest : IRequest
    {
        public long ProjectId { get; set; }
        
        public GrantRoleDto GrantRoleDto { get; set; } = null!;
    }
}