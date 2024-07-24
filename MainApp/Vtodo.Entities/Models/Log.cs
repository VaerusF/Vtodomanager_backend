using Vtodo.Entities.Enums;

namespace Vtodo.Entities.Models;

public class Log
{
    public string ServiceName { get; set; } = null!;
    public CustomLogLevels LogLevel { get; set; }
    public string Message { get; set; } = null!;
    public DateTime DateTime { get; set; }
}