using System.Net;

namespace Vtodo.UseCases.Handlers.Errors.Dto.AlreadyExists;

internal class EmailAlreadyExistsError : ClientError
{
    public EmailAlreadyExistsError()
    {
        Code = HttpStatusCode.BadRequest;
        Message = "Email already taken";
    }
}