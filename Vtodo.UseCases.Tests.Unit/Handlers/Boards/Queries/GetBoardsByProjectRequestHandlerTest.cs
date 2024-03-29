using System.Collections.Generic;
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
using Vtodo.UseCases.Handlers.Boards.Queries.GetBoardsByProject;
using Vtodo.UseCases.Handlers.Errors.Commands;
using Vtodo.UseCases.Handlers.Errors.Dto.NotFound;
using Xunit;

namespace Vtodo.UseCases.Tests.Unit.Handlers.Boards.Queries
{
    public class GetBoardsByProjectRequestHandlerTest
    {
        private AppDbContext _dbContext = null!;

        [Fact]
        public async void Handle_SuccessfulGetBoardsByProject_ReturnsTaskListBoardDto()
        {
            SetupDbContext();

            var request = new GetBoardsByProjectRequest() { ProjectId = 1};

            var mapperMock = SetupMapperMock();
            mapperMock.Setup(x => x.Map<List<BoardDto>>(It.IsAny<List<Board>>())).Returns(new List<BoardDto>()
            {
                new BoardDto() {Id = _dbContext.Boards.First(x => x.Id == 1).Id,
                    Title = _dbContext.Boards.First(x => x.Id == 1).Title,
                    PrioritySort = _dbContext.Boards.First(x => x.Id == 1).PrioritySort,
                    ImageHeaderPath = _dbContext.Boards.First(x => x.Id == 1).ImageHeaderPath
                },
                new BoardDto() {Id = _dbContext.Boards.First(x => x.Id == 2).Id, 
                    Title = _dbContext.Boards.First(x => x.Id == 2).Title,
                    PrioritySort = _dbContext.Boards.First(x => x.Id == 2).PrioritySort,
                    ImageHeaderPath = _dbContext.Boards.First(x => x.Id == 2).ImageHeaderPath
                }
            });
            
            var getBoardsByProjectRequestHandler = new GetBoardsByProjectRequestHandler(
                _dbContext, 
                SetupProjectSecurityServiceMock().Object, 
                mapperMock.Object,
                SetupMockMediatorService().Object);

            var result = await getBoardsByProjectRequestHandler.Handle(request, CancellationToken.None);
            
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            
            CleanUp();
        }

        [Fact]
        public async void Handle_ProjectNotFound_SendProjectNotFoundError()
        {
            SetupDbContext();

            var request = new GetBoardsByProjectRequest() { ProjectId = 3 };
            
            var mediatorMock = SetupMockMediatorService();
            var error = new ProjectNotFoundError();
            
            var getBoardsByProjectRequestHandler = new GetBoardsByProjectRequestHandler(
                _dbContext, 
                SetupProjectSecurityServiceMock().Object, 
                SetupMapperMock().Object,
                mediatorMock.Object
            );
            
            var result = await getBoardsByProjectRequestHandler.Handle(request, CancellationToken.None);
            
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
            
            _dbContext.Projects.Add(new Project() {Title = "Test Project"});
            _dbContext.Projects.Add(new Project() {Title = "Test Project2"});
            _dbContext.SaveChanges();
            
            _dbContext.Boards.Add(new Board() {Title = "Test Board", PrioritySort = 0, Project = _dbContext.Projects.First()});
            _dbContext.Boards.Add(new Board() {Title = "Test Board2", PrioritySort = 0, Project = _dbContext.Projects.First()});
            _dbContext.SaveChanges();
        }
        
        private void CleanUp()
        {
            _dbContext?.Database.EnsureDeleted();
            _dbContext?.Dispose();
        }
    }
}