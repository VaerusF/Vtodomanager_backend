using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vtodo.Entities.Models
{
    public class ProjectBoardFile
    {
        [ForeignKey("Project"), Column(Order = 0)]
        public long ProjectId { get; set; }
        
        [ForeignKey("Board"), Column(Order = 1)]
        public long BoardId { get; set; }
        
        [Required]
        public string FileName { get; set; } = string.Empty;
    }
}