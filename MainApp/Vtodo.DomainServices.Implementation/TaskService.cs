using Vtodo.DomainServices.Interfaces;
using Vtodo.Entities.Enums;
using Vtodo.Entities.Exceptions;
using Vtodo.Entities.Models;

namespace Vtodo.DomainServices.Implementation;

internal class TaskService : ITaskService
{
    public TaskM CreateTask(string title, string description, Board board, bool isCompleted = false, 
        long? endDateTimeStamp = null, TaskPriority priority = TaskPriority.None, TaskM? newParentTaskM = null)
    {
        var task = new TaskM()
        {
            Title = title,
            Description = description,
            Priority = priority,
            PrioritySort = 0,
            EndDate = endDateTimeStamp == null
                ? null
                : DateTimeOffset.FromUnixTimeSeconds((long)endDateTimeStamp).DateTime,
            IsCompleted = isCompleted,
            Board = board,
            ParentTask = newParentTaskM
        };

        return task;
    }

    public void UpdateTask(TaskM task, string title, string description, long? endDateTimeStamp)
    {
        task.Title = title;
        task.Description = description;
        task.EndDate = endDateTimeStamp == null
            ? null
            : DateTimeOffset.FromUnixTimeSeconds((long)endDateTimeStamp).DateTime;
    }
    
    public void UpdateTaskComplete(TaskM task, bool isCompleted)
    {
        task.IsCompleted = isCompleted;
    }
    
    public void UpdateTaskPriority(TaskM task, TaskPriority priority)
    {
        task.Priority = priority;
    }

    public void MoveTaskToRoot(TaskM task)
    {
        task.ParentTask = null;
    }

    public void MoveTaskToAnotherTask(TaskM task, TaskM newParentTask)
    {
        if (task.Id == newParentTask.Id) throw new TaskIdEqualNewParentTaskIdException();
        if (task.ParentTask != null && task.ParentTask.Id == newParentTask.Id) throw new NewParentTaskIdEqualOldIdException();
        
        task.ParentTask = newParentTask;
    }

    public void MoveAllTaskFromListToAnotherBoard(List<TaskM> tasksList, Board newBoard)
    {
        if (tasksList.Count > 0 && tasksList.First().Board.Id == newBoard.Id) throw new NewBoardIdEqualOldIdException();

        foreach (var task in tasksList)
        {
            task.Board = newBoard;
        }
    }
}