using System.Net;

namespace Vtodo.UseCases.Handlers.Errors.Dto.InvalidOperation;

internal class DifferentBoardsError : ClientError
{
    public DifferentBoardsError()
    {
        Code = HttpStatusCode.BadRequest;
        Message = "Board ids should be equals";
    }
}