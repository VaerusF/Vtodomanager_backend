using AutoMapper;
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
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        
        public CreateAccountRequestHandler(
            IDbContext dbContext, 
            ISecurityService securityService,
            IJwtService jwtService,
            IConfigService configService,
            IMapper mapper,
            IMediator mediator)
        {
            _dbContext = dbContext;
            _securityService = securityService;
            _jwtService = jwtService;
            _configService = configService;
            _mapper = mapper;
            _mediator = mediator;
        }
        
        public async Task<JwtTokensDto?> Handle(CreateAccountRequest request, CancellationToken cancellationToken)
        {
            var createDto = request.CreateAccountDto;
            var account = _mapper.Map<Account>(createDto);

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
                
            
            if (!string.IsNullOrWhiteSpace(createDto.Firstname)) account.Firstname = createDto.Firstname;
            if (!string.IsNullOrWhiteSpace(createDto.Surname)) account.Surname = createDto.Surname;

            account.HashedPassword = _securityService.HashPassword(createDto.Password, _configService.HasherKeySize, _configService.HasherIterations, out byte[] salt);
            account.Salt = salt;
            
            _dbContext.Accounts.Add(account);

            await _dbContext.SaveChangesAsync(cancellationToken);

            var accessToken = _jwtService.GenerateNewTokensAfterLogin(account, out string refreshToken);
            
            await _mediator.Send(new SendConfirmAccountUrlRequest() { Account = account }, cancellationToken);
            
            return new JwtTokensDto() {AccessToken = accessToken, RefreshToken = refreshToken};
        }
    }
}