using System.Net;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Vtodo.Entities.Exceptions;
using Vtodo.UseCases.Handlers.Errors.Commands;
using Vtodo.UseCases.Handlers.Errors.Dto;

namespace Vtodo.Web.Utils
{
    public class ExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IMediator _mediator;

        public ExceptionHandlerMiddleware(RequestDelegate next, IMediator mediator)
        {
            _next = next;
            _mediator = mediator;
        }
        
        public async Task Invoke(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (CustomException e)
            {
                if (!httpContext.Response.HasStarted)
                    await _mediator.Send(new SendErrorToClientRequest() { Error = new ClientError() { Code = e.Code, Message = e.Message} });
            }
            catch (Exception e)
            {
                if (!httpContext.Response.HasStarted)
                    await _mediator.Send(new SendErrorToClientRequest() { Error = new ClientError() { Code = HttpStatusCode.InternalServerError, Message = "Internal server error"} });
            }
        }
    }
}