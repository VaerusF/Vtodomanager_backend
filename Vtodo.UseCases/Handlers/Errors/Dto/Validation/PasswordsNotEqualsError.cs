using System.Net;

namespace Vtodo.UseCases.Handlers.Errors.Dto.AlreadyExists;

internal class PasswordsNotEqualsError : ClientError
{
    public PasswordsNotEqualsError()
    {
        Code = HttpStatusCode.BadRequest;
        Message = "Passwords not equals";
    }
}