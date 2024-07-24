using System.ComponentModel.DataAnnotations;
using Vtodo.Entities.Enums;

namespace Vtodo.Entities.Models;

public class Log
{
    [Key]
    public Guid LogId { get; set; }
    public string ServiceName { get; set; } = null!;
    public CustomLogLevels LogLevel { get; set; }
    public string Message { get; set; } = null!;
    public DateTime DateTime { get; set; }
}