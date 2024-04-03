using System.Linq;
using System.Threading;
using AutoMapper;
using MediatR;
using Moq;
using Vtodo.DataAccess.Postgres;
using Vtodo.Entities.Enums;
using Vtodo.Entities.Exceptions;
using Vtodo.Entities.Models;
using Vtodo.Infrastructure.Interfaces.Services;
using Vtodo.Tests.Utils;
using Vtodo.UseCases.Handlers.Boards.Commands.CreateBoard;
using Vtodo.UseCases.Handlers.Boards.Dto;
using Vtodo.UseCases.Handlers.Errors.Commands;
using Vtodo.UseCases.Handlers.Errors.Dto.NotFound;
using Xunit;

namespace Vtodo.UseCases.Tests.Unit.Handlers.Boards.Commands
{
    public class CreateBoarRequestHandlerTest
    {
        private AppDbContext _dbContext = null!;

        [Fact]
        public async void Handle_SuccessfulCreateBoard_ReturnsTask()
        {
            SetupDbContext();

            var createBoardDto = new CreateBoardDto() { Title ="Test Board", ProjectId = 1, PrioritySort = 0};
            var request = new CreateBoardRequest() {CreateBoardDto = createBoardDto};
            
            var mockMapper = SetupMockMapper();
            mockMapper.Setup(x => x.Map<Board>(createBoardDto)).Returns(new Board() {Title = createBoardDto.Title, PrioritySort = 0});
            
            var createBoardRequestHandler = new CreateBoardRequestHandler(
                _dbContext, 
                SetupProjectSecurityService().Object, 
                mockMapper.Object,
                SetupMockMediatorService().Object);
            
            await createBoardRequestHandler.Handle(request, CancellationToken.None);
            
            Assert.NotNull(_dbContext.Boards.FirstOrDefault(x => x.Title == createBoardDto.Title));
            CleanUp();
        }
        
        [Fact]
        public async void Handle_ProjectNotFound_SendProjectNotFoundError()
        {
            SetupDbContext();

            var createBoardDto = new CreateBoardDto() { Title ="Test Board", ProjectId = 2, PrioritySort = 0};
            var request = new CreateBoardRequest() {CreateBoardDto = createBoardDto};
            
            var mockMapper = SetupMockMapper();
            mockMapper.Setup(x => x.Map<Board>(createBoardDto)).Returns(new Board() {Title = createBoardDto.Title, PrioritySort = 0});
            
            var mediatorMock = SetupMockMediatorService();
            var error = new ProjectNotFoundError();
            
            var createBoardRequestHandler = new CreateBoardRequestHandler(
                _dbContext, 
                SetupProjectSecurityService().Object, 
                mockMapper.Object,
                mediatorMock.Object
            );
            
            await createBoardRequestHandler.Handle(request, CancellationToken.None);
            
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
        
        private static Mock<IMapper> SetupMockMapper()
        {
            return new Mock<IMapper>();
        }
        
        private void SetupDbContext()
        {
            _dbContext = TestDbUtils.SetupTestDbContextInMemory();
            
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