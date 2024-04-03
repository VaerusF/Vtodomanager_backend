using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Vtodo.Infrastructure.Interfaces.Services;
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

namespace Vtodo.UseCases.Handlers.Accounts.Commands.LogoutAll
{
    internal class LogoutAllRequestHandler : IRequestHandler<LogoutAllRequest>
    {
        private readonly IJwtService _jwtService;
        private readonly ICurrentAccountService _currentAccountService;
        
        public LogoutAllRequestHandler(IJwtService jwtService, ICurrentAccountService currentAccountService)
        {
            _jwtService = jwtService;
            _currentAccountService = currentAccountService;
        }
        
        public async Task Handle(LogoutAllRequest request, CancellationToken cancellationToken)
        {
            _jwtService.InvalidateAllRefreshTokens(_currentAccountService.GetAccount());
        }
    }
}