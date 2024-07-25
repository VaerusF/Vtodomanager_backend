using System.ComponentModel.DataAnnotations;
using Vtodo.Entities.Enums;

namespace Vtodo.UseCases.Handlers.ProjectsRoles.Dto
{
    public class KickMemberDto
    {
        //TODO переделать на username / email
        [Required]
        public long AccountId { get; set; }
    }
}