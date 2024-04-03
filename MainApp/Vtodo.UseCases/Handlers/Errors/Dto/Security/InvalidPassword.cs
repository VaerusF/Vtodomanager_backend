using System.Net;

namespace Vtodo.UseCases.Handlers.Errors.Dto.Security;

internal class InvalidPasswordError : ClientError
{
    public InvalidPasswordError()
    {
        Code = HttpStatusCode.Forbidden;
        Message = "Invalid password";
    }
}