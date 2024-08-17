using MediatR;
using Vtodo.Infrastructure.Interfaces.Services;

namespace Vtodo.UseCases.Handlers.Accounts.Commands.Logout
{
    internal class LogoutRequestHandler : IRequestHandler<LogoutRequest>
    {
        private readonly IJwtService _jwtService;
        private readonly ICurrentAccountService _currentAccountService;
        
        public LogoutRequestHandler(IJwtService jwtService, ICurrentAccountService currentAccountService)
        {
            _jwtService = jwtService;
            _currentAccountService = currentAccountService;
        }
        
        public async Task Handle(LogoutRequest request, CancellationToken cancellationToken)
        {
            _jwtService.InvalidateRefreshToken(_currentAccountService.GetAccount(), request.LogoutDto.RefreshToken);
        }
    }
}