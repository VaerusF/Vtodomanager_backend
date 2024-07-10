using Vtodo.Entities.Models;

namespace Vtodo.DomainServices.Interfaces;

internal interface IBoardService
{
    void UpdateBoard(Board board, string title);
    void UpdateBoardPrioritySort(Board board, int priority);
    void SwapBoardsPrioritySort(Board board, Board board2);
    void MoveBoardToAnotherProject(Board board, Project newProject);
    void UpdateImageHeaderPath(Board board, string? savedFileName);
}