using AutoMapper;
using MediatR;
using Moq;
using Vtodo.DataAccess.Postgres;
using Vtodo.Entities.Enums;
using Vtodo.Entities.Models;
using Vtodo.Infrastructure.Interfaces.Services;
using Vtodo.Tests.Utils;
using Vtodo.UseCases.Handlers.Errors.Commands;
using Vtodo.UseCases.Handlers.Errors.Dto.NotFound;
using Vtodo.UseCases.Handlers.Projects.Dto;
using Vtodo.UseCases.Handlers.Projects.Queries.GetProject;
using Xunit;

namespace Vtodo.UseCases.Tests.Unit.Handlers.Projects.Queries
{
    public class GetProjectRequestHandlerTest
    {
        private AppDbContext _dbContext = null!;

        [Fact]
        public async void Handle_SuccessfulGetProjectFromCache_ReturnsTaskProjectDto()
        {
            SetupDbContext();
            
            var request = new GetProjectRequest() { Id = 1 };
            
            var projectSecurityServiceMock = SetupProjectSecurityServiceMock();
            
            var mapperMock = SetupMapperMock();
            mapperMock.Setup(x => x.Map<ProjectDto>(It.IsAny<Project>())).Returns(new ProjectDto()
            {
                Title = _dbContext.Projects.First(x => x.Id == request.Id).Title,
                CreationDate = new DateTimeOffset(_dbContext.Projects.First(x => x.Id == request.Id).CreationDate).ToUnixTimeMilliseconds()
            });
            
            var getProjectRequestHandler = new GetProjectRequestHandler(
                _dbContext, 
                projectSecurityServiceMock.Object, 
                mapperMock.Object, 
                SetupMockMediatorService().Object
            );

            var result = await getProjectRequestHandler.Handle(request, CancellationToken.None);
            
            projectSecurityServiceMock.Verify(x => x.CheckAccess(It.IsAny<long>(), ProjectRoles.ProjectMember), Times.Once);
            Assert.NotNull(result);
            
            CleanUp();
        }
        
        [Fact]
        public async void Handle_SuccessfulGetProjectFromDb_ReturnsTaskProjectDto()
        {
            SetupDbContext();

            var request = new GetProjectRequest() { Id = 1 };

            var projectSecurityServiceMock = SetupProjectSecurityServiceMock();
            
            var mapperMock = SetupMapperMock();
            mapperMock.Setup(x => x.Map<ProjectDto>(It.IsAny<Project>())).Returns(new ProjectDto()
            {
                Title = _dbContext.Projects.First(x => x.Id == request.Id).Title,
                CreationDate = new DateTimeOffset(_dbContext.Projects.First(x => x.Id == request.Id).CreationDate).ToUnixTimeMilliseconds()
            });
            
            var getProjectRequestHandler = new GetProjectRequestHandler(
                _dbContext, 
                projectSecurityServiceMock.Object, 
                mapperMock.Object, 
                SetupMockMediatorService().Object
            );

            var result = await getProjectRequestHandler.Handle(request, CancellationToken.None);
            
            projectSecurityServiceMock.Verify(x => x.CheckAccess(It.IsAny<long>(), ProjectRoles.ProjectMember), Times.Once);
            Assert.NotNull(result);
            
            CleanUp();
        }

        [Fact]
        public async void Handle_ProjectNotFound_SendProjectNotFoundError()
        {
            SetupDbContext();
            
            var request = new GetProjectRequest() { Id = 10 };
            
            var projectSecurityServiceMock = SetupProjectSecurityServiceMock();
            
            var mediatorMock = SetupMockMediatorService();
            var error = new ProjectNotFoundError();
            
            var getProjectRequestHandler = new GetProjectRequestHandler(
                _dbContext, 
                projectSecurityServiceMock.Object,
                SetupMapperMock().Object, 
                mediatorMock.Object
            );
            
            var result = await getProjectRequestHandler.Handle(request, CancellationToken.None);
            mediatorMock.Verify(x => x.Send(It.Is<SendErrorToClientRequest>(y => 
                        y.Error.GetType() == error.GetType()), 
                    It.IsAny<CancellationToken>()), Times.Once, $"Error request type is not a { error.GetType() }");
            
            projectSecurityServiceMock.Verify(x => x.CheckAccess(It.IsAny<long>(), ProjectRoles.ProjectMember), Times.Once);
            Assert.Null(result);
            
            CleanUp();
        }
        
        private static Mock<IMediator> SetupMockMediatorService()
        {
            var mock = new Mock<IMediator>();
            
            return mock;
        }

        private static Mock<IMapper> SetupMapperMock()
        {
            return new Mock<IMapper>();
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