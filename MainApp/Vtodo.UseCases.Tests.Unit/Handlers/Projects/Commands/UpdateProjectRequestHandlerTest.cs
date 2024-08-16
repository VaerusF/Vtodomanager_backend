using MediatR;
using Moq;
using Vtodo.DataAccess.Postgres;
using Vtodo.DomainServices.Interfaces;
using Vtodo.Entities.Enums;
using Vtodo.Entities.Models;
using Vtodo.Infrastructure.Interfaces.Services;
using Vtodo.Tests.Utils;
using Vtodo.UseCases.Handlers.Errors.Commands;
using Vtodo.UseCases.Handlers.Errors.Dto.NotFound;
using Vtodo.UseCases.Handlers.Projects.Commands.UpdateProject;
using Vtodo.UseCases.Handlers.Projects.Dto;
using Xunit;

namespace Vtodo.UseCases.Tests.Unit.Handlers.Projects.Commands
{
    public class UpdateProjectRequestHandlerTest
    {
        private AppDbContext _dbContext = null!;
        
        [Fact]
        public async void Handle_SuccessfulUpdateProject_ReturnsTask()
        {
            SetupDbContext();
            
            var mockProjectService = SetupMockProjectService();
            mockProjectService.Setup(x => x.UpdateProject(It.IsAny<Project>(), It.IsAny<string>()))
                .Callback((Project project, string title) =>
                    {
                        project.Title = title;
                    }
            );
            
            var projectSecurityServiceMock = SetupProjectSecurityServiceMock();
            
            var updateProjectDto = new UpdateProjectDto() { Title = "Test update project"};
            
            var request = new UpdateProjectRequest() { Id = 1, UpdateProjectDto = updateProjectDto};

            var account = _dbContext.Accounts.First();
            
            var currentAccountServiceMock = SetupCurrentAccountServiceMock();
            currentAccountServiceMock.Setup(x => x.GetAccount()).Returns(account);
            
            var updateProjectRequestHandler = new UpdateProjectRequestHandler(
                _dbContext, 
                projectSecurityServiceMock.Object, 
                mockProjectService.Object,
                SetupMockMediatorService().Object
            );

            await updateProjectRequestHandler.Handle(request, CancellationToken.None);
            
            projectSecurityServiceMock.Verify(x => x.CheckAccess(It.IsIn(request.Id), ProjectRoles.ProjectUpdate), Times.Once);
            
            mockProjectService.Verify(x => x.UpdateProject(It.IsAny<Project>(), It.IsAny<string>()), Times.Once);
            
            Assert.NotNull(_dbContext.Projects.FirstOrDefault(x => x.Id == request.Id && x.Title == updateProjectDto.Title));
            
            CleanUp();
        }

        [Fact]
        public async void Handle_ProjectNotFound_ThrowsProjectNotFoundException()
        {
            SetupDbContext();

            var updateProjectDto = new UpdateProjectDto() { Title = "Test update project" };
            
            var request = new UpdateProjectRequest() { Id = 2, UpdateProjectDto = updateProjectDto };
            
            var projectSecurityServiceMock = SetupProjectSecurityServiceMock();
            
            var account = _dbContext.Accounts.First();
            
            var mediatorMock = SetupMockMediatorService();
            var error = new ProjectNotFoundError();
            
            var currentAccountServiceMock = SetupCurrentAccountServiceMock();
            currentAccountServiceMock.Setup(x => x.GetAccount()).Returns(account);
            
            var updateProjectRequestHandler = new UpdateProjectRequestHandler(
                _dbContext, 
                projectSecurityServiceMock.Object, 
                SetupMockProjectService().Object,
                mediatorMock.Object
            );

            await updateProjectRequestHandler.Handle(request, CancellationToken.None);
            
            projectSecurityServiceMock.Verify(x => x.CheckAccess(It.IsIn(request.Id), ProjectRoles.ProjectUpdate), Times.Once);
            
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
        
        private static Mock<IProjectService> SetupMockProjectService()
        {
            var mock = new Mock<IProjectService>();
            
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