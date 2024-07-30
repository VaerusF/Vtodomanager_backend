using MediatR;
using Vtodo.Entities.Models;

namespace Vtodo.UseCases.Handlers.Accounts.Commands.SendConfirmAccountUrl;

public class SendConfirmAccountUrlRequest : IRequest
{
    public Account Account { get; set; } = null!;
}