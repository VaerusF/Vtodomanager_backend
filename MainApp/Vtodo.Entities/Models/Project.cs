using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vtodo.Entities.Models
{
    public class Project
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        
        [Required]
        public string Title { get; set; } = string.Empty;

        public ICollection<Board> Boards { get; set; } = default!;
        
        public DateTime CreationDate { get; private set; } = DateTime.UtcNow;
    }
}