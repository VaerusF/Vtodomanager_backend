using System.Net;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using VtodoManager.NewsService.Entities.Enums;
using VtodoManager.NewsService.Entities.Exceptions;
using VtodoManager.NewsService.Entities.Models;
using VtodoManager.NewsService.UseCases.Handlers.Errors.Commands;
using VtodoManager.NewsService.UseCases.Handlers.Errors.Dto;
using VtodoManager.NewsService.UseCases.Handlers.Logs.Commands.SendLogToLogger;

namespace VtodoManager.NewsService.Web.Utils
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
                                    ServiceName = "NewsService",
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