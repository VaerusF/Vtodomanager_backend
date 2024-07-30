using MediatR;
using Microsoft.EntityFrameworkCore;
using Vtodo.Infrastructure.Interfaces.DataAccess;
using Vtodo.Infrastructure.Interfaces.Services;
using Vtodo.UseCases.Handlers.Errors.Commands;
using Vtodo.UseCases.Handlers.Errors.Dto.Security;

namespace Vtodo.UseCases.Handlers.Accounts.Commands.ConfirmAccount;

internal class ConfirmAccountRequestHandler : IRequestHandler<ConfirmAccountRequest>
{
    private readonly IDbContext _dbContext;
    private readonly ICurrentAccountService _currentAccountService;
    private readonly IMediator _mediator;

    public ConfirmAccountRequestHandler(
        IDbContext dbContext,
        ICurrentAccountService currentAccountService,
        IMediator mediator)
    {
        _dbContext = dbContext;
        _currentAccountService = currentAccountService;
        _mediator = mediator;
    }
    
    public async Task Handle(ConfirmAccountRequest request, CancellationToken cancellationToken)
    {
        var account = _currentAccountService.GetAccount();

        if (account.IsVerified)
        {
            await _mediator.Send(new SendErrorToClientRequest() { Error = new AccessDeniedError() }, cancellationToken);
            return;
        }
        
        var confirmAccountUrl = await _dbContext.ConfirmAccountUrls.FirstOrDefaultAsync(x =>
            x.Account.Id == account.Id &&
            x.UrlPart == request.UrlPart, cancellationToken: cancellationToken);

        if (confirmAccountUrl == null)
        {
            await _mediator.Send(new SendErrorToClientRequest() { Error = new AccessDeniedError() }, cancellationToken);
            return;
        }

        account.IsVerified = true;

        _dbContext.ConfirmAccountUrls.Remove(confirmAccountUrl);
        
        await _dbContext.SaveChangesAsync(cancellationToken);
        
    }
}