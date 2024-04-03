using System.Net;

namespace Vtodo.UseCases.Handlers.Errors.Dto.InvalidOperation;

internal class NewProjectIdEqualOldIdError : ClientError
{
    public NewProjectIdEqualOldIdError()
    {
        Code = HttpStatusCode.BadRequest;
        Message = "New project id should not be equal to old project id";
    }
}