using Vtodo.DomainServices.Interfaces;
using Vtodo.Entities.Models;

namespace Vtodo.DomainServices.Implementation;

internal class ProjectService : IProjectService
{
    public Project CreateProject(string title)
    {
        var project = new Project()
        {
            Title = title
        };

        return project;
    }

    public void UpdateProject(Project project, string title)
    {
        project.Title = title;
    }
}