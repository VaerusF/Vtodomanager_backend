using System.ComponentModel.DataAnnotations;
using Vtodo.Entities.Enums;

namespace Vtodo.UseCases.Handlers.ProjectsRoles.Dto
{
    public class RevokeRoleDto
    {
        //TODO переделать на username / email
        [Required]
        public long AccountId { get; set; }
        
        [Required]
        [EnumDataType(typeof(ProjectRoles))]
        public ProjectRoles Role { get; set; }
    }
}