using MediatR;

namespace Vtodo.UseCases.Handlers.Accounts.Commands.ConfirmAccount;

public class ConfirmAccountRequest : IRequest
{
    public string UrlPart { get; set; } = null!;
}