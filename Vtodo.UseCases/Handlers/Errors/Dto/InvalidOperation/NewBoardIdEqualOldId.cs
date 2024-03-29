using System.Net;

namespace Vtodo.UseCases.Handlers.Errors.Dto.InvalidOperation;

internal class NewBoardIdEqualOldIdError : ClientError
{
    public NewBoardIdEqualOldIdError()
    {
        Code = HttpStatusCode.BadRequest;
        Message = "New board id should not be equal to old board id";
    }
}