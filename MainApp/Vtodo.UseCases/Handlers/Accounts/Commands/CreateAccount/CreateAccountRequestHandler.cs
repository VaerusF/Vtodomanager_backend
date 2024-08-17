using Vtodo.Entities.Models;
using Vtodo.Infrastructure.Interfaces.DataAccess;
using MediatR;
using Vtodo.DomainServices.Interfaces;
using Vtodo.Infrastructure.Interfaces.Services;
using Vtodo.UseCases.Handlers.Accounts.Commands.SendConfirmAccountUrl;
using Vtodo.UseCases.Handlers.Accounts.Dto;
using Vtodo.UseCases.Handlers.Errors.Commands;
using Vtodo.UseCases.Handlers.Errors.Dto.AlreadyExists;

namespace Vtodo.UseCases.Handlers.Accounts.Commands.CreateAccount
{
    internal class CreateAccountRequestHandler : IRequestHandler<CreateAccountRequest, JwtTokensDto?>
    {
        private readonly IDbContext _dbContext;
        private readonly ISecurityService _securityService;
        private readonly IJwtService _jwtService;
        private readonly IConfigService _configService;
        private readonly IMediator _mediator;
        private readonly IAccountService _accountService;
        
        public CreateAccountRequestHandler(
            IDbContext dbContext, 
            ISecurityService securityService,
            IJwtService jwtService,
            IConfigService configService,
            IMediator mediator,
            IAccountService accountService)
        {
            _dbContext = dbContext;
            _securityService = securityService;
            _jwtService = jwtService;
            _configService = configService;
            _mediator = mediator;
            _accountService = accountService;
        }
        
        public async Task<JwtTokensDto?> Handle(CreateAccountRequest request, CancellationToken cancellationToken)
        {
            var createDto = request.CreateAccountDto;

            if (_dbContext.Accounts.FirstOrDefault(x => x.Email == createDto.Email) != null)
            {
                await _mediator.Send(new SendErrorToClientRequest() { Error = new EmailAlreadyExistsError() }, cancellationToken);
                return null;
            }

            if (_dbContext.Accounts.FirstOrDefault(x => x.Username == createDto.Username) != null)
            {
                await _mediator.Send(new SendErrorToClientRequest() { Error = new UsernameAlreadyExistsError() }, cancellationToken);
                return null;
            }

            if (createDto.Password != createDto.ConfirmPassword)
            {
                await _mediator.Send(new SendErrorToClientRequest() { Error = new PasswordsNotEqualsError() }, cancellationToken);
                return null;
            }

            var account = _accountService.CreateAccount(
                createDto.Email, 
                createDto.Username, 
                _securityService.HashPassword(createDto.Password, _configService.HasherKeySize, _configService.HasherIterations, out byte[] salt), 
                salt,
                createDto.Firstname,
                createDto.Surname
            );
            
            _dbContext.Accounts.Add(account);

            await _dbContext.SaveChangesAsync(cancellationToken);

            var accessToken = _jwtService.GenerateNewTokensAfterLogin(account, out string refreshToken);
            
            await _mediator.Send(new SendConfirmAccountUrlRequest() { Account = account }, cancellationToken);
            
            return new JwtTokensDto() {AccessToken = accessToken, RefreshToken = refreshToken};
        }
    }
}