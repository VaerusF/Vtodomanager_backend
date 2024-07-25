using MediatR;
using Vtodo.UseCases.Handlers.ProjectsRoles.Dto;

namespace Vtodo.UseCases.Handlers.ProjectsRoles.Commands.AddMember
{
    public class AddMemberRequest : IRequest
    {
        public long ProjectId { get; set; }
        
        public AddMemberDto AddMemberDto { get; set; } = null!;
    }
}