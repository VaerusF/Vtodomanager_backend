using Vtodo.DomainServices.Interfaces;
using Vtodo.Entities.Exceptions;
using Vtodo.Entities.Models;

namespace Vtodo.DomainServices.Implementation;

internal class BoardService : IBoardService
{
    public Board CreateBoard(string title, Project project, int prioritySort = 0)
    {
        var board = new Board()
        {
            Title = title,
            PrioritySort = prioritySort,
            Project = project
        };
        
        return board;
    }

    public void UpdateBoard(Board board, string title)
    {
        board.Title = title;
    }

    public void UpdateBoardPrioritySort(Board board, int priority)
    {
        board.PrioritySort = priority;
    }

    public void SwapBoardsPrioritySort(Board board, Board board2)
    {
        (board.PrioritySort, board2.PrioritySort) = (board2.PrioritySort, board.PrioritySort);
    }

    public void MoveBoardToAnotherProject(Board board, Project newProject)
    {
        if (board.Project.Id == newProject.Id) throw new NewProjectIdEqualOldIdException();
        
        board.Project = newProject;
    }

    public void UpdateImageHeaderPath(Board board, string? savedFileName)
    {
        board.ImageHeaderPath = savedFileName;
    }
}