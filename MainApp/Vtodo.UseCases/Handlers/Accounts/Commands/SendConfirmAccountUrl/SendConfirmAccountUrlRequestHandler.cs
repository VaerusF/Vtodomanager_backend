using MediatR;
using Microsoft.AspNetCore.Http;
using Vtodo.DomainServices.Interfaces;
using Vtodo.Entities.Models;
using Vtodo.Infrastructure.Interfaces.DataAccess;
using Vtodo.Infrastructure.Interfaces.Services;

namespace Vtodo.UseCases.Handlers.Accounts.Commands.SendConfirmAccountUrl;

internal class SendConfirmAccountUrlRequestHandler : IRequestHandler<SendConfirmAccountUrlRequest>
{
    private readonly ISecurityService _securityService;
    private readonly IDbContext _dbContext;
    private readonly ICurrentAccountService _currentAccountService;
    private readonly IConfigService _configService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IMediator _mediator;
    
    public SendConfirmAccountUrlRequestHandler(
        ISecurityService securityService, 
        IDbContext dbContext,
        ICurrentAccountService currentAccountService,
        IConfigService configService,
        IHttpContextAccessor httpContextAccessor,
        IMediator mediator)
    {
        _securityService = securityService;
        _dbContext = dbContext;
        _currentAccountService = currentAccountService;
        _configService = configService;
        _httpContextAccessor = httpContextAccessor;
        _mediator = mediator;
    }
    
    public async Task Handle(SendConfirmAccountUrlRequest request, CancellationToken cancellationToken)
    {
        var account = request.Account;
        var hashedPart = _securityService.GenerateConfirmAccountHashedUrlPart(
            account,
            _configService.HasherKeySize, 
            _configService.HasherIterations, 
            out byte[] salt 
        );

        var httpRequest = _httpContextAccessor.HttpContext.Request;
        
        var url = $"{_configService.FrontClientAddress}/confirm_account/{hashedPart}";
        //TODO Send Email
        _dbContext.ConfirmAccountUrls.Add(new ConfirmAccountUrl() { Account = account, UrlPart = hashedPart });
        
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}