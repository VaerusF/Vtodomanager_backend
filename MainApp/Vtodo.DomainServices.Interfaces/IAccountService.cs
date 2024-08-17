using Vtodo.Entities.Models;

namespace Vtodo.DomainServices.Interfaces;

internal interface IAccountService
{
    Account CreateAccount(
        string email, 
        string username, 
        string hashedPassword,
        byte[] salt,
        string? firstname = null, 
        string? surname = null
    );
}