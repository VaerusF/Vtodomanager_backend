using MediatR;
using Vtodo.UseCases.Handlers.Errors.Dto;

namespace Vtodo.UseCases.Handlers.Errors.Commands;

internal class SendErrorToClientRequest : IRequest
{
    public ClientError Error { get; set; } = null!;
}