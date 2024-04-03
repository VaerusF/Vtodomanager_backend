using System.Net;

namespace Vtodo.UseCases.Handlers.Errors.Dto.AlreadyExists;

internal class UsernameAlreadyExistsError : ClientError
{
    public UsernameAlreadyExistsError()
    {
        Code = HttpStatusCode.BadRequest;
        Message = "Username already taken";
    }
}