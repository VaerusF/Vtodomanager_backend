using System.Net;

namespace Vtodo.UseCases.Handlers.Errors.Dto.InvalidOperation;

internal class NewParentTaskIdEqualOldIdError : ClientError
{
    public NewParentTaskIdEqualOldIdError()
    {
        Code = HttpStatusCode.BadRequest;
        Message = "New parent task id should not be equal to old parent task id";
    }
}