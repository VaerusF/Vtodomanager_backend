using Vtodo.Entities.Models;

namespace Vtodo.DomainServices.Interfaces;

internal interface IProjectService
{
    Project CreateProject(string title);
    void UpdateProject(Project project, string title);
}