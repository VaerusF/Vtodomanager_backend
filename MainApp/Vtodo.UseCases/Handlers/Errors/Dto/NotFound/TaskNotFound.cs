using System.Net;

namespace Vtodo.UseCases.Handlers.Errors.Dto.NotFound;

internal class TaskNotFoundError : ClientError
{
    public TaskNotFoundError()
    {
        Code = HttpStatusCode.NotFound;
        Message = "Task not found";
    }
}