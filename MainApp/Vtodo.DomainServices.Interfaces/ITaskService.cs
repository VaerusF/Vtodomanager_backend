using Vtodo.Entities.Enums;
using Vtodo.Entities.Models;

namespace Vtodo.DomainServices.Interfaces;

internal interface ITaskService
{
    TaskM CreateTask(string title, string description, Board board, bool isCompleted = false, 
        long? endDateTimeStamp = null, TaskPriority priority = TaskPriority.None, TaskM? newParentTaskM = null);
    void UpdateTask(TaskM task, string title, string description, long? endDateTimeStamp);
    void UpdateTaskComplete(TaskM task, bool isCompleted);
    void UpdateTaskPriority(TaskM task, TaskPriority priority);
    void MoveTaskToRoot(TaskM task);
    void MoveTaskToAnotherTask(TaskM task, TaskM newParentTask);
    void MoveAllTaskFromListToAnotherBoard(List<TaskM> tasksList, Board newBoard);
}