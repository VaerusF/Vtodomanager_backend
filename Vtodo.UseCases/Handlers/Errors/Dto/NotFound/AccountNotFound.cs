using System.Net;

namespace Vtodo.UseCases.Handlers.Errors.Dto.NotFound;

internal class AccountNotFoundError : ClientError
{
    public AccountNotFoundError()
    {
        Code = HttpStatusCode.NotFound;
        Message = "Account not found";
    }
}