using Vtodo.DomainServices.Interfaces;
using Vtodo.Entities.Models;

namespace Vtodo.DomainServices.Implementation;

internal class AccountService : IAccountService
{
    public Account CreateAccount(string email, string username, string hashedPassword, byte[] salt, string? firstname = null,
        string? surname = null)
    {
        var account = new Account()
        {
            Email = email,
            Username = username,
            HashedPassword = hashedPassword,
            Salt = salt,
            Firstname = firstname,
            Surname = surname,
        };

        return account;
    }
}