using Vtodo.Entities.Models;

namespace Vtodo.DomainServices.Interfaces;

internal interface IProjectService
{
    void UpdateProject(Project project, string title);
}