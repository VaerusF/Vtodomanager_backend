namespace VtodoManager.Logger.Infrastructure.Implementation.Options;

internal class ConnectionStringsOptions
{
    public string PgSqlConnection { get; set; } = string.Empty;
    public string RabbitMqLogger { get; set; } = string.Empty;
}