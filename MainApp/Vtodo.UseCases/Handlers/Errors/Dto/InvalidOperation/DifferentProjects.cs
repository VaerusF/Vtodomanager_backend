using System.Net;

namespace Vtodo.UseCases.Handlers.Errors.Dto.InvalidOperation;

internal class DifferentProjectsError : ClientError
{
    public DifferentProjectsError()
    {
        Code = HttpStatusCode.BadRequest;
        Message = "Project ids should be equals";
    }
}