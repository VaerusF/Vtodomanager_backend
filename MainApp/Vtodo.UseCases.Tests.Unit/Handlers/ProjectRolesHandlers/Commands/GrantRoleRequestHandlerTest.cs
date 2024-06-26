using System.Collections.Generic;
using System.Linq;
using System.Threading;
using MediatR;
using Moq;
using Vtodo.DataAccess.Postgres;
using Vtodo.Entities.Enums;
using Vtodo.Entities.Exceptions;
using Vtodo.Entities.Models;
using Vtodo.Infrastructure.Interfaces.Services;
using Vtodo.Tests.Utils;
using Vtodo.UseCases.Handlers.Errors.Commands;
using Vtodo.UseCases.Handlers.Errors.Dto.InvalidOperation;
using Vtodo.UseCases.Handlers.Errors.Dto.NotFound;
using Vtodo.UseCases.Handlers.ProjectsRoles.Commands.GrantRole;
using Vtodo.UseCases.Handlers.ProjectsRoles.Dto;
using Xunit;

namespace Vtodo.UseCases.Tests.Unit.Handlers.ProjectRolesHandlers.Commands
{
    public class GrantRoleRequestHandlerTest
    {
        private AppDbContext _dbContext = null!;

        [Fact]
        public async void Handle_SuccessfulGrantRole_ReturnsTask()
        {
            SetupDbContext();

            var grantRoleDto = new GrantRoleDto() { AccountId = 3, Role = ProjectRoles.ProjectUpdate};            
            
            var request = new GrantRoleRequest() { ProjectId = 1, GrantRoleDto = grantRoleDto};

            var grantRoleRequestHandler = new GrantRoleRequestHandler(
                _dbContext, 
                SetupProjectSecurityServiceMock().Object,
                SetupMockMediatorService().Object);
            
            await grantRoleRequestHandler.Handle(request, CancellationToken.None);
            
            Assert.NotNull(_dbContext.ProjectAccountsRoles.FirstOrDefault(x =>
                x.ProjectId == request.ProjectId && 
                x.AccountId == grantRoleDto.AccountId && 
                x.ProjectRole == grantRoleDto.Role));
            
            CleanUp();
        }

        [Fact]
        public async void Handle_AttemptAddMemberFromGrantRole_SendAttemptAddMemberFromGrantRoleError()
        {
            SetupDbContext();
            
            var grantRoleDto = new GrantRoleDto() { AccountId = 4, Role = ProjectRoles.ProjectMember};            
            
            var request = new GrantRoleRequest() { ProjectId = 1, GrantRoleDto = grantRoleDto};
            
            var mediatorMock = SetupMockMediatorService();
            var error = new AttemptAddMemberFromGrantRoleError();
            
            var grantRoleRequestHandler = new GrantRoleRequestHandler(
                _dbContext, 
                SetupProjectSecurityServiceMock().Object,
                mediatorMock.Object
            );

            await grantRoleRequestHandler.Handle(request, CancellationToken.None);
            mediatorMock.Verify(x => x.Send(It.Is<SendErrorToClientRequest>(y => 
                        y.Error.GetType() == error.GetType()), 
                    It.IsAny<CancellationToken>()), Times.Once, $"Error request type is not a { error.GetType() }");
            
            CleanUp();
        }
        
        [Fact]
        public async void Handle_ProjectNotFound_SendProjectNotFoundError()
        {
            SetupDbContext();
            
            var grantRoleDto = new GrantRoleDto() { AccountId = 3, Role = ProjectRoles.ProjectUpdate};            
            
            var request = new GrantRoleRequest() { ProjectId = 5, GrantRoleDto = grantRoleDto};
            
            var mediatorMock = SetupMockMediatorService();
            var error = new ProjectNotFoundError();
            
            var grantRoleRequestHandler = new GrantRoleRequestHandler(
                _dbContext, 
                SetupProjectSecurityServiceMock().Object,
                mediatorMock.Object
            );
            
            await grantRoleRequestHandler.Handle(request, CancellationToken.None);
            mediatorMock.Verify(x => x.Send(It.Is<SendErrorToClientRequest>(y => 
                        y.Error.GetType() == error.GetType()), 
                    It.IsAny<CancellationToken>()), Times.Once, $"Error request type is not a { error.GetType() }");
            
            CleanUp();
        }

        [Fact]
        public async void Handle_AccountNotFound_SendAccountNotFoundError()
        {
            SetupDbContext();
            
            var grantRoleDto = new GrantRoleDto() { AccountId = 30, Role = ProjectRoles.ProjectUpdate};            
            
            var request = new GrantRoleRequest() { ProjectId = 1, GrantRoleDto = grantRoleDto};
            
            var mediatorMock = SetupMockMediatorService();
            var error = new AccountNotFoundError();
            
            var grantRoleRequestHandler = new GrantRoleRequestHandler(
                _dbContext, 
                SetupProjectSecurityServiceMock().Object,
                mediatorMock.Object
            );

            await grantRoleRequestHandler.Handle(request, CancellationToken.None);
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
            mock.Setup(x => x.GrantRole(It.IsAny<Project>(), It.IsAny<Account>(), It.IsAny<ProjectRoles>()))
                .Callback((Project project, Account account, ProjectRoles role) =>
            {
                _dbContext.ProjectAccountsRoles.Add(new ProjectAccountsRoles() { ProjectId = project.Id, AccountId = account.Id, ProjectRole = role });
                _dbContext.SaveChanges();
            });
            
            return mock;
        }
        
        private void SetupDbContext()
        {
            _dbContext = TestDbUtils.SetupTestDbContextInMemory();

            _dbContext.Accounts.Add(new Account() { Email = "test@test.ru", Username = "test", HashedPassword = "test" , Salt = new byte[64]});
            _dbContext.Accounts.Add(new Account() { Email = "test2@test.ru", Username = "test2", HashedPassword = "test2" , Salt = new byte[64]});
            _dbContext.Accounts.Add(new Account() { Email = "test3@test.ru", Username = "test3", HashedPassword = "test3" , Salt = new byte[64]});
            _dbContext.Accounts.Add(new Account() { Email = "test4@test.ru", Username = "test4", HashedPassword = "test4" , Salt = new byte[64]});
            
            _dbContext.Projects.Add(new Project() {Title = "Test Project"});
            _dbContext.SaveChanges();

            _dbContext.ProjectAccountsRoles.AddRange(new List<ProjectAccountsRoles>()
            {
                new ProjectAccountsRoles()
            {
                AccountId = _dbContext.Accounts.First(x => x.Id == 1).Id, 
                ProjectId  = _dbContext.Projects.First(x => x.Id == 1).Id, 
                ProjectRole = ProjectRoles.ProjectOwner
            },
                new ProjectAccountsRoles()
                {
                    AccountId = _dbContext.Accounts.First(x => x.Id == 2).Id, 
                    ProjectId  = _dbContext.Projects.First(x => x.Id == 1).Id, 
                    ProjectRole = ProjectRoles.ProjectMember
                }, new ProjectAccountsRoles()
            {
                AccountId = _dbContext.Accounts.First(x => x.Id == 3).Id, 
                ProjectId  = _dbContext.Projects.First(x => x.Id == 1).Id, 
                ProjectRole = ProjectRoles.ProjectMember
            }
            });
            _dbContext.SaveChanges();
        }
        
        private void CleanUp()
        {
            _dbContext?.Database.EnsureDeleted();
            _dbContext?.Dispose();
        }
    }
}