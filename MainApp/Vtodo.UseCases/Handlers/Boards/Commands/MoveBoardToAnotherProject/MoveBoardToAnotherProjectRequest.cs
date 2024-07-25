using MediatR;
using Vtodo.UseCases.Handlers.Boards.Dto;

namespace Vtodo.UseCases.Handlers.Boards.Commands.MoveBoardToAnotherProject
{
    public class MoveBoardToAnotherProjectRequest : IRequest
    {
        public long BoardId { get; set; }
        
        public long ProjectId { get; set; }
    }
}