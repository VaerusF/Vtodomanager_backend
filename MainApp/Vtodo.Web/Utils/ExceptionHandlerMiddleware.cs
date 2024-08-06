using System.Net;
using MediatR;
using Vtodo.Entities.Enums;
using Vtodo.Entities.Exceptions;
using Vtodo.Entities.Models;
using Vtodo.UseCases.Handlers.Errors.Commands;
using Vtodo.UseCases.Handlers.Errors.Dto;
using Vtodo.UseCases.Handlers.Logs.Commands.SendLogToLogger;

namespace Vtodo.Web.Utils
{
    internal class ExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        
        public ExceptionHandlerMiddleware(
            RequestDelegate next
        )
        {
            _next = next;
        }
        
        public async Task Invoke(HttpContext httpContext, IMediator mediator, ILogger<ExceptionHandlerMiddleware> logger)
        {
            try
            {
                await _next(httpContext);
            }
            catch (CustomException e)
            {
                if (!httpContext.Response.HasStarted)
                    await mediator.Send(new SendErrorToClientRequest() { Error = new ClientError() { Code = e.Code, Message = e.Message} });
            }
            catch (Exception ex)
            {
                if (!httpContext.Response.HasStarted)
                {
                    try
                    { 
                        await mediator.Send(new SendLogToLoggerRequest() { Log = new Log()
                                {
                                    ServiceName = "MainApp",
                                    LogLevel = CustomLogLevels.Error, 
                                    Message = $"Exception:  {ex.Message},\r\n{ex.InnerException}",
                                    DateTime = DateTime.UtcNow
                                }
                            }
                        );
                    }
                    catch (Exception ex2)
                    {
                        logger.Log(LogLevel.Critical, $"RabbitMqLogger error: {ex.Message},\r\n{ex2.InnerException}");
                    }

                    await mediator.Send(new SendErrorToClientRequest() { Error = new ClientError() { Code = HttpStatusCode.InternalServerError, Message = "Internal server error"} });
                }
                   
            }
        }
    }
}