using System.Net;

namespace Vtodo.UseCases.Handlers.Errors.Dto.InvalidOperation;

internal class TaskIdsEqualsIdError : ClientError
{
    public TaskIdsEqualsIdError()
    {
        Code = HttpStatusCode.BadRequest;
        Message = "Task ids should not be equals";
    }
}