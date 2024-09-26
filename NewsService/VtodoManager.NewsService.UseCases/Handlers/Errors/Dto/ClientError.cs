using System.Net;

namespace VtodoManager.NewsService.UseCases.Handlers.Errors.Dto;

internal class ClientError
{
    public string Message { get; set; } = "";
    public HttpStatusCode Code { get; set; } = HttpStatusCode.InternalServerError;
    
}