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
using Vtodo.UseCases.Handlers.Boards.Commands.MoveBoardToAnotherProject;
using Vtodo.UseCases.Handlers.Errors.Commands;
using Vtodo.UseCases.Handlers.Errors.Dto.InvalidOperation;
using Vtodo.UseCases.Handlers.Errors.Dto.NotFound;
using Xunit;

namespace Vtodo.UseCases.Tests.Unit.Handlers.Boards.Commands
{
    public class MoveBoardToAnotherProjectRequestHandlerTest
    {
        private AppDbContext _dbContext = null!;

        [Fact]
        public async void Handle_SuccessfulMoveBoardToAnotherProject_ReturnsTask()
        {
            SetupDbContext();

            var request = new MoveBoardToAnotherProjectRequest() { BoardId = 1, ProjectId = 2};
            
            var moveBoardToAnotherProjectRequestHandler = new MoveBoardToAnotherProjectRequestHandler(
                _dbContext, 
                SetupProjectSecurityService().Object,
                SetupMockMediatorService().Object);

            await moveBoardToAnotherProjectRequestHandler.Handle(request, CancellationToken.None);
            
            Assert.Null(_dbContext.Boards.FirstOrDefault(x => x.Project == _dbContext.Projects.First(d => d.Id == 1)));
            Assert.NotNull(_dbContext.Boards.FirstOrDefault(x => x.Project == _dbContext.Projects.First(d => d.Id == 2)));
            
            CleanUp();
        }

        [Fact]
        public async void Handle_BoardNotFound_SendBoardNotFoundError()
        {
            SetupDbContext();

            var request = new MoveBoardToAnotherProjectRequest() { BoardId = 2, ProjectId = 2};
            
            var mediatorMock = SetupMockMediatorService();
            var error = new BoardNotFoundError();
            
            var moveBoardToAnotherProjectRequestHandler = new MoveBoardToAnotherProjectRequestHandler(
                _dbContext, 
                SetupProjectSecurityService().Object,
                mediatorMock.Object
            );

            await moveBoardToAnotherProjectRequestHandler.Handle(request, CancellationToken.None);
            mediatorMock.Verify(x => x.Send(It.Is<SendErrorToClientRequest>(y => 
                        y.Error.GetType() == error.GetType()), 
                    It.IsAny<CancellationToken>()), Times.Once, $"Error request type is not a { error.GetType() }");

            CleanUp();
        }
        
        [Fact]
        public async void Handle_NewProjectIdEqualOldIdException_SendNewProjectIdEqualOldIdError()
        {
            SetupDbContext();

            var request = new MoveBoardToAnotherProjectRequest() { BoardId = 1, ProjectId = 1};
            
            var mediatorMock = SetupMockMediatorService();
            var error = new NewProjectIdEqualOldIdError();
            
            var moveBoardToAnotherProjectRequestHandler = new MoveBoardToAnotherProjectRequestHandler(
                _dbContext, 
                SetupProjectSecurityService().Object,
                mediatorMock.Object
            );
            
            await moveBoardToAnotherProjectRequestHandler.Handle(request, CancellationToken.None);
            
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
        
        private static Mock<IProjectSecurityService> SetupProjectSecurityService()
        {
            var mock = new Mock<IProjectSecurityService>();
            mock.Setup(x => x.CheckAccess(It.IsAny<Project>(), It.IsAny<ProjectRoles>())).Verifiable();
            
            return new Mock<IProjectSecurityService>();
        }
        
        private void SetupDbContext()
        {
            _dbContext = TestDbUtils.SetupTestDbContextInMemory();
            
            _dbContext.Projects.Add(new Project() {Title = "Test Project"});
            _dbContext.Projects.Add(new Project() {Title = "Test Project2"});
            _dbContext.SaveChanges();
            
            _dbContext.Boards.Add(new Board() {Title = "Test Board", PrioritySort = 0, Project = _dbContext.Projects.First()});
            _dbContext.SaveChanges();
        }
        
        private void CleanUp()
        {
            _dbContext?.Database.EnsureDeleted();
            _dbContext?.Dispose();
        }
    }
}