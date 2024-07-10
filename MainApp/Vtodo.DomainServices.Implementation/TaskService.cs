using Vtodo.DomainServices.Interfaces;
using Vtodo.Entities.Enums;
using Vtodo.Entities.Models;

namespace Vtodo.DomainServices.Implementation;

internal class TaskService : ITaskService
{
    public void UpdateTask(TaskM task, string title, string description, bool isCompleted, int? endDateTimeStamp, 
        TaskPriority priority)
    {
        task.Title = title;
        task.Description = description;
        task.IsCompleted = isCompleted;
        if (endDateTimeStamp == null)
        {
            task.EndDate = null;
        }
        else
        {
            task.EndDate = DateTimeOffset.FromUnixTimeSeconds((int) endDateTimeStamp).DateTime;
        }

        task.Priority = priority;
    }

    public void MoveTaskToRoot(TaskM task)
    {
        task.ParentTask = null;
    }

    public void MoveTaskToAnotherTask(TaskM task, TaskM newParentTask)
    {
        task.ParentTask = newParentTask;
    }

    public void MoveAllTaskFromListToAnotherBoard(List<TaskM> tasksList, Board newBoard)
    {
        foreach (var task in tasksList)
        {
            task.Board = newBoard;
        }
    }
}