using System.Net;

namespace Vtodo.UseCases.Handlers.Errors.Dto.NotFound;

internal class ProjectNotFoundError : ClientError
{
    public ProjectNotFoundError()
    {
        Code = HttpStatusCode.NotFound;
        Message = "Project not found";
    }
}