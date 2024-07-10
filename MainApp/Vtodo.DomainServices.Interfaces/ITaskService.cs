using Vtodo.Entities.Enums;
using Vtodo.Entities.Models;

namespace Vtodo.DomainServices.Interfaces;

internal interface ITaskService
{
    void UpdateTask(TaskM task, string title, string description, 
        bool isCompleted, int? endDateTimeStamp, TaskPriority priority);

    void MoveTaskToRoot(TaskM task);
    void MoveTaskToAnotherTask(TaskM task, TaskM newParentTask);
    void MoveAllTaskFromListToAnotherBoard(List<TaskM> tasksList, Board newBoard);
}