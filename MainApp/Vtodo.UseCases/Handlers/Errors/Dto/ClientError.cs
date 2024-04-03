using System.Net;

namespace Vtodo.UseCases.Handlers.Errors.Dto;

internal class ClientError
{
    public string Message { get; set; } = "";
    public HttpStatusCode Code { get; set; } = HttpStatusCode.InternalServerError;
    
}