using MediatR;
using VtodoManager.NewsService.UseCases.Handlers.Errors.Dto;

namespace VtodoManager.NewsService.UseCases.Handlers.Errors.Commands;

internal class SendErrorToClientRequest : IRequest
{
    public ClientError Error { get; set; } = null!;
}