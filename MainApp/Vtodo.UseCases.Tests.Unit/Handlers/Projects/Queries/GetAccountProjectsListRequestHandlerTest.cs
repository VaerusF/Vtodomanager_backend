using Moq;
using Vtodo.DataAccess.Postgres;
using Vtodo.Entities.Enums;
using Vtodo.Entities.Models;
using Vtodo.Infrastructure.Interfaces.Services;
using Vtodo.Tests.Utils;
using Vtodo.UseCases.Handlers.Projects.Queries.GetUserProjectList;
using Xunit;

namespace Vtodo.UseCases.Tests.Unit.Handlers.Projects.Queries
{
    public class GetAccountProjectsListRequestHandlerTest
    {
        private AppDbContext _dbContext = null!;
        
        [Fact]
        public async void Handle_SuccessfulGetAccountProjectsListFromDb_ReturnsTaskListProjectDto()
        {
            SetupDbContext();

            var account = _dbContext.Accounts.First(x => x.Id == 1);
            
            var currentAccountServiceMock = SetupCurrentAccountService();
            currentAccountServiceMock.Setup(x => x.GetAccount()).Returns(account);
            
            var request = new GetAccountProjectsListRequest() { };
            
            var getAccountProjectsListRequestHandler = new GetAccountProjectsListRequestHandler(
                _dbContext, 
                SetupProjectSecurityServiceMock().Object, 
                currentAccountServiceMock.Object
            );

            var result = await getAccountProjectsListRequestHandler.Handle(request, CancellationToken.None);
            
            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
            
            CleanUp();
        }
        
        private static Mock<ICurrentAccountService> SetupCurrentAccountService()
        {
            return new Mock<ICurrentAccountService>();
        }
        
        private static Mock<IProjectSecurityService> SetupProjectSecurityServiceMock()
        {
            var mock = new Mock<IProjectSecurityService>();
            mock.Setup(x => x.CheckAccess(It.IsAny<Project>(), It.IsAny<ProjectRoles>())).Verifiable();
            
            return mock;
        }
        
        private void SetupDbContext()
        {
            _dbContext = TestDbUtils.SetupTestDbContextInMemory();

            _dbContext.Accounts.Add(new Account() { Email = "test@test.ru", Username = "test", HashedPassword = "test" , Salt = new byte[64]});
            _dbContext.Accounts.Add(new Account() { Email = "test2@test.ru", Username = "test2", HashedPassword = "test2" , Salt = new byte[64]});
            _dbContext.Accounts.Add(new Account() { Email = "test3@test.ru", Username = "test3", HashedPassword = "test3" , Salt = new byte[64]});
            
            _dbContext.Projects.Add(new Project() {Title = "Test Project"});
            _dbContext.Projects.Add(new Project() {Title = "Test Project2"});
            _dbContext.Projects.Add(new Project() {Title = "Test Project3"});

            _dbContext.SaveChanges();

            _dbContext.ProjectAccountsRoles.Add(new ProjectAccountsRoles()
            {
                Project = _dbContext.Projects.First(x => x.Id == 1),
                Account = _dbContext.Accounts.First(x => x.Id == 1),
                ProjectRole = ProjectRoles.ProjectMember
            });
            
            _dbContext.ProjectAccountsRoles.Add(new ProjectAccountsRoles()
            {
                Project = _dbContext.Projects.First(x => x.Id == 1),
                Account = _dbContext.Accounts.First(x => x.Id == 1),
                ProjectRole = ProjectRoles.ProjectOwner
            });
            
            _dbContext.ProjectAccountsRoles.Add(new ProjectAccountsRoles()
            {
                Project = _dbContext.Projects.First(x => x.Id == 3),
                Account = _dbContext.Accounts.First(x => x.Id == 1),
                ProjectRole = ProjectRoles.ProjectMember
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