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
using Vtodo.UseCases.Handlers.Boards.Commands.UpdateBoard;
using Vtodo.UseCases.Handlers.Boards.Dto;
using Vtodo.UseCases.Handlers.Errors.Commands;
using Vtodo.UseCases.Handlers.Errors.Dto.NotFound;
using Xunit;

namespace Vtodo.UseCases.Tests.Unit.Handlers.Boards.Commands
{
    public class UpdateBoardRequestHandlerTest
    {
        private AppDbContext _dbContext = null!;

        [Fact]
        public async void Handle_SuccessfulUpdateBoard_ReturnsTask()
        {
            SetupDbContext();
            var updateBoardDto = new UpdateBoardDto()
            {
                Title = "Updated Text Board",
                PrioritySort = 1
            };
            
            var request = new UpdateBoardRequest() { Id = 1, UpdateBoardDto = updateBoardDto};
            
            var updateBoardRequestHandler = new UpdateBoardRequestHandler(
                _dbContext, 
                SetupProjectSecurityService().Object,
                SetupMockMediatorService().Object);
            
            await updateBoardRequestHandler.Handle(request, CancellationToken.None);
            
            Assert.Null(_dbContext.Boards.FirstOrDefault( x => x.Id == request.Id && x.Title == "Test Board"));
            Assert.NotNull(_dbContext.Boards.FirstOrDefault(x => x.Id == request.Id && x.Title == updateBoardDto.Title && x.PrioritySort == updateBoardDto.PrioritySort));
            
            CleanUp();
        }
        
        [Fact]
        public async void Handle_BoardNotFound_SendBoardNotFoundError()
        {
            SetupDbContext();
            var updateBoardDto = new UpdateBoardDto()
            {
                Title = "Updated Text Board",
                PrioritySort = 1
            };
            
            var request = new UpdateBoardRequest() { Id = 2, UpdateBoardDto = updateBoardDto};
            
            var mediatorMock = SetupMockMediatorService();
            var error = new BoardNotFoundError();
            
            var updateBoardRequestHandler = new UpdateBoardRequestHandler(
                _dbContext, 
                SetupProjectSecurityService().Object,
                mediatorMock.Object
            );
            
            await updateBoardRequestHandler.Handle(request, CancellationToken.None);
            
            mediatorMock.Verify(x => x.Send(It.Is<SendErrorToClientRequest>(y => 
                        y.Error.GetType() == error.GetType()), 
                    It.IsAny<CancellationToken>()), Times.Once, $"Error request type is not a { error.GetType() }");

            CleanUp();
        }
        
        private static Mock<IProjectSecurityService> SetupProjectSecurityService()
        {
            var mock = new Mock<IProjectSecurityService>();
            mock.Setup(x => x.CheckAccess(It.IsAny<Project>(), It.IsAny<ProjectRoles>())).Verifiable();
            
            return new Mock<IProjectSecurityService>();
        }
        
        private static Mock<IMediator> SetupMockMediatorService()
        {
            var mock = new Mock<IMediator>();
            
            return mock;
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