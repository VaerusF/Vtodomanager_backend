using Vtodo.Infrastructure.Interfaces.DataAccess;
using MediatR;
using Vtodo.Infrastructure.Interfaces.Services;
using Vtodo.UseCases.Handlers.Accounts.Dto;
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

namespace Vtodo.UseCases.Handlers.Accounts.Commands.RefreshTokens
{
    internal class RefreshTokensRequestHandler : IRequestHandler<RefreshTokensRequest, JwtTokensDto>
    {
        private readonly IDbContext _dbContext;
        private readonly IJwtService _jwtService;

        public RefreshTokensRequestHandler(
            IDbContext dbContext,
            IJwtService jwtService)
        {
            _dbContext = dbContext;
            _jwtService = jwtService;

        }
        
        public async Task<JwtTokensDto> Handle(RefreshTokensRequest request, CancellationToken cancellationToken)
        {
            var accessToken = _jwtService.RefreshTokens(request.RefreshTokensDto.RefreshToken, out string refreshToken);
            
            return new JwtTokensDto() {AccessToken = accessToken, RefreshToken = refreshToken};
        }
    }
}