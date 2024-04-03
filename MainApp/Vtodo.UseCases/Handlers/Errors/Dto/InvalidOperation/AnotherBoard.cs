using System.Net;

namespace Vtodo.UseCases.Handlers.Errors.Dto.InvalidOperation;

internal class AnotherBoardError : ClientError
{
    public AnotherBoardError()
    {
        Code = HttpStatusCode.BadRequest;
        Message = "Another board when moving task to task";
    }
}