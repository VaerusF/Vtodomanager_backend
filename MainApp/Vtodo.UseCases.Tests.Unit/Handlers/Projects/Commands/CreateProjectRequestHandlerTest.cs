using AutoMapper;
using Moq;
using Vtodo.DataAccess.Postgres;
using Vtodo.Entities.Enums;
using Vtodo.Entities.Models;
using Vtodo.Infrastructure.Interfaces.Services;
using Vtodo.Tests.Utils;
using Vtodo.UseCases.Handlers.Projects.Commands.CreateProject;
using Vtodo.UseCases.Handlers.Projects.Dto;
using Xunit;

namespace Vtodo.UseCases.Tests.Unit.Handlers.Projects.Commands
{
    public class CreateProjectRequestHandlerTest
    {
        private AppDbContext _dbContext = null!;
        
        [Fact]
        public async void Handle_SuccessfulCreateProject_ReturnsTaskProjectDto()
        {
            SetupDbContext();
            
            var createProjectDto = new CreateProjectDto() { Title = "Test project create"};
            
            var request = new CreateProjectRequest() { CreateProjectDto = createProjectDto};

            var account = _dbContext.Accounts.First();
            
            var currentAccountServiceMock = SetupCurrentAccountServiceMock();
            currentAccountServiceMock.Setup(x => x.GetAccount()).Returns(account);
            
            var mapper = SetupMapperMock();
            mapper.Setup(x => x.Map<Project>(It.IsAny<CreateProjectDto>())).Returns(new Project() { Title = createProjectDto.Title});

            var createProjectRequestHandler = new CreateProjectRequestHandler(
                _dbContext, 
                currentAccountServiceMock.Object, 
                SetupProjectSecurityServiceMock().Object, 
                mapper.Object
            );

            await createProjectRequestHandler.Handle(request, CancellationToken.None);
            
            Assert.NotNull(_dbContext.Projects.FirstOrDefault(x => x.Title == createProjectDto.Title));
            
            CleanUp();
        }

        
        private Mock<ICurrentAccountService> SetupCurrentAccountServiceMock()
        {
            var mock = new Mock<ICurrentAccountService>();

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
            
            _dbContext.Projects.Add(new Project() {Title = "Test Project2"});
            _dbContext.SaveChanges();
        }
        
        private void CleanUp()
        {
            _dbContext?.Database.EnsureDeleted();
            _dbContext?.Dispose();
        }
    }
}