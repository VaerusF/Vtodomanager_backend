using System.ComponentModel.DataAnnotations.Schema;

namespace Vtodo.Entities.Models;

public class ConfirmAccountUrl
{
    [ForeignKey("Account"), Column(Order = 0)]
    public long AccountId { get; set; }
    
    public Account Account { get; set; } = null!;
    
    public string UrlPart { get; set; } = string.Empty;
}