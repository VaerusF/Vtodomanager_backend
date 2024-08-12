using System;
using System.IO;
using MediatR;
using Vtodo.UseCases.Handlers.Projects.Dto;

namespace Vtodo.UseCases.Handlers.Boards.Commands.DeleteBoardHeaderBackground
{
    public class DeleteBoardHeaderBackgroundRequest : IRequest
    {
        public long ProjectId { get; set; }
        public long BoardId { get; set; }
    }
}