using System.Net;

namespace VtodoManager.NewsService.UseCases.Handlers.Errors.Dto.Security;

internal class AccessDeniedError : ClientError
{
    public AccessDeniedError()
    {
        Code = HttpStatusCode.Forbidden;
        Message = "Access denied";
    }
}