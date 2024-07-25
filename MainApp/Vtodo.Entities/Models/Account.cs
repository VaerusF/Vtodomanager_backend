using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vtodo.Entities.Models
{
    public class Account
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        public string Username { get; set; } = string.Empty;

        public string? Firstname { get; set; }
        
        public string? Surname { get; set; }

        [Required]
        public string HashedPassword { get; set; } = string.Empty;

        [Required] 
        public byte[] Salt { get; set; } = default!;
        
        public DateTime RegisteredAt { get; private set; } = DateTime.UtcNow;

        public bool IsBanned { get; set; }
        
        public bool IsVerified { get; set; }
    }
}