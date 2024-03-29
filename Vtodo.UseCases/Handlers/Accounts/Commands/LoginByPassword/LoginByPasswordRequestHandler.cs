using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Vtodo.Entities.Models;
using Vtodo.Infrastructure.Interfaces.DataAccess;
using MediatR;
using Vtodo.DomainServices.Interfaces;
using Vtodo.Entities.Exceptions;
using Vtodo.Infrastructure.Interfaces.Services;
using Vtodo.UseCases.Handlers.Accounts.Dto;
using Vtodo.UseCases.Handlers.Errors.Commands;
using Vtodo.UseCases.Handlers.Errors.Dto.NotFound;
using Vtodo.UseCases.Handlers.Errors.Dto.Security;

namespace Vtodo.UseCases.Handlers.Accounts.Commands.LoginByPassword
{
    internal class LoginByPasswordRequestHandler : IRequestHandler<LoginByPasswordRequest, JwtTokensDto?>
    {
        private readonly IDbContext _dbContext;
        private readonly ISecurityService _securityService;
        private readonly IJwtService _jwtService;
        private readonly IConfigService _configService;
        private readonly IMediator _mediator;

        public LoginByPasswordRequestHandler(
            IDbContext dbContext, 
            ISecurityService securityService,
            IJwtService jwtService,
            IConfigService configService,
            IMediator mediator)
        {
            _dbContext = dbContext;
            _securityService = securityService;
            _jwtService = jwtService;
            _configService = configService;
            _mediator = mediator;
        }
        
        public async Task<JwtTokensDto?> Handle(LoginByPasswordRequest request, CancellationToken cancellationToken)
        {
            var loginDto = request.LoginByPasswordDto;
            
            var account = _dbContext.Accounts.FirstOrDefault(x => x.Email == loginDto.Email);
            if (account == null)
            {
                await _mediator.Send(new SendErrorToClientRequest() { Error = new AccountNotFoundError() }, cancellationToken);
                return null;
            }
            
            var isRightPassword = _securityService.VerifyPassword(loginDto.Password, account.HashedPassword, _configService.HasherKeySize,
                _configService.HasherIterations, account.Salt);

            if (!isRightPassword)
            {
                await _mediator.Send(new SendErrorToClientRequest() { Error = new InvalidPasswordError() }, cancellationToken);
                return null;
            }
            
            var accessToken = _jwtService.GenerateNewTokensAfterLogin(account, out string refreshToken);
            
            return new JwtTokensDto() {AccessToken = accessToken, RefreshToken = refreshToken};
        }
    }
}