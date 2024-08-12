using MediatR;
using Moq;
using Vtodo.DataAccess.Postgres;
using Vtodo.Entities.Enums;
using Vtodo.Entities.Models;
using Vtodo.Infrastructure.Interfaces.Services;
using Vtodo.Tests.Utils;
using Vtodo.UseCases.Handlers.Errors.Commands;
using Vtodo.UseCases.Handlers.Errors.Dto.NotFound;
using Vtodo.UseCases.Handlers.Projects.Commands.DeleteProject;
using Xunit;

namespace Vtodo.UseCases.Tests.Unit.Handlers.Projects.Commands
{
    public class DeleteProjectRequestHandlerTest
    {
        private AppDbContext _dbContext = null!;
        
        [Fact]
        public async void Handle_SuccessfulDeleteProject_ReturnsTask()
        {
            SetupDbContext();
            
            var request = new DeleteProjectRequest() { Id = 1 };

            var projectSecurityServiceMock = SetupProjectSecurityServiceMock();
            
            var account = _dbContext.Accounts.First();
            
            var currentAccountServiceMock = SetupCurrentAccountServiceMock();
            currentAccountServiceMock.Setup(x => x.GetAccount()).Returns(account);
            
            var deleteProjectRequestHandler = new DeleteProjectRequestHandler(
                _dbContext, 
                projectSecurityServiceMock.Object, 
                SetupMockMediatorService().Object
            );
            
            await deleteProjectRequestHandler.Handle(request, CancellationToken.None);
            
            projectSecurityServiceMock.Verify(x => x.CheckAccess(It.IsAny<long>(), ProjectRoles.ProjectOwner));
            
            Assert.Null(_dbContext.Projects.FirstOrDefault(x => x.Id == request.Id));
            
            CleanUp();
        }

        [Fact]
        public async void Handle_ProjectNotFound_SendProjectNotFoundError()
        {
            SetupDbContext();

            var request = new DeleteProjectRequest() { Id = 2 };
            
            var projectSecurityServiceMock = SetupProjectSecurityServiceMock();
            
            var account = _dbContext.Accounts.First();
            
            var mediatorMock = SetupMockMediatorService();
            var error = new ProjectNotFoundError();
            
            var currentAccountServiceMock = SetupCurrentAccountServiceMock();
            currentAccountServiceMock.Setup(x => x.GetAccount()).Returns(account);
            
            var deleteProjectRequestHandler = new DeleteProjectRequestHandler(
                _dbContext, 
                projectSecurityServiceMock.Object, 
                mediatorMock.Object
            );

            await deleteProjectRequestHandler.Handle(request, CancellationToken.None);
            
            projectSecurityServiceMock.Verify(x => x.CheckAccess(It.IsAny<long>(), ProjectRoles.ProjectOwner));
            
            mediatorMock.Verify(x => x.Send(It.Is<SendErrorToClientRequest>(y => 
                        y.Error.GetType() == error.GetType()), 
                    It.IsAny<CancellationToken>()), Times.Once, $"Error request type is not a { error.GetType() }");

            CleanUp();
        }
        
        private Mock<ICurrentAccountService> SetupCurrentAccountServiceMock()
        {
            var mock = new Mock<ICurrentAccountService>();

            return mock;
        }
        
        private static Mock<IMediator> SetupMockMediatorService()
        {
            var mock = new Mock<IMediator>();
            
            return mock;
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