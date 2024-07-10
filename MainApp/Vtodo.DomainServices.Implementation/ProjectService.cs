using Vtodo.DomainServices.Interfaces;
using Vtodo.Entities.Models;

namespace Vtodo.DomainServices.Implementation;

internal class ProjectService : IProjectService
{
    public void UpdateProject(Project project, string title)
    {
        project.Title = title;
    }
}