using MediatR;
using Moq;
using Vtodo.DataAccess.Postgres;
using Vtodo.Entities.Enums;
using Vtodo.Entities.Models;
using Vtodo.Infrastructure.Interfaces.Services;
using Vtodo.Tests.Utils;
using Vtodo.UseCases.Handlers.Errors.Commands;
using Vtodo.UseCases.Handlers.Errors.Dto.NotFound;
using Vtodo.UseCases.Handlers.ProjectsRoles.Commands.AddMember;
using Vtodo.UseCases.Handlers.ProjectsRoles.Dto;
using Xunit;

namespace Vtodo.UseCases.Tests.Unit.Handlers.ProjectRolesHandlers.Commands
{
    public class AddMemberRequestHandlerTest
    {
        private AppDbContext _dbContext = null!;

        [Fact]
        public async void Handle_SuccessfulAddMember_ReturnsTask()
        {
            SetupDbContext();

            var addMemberDto = new AddMemberDto() {AccountId = 2};

            var request = new AddMemberRequest() {ProjectId = 1, AddMemberDto = addMemberDto};

            var addMemberRequestHandler = new AddMemberRequestHandler(
                _dbContext,
                SetupProjectSecurityServiceMock().Object,
                SetupMockMediatorService().Object);

            await addMemberRequestHandler.Handle(request, CancellationToken.None);

            Assert.NotNull(_dbContext.ProjectAccountsRoles.FirstOrDefault(x =>
                x.AccountId == addMemberDto.AccountId &&
                x.ProjectId == request.ProjectId &&
                x.ProjectRole == ProjectRoles.ProjectMember
            ));

            CleanUp();
        }

        [Fact]
        public async void Handle_ProjectNotFound_SendProjectNotFoundError()
        {
            SetupDbContext();

            var addMemberDto = new AddMemberDto() {AccountId = 2};

            var request = new AddMemberRequest() {ProjectId = 3, AddMemberDto = addMemberDto};

            var mediatorMock = SetupMockMediatorService();
            var error = new ProjectNotFoundError();
            
            var addMemberRequestHandler = new AddMemberRequestHandler(
                _dbContext,
                SetupProjectSecurityServiceMock().Object,
                mediatorMock.Object
            );
            
            await addMemberRequestHandler.Handle(request, CancellationToken.None);
            mediatorMock.Verify(x => x.Send(It.Is<SendErrorToClientRequest>(y => 
                        y.Error.GetType() == error.GetType()), 
                    It.IsAny<CancellationToken>()), Times.Once, $"Error request type is not a { error.GetType() }");
            
            CleanUp();
        }

        [Fact]
        public async void Handle_AccountNotFound_SendAccountNotFoundError()
        {
            SetupDbContext();

            var addMemberDto = new AddMemberDto() {AccountId = 5};

            var request = new AddMemberRequest() {ProjectId = 1, AddMemberDto = addMemberDto};

            var mediatorMock = SetupMockMediatorService();
            var error = new AccountNotFoundError();
            
            var addMemberRequestHandler = new AddMemberRequestHandler(
                _dbContext,
                SetupProjectSecurityServiceMock().Object,
                mediatorMock.Object
            );
            
            await addMemberRequestHandler.Handle(request, CancellationToken.None);
            
            mediatorMock.Verify(x => x.Send(It.Is<SendErrorToClientRequest>(y => 
                        y.Error.GetType() == error.GetType()), 
                    It.IsAny<CancellationToken>()), Times.Once, $"Error request type is not a { error.GetType() }");
            
            CleanUp();
        }
        
        private static Mock<IMediator> SetupMockMediatorService()
        {
            var mock = new Mock<IMediator>();
            
            return mock;
        }
        
        private Mock<IProjectSecurityService> SetupProjectSecurityServiceMock()
        {
            var mock = new Mock<IProjectSecurityService>();
            mock.Setup(x => x.CheckAccess(It.IsAny<Project>(), It.IsAny<ProjectRoles>())).Verifiable();
            mock.Setup(x => x.AddMember(It.IsAny<Project>(), It.IsAny<Account>())).Callback((Project project, Account account) =>
            {
                _dbContext.ProjectAccountsRoles.Add(new ProjectAccountsRoles() {ProjectId = project.Id, AccountId = account.Id, ProjectRole = ProjectRoles.ProjectMember});
                _dbContext.SaveChanges();
            });
            
            return mock;
        }
        
        private void SetupDbContext()
        {
            _dbContext = TestDbUtils.SetupTestDbContextInMemory();

            _dbContext.Accounts.Add(new Account() { Email = "test@test.ru", Username = "test", HashedPassword = "test" , Salt = new byte[64]});
            _dbContext.Accounts.Add(new Account() { Email = "test2@test.ru", Username = "test2", HashedPassword = "test2" , Salt = new byte[64]});
            
            _dbContext.Projects.Add(new Project() {Title = "Test Project"});
            _dbContext.SaveChanges();
        }
        
        private void CleanUp()
        {
            _dbContext?.Database.EnsureDeleted();
            _dbContext?.Dispose();
        }
    }
}