using System.Net;

namespace Vtodo.UseCases.Handlers.Errors.Dto.InvalidOperation;

internal class BoardIdsEqualsIdError : ClientError
{
    public BoardIdsEqualsIdError()
    {
        Code = HttpStatusCode.BadRequest;
        Message = "Board ids should not be equals";
    }
}