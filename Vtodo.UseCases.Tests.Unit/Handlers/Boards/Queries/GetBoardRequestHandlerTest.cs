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
using Vtodo.UseCases.Handlers.Boards.Dto;
using Vtodo.UseCases.Handlers.Boards.Queries.GetBoard;
using Vtodo.UseCases.Handlers.Errors.Commands;
using Vtodo.UseCases.Handlers.Errors.Dto.NotFound;
using Xunit;

namespace Vtodo.UseCases.Tests.Unit.Handlers.Boards.Queries
{
    public class GetBoardRequestHandlerTest
    {
        private AppDbContext _dbContext = null!;

        [Fact]
        public async void Handle_SuccessfulGetBoard_ReturnsTaskBoardDto()
        {
            SetupDbContext();
            
            var request = new GetBoardRequest() { Id = 1 };

            var mapperMock = SetupMapperMock();
            mapperMock.Setup(x => x.Map<BoardDto>(It.IsAny<Board>())).Returns(new BoardDto()
            {
                Id = request.Id,
                ImageHeaderPath = _dbContext.Boards.First(x => x.Id == request.Id).ImageHeaderPath,
                Title = _dbContext.Boards.First(x => x.Id == request.Id).Title,
                PrioritySort = _dbContext.Boards.First(x => x.Id == request.Id).PrioritySort,
                ProjectId = _dbContext.Boards.First(x => x.Id == request.Id).Project.Id
            });
            
            var getBoardRequestHandler = new GetBoardRequestHandler(
                _dbContext, 
                SetupProjectSecurityServiceMock().Object, 
                mapperMock.Object,
                SetupMockMediatorService().Object);

            var result = await getBoardRequestHandler.Handle(request, CancellationToken.None);
            
            Assert.NotNull(result);
            CleanUp();
        }
        
        [Fact]
        public async void Handle_BoardNotFound_SendBoardNotFoundError()
        {
            SetupDbContext();
            
            var request = new GetBoardRequest() { Id = 2 };
            
            var mediatorMock = SetupMockMediatorService();
            var error = new BoardNotFoundError();
            
            var getBoardRequestHandler = new GetBoardRequestHandler(
                _dbContext, 
                SetupProjectSecurityServiceMock().Object, 
                SetupMapperMock().Object,
                mediatorMock.Object
            );
            
            var result = await getBoardRequestHandler.Handle(request, CancellationToken.None);
            
            mediatorMock.Verify(x => x.Send(It.Is<SendErrorToClientRequest>(y => 
                        y.Error.GetType() == error.GetType()), 
                    It.IsAny<CancellationToken>()), Times.Once, $"Error request type is not a { error.GetType() }");
            Assert.Null(result);
            
            CleanUp();
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
        
        private static Mock<IMapper> SetupMapperMock()
        {
            return new Mock<IMapper>();
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