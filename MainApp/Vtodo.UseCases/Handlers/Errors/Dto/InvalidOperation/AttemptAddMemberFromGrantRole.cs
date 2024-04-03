using System.Net;

namespace Vtodo.UseCases.Handlers.Errors.Dto.InvalidOperation;

internal class AttemptAddMemberFromGrantRoleError : ClientError
{
    public AttemptAddMemberFromGrantRoleError()
    {
        Code = HttpStatusCode.BadRequest;
        Message = "Using a 'grant role' instead of a 'add member' to add a user";
    }
}