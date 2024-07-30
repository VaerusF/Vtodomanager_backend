using MediatR;
using Moq;
using Vtodo.DataAccess.Postgres;
using Vtodo.Entities.Enums;
using Vtodo.Entities.Models;
using Vtodo.Infrastructure.Interfaces.Services;
using Vtodo.Tests.Utils;
using Vtodo.UseCases.Handlers.Accounts.Commands.ConfirmAccount;
using Vtodo.UseCases.Handlers.Errors.Commands;
using Vtodo.UseCases.Handlers.Errors.Dto.Security;
using Xunit;

namespace Vtodo.UseCases.Tests.Unit.Handlers.Accounts.Commands;

public class ConfirmAccountRequestHandlerTest
{
    private AppDbContext _dbContext = null!;
    
    [Fact]
    public async void Handle_SuccessfulConfirmAccount_ReturnsTask()
    {
        SetupDbContext();
        
        var currentAccountTest = SetupCurrentAccountServiceMock();
        currentAccountTest.Setup(x => x.GetAccount()).Returns(_dbContext.Accounts.First());
        
        _dbContext.ConfirmAccountUrls.Add(new ConfirmAccountUrl()
            { Account = _dbContext.Accounts.First(), UrlPart = "DEDBA12AB70933407547E3A9195DC8A9F6E1CFD48F8FB78F42CA97E238F4B443C4AF2F649CCF36849FDB3CF2D022AA7EF7B933818C47328B9C163460298E57B4" 
        });
        await _dbContext.SaveChangesAsync();        

        var request = new ConfirmAccountRequest() { UrlPart = "DEDBA12AB70933407547E3A9195DC8A9F6E1CFD48F8FB78F42CA97E238F4B443C4AF2F649CCF36849FDB3CF2D022AA7EF7B933818C47328B9C163460298E57B4" };

        var confirmAccountRequestHandler = new ConfirmAccountRequestHandler(
            _dbContext,
            currentAccountTest.Object,
            SetupMockMediatorService().Object
        );

        await confirmAccountRequestHandler.Handle(request, CancellationToken.None);

        Assert.Null(_dbContext.ConfirmAccountUrls.FirstOrDefault(
            x => x.AccountId == _dbContext.Accounts.First().Id &&
                 x.UrlPart == request.UrlPart
        ));
        
        Assert.True(_dbContext.Accounts.First().IsVerified);

        CleanUp();
    }

    [Fact]
    public async void Handle_AccountIsVerified_SendAccessDeniedError()
    {
        SetupDbContext();
        
        var mediatorMock = SetupMockMediatorService();
        
        var currentAccountTest = SetupCurrentAccountServiceMock();
        currentAccountTest.Setup(x => x.GetAccount()).Returns(_dbContext.Accounts.First());
        
        _dbContext.Accounts.First().IsVerified = true;
        await _dbContext.SaveChangesAsync();
        
        var request = new ConfirmAccountRequest() { UrlPart = "DEDBA12AB70933407547E3A9195DC8A9F6E1CFD48F8FB78F42CA97E238F4B443C4AF2F649CCF36849FDB3CF2D022AA7EF7B933818C47328B9C163460298E57B4" };
        var error = new AccessDeniedError();
        
        var confirmAccountRequestHandler = new ConfirmAccountRequestHandler(
            _dbContext,
            currentAccountTest.Object,
            mediatorMock.Object
        );
        
        await confirmAccountRequestHandler.Handle(request, CancellationToken.None);
        
        mediatorMock.Verify(x => x.Send(It.Is<SendErrorToClientRequest>(y => 
                    y.Error.GetType() == error.GetType()), 
                It.IsAny<CancellationToken>()), Times.Once, $"Error request type is not a { error.GetType() }");
        
        CleanUp();
    }
    
    [Fact]
    public async void Handle_ConfirmAccountUrlNotFound_SendAccessDeniedError()
    {
        SetupDbContext();
        
        var mediatorMock = SetupMockMediatorService();
        
        var currentAccountTest = SetupCurrentAccountServiceMock();
        currentAccountTest.Setup(x => x.GetAccount()).Returns(_dbContext.Accounts.First());
        
        await _dbContext.SaveChangesAsync();
        
        var request = new ConfirmAccountRequest() { UrlPart = "DEDBA12AB70933407547E3A9195DC8A9F6E1CFD48F8FB78F42CA97E238F4B443C4AF2F649CCF36849FDB3CF2D022AA7EF7B933818C47328B9C163460298E57B4" };
        var error = new AccessDeniedError();
        
        var confirmAccountRequestHandler = new ConfirmAccountRequestHandler(
            _dbContext,
            currentAccountTest.Object,
            mediatorMock.Object
        );
        
        await confirmAccountRequestHandler.Handle(request, CancellationToken.None);
        
        mediatorMock.Verify(x => x.Send(It.Is<SendErrorToClientRequest>(y => 
                    y.Error.GetType() == error.GetType()), 
                It.IsAny<CancellationToken>()), Times.Once, $"Error request type is not a { error.GetType() }");
        
        CleanUp();
    }

    private static Mock<ICurrentAccountService> SetupCurrentAccountServiceMock()
    {
        var mock = new Mock<ICurrentAccountService>();

        return mock;
    }
    
    private static Mock<IMediator> SetupMockMediatorService()
    {
        var mock = new Mock<IMediator>();
            
        return mock;
    }

    
    private void SetupDbContext()
    {
        _dbContext = TestDbUtils.SetupTestDbContextInMemory();
        
        _dbContext.Accounts.Add(new Account() { Email = "test@test.ru", Username = "test", HashedPassword = "test" , Salt = new byte[64]});
        _dbContext.SaveChanges();
    }
    
    private void CleanUp()
    {
        _dbContext?.Database.EnsureDeleted();
        _dbContext?.Dispose();
    }
}