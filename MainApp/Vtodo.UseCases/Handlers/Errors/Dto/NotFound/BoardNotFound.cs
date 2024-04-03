using System.Net;

namespace Vtodo.UseCases.Handlers.Errors.Dto.NotFound;

internal class BoardNotFoundError : ClientError
{
    public BoardNotFoundError()
    {
        Code = HttpStatusCode.NotFound;
        Message = "Board not found";
    }
}