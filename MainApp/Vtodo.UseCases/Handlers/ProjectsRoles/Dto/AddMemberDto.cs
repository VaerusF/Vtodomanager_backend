using System.ComponentModel.DataAnnotations;

namespace Vtodo.UseCases.Handlers.ProjectsRoles.Dto
{
    public class AddMemberDto
    {
        //TODO переделать на username / email
        [Required]
        public long AccountId { get; set; }
    }
}