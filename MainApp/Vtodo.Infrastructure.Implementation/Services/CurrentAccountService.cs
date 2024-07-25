using System.Linq;
using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Http;
using Vtodo.Entities.Exceptions;
using Vtodo.Entities.Models;
using Vtodo.Infrastructure.Interfaces.DataAccess;
using Vtodo.Infrastructure.Interfaces.Services;

namespace Vtodo.Infrastructure.Implementation.Services
{
    internal class CurrentAccountService : ICurrentAccountService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IDbContext _dbContext;
        private readonly IMediator _mediator;
        
        public CurrentAccountService(
            IHttpContextAccessor httpContextAccessor, 
            IDbContext dbContext, 
            IMediator mediator)
        {
            _httpContextAccessor = httpContextAccessor;
            _dbContext = dbContext;
            _mediator = mediator;
        }

        public Account GetAccount()
        {
            if (!(_httpContextAccessor.HttpContext.User?.Identity?.IsAuthenticated ?? false)) throw new UnauthorizedException();
            
            long.TryParse(_httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId);

            return _dbContext.Accounts.FirstOrDefault(x => x.Id == userId) ?? throw new UnauthorizedException();
        }
    }
}