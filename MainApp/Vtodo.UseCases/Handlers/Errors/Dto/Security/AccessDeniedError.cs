using System.Net;

namespace Vtodo.UseCases.Handlers.Errors.Dto.Security;

internal class AccessDeniedError : ClientError
{
    public AccessDeniedError()
    {
        Code = HttpStatusCode.Forbidden;
        Message = "Access denied";
    }
}