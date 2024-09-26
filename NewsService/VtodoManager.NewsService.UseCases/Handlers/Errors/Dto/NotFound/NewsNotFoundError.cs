using System.Net;

namespace VtodoManager.NewsService.UseCases.Handlers.Errors.Dto.NotFound;

internal class NewsNotFoundError : ClientError
{
    public NewsNotFoundError()
    {
        Code = HttpStatusCode.NotFound;
        Message = "News not found";
    }
}