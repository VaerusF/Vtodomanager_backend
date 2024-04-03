using System.Net;

namespace Vtodo.UseCases.Handlers.Errors.Dto.InvalidOperation;

internal class TaskIdEqualNewParentTaskIdError : ClientError
{
    public TaskIdEqualNewParentTaskIdError()
    {
        Code = HttpStatusCode.BadRequest;
        Message = "New parent task id should not be equal to task id";
    }
}