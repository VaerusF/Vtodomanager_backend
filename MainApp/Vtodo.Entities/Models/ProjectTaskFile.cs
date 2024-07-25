using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vtodo.Entities.Models
{
    public class ProjectTaskFile
    {
        [ForeignKey("Project"), Column(Order = 0)]
        public long ProjectId { get; set; }
        
        [ForeignKey("TaskM"), Column(Order = 1)]
        public long TaskId { get; set; }
        
        [Required]
        public string FileName { get; set; } = string.Empty;
    }
}