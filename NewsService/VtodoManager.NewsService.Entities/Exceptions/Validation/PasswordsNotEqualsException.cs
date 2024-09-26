using System.Net;

namespace VtodoManager.NewsService.Entities.Exceptions
{
    public class InvalidStringLengthException : CustomException
    {
        public InvalidStringLengthException(string message = "Invalid string length") : base(HttpStatusCode.BadRequest, message)
        {
            
        }
    }
}