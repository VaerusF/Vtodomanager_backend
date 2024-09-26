using MediatR;
using Microsoft.AspNetCore.Http;

namespace VtodoManager.NewsService.UseCases.Handlers.Errors.Commands;

internal class SendErrorToClientRequestHandler : IRequestHandler<SendErrorToClientRequest>
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public SendErrorToClientRequestHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }
    
    
    public async Task Handle(SendErrorToClientRequest request, CancellationToken cancellationToken)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (!httpContext.Response.HasStarted)
        {
            var errorDto = request.Error;
        
            httpContext.Response.ContentType = "application/json";
            httpContext.Response.StatusCode = (int)errorDto.Code;
            
            await httpContext.Response.WriteAsync($"{{\"errors\": \"{ errorDto.Message }\" }}", cancellationToken: cancellationToken);
        }
    }
}