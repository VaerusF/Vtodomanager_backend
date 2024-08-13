using MediatR;
using Microsoft.AspNetCore.Mvc;
using Vtodo.UseCases.Handlers.ProjectsRoles.Commands.AddMember;
using Vtodo.UseCases.Handlers.ProjectsRoles.Commands.ChangeOwner;
using Vtodo.UseCases.Handlers.ProjectsRoles.Commands.GrantRole;
using Vtodo.UseCases.Handlers.ProjectsRoles.Commands.RevokeAllRole;
using Vtodo.UseCases.Handlers.ProjectsRoles.Commands.RevokeRole;
using Vtodo.UseCases.Handlers.ProjectsRoles.Dto;

namespace Vtodo.Controllers
{
    [Route("api/v1/projects/{projectId:long}/roles/")]
    [ApiController]
    public class ProjectsRolesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ProjectsRolesController(IMediator mediator)
        {
            _mediator = mediator;
        }
        
        /// <summary>
        /// Grant role (One of ProjectAdmin, ProjectUpdate)
        /// </summary>
        /// <param name="projectId">Project id</param>
        /// <param name="role">Grant role Dto</param>
        /// <response code="200"></response>
        /// <response code="400">Account not a member in project</response>
        /// <response code="400">Account already has this role</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="402">Attempt change owner or add member</response>
        /// <response code="403">Access denied</response>
        /// <response code="404">Project not found</response>
        /// <response code="404">Account not found</response>
        [HttpPost("grant/")]
        public async Task Grant(long projectId, [FromBody] GrantRoleDto role)
        {
            await _mediator.Send(new GrantRoleRequest() { ProjectId = projectId, GrantRoleDto = role});
        }
        
        /// <summary>
        /// Revoke role (One of ProjectAdmin, ProjectUpdate, ProjectMember)
        /// </summary>
        /// <param name="projectId">Project id</param>
        /// <param name="role">Revoke role Dto</param>
        /// <response code="200"></response>
        /// <response code="400">Account is not a member in project</response>
        /// <response code="400">Account already has this role</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="402">Attempt remove owner</response>
        /// <response code="403">Access denied</response>
        /// <response code="404">Project not found</response>
        /// <response code="404">Account with this role not found in project</response>
        [HttpDelete("revoke/")]
        public async Task Revoke(long projectId, [FromBody] RevokeRoleDto role)
        {
            await _mediator.Send(new RevokeRoleRequest() { ProjectId = projectId, RevokeRoleDto = role});
        }
        
        /// <summary>
        /// Revoke all roles (Kick member) 
        /// </summary>
        /// <param name="projectId">Project id</param>
        /// <param name="role">Kick member Dto</param>
        /// <response code="200"></response>
        /// <response code="400">Account already has this role</response>
        /// <response code="400">Account is not a member in project</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="402">Attempt remove owner</response>
        /// <response code="403">Access denied</response>
        /// <response code="404">Project not found</response>
        [HttpDelete("kick_member/")]
        public async Task RevokeAll(long projectId, [FromBody] KickMemberDto role)
        {
            await _mediator.Send(new KickMemberRequest() { ProjectId = projectId, KickMemberDto = role});
        }
        
        /// <summary>
        /// Grant member role to account
        /// </summary>
        /// <param name="projectId">Project id</param>
        /// <param name="addMemberDto">Add member Dto</param>
        /// <response code="200"></response>
        /// <response code="400">Account already has this role</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="403">Access denied</response>
        /// <response code="404">Project not found</response>
        /// <response code="404">Account not found</response>
        [HttpPost("add_member/")]
        public async Task AddMember(long projectId, [FromBody] AddMemberDto addMemberDto)
        {
            await _mediator.Send(new AddMemberRequest() { ProjectId = projectId, AddMemberDto = addMemberDto});
        }

        /// <summary>
        /// Change project owner
        /// </summary>
        /// <param name="projectId">Project id</param>
        /// <param name="changeOwnerDto">Change owner Dto</param>
        /// <response code="200"></response>
        /// <response code="400">Account is not a member in project</response>
        /// <response code="400">Account already has this role</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="403">Access denied</response>
        /// <response code="404">Project not found</response>
        /// <response code="404">Account not found</response>
        [HttpPut("change_owner/")]
        public async Task ChangeOwner(long projectId, [FromBody] ChangeOwnerDto changeOwnerDto)
        {
            await _mediator.Send(new ChangeOwnerRequest() { ProjectId = projectId, ChangeOwnerDto = changeOwnerDto});
        }
    }
}